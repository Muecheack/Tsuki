using cs.DbAction;
using cs.Mods.Notifation;
using cs.Repo;
using cs.Services;
using Microsoft.AspNetCore.Mvc;

namespace cs.Endpoints;

public sealed class ChatEndpoints
{
    public static async Task<IResult> Chat(
        [FromBody] UserChatDto chat,
        DbActions dbAction,
        SendToAi sendToAi,
        IConfiguration config,
        Shell_CallAi shell_CallAi,
        Shell_TimerEvent shell_TimerEvent)
    {
        if (EndpointValidation.Required(
                ("userName", chat.userName),
                ("message", chat.message),
                ("type", chat.type)) is { } validation)
        {
            return validation;
        }

        // 用户发消息就处理用户(service), AI回消息才处理AI(client)
        var userName = chat.userName;
        var content = chat.message;
        var type = chat.type;
        var currentSessionId = chat.sessionId;

        // AI 服务地址：开发默认 10086，打包后由 Electron 通过 Address__Pyai 改端口。
        var aiUrl = (config["Address:Pyai"] ?? "http://127.0.0.1:10086").TrimEnd('/') + "/api/ai";

        // 拿userId
        var safeGetId = dbAction.WithExistenceCheck(dbAction.UserExitAsync)(dbAction.GetUserId);
        var userId = await safeGetId(userName);

        // 拿key
        var apiKey = await dbAction.GetApiKeyAsync(userName);
        // 不携带sessionId则创建会话
        if (!chat.sessionId.HasValue)
        {
            try
            {
                if (userId is { } uid)
                {
                    // 创建会话
                    var sessionId = await dbAction.CreateSessionAsync(uid);
                    return await ProcessChatAsync(dbAction, sendToAi, shell_CallAi, shell_TimerEvent: shell_TimerEvent, aiUrl, sessionId, uid,
                        userName, content, type, apiKey, isNewSession: true);
                }

                return Results.BadRequest(new
                {
                    code = 400,
                    message = "根本没有这种userId!"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // 携带sessionId就原地添加
        try
        {
            if (userId is { } uid && currentSessionId is { } csid)
            {
                return await ProcessChatAsync(dbAction, sendToAi, shell_CallAi, shell_TimerEvent: shell_TimerEvent, aiUrl, csid, uid,
                    userName, content, type, apiKey, isNewSession: false);
            }

            return Results.BadRequest(new
            {
                code = 400,
                message = "反正就是有问题!没有userId或者没填sessionId!(不对, 没填sessionId应该是走if, 哎呀反正有问题)"
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // 落库 -> 发 AI -> 落库 -> 响应前端 的链条
    private static async Task<IResult> ProcessChatAsync(
        DbActions dbAction,
        SendToAi sendToAi,
        Shell_CallAi shell_CallAi,
        Shell_TimerEvent shell_TimerEvent,
        string aiUrl,
        int sessionId,
        int userId,
        string userName,
        string? content,
        string? type,
        string? apiKey,
        bool isNewSession)
    {
        await SaveUserMessageAsync(dbAction, sessionId, userId, content, type);
        var history = await dbAction.GetChatHistoryAsync(sessionId);
        var payload = await dbAction.MapRoleContent(sessionId, userId, userName, history, apiKey);

        // 把这些数据扔给Notifation模块用, 顺便启动计时器
        shell_CallAi.GetInfoFactory = () => (sessionId, memberId: userId, userName, apiKey);
        shell_TimerEvent.PutTimerBack(sessionId);
        var reply = await sendToAi.SendAiAsync(aiUrl, payload);

        if (isNewSession)
        {
            if (reply.sessionId is { } replySessionId)
            {
                var aiId = await ResolveAiIdAsync(dbAction, reply.aiName, replySessionId, userId);
                await dbAction.SaveChatInMessageAsync(replySessionId, aiId,
                    reply.message, reply.type);
            }
        }
        else
        {
            var aiId = await ResolveAiIdAsync(dbAction, reply.aiName, sessionId, userId);
            await dbAction.SaveChatInMessageAsync(sessionId, aiId,
                reply.message, reply.type);
        }

        return Results.Ok(new
        {
            code = 200,
            message = "Ok",
            data = new
            {
                reply.sessionId,
                reply.message
            }
        });
    }

    /// 用户消息落库，“只发文件”那轮的空正文跳过。
    private static async Task SaveUserMessageAsync(
        DbActions dbAction,
        int sessionId,
        int userId,
        string? content,
        string? type)
    {
        if (!string.IsNullOrWhiteSpace(content))
            await dbAction.SaveChatInMessageAsync(sessionId, userId, content, type);
    }

    /// AI 消息优先记在当前会话已有的 AI 名下，没有就按名字复用，再没有才新建。
    private static async Task<int> ResolveAiIdAsync(
        DbActions dbAction,
        string aiNameFromAi,
        int sessionId,
        int userId)
    {
        // ① 会话里本来就有 AI(ApplyFriend 建的那条, 带 tags + 头像), AI 的话就记在它头上
        if (await dbAction.GetAiIdInSessionAsync(sessionId, userId) is { } inSession)
            return inSession;

        // ② 名字能对上就复用, 但绝不能是真人自己(历史数据里存在 AI 用了用户名的脏行)
        if (await dbAction.GetAiIdByNameAsync(aiNameFromAi, userId) is { } byName)
        {
            await dbAction.AddUserInSessionParticipantAsync(byName, sessionId);
            return byName;
        }

        // ③ 兜底新建, 并拉进会话
        var createdId = await dbAction.AddAIInDbAsync(aiNameFromAi);
        await dbAction.AddUserInSessionParticipantAsync(createdId, sessionId);
        return createdId;
    }
}

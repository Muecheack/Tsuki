using cs.DbAction;
using cs.Services;
using Microsoft.AspNetCore.Mvc;

namespace cs.Controllers;

[ApiController]
[Route("api/[controller]")] // api/Chat
public class ChatController : ControllerBase
{
    private readonly DbActions _dbAction;
    private readonly SendToAi _sendToAi;
    private readonly IConfiguration _config;

    public ChatController(DbActions dbAction, SendToAi sendToAi, IConfiguration config)
    {
        _dbAction = dbAction;
        _sendToAi = sendToAi;
        _config = config;
    }

    [HttpPost("chat1")] // api/Chat/chat1
    public async Task<IActionResult> Foo([FromBody] UserChatDto chat)
    {
        // 用户发消息就处理用户(service), AI回消息才处理AI(client)

        var userName = chat.userName;
        var content = chat.message;
        var type = chat.type;
        var currentSessionId = chat.sessionId;
        
        // AI 服务基址：开发默认本机 10086；打包后由 Electron 主进程用环境变量 Address__Pyai
        // 指到它动态挑出来的端口。基址和路径分开配，聊天和预人设共用一个基址。
        var aiUrl = (_config["Address:Pyai"] ?? "http://127.0.0.1:10086").TrimEnd('/') + "/api/ai";

        // 拿userId
        var safeGetId = _dbAction.WithExistenceCheck(_dbAction.UserExitAsync)(_dbAction.GetUserId);
        var userId = await safeGetId(userName);

        // 拿key
        var apiKey = await _dbAction.GetApiKeyAsync(userName);
        // 不携带sessionId则创建会话
        if (!chat.sessionId.HasValue)
        {
            try
            {
                if (userId is { } uid)
                {
                    // 创建会话
                    var sessionId = await _dbAction.CreateSessionAsync(uid);
                    // 保存消息
                    await SaveUserMessageAsync(sessionId, uid, content, type);
                    // 扔给ai
                    var history = await _dbAction.GetChatHistoryAsync(sessionId);
                    var payload = await _dbAction.MapRoleContent(sessionId, uid, userName, history, apiKey);
                    var reply   = await _sendToAi.SendAiAsync(aiUrl, payload);
                    
                    Console.WriteLine($"收到回复:{reply.message}, ai名字:{reply.aiName}");
                    Console.WriteLine($"用户名字:{userName}");

                    // AI走相同路径(存数据库, 返回给前端)
                    if (reply.sessionId is { } asid)
                    {
                        var aiId = await ResolveAiIdAsync(reply.aiName, asid, uid);
                        await _dbAction.SaveChatInMessageAsync(asid, aiId,
                            reply.message, reply.type);
                    }

                    return Ok(new
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

                return BadRequest(new
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
                // 保存消息
                await SaveUserMessageAsync(csid, uid, content, type);
                // 扔给ai
                var history = await _dbAction.GetChatHistoryAsync(csid);
                var payload = await _dbAction.MapRoleContent(csid, uid, userName, history, apiKey);
                var reply   = await _sendToAi.SendAiAsync(aiUrl, payload);
                
                Console.WriteLine($"收到回复:{reply.message}, ai名字:{reply.aiName}");
                Console.WriteLine($"用户:{userName}");
                // AI走相同路径(存数据库, 返回给前端)
                var aiId = await ResolveAiIdAsync(reply.aiName, csid, uid);
                await _dbAction.SaveChatInMessageAsync(csid, aiId,
                    reply.message, reply.type);
                // TODO: 前端要求格式并非如此
                return Ok(new
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

            return BadRequest(new
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

    /// 用户消息落库。空正文直接跳过 —— 前端「只发文件」时会用 type=file + 空 message 触发这一轮，
    /// 文件正文此时已经由 FileController 入池了，再存一条空消息只会污染历史。
    private async Task SaveUserMessageAsync(int sessionId, int userId, string? content, string? type)
    {
        if (!string.IsNullOrWhiteSpace(content))
            await _dbAction.SaveChatInMessageAsync(sessionId, userId, content, type);
    }

    /// AI 回完话之后: 决定这条 AI 消息记在哪个用户头上。
    /// 坑: pyai 回的 aiName 其实是"会话字典的 key", 也就是用户名(见 pyai/Controller/ordinary_chat.py 的 "aiName": name),
    /// 拿它直接 AddAIInDbAsync 就会造出一堆叫用户名/重名的空壳 AI —— 数据库里 keke 已经堆了 40 条。
    /// 所以顺序是: ① 会话里已有的那位 AI ② 按名字找(排除真人自己) ③ 真没有才新建。
    private async Task<int> ResolveAiIdAsync(string aiNameFromAi, int sessionId, int userId)
    {
        // ① 会话里本来就有 AI(ApplyFriend 建的那条, 带 tags + 头像), AI 的话就记在它头上
        if (await _dbAction.GetAiIdInSessionAsync(sessionId, userId) is { } inSession)
            return inSession;

        // ② 名字能对上就复用, 但绝不能是真人自己(历史数据里存在 AI 用了用户名的脏行)
        if (await _dbAction.GetAiIdByNameAsync(aiNameFromAi, userId) is { } byName)
        {
            await _dbAction.AddUserInSessionParticipantAsync(byName, sessionId);
            return byName;
        }

        // ③ 兜底新建, 并拉进会话
        Console.WriteLine($"[warn] session {sessionId} 里没有 AI 参与者, 按名字 \"{aiNameFromAi}\" 新建一个");
        var createdId = await _dbAction.AddAIInDbAsync(aiNameFromAi);
        await _dbAction.AddUserInSessionParticipantAsync(createdId, sessionId);
        return createdId;
    }
}
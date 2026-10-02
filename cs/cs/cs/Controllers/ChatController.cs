using cs.DbAction;
using cs.Services;
using Microsoft.AspNetCore.Mvc;

namespace cs.Controllers;

[ApiController]
[Route("api/[controller]")] // api/Chat
public class ChatController : ControllerBase
{
    /*
         用户发消息就处理用户(service), AI回消息才处理AI(client)
        */

    private readonly DbActions _dbAction;
    private readonly SendToAi _sendToAi;

    public ChatController(DbActions dbAction, SendToAi sendToAi)
    {
        _dbAction = dbAction;
        _sendToAi = sendToAi;
    }

    [HttpPost("chat1")] // api/Chat/chat1
    public async Task<IActionResult> Foo([FromBody] UserChatDto chat)
    {
        // 用户发消息就处理用户(service), AI回消息才处理AI(client)

        var userName = chat.userName;
        var content = chat.message;
        var type = chat.type;
        var currentSessionId = chat.sessionId;

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
                    await _dbAction.SaveChatInMessageAsync(sessionId, uid, content, type);
                    // 扔给ai
                    // TODO: 到时候上AI端看一眼真实路径再改
                    var reply = await _sendToAi.SendAiAsync("http://127.0.0.1:10086/api/ai",
                        _dbAction.MapRoleContent(sessionId, uid, userName,
                            await _dbAction.GetChatHistoryAsync(sessionId), apiKey));
                    Console.WriteLine($"收到回复:{reply.message}, ai名字:{reply.aiName}");
                    Console.WriteLine($"用户名字:{userName}");

                    // AI走相同路径(参进SessionParticipant, 存数据库, 返回给前端)
                    // 添加AI进User
                    var aiId = await _dbAction.AddAIInDbAsync(reply.aiName);
                    if (reply.sessionId is { } asid)
                    {
                        // 参进SessionParticipant
                        await _dbAction.AddUserInSessionParticipantAsync(aiId, asid
                        );
                        // 保存
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
                await _dbAction.SaveChatInMessageAsync(csid, uid, content, type);
                // 扔给ai
                // TODO: 到时候上AI端看一眼真实路径再改
                var reply = await _sendToAi.SendAiAsync("http://127.0.0.1:10086/api/ai",
                    _dbAction.MapRoleContent(csid, uid, userName, await _dbAction.GetChatHistoryAsync(csid), apiKey));
                Console.WriteLine($"收到回复:{reply.message}, ai名字:{reply.aiName}");
                Console.WriteLine($"用户:{userName}");
                // AI走相同路径(参进SessionParticipant, 存数据库, 返回给前端)
                // 添加AI进User
                var aiId = await _dbAction.AddAIInDbAsync(reply.aiName);
                // 参进SessionParticipant
                await _dbAction.AddUserInSessionParticipantAsync(aiId, csid
                );
                // 保存
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
}
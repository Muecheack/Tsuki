using cs.Shell;
using Microsoft.AspNetCore.Mvc;

namespace cs.Controllers;

[ApiController]
[Route("api/[controller]")] // api/ApplyFriend
public class ApplyFriendController : ControllerBase
{
    private readonly DbIO _dbIo;

    public ApplyFriendController(DbIO dbIo)
    {
        _dbIo = dbIo;
    }

    [HttpPost("apply")] // api/ApplyFriend/apply
    public async Task<IActionResult> ApplyFriend([FromBody] ApplyFriendDto dto)
    {
        var tagList = dto.tags;
        var avatarUrl = dto.avatarUrl;
        var preReply = dto.preReply;
        var userName = dto.userName;
        var aiName = dto.aiName;

        // 存AI -> 存Tag -> 存Avatar -> 建会话 -> 加聊天 -> 存消息
        var aiId = await _dbIo.AddAIAsync(aiName);
        await _dbIo.SaveTagAsync(tagList, aiId);
        await _dbIo.SaveAvatarAsync(avatarUrl, aiId);
        var (state, userId) = await _dbIo.GetUserIdAsync(userName);
        if (state == IOState.NotFound)
            return BadRequest(new
            {
                code = 400,
                message = "根本没有这个用户",
            });

        var sessionId = await _dbIo.CreateSessionAsync(userId.Value);
        await _dbIo.JoinSessionAsync(sessionId, aiId);
        await _dbIo.SaveChatInMessageAsync(sessionId, aiId, preReply);
        return Ok(new
        {
            code = 200,
            message = "落库成功!",
            data = new
            {
                sessionId
            }
        });
    }
}
using cs.Shell;
using Microsoft.AspNetCore.Mvc;

namespace cs.Controllers;

[ApiController]
[Route("api/[controller]")] // api/ApplyFriend
public class ApplyFriendController : ControllerBase
{
    private readonly DbIO _dbIo;
    private readonly ILogger<ApplyFriendController> _logger;

    public ApplyFriendController(DbIO dbIo, ILogger<ApplyFriendController> logger)
    {
        _dbIo = dbIo;
        _logger = logger;
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

        // 以前这俩的返回值直接扔了, 落库失败也是 200, 结果 AI 一直没头像/没标签却没人发现
        var tagState = await _dbIo.SaveTagAsync(tagList, aiId);
        if (tagState != IOState.Found)
            _logger.LogWarning("AI 标签没落库 aiId={AiId} tags={Tags}", aiId, string.Join(",", tagList));

        var avatarState = await _dbIo.SaveAvatarAsync(avatarUrl, aiId);
        if (avatarState != IOState.Found)
            _logger.LogWarning("AI 头像没落库 aiId={AiId} avatarUrl={AvatarUrl}", aiId, avatarUrl);

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
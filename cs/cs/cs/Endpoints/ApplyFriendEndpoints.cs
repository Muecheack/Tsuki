using cs.Shell;
using Microsoft.AspNetCore.Mvc;

namespace cs.Endpoints;

public sealed class ApplyFriendEndpoints
{
    public static async Task<IResult> ApplyFriend(
        [FromBody] ApplyFriendDto dto,
        DbIO dbIo,
        ILogger<ApplyFriendEndpoints> logger)
    {
        if (EndpointValidation.Required(
                ("tags", dto.tags),
                ("avatarUrl", dto.avatarUrl),
                ("preReply", dto.preReply),
                ("userName", dto.userName),
                ("aiName", dto.aiName)) is { } validation)
        {
            return validation;
        }

        var tagList = dto.tags;
        var avatarUrl = dto.avatarUrl;
        var preReply = dto.preReply;
        var userName = dto.userName;
        var aiName = dto.aiName;

        // 存AI -> 存Tag -> 存Avatar -> 建会话 -> 加聊天 -> 存消息
        var aiId = await dbIo.AddAIAsync(aiName);

        // 以前这俩的返回值直接扔了, 落库失败也是 200, 结果 AI 一直没头像/没标签却没人发现
        var tagState = await dbIo.SaveTagAsync(tagList, aiId);
        if (tagState != IOState.Found)
            logger.LogWarning("AI 标签没落库 aiId={AiId} tags={Tags}", aiId, string.Join(",", tagList));

        var avatarState = await dbIo.SaveAvatarAsync(avatarUrl, aiId);
        if (avatarState != IOState.Found)
            logger.LogWarning("AI 头像没落库 aiId={AiId} avatarUrl={AvatarUrl}", aiId, avatarUrl);

        var (state, userId) = await dbIo.GetUserIdAsync(userName);
        if (state == IOState.NotFound || userId is not { } uid)
            return Results.BadRequest(new
            {
                code = 400,
                message = "根本没有这个用户",
            });

        var sessionId = await dbIo.CreateSessionAsync(uid);
        await dbIo.JoinSessionAsync(sessionId, aiId);
        await dbIo.SaveChatInMessageAsync(sessionId, aiId, preReply);
        return Results.Ok(new
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

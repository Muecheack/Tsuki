using cs.Core;
using cs.Shell;
using Microsoft.AspNetCore.Mvc;

namespace cs.Endpoints;

public sealed class PrePersonEndpoints
{
    public static async Task<IResult> PrePerson(
        [FromBody] PrePersonDto prePersonDto,
        DbIO dbIO,
        HttpIO httpIO,
        ILogger<PrePersonEndpoints> logger,
        IConfiguration config)
    {
        if (EndpointValidation.Required(
                ("userName", prePersonDto.userName),
                ("aiName", prePersonDto.aiName)) is { } validation)
        {
            return validation;
        }

        var userName = prePersonDto.userName;
        var aiName = prePersonDto.aiName;

        // 预人设接口地址，打包后由 Address__Pyai 覆盖。
        var aiPreUrl = (config["Address:Pyai"] ?? "http://127.0.0.1:10086").TrimEnd('/') + "/api/pre";

        logger.LogInformation("收到预人设请求 userName={UserName} aiName={AiName}", userName, aiName);

        try
        {
            var (state, userId, gender, callName, apiKey) = await dbIO.GetUserInfoAsync(userName);
            if (state == IOState.NotFound)
            {
                logger.LogWarning("用户不存在，拒绝预人设 userName={UserName}", userName);
                return Results.BadRequest(new { code = 400, message = "用户不存在" });
            }

            if (userId is { } uid && gender is { } gen && callName is { } call && apiKey is { } apiK)
            {
                var tags = await dbIO.GetTagsAsync();
                var avatars = await dbIO.GetAvatar();
                logger.LogInformation(
                    "已收集上下文 userId={UserId}  tagCount={TagCount} avatarCount={AvatarCount}",
                    uid, tags.Count, avatars.Count);

                var payload = SwitchFormat.FormatDict(userName, aiName, gen, call, tags, avatars, apiK);
                var reply = await httpIO.ToAi(payload, aiPreUrl);
                logger.LogInformation(
                    "AI 预人设返回 avatarId={AvatarId} selectedTags={SelectedTags}",
                    reply.selected.avatarId, string.Join(", ", reply.selected.tags));

                var selectedAvatar = await dbIO.GetAvatarByIdAsync(reply.selected.avatarId);

                return Results.Ok(new
                {
                    code = 200,
                    message = "预人设成功",
                    data = new
                    {
                        reply.reply,
                        selected = new
                        {
                            reply.selected.tags,
                            avatarUrl = selectedAvatar
                        }
                    }
                });
            }

            logger.LogWarning(
                "用户资料不完整，无法预人设 userName={UserName} userId={UserId} gender={Gender} callName={CallName}",
                userName, userId, gender, callName);
            return Results.BadRequest(new { code = 400, message = "用户不存在|缺少性别|昵称" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "预人设处理失败 userName={UserName} aiName={AiName}", userName, aiName);
            return Results.BadRequest(new
            {
                code = 400,
                message = $"出错了:{ex.Message}"
            });
        }
    }
}

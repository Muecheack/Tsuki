using cs.Core;
using cs.Shell;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace cs.Controllers;

[ApiController]
[Route("api/[controller]")] // api/PrePerson
public class PrePersonController : ControllerBase
{
    private readonly DbIO _dbIO;
    private readonly HttpIO _httpIO;
    private readonly ILogger<PrePersonController> _logger;
    /// AI 服务预人设接口地址，基址来自 Address:Pyai（打包后由 Electron 用 Address__Pyai 覆盖）
    private readonly string _aiPreUrl;

    public PrePersonController(DbIO dbIO, HttpIO httpIO, ILogger<PrePersonController> logger, IConfiguration config)
    {
        _dbIO = dbIO;
        _httpIO = httpIO;
        _logger = logger;
        _aiPreUrl = (config["Address:Pyai"] ?? "http://127.0.0.1:10086").TrimEnd('/') + "/api/pre";
    }

    [HttpPost("preperson1")] // api/PrePerson/perperson1
    public async Task<IActionResult> PrePerson([FromBody] PrePersonDto prePersonDto)
    {
        var userName = prePersonDto.userName;
        var aiName = prePersonDto.aiName;

        _logger.LogInformation("收到预人设请求 userName={UserName} aiName={AiName}", userName, aiName);

        try
        {
            var (state, userId, gender, callName, apiKey) = await _dbIO.GetUserInfoAsync(userName);
            if (state == IOState.NotFound)
            {
                _logger.LogWarning("用户不存在，拒绝预人设 userName={UserName}", userName);
                return BadRequest(new { code = 400, message = "用户不存在" });
            }

            if (userId is { } uid && gender is { } gen && callName is { } call && apiKey is { } apiK)
            {
                var tags = await _dbIO.GetTagsAsync();
                var avatars = await _dbIO.GetAvatar();
                _logger.LogInformation(
                    "已收集上下文 userId={UserId}  tagCount={TagCount} avatarCount={AvatarCount}",
                    uid, tags.Count, avatars.Count);

                var payload = SwitchFormat.FormatDict(userName, aiName, gen, call, tags, avatars, apiK);
                var reply = await _httpIO.ToAi(payload, _aiPreUrl);
                _logger.LogInformation(
                    "AI 预人设返回 avatarId={AvatarId} selectedTags={SelectedTags}",
                    reply.selected.avatarId, string.Join(", ", reply.selected.tags));

                var selectedAvatar = await _dbIO.GetAvatarByIdAsync(reply.selected.avatarId);

                return Ok(new
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

            _logger.LogWarning(
                "用户资料不完整，无法预人设 userName={UserName} userId={UserId} gender={Gender} callName={CallName}",
                userName, userId, gender, callName);
            return BadRequest(new { code = 400, message = "用户不存在|缺少性别|昵称" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预人设处理失败 userName={UserName} aiName={AiName}", userName, aiName);
            return BadRequest(new
            {
                code = 400,
                message = $"出错了:{ex.Message}"
            });
        }
    }
}
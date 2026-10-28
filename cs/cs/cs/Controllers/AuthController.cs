using System.Security.Claims;
using cs.DbAction;
using cs.Repo;
using cs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace cs.Controllers;

/// 负责认证+转发消息
[ApiController]
[Route("api/[controller]")] // api/Auth
public class AuthController : ControllerBase
{
    private readonly KeKeContext _db;
    private readonly JwtTokenService _jwt;
    private readonly DbActions _dbActions;
    private readonly IConfiguration _config;

    public AuthController(KeKeContext db, JwtTokenService jwt, DbActions dbActions, IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _dbActions = dbActions;
        _config = config;
    }

    /// 鉴权开关：与 Program.cs 读的是同一个键，本地版为 false
    private bool AuthEnabled => _config.GetValue<bool>("Auth:Enable");

    [AllowAnonymous]
    [HttpPost("register")] // api/Auth/register
    public async Task<IActionResult> Register([FromBody] UserDto user)
    {
        try
        {
            string name = user.name;
            string gender = user.gender;
            string call = user.call;
            string apiKey = user.apiKey;

            var hasUser = await _dbActions.UserExitAsync(name);
            if (hasUser)
            {
                return BadRequest(new { code = 400, message = "用户已存在", data = new { } });
            }

            // 先存数据库（返回新增的 User，SaveChanges 后 Id 已回填）
            var saved = await _dbActions.AddInDbAsync(name, gender, call, apiKey);

            // 再鉴权：用新用户的 Id 签发 token。
            // 鉴权关闭（本地版）时不签 JWT —— 但前端把这个字段当"免登录凭证"用
            // （router/index.ts 判断 bot-face-token 是否存在，App.vue 靠它决定要不要补发人设），
            // 所以给一个本地档案标识：不参与任何后端校验，只表示"本机已经有档案了"。
            var token = AuthEnabled
                ? _jwt.CreateToken(new[]
                {
                    new Claim("UserId", saved.Id.ToString()),
                    new Claim("Name", saved.Name ?? ""),
                })
                : Guid.NewGuid().ToString("N");
            return Ok(new { code = 200, message = "注册通过了喵", data = new { saved.Id, token } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
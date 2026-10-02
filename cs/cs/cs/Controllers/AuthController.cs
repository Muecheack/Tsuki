using System.Security.Claims;
using cs.DbAction;
using cs.Repo;
using cs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cs.Controllers;

/// 负责认证+转发消息
[ApiController]
[Route("api/[controller]")] // api/Auth
public class AuthController : ControllerBase
{
    private readonly KeKeContext _db;
    private readonly JwtTokenService _jwt;
    private readonly DbActions _dbActions;

    public AuthController(KeKeContext db, JwtTokenService jwt, DbActions dbActions)
    {
        _db = db;
        _jwt = jwt;
        _dbActions = dbActions;
    }

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

            // 再鉴权：用新用户的 Id 签发 token
            var token = _jwt.CreateToken(new[]
            {
                new Claim("UserId", saved.Id.ToString()),
                new Claim("Name", saved.Name ?? ""),
            });
            return Ok(new { code = 200, message = "注册通过了喵", data = new { saved.Id, token } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
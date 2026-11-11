using System.Security.Claims;
using cs.DbAction;
using cs.Services;
using Microsoft.AspNetCore.Mvc;

namespace cs.Endpoints;

public sealed class AuthEndpoints
{
    public static async Task<IResult> Register(
        [FromBody] UserDto user,
        DbActions dbActions,
        JwtTokenService jwt,
        IConfiguration config)
    {
        if (EndpointValidation.Required(
                ("name", user.name),
                ("gender", user.gender),
                ("call", user.call),
                ("apiKey", user.apiKey)) is { } validation)
        {
            return validation;
        }

        try
        {
            string name = user.name;
            string gender = user.gender;
            string call = user.call;
            string apiKey = user.apiKey;

            var hasUser = await dbActions.UserExitAsync(name);
            if (hasUser)
            {
                return Results.BadRequest(new { code = 400, message = "用户已存在", data = new { } });
            }

            // 先落库，拿到自增 Id
            var saved = await dbActions.AddInDbAsync(name, gender, call, apiKey);

            // 云端用新用户 Id 签 token；本地不签 JWT。
            // 前端拿 token 当“已有档案”的标记，所以本地也塞个随机串。
            var authEnabled = config.GetValue<bool>("Auth:Enable");
            var token = authEnabled
                ? jwt.CreateToken(new[]
                {
                    new Claim("UserId", saved.Id.ToString()),
                    new Claim("Name", saved.Name ?? ""),
                })
                : Guid.NewGuid().ToString("N");
            return Results.Ok(new { code = 200, message = "注册通过了喵", data = new { saved.Id, token } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace cs.Services;

    /// <summary>
    /// JWT 令牌签发服务（通用样板，与具体业务无关）。
    /// 在业务代码（注册/登录控制器）里注入它来签发 token。
    /// </summary>
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// 签发 JWT。claims 里放"以后能认出这个用户"的信息，例如 new Claim("UserId", user.Id.ToString())。
        /// 有效期 7 天。
        /// </summary>
        public string CreateToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }


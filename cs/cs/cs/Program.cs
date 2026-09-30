using System.Text;
using cs.DbAction;
using cs.Repo;
using cs.Services;
using cs.Shell;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

namespace cs;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<KeKeContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // ── JWT 鉴权样板（全球通用，与业务无关）──
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 验签名：token 必须是用 Jwt:Key 签的
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    // 验发行方 / 受众：防别处 token 冒充
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    // 验过期
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        builder.Services.AddAuthorization();
        // 通用 token 签发服务，注册进容器供业务代码注入
        builder.Services.AddScoped<JwtTokenService>();
        // 注入DbAction
        builder.Services.AddScoped<DbActions>();
        // 注册SendToAi
        builder.Services.AddScoped<SendToAi>();
        // 注册DbIo
        builder.Services.AddScoped<DbIO>();
        // http注入
        builder.Services.AddHttpClient<SendToAi>();
        // 注册HttpIo（PrePersonController 依赖 HttpIO，此前漏注册导致该接口 500）
        builder.Services.AddHttpClient<HttpIO>();
        // ── CORS 样板：开发期 Allow All，上线前收紧为前端域名 ──
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("dev", policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.MapOpenApi();

        app.UseHttpsRedirection();

        app.UseCors("dev"); // 跨域放行，必须在 UseAuthentication 之前
        app.UseAuthentication(); // 验票，把 token 还原成用户身份 —— 必须先于 UseAuthorization
        app.UseAuthorization();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "Resource")),
            RequestPath = "/Resource" // 访问前缀
        });
        app.MapControllers();

        app.Run();
    }
}
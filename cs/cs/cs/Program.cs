using System.Text;
using cs.DbAction;
using cs.Repo;
using cs.Services;
using cs.Shell;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
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

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<KeKeContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // ── 鉴权开关 ──
        // Auth:Enable = false（本地版)
        // Auth:Enable = true （云端版）
        //                              除标了 [AllowAnonymous] 的接口（如注册）外都要求 token。
        // 键名必须和 appsettings.json 的嵌套结构对上 —— "Auth": { "Enable": ... } → "Auth:Enable"。
        // 环境变量覆盖用双下划线：Auth__Enable / Jwt__Key（写成 "Auth: Enable"（带空格）是取不到的）。
        var authEnable = builder.Configuration.GetValue<bool>("Auth:Enable");
        Console.WriteLine($"[auth] enabled={authEnable}");

        if (authEnable)
        {
            // 开着鉴权却没有可用密钥时直接启动失败：否则 SymmetricSecurityKey 会等到首个请求
            // 解析 JwtBearerOptions 时才抛异常，表现成"所有接口 500"，极难排查。
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException(
                    "Auth:Enable=true 但 Jwt:Key 缺失或过短（HS256 至少 32 字节）。请设置环境变量 Jwt__Key。");
            }

            // ── JWT 鉴权样板（全球通用，与业务无关）──
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // 验签名：token 必须是用 Jwt:Key 签的
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
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

            // 全局挂 AuthorizeFilter：除标了 [AllowAnonymous] 的接口外都要求 token
            builder.Services.AddControllers(options => options.Filters.Add(new AuthorizeFilter()));
        }
        else
        {
            // 本地版：无认证中间件、无全局过滤器，接口全部匿名
            builder.Services.AddControllers();
        }

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

        // 认证中间件必须与上面的 AddAuthentication/AddAuthorization 成对出现：
        // 只调 UseAuthorization 而没注册 AddAuthorization，启动时会抛
        // "Unable to find the required services ... AddAuthorization"。
        if (authEnable)
        {
            app.UseAuthentication(); // 验票，把 token 还原成用户身份 —— 必须先于 UseAuthorization
            app.UseAuthorization();
        }

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
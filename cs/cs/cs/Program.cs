using System.Text;
using cs.DbAction;
using cs.Repo;
using cs.Services;
using cs.Shell;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Data.Sqlite;
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

        // ── 数据库：SQLite 单文件库 ──
        // 连接串里的相对路径是按【进程当前工作目录】解析的，而 dotnet run、桌面壳拉起 exe、
        // 从别的目录调试，三者工作目录不一定相同，会把 KeKe.db 散落到好几个地方（各建一份空库）。
        // 这里统一锚定到 ContentRootPath，保证全项目只有一份库。
        var rawConnection = builder.Configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException(
                                "缺少连接串 ConnectionStrings:DefaultConnection");
        var sqliteConnection = new SqliteConnectionStringBuilder(rawConnection);
        if (!Path.IsPathRooted(sqliteConnection.DataSource))
        {
            sqliteConnection.DataSource =
                Path.Combine(builder.Environment.ContentRootPath, sqliteConnection.DataSource);
        }

        builder.Services.AddDbContext<KeKeContext>(options => options.UseSqlite(sqliteConnection.ToString()));

        // ── 鉴权开关 ──
        // Auth:Enable = false（本地版)
        // Auth:Enable = true （云端版）
        //                              除标了 [AllowAnonymous] 的接口（如注册）外都要求 token。
        // 键名必须和 appsettings.json 的嵌套结构对上 —— "Auth": { "Enable": ... } → "Auth:Enable"。
        // 环境变量覆盖用双下划线：Auth__Enable / Jwt__Key（写成 "Auth: Enable"（带空格）是取不到的）。
        var authEnable = builder.Configuration.GetValue<bool>("Auth:Enable");

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

        // ── 建库 / 迁移 ──
        // SQLite 是文件库，没有 DBA 会在部署后去跑 `dotnet ef database update`：
        // 启动时就把库对齐到最新迁移，库文件不存在则连带建表 + 灌种子数据（HasData）。
        // journal_mode=WAL 是写在库头里的持久开关（重复执行无害），
        // 它让读请求不再被写事务挡住 —— 默认的 delete 模式下并发一写读就互相卡。
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<KeKeContext>();
            db.Database.Migrate();
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.MapOpenApi();

        // 注：不再需要 app.UseHttpsRedirection()。桌面壳拉起 exe 时只监听 127.0.0.1 的 http，
        // 没有 https 端口可跳，留着只会每次启动刷一条 "Failed to determine the https port" 警告。

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
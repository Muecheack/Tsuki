using System.Text;
using cs.DbAction;
using cs.Endpoints;
using cs.Mods.Notifation;
using cs.Repo;
using cs.Services;
using cs.Shell;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        builder.Services.AddOpenApi();

        // SQLite 路径按当前目录解析容易到处建库，统一放到 ContentRootPath。
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

        // false 是本地版，true 是云端版；云端除注册外都要 token。
        // 配置键写 Auth:Enable，环境变量写 Auth__Enable。
        var authEnable = builder.Configuration.GetValue<bool>("Auth:Enable");

        if (authEnable)
        {
            // key 没配好就直接启动失败，别等第一个请求才炸。
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException(
                    "Auth:Enable=true 但 Jwt:Key 缺失或过短（HS256 至少 32 字节）。请设置环境变量 Jwt__Key。");
            }

            // JWT 鉴权
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
        }

        // token 签发
        builder.Services.AddScoped<JwtTokenService>();
        // 注入DbAction
        builder.Services.AddScoped<DbActions>();
        // 注册SendToAi
        builder.Services.AddScoped<SendToAi>();
        // 注册DbIo
        builder.Services.AddScoped<DbIO>();
        // 注册TimerEvent
        builder.Services.AddScoped<Shell_TimerEvent>();
        // http注入
        builder.Services.AddHttpClient<SendToAi>();
        // 注册 HttpIo（之前漏了，接口一直 500）
        builder.Services.AddHttpClient<HttpIO>();
        // CORS：开发全放开，上线再收紧
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("dev", policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });
        // 注册 SignalR
        builder.Services.AddSignalR();

        var app = builder.Build();

        // 启动时自动迁移；库不存在就建表并灌种子数据。
        // 开 WAL，读写别互相卡。
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<KeKeContext>();
            db.Database.Migrate();
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }

        if (app.Environment.IsDevelopment()) app.MapOpenApi();

        // 桌面壳只走 127.0.0.1 的 http，不需要 HttpsRedirection。

        app.UseCors("dev"); // 跨域放行，必须在 UseAuthentication 之前

        // UseAuthentication/UseAuthorization 要配套注册，不然启动就报错。
        if (authEnable)
        {
            app.UseAuthentication(); // 先验 token，再判权限
            app.UseAuthorization();
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "Resource")),
            RequestPath = "/Resource" // 访问前缀
        });

        var api = app.MapGroup("api");
        if (authEnable) api.RequireAuthorization();

        // 注册 SignalR
        api.MapHub<ChatHub>("/api/chatHub");

        api.MapPost("/Auth/register", AuthEndpoints.Register).AllowAnonymous();
        api.MapPost("/ApplyFriend/apply", ApplyFriendEndpoints.ApplyFriend);
        api.MapPost("/Chat/chat1", ChatEndpoints.Chat);
        api.MapPost("/File/file1", FileEndpoints.ReceivedFile).DisableAntiforgery();
        api.MapPost("/PrePerson/preperson1", PrePersonEndpoints.PrePerson);

        app.Run();
    }
}

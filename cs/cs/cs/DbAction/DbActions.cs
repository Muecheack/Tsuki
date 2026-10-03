using cs.Repo;
using Microsoft.EntityFrameworkCore;

namespace cs.DbAction;

public class DbActions
{
    private readonly KeKeContext _db;

    public DbActions(KeKeContext db)
    {
        _db = db;
    }
    
    /// 把前端传入的名字，性别，称呼，key扔数据库里
    public async Task<User> AddInDbAsync(string name, string gender, string callName, string apiKey, int? avatarUrlId = null)
    {
        var user = new User { Name = name, Gender = gender, CallName = callName, ApiKey = apiKey, AvatarUrlId = avatarUrlId };
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        return user;
    }


    // 添加AI(光加个名)
    public async Task<int> AddAIInDbAsync(string aiName)
    {
        var ai = new User { Name = aiName };
        await _db.Users.AddAsync(ai);
        await _db.SaveChangesAsync();
        return ai.Id;
    }
    
    /// 拿UserId, 无逻辑
    public async Task< int?> GetUserId(string userName)
    {
        var userId = await _db.Users.FirstOrDefaultAsync(x => x.Name == userName);
        return userId?.Id;
    }
    
    // 拿key, 再写一遍是图方便, 否则要拆迁
    public async Task<string> GetApiKeyAsync(string userName)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Name == userName);
        return user.ApiKey;
    }

    /// 首次会话创建
    public async Task<int> CreateSessionAsync(int memberId)
    {
        var session = new Session();
        await _db.Sessions.AddAsync(session);
        await _db.SaveChangesAsync();
        await _db.SessionParticipants.AddRangeAsync(
            new SessionParticipant { SessionId = session.Id, UserId = memberId, JoinedAt = DateTime.Now }
        );
        await _db.SaveChangesAsync();
        return session.Id;
    }


    /// 添加成员在同一会话
    public async Task AddUserInSessionParticipantAsync(int memberId, int sessionId)
    {
        var s = new SessionParticipant() { UserId = memberId, SessionId = sessionId };
        await _db.SessionParticipants.AddAsync(s);
        await _db.SaveChangesAsync();
    }


    /// 保存会话
    public async Task SaveChatInMessageAsync(int sessionId, int memberId, string memberContent, string? type = "text")
    {
        // 新增消息而非修改
        var messags = new Messages
        {
            SessionId = sessionId,
            UserId = memberId,
            Content = memberContent,
            Type = type
        };
        await _db.Messages.AddAsync(messags);
        await _db.SaveChangesAsync();
    }
    
    /// 判断用户名是否存在
    public async Task<bool> UserExitAsync(string name)
    {
        return await _db.Users.AnyAsync(x => x.Name == name);
    }
    
    /// 装饰器, 用户存在就返回Id, 不存在就返回
    public Func<Func<string, Task<int?>>, Func<string, Task<int?>>> WithExistenceCheck(Func<string, Task<bool>> isExist)
    {
        return getId => async name => await isExist(name) ? await getId(name) : (int?)null;
    }
    
    /// 把当前session_id的所有会话拿出来
    public async Task<List<Messages>> GetChatHistoryAsync(int currentSessionId)
    {
        var fi = await _db.Messages.Where(x => x.SessionId == currentSessionId).ToListAsync();
        return fi;
    }
    
    /// 库里存的东西 → 模型能读的人话
    /// 文件消息(Type=file)的 Content 是一段 JSON, 直接塞给模型全是引号括号噪音,
    /// 这里把它拆成人话: 只有前端截断好的 preview 会进上下文, 文件本体永远不进
    private static string ToAiReadableContent(Messages message)
    {
        var content = message.Content ?? string.Empty;
        if (message.Type != "file") return content;

        try
        {
            var file = System.Text.Json.JsonSerializer.Deserialize<FileMessageDto>(content, FileJsonOptions);
            // 解不出文件名(老数据/被手改成普通文本)就当普通文本处理
            if (file is null || string.IsNullOrWhiteSpace(file.name)) return content;

            var head = $"[用户发来文件] {file.name}（{file.size} 字节）";
            if (!string.IsNullOrWhiteSpace(file.url)) head += $"，下载地址：{file.url}";
            return string.IsNullOrWhiteSpace(file.preview) ? head : $"{head}\n文件内容如下：\n{file.preview}";
        }
        catch (System.Text.Json.JsonException)
        {
            // 不是 JSON, 原样返回, 至少别把整条链路 500 掉
            return content;
        }
    }

    // 前端发来的是 camelCase(name/size/url/preview), C# record 的属性是 PascalCase, 得忽略大小写
    private static readonly System.Text.Json.JsonSerializerOptions FileJsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    /// 转换成pyai要的格式
    public AiPayload MapRoleContent(int sessionId, int userId, string userName, List<Messages> messages, string apiKey)
    {
        var result = messages.Select(x => new RoleContent
        (
            x.UserId == userId ? "user" : "assistant",
            ToAiReadableContent(x)
        )).ToList();
        var end = new Dictionary<string, List<RoleContent>>
        {
            [userName] = result
        };
        return new AiPayload(apiKey, sessionId, end);
    }
}
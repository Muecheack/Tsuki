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

    /// 按名字找 AI 的 Id, 有头像(=建的时候带人设)的优先
    /// 老数据里同一个 AI 名字底下堆过好几条空壳用户, 直接 FirstOrDefault 可能抓到没头像的那条
    /// excludeUserId: 排除真人自己(历史数据里有 AI 被存成用户名的情况, 别把真人的 id 当成 AI)
    public async Task<int?> GetAiIdByNameAsync(string aiName, int excludeUserId)
    {
        return await _db.Users
            .Where(x => x.Name == aiName && x.Id != excludeUserId)
            .OrderByDescending(x => x.AvatarUrlId ?? 0) // 头像 id 都从 1 开始, null 排最后
            .ThenBy(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
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

    /// 找这个会话里"那位 AI"完整信息: 优先挑有头像(=建的时候带人设)的, 再按 id 稳定排序
    /// 之所以不用 pyai 回来的 aiName 定身份: 它回的是会话字典的 key(用户名), 拿它建用户会造同名空壳
    private async Task<User?> GetAiOfSessionAsync(int sessionId, int userId)
    {
        var aiIds = await _db.SessionParticipants
            .Where(x => x.SessionId == sessionId && x.UserId != userId)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();
        if (aiIds.Count == 0) return null;

        var ais = await _db.Users
            .Include(x => x.AvatarUrl)
            .Where(x => aiIds.Contains(x.Id))
            .ToListAsync();

        return ais.FirstOrDefault(x => x.AvatarUrlId is not null) ?? ais.OrderBy(x => x.Id).FirstOrDefault();
    }

    /// 这个会话里那位 AI 的 id(没有 AI 参与者就返回 null)
    public async Task<int?> GetAiIdInSessionAsync(int sessionId, int userId)
    {
        var ai = await GetAiOfSessionAsync(sessionId, userId);
        return ai?.Id;
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
    
    // 跟据sessionId把当前AI的name, tags, avatar描述拿出来
    private async Task<AiInfo> GetInfoOfCurrAi(int sessionId, int userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        var ai = await GetAiOfSessionAsync(sessionId, userId);

        if (ai is null)
        {
            // 会话里压根没有 AI: 给个空人设, 因为 pyai 端是直接 persona.get(...), 给 null 会在那边崩
            return new AiInfo(
                string.Empty, string.Empty,
                user?.CallName ?? string.Empty, user?.Gender ?? string.Empty,
                new List<string>());
        }

        var tags = await _db.TagsUser
            .Where(x => x.UserId == ai.Id)
            .Select(x => x.Tags.TagName)
            .ToListAsync();

        return new AiInfo(
            ai.Name ?? string.Empty,
            ai.AvatarUrl?.Description ?? string.Empty, // 以前这里直接 .Description, AI 没头像就空引用
            user?.CallName ?? string.Empty,
            user?.Gender ?? string.Empty,
            tags);
    }

    /// 转换成pyai要的格式
    public async Task<AiPayload> MapRoleContent(int sessionId, int userId, string userName, List<Messages> messages, string apiKey)
    {
        var result = messages.Select(x => new RoleContent
        (
            x.UserId == userId ? "user" : "assistant",
            x.Content
        )).ToList();
        var end = new Dictionary<string, List<RoleContent>>
        {
            [userName] = result
        };
        var aiInfo = await GetInfoOfCurrAi(sessionId, userId);
        return new AiPayload(apiKey, sessionId, aiInfo, end);
    }
}
using cs.Repo;
using Microsoft.EntityFrameworkCore;

namespace cs.Shell;

public class DbIO
{
    private readonly KeKeContext _db;

    public DbIO(KeKeContext db)
    {
        _db = db;
    }

    // 存AI
    public async Task<int> AddAIAsync(string name)
    {
        var user = new User { Name = name };
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    // 返回用户信息的元组 
    public async Task<(IOState state, int? userId, string? gender, string? callName, string? apiKey)> GetUserInfoAsync(string userName)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == userName);
        if (user is null) return (IOState.NotFound, null, null, null, null);
        return (IOState.Found, user.Id, user.Gender, user.CallName, user.ApiKey);
    }

    // 创建会话
    public async Task<int> CreateSessionAsync(int memberId)
    {
        var session = new Session();
        await _db.Sessions.AddAsync(session);
        await _db.SaveChangesAsync();
        await _db.SessionParticipants.AddRangeAsync(new SessionParticipant
        { SessionId = session.Id, UserId = memberId, JoinedAt = DateTime.Now });
        await _db.SaveChangesAsync();
        return session.Id;
    }

    // 加入会话
    public async Task JoinSessionAsync(int sessionId, int memberId)
    {
        await _db.SessionParticipants.AddAsync(new SessionParticipant
        { SessionId = sessionId, UserId = memberId, JoinedAt = DateTime.Now });
        await _db.SaveChangesAsync();
    }

    // 把Tags表里的tag全拿出来
    public async Task<List<string>> GetTagsAsync()
    {
        return await _db.Tags.Select(x => x.TagName).ToListAsync();
    }

    // 把AvatarUrl表里的Description和Id全拿出来成列表
    public async Task<List<IdDescription>> GetAvatar()
    {
        return await _db.AvatarUrls.Select(x => new IdDescription
        (
            x.Id,
            x.Description
        )).ToListAsync();
    }

    // 根据id拿url
    public async Task<string> GetAvatarByIdAsync(int id)
    {
        var target = await _db.AvatarUrls.FirstOrDefaultAsync(x => x.Id == id);
        return target.Url;
    }

    // 判断Ai是否已存在
    public Task<bool> IsAiExite(string aiName)
    {
        return _db.Users.AnyAsync(x => x.Name == aiName);
    }

    // 存Tag
    public async Task<IOState> SaveTagAsync(List<string> tags, int aiId)
    {
        if (!await _db.Users.AnyAsync(x => x.Id == aiId)) return IOState.NotFound;

        var tagId = await _db.Tags
            .Where(x => tags.Contains(x.TagName))
            .Select(x => x.Id)
            .ToListAsync();
        if (tagId.Count == 0) return IOState.Found;

        var tagRepetition = await _db.TagsUser
            .Where(x => x.UserId == aiId && tagId.Contains(x.TagsId))
            .Select(x => x.TagsId)
            .ToListAsync();

        // 只补写还没存过的; 全都存过了就直接返回(以前这里写反了: 一条重复都没有时反而 return, 导致 tag 从来没写进去)
        var add = tagId.Except(tagRepetition)
            .Select(x => new TagsUser { UserId = aiId, TagsId = x })
            .ToList();
        if (add.Count == 0) return IOState.Found;

        await _db.TagsUser.AddRangeAsync(add);
        await _db.SaveChangesAsync();
        return IOState.Found;
    }

    // 存avatar-url
    // 坑: 前端传的是拼了 baseUrl 的绝对地址(http://localhost:5108/Resource/Avatar/x.jpg),
    // 库里存的是相对路径(Resource/Avatar/x.jpg), 直接全等比较永远匹配不上 —— 所以这里先归一化再比
    public async Task<IOState> SaveAvatarAsync(string avatarUrl, int aiId)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl)) return IOState.NotFound;

        var ai = await _db.Users.FirstOrDefaultAsync(x => x.Id == aiId);
        if (ai is null) return IOState.NotFound;

        var rel = NormalizeStaticPath(avatarUrl);
        var fileName = Path.GetFileNameWithoutExtension(rel);

        var avatar = await _db.AvatarUrls.FirstOrDefaultAsync(x => x.Url == rel)
                     ?? await _db.AvatarUrls.FirstOrDefaultAsync(x => x.Url == "/" + rel)
                     ?? await _db.AvatarUrls.FirstOrDefaultAsync(x => x.Name == fileName);
        if (avatar is null) return IOState.NotFound;

        ai.AvatarUrlId = avatar.Id; // 覆盖, 管他原本有没有值
        await _db.SaveChangesAsync();
        return IOState.Found;
    }

    /// 把 http://host/Resource/Avatar/x.jpg / Resource/Avatar/x.jpg / /Resource/... 统一成不带前导斜杠的相对路径
    private static string NormalizeStaticPath(string url)
    {
        var path = url.Trim();
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri)) path = uri.AbsolutePath;
        return path.Replace('\\', '/').TrimStart('/');
    }

    // 名字 -> Id
    public async Task<(IOState, int?)> GetUserIdAsync(string memberName)
    {
        var member = await _db.Users
            .Where(x => x.Name == memberName)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
        return member is null ? (IOState.NotFound, null) : (IOState.Found, member);
    }

    // 保存消息
    public async Task SaveChatInMessageAsync(int sessionId, int memberId, string memberContent, string type = "text")
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
}
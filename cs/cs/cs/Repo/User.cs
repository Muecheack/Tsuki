using System;
using System.Collections.Generic;

namespace cs.Repo;

// 用户信息(注册)
public partial class User
{
    public int Id { get; set; }
    
    /// AI和真人公用
    public string? Name { get; set; }
    
    /// 真人专属AI为null
    public string? Gender { get; set; }
    
    /// 真人专属AI为null
    public string? CallName { get; set; }
    
    /// 真人专属AI为null
    public string? ApiKey { get; set; }
    
    /// AI专属真人为null
    public virtual List<TagsUser>? TagsAI { get; set; } = new();
    
    public virtual List<SessionParticipant> SessionParticipants { get; set; } = new();
    
    /// 一个用户一个头像
    public int? AvatarUrlId { get; set; }
    public virtual AvatarUrl AvatarUrl { get; set; } = null!;
}
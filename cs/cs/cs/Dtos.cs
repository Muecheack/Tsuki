namespace cs;

// 注册页那堆属性
public record UserDto(
    string name,
    string gender,
    string call,
    string apiKey
);

// 映射给AI用的
public record RoleContent(string role, string content);

public record UserChatDto(string userName, int? sessionId, string message, string type);

// AI返回的格式
public record AIChatDto(string aiName, int? sessionId, string message, string type);

// 转发给 AI 端的载荷：带上 sessionId，AI 端回传时才知道往哪个会话存
public record AiPayload(string apiKey, int sessionId, Dictionary<string, List<RoleContent>> messages);

public record PrePersonDto(string userName, string aiName);

// userName: {...}
public record UserNamePayload(
    string aiName,
    List<RoleContent> session,
    List<string> tags,
    List<IdDescription> avatars,
    string gender,
    string callName);

public record PrePersonPayload(string apiKey, Dictionary<string, UserNamePayload> sessionDict);

public record IdDescription(int id, string description);

// pyai返回的格式(预人设)
public record PrePersonReplyDto(string reply, SelectedDto selected);

public record SelectedDto(List<string> tags, int avatarId);

// 点击好友申请的dto
public record ApplyFriendDto(List<string> tags, string avatarUrl, string preReply, string userName, string aiName);

// Messages.Type == "file" 时, Messages.Content 里存的 JSON(前端上传成功后发过来)
// preview 是前端已经截断好的文本预览 —— 只有它会被拼进 AI 上下文, 文件本体永远不进
public record FileMessageDto(string? name, long size, string? url, string? preview);
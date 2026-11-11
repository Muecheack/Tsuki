namespace cs.Core;

public static class SwitchFormat
{
    // 转成 pyai 要的格式，大概是 { apiKey, userNameDict: { 用户名: 人设... } }
    // 这里先只塞当前用户。
    public static PrePersonPayload FormatDict(string userName, string aiName, string gender,
        string callName, List<string> tags, List<IdDescription> avatars, string apiKey)
    {
        var userNamePayload = new UserNamePayload(aiName, new List<RoleContent>(), tags, avatars, gender, callName);
        var userNameDict = new Dictionary<string, UserNamePayload>
        {
            [userName] = userNamePayload
        };
        return new PrePersonPayload(apiKey, userNameDict);
    }
}

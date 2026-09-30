namespace cs.Core;

public static class SwitchFormat
{
    // 转换成给pyai要的格式
    // 大概长这样:
    // {
    //  "sessionId": 123,
    //  "userNameDict": {
    //      "张三": {
    //           "aiName": "...",
    //            "session": [],
    //            "tags": [...],
    //            "avatars": [{...}],
    //            "gender": "...",
    //          "callName": "..."
    //       }
    //    }
    // }
    public static PrePersonPayload FormatDict(string userName, string aiName, string gender,
        string callName, List<string> tags, List<IdDescription> avatars)
    {
        var userNamePayload = new UserNamePayload(aiName, new List<RoleContent>(), tags, avatars, gender, callName);
        var userNameDict = new Dictionary<string, UserNamePayload>
        {
            [userName] = userNamePayload
        };
        return new PrePersonPayload(userNameDict);
    }
}
using System;
using Microsoft.AspNetCore.SignalR;

namespace cs;

public class ChatHub : Hub
{
    // codex: 超时消息按数据库用户 Id 分组推送。
    public static string UserGroup(int userId) => $"user:{userId}";

    // codex: 前端连接成功后调用一次，重连后需要再次调用。
    public Task RegisterUser(int userId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
    }

    public async Task SendMessage(string sender, string content)
    {
        await Clients.All.SendAsync("receivedMessage", sender, content);
    }
}

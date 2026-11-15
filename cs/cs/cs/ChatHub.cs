using System;
using Microsoft.AspNetCore.SignalR;

namespace cs;

public class ChatHub : Hub
{
    public async Task SendMessage(string sender, string content)
    {
        await Clients.All.SendAsync("receivedMessage", sender, content);
    }
}

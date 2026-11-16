using System;
using System.Diagnostics;
using cs.DbAction;
using cs.Repo;
using cs.Services;
using cs.Shell;
using Microsoft.AspNetCore.SignalR;

namespace cs.Mods.Notifation;

public class Shell_CallAi
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly Shell_TimerEvent timerEvent;
    public Shell_CallAi(IServiceScopeFactory serviceScope, Shell_TimerEvent shell_Timer)
    {
        serviceScopeFactory = serviceScope;
        timerEvent = shell_Timer;
        timerEvent.OverTime += OnOverTime;

        // TODO
    }

    public async Task<AIChatDto> CallAi(
        int min,
        int sessionId,
        int memberId,
        string userName)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var _dbIO = scope.ServiceProvider.GetRequiredService<DbIO>();
        var dbAction = scope.ServiceProvider.GetRequiredService<DbActions>();
        var _configuation = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var _sendToAi = scope.ServiceProvider.GetRequiredService<SendToAi>();

        var PRE_PROMPT = $"(过了{min}分钟也没消息)";
        await _dbIO.SaveChatInMessageAsync(sessionId, memberId, memberContent: PRE_PROMPT);
        var apiKey = await dbAction.GetApiKeyAsync(userName);
        var history = await dbAction.GetChatHistoryAsync(sessionId);
        var payload = await dbAction.MapRoleContent(sessionId, userId: memberId, userName, messages: history, apiKey);
        var aiUrl = (_configuation["Address:Pyai"] ?? "http://128.0.0.1:10086").TrimEnd('/') + "/api/ai";
        return await _sendToAi.SendAiAsync(aiUrl, payload);
    }

    private async Task<int?> ResolveAiIdAsync(
            string aiNameFromAi,
            int sessionId,
            int userId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbAction = scope.ServiceProvider.GetRequiredService<DbActions>();
        // ① 会话里本来就有 AI(ApplyFriend 建的那条, 带 tags + 头像), AI 的话就记在它头上
        if (await dbAction.GetAiIdInSessionAsync(sessionId, userId) is { } inSession)
            return inSession;

        // ② 名字能对上就复用, 但绝不能是真人自己(历史数据里存在 AI 用了用户名的脏行)
        if (await dbAction.GetAiIdByNameAsync(aiNameFromAi, userId) is { } byName)
        {
            await dbAction.AddUserInSessionParticipantAsync(byName, sessionId);
            return byName;
        }
        return null;
    }

    private async Task OnOverTime(Shell_TimerEvent.Infos infos, CancellationToken token)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var _dbIO = scope.ServiceProvider.GetRequiredService<DbIO>();
        var _hubContext = scope.ServiceProvider.GetRequiredService<IHubContext>();

        var timeSpan = infos.timeSpan;
        var userId = infos.userId;
        var userName = infos.userName;
        var sessionId = infos.sessionId;

        var aiReply = await CallAi(
            timeSpan,
            sessionId,
            memberId: userId,
            userName);
        var aiName = aiReply.aiName;
        var wantReply = aiReply.wantReply switch
        {
            "want" => true,
            _ => false
        };
        if (wantReply == false) return;
        var aiId = await ResolveAiIdAsync(aiName, sessionId, userId);
        if (aiId is { } aId)
        {
            await _dbIO.SaveChatInMessageAsync(sessionId, memberId: aId, memberContent: aiReply.message);
        }
        else return; // Ai不存在就不再执行
        await _hubContext.Clients.User($"{sessionId}").SendAsync("receivedMessage", aiName, aiReply.message);
    }
}

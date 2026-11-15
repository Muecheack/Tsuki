using System;
using System.Diagnostics;
using cs.DbAction;
using cs.Services;
using cs.Shell;
using Microsoft.AspNetCore.SignalR;

namespace cs.Mods.Notifation;

public class Shell_CallAi
{
    private readonly SendToAi _sendToAi;
    private readonly Shell_TimerEvent _timerEvent;
    private readonly IConfiguration _configuration;
    private readonly DbIO _dbIO;
    private readonly DbActions dbAction;
    private readonly IHubContext _hubContext;
    public Shell_CallAi(SendToAi sendToAi, Shell_TimerEvent timerEvent, IConfiguration configuration, DbIO dbIO, IHubContext hubContext, DbActions _dbAction)
    {
        _sendToAi = sendToAi;
        _timerEvent = timerEvent;
        _configuration = configuration;
        _dbIO = dbIO;
        _hubContext = hubContext;
        dbAction = _dbAction;
        _timerEvent.OverTime += (timeSpan) => _ = OnOverTime(timeSpan);
        // TODO
    }
    public Func<(int sessionId, int memberId, string userName, string apiKey)>? GetInfoFactory { get; set; }

    public async Task<(AIChatDto, int sessionId, int userId)> CallAi(string min)
    {
        var PRE_PROMPT = $"(过了{min}分钟也没消息)";
        var (sessionId, memberId, userName, apiKey) = GetInfoFactory?.Invoke() ?? (-11, -11, "", ""); // 乱填的魔法数字, 反正无效
        System.Console.WriteLine($"GetInfoFactory拿到的:sessionId:{sessionId}, memberId:{memberId}");
        await _dbIO.SaveChatInMessageAsync(sessionId, memberId, memberContent: PRE_PROMPT);
        var history = await dbAction.GetChatHistoryAsync(sessionId);
        var payload = await dbAction.MapRoleContent(sessionId, userId: memberId, userName, messages: history, apiKey);
        var aiUrl = (_configuration["Address:Pyai"] ?? "http://128.0.0.1:10086").TrimEnd('/') + "/api/ai";
        return (await _sendToAi.SendAiAsync(aiUrl, payload), sessionId, userId: memberId);
    }

    private async Task<int?> ResolveAiIdAsync(
            string aiNameFromAi,
            int sessionId,
            int userId)
    {
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

    private async Task OnOverTime(string timeSpan)
    {
        var (aiReply, sessionId, userId) = await CallAi(timeSpan);
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

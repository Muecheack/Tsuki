using System;
using cs.Repo;
using Microsoft.EntityFrameworkCore;

namespace cs.Mods.Notifation;

public class Shell_TimerEvent(IServiceScopeFactory _scopeFacotry)
{
    private CancellationTokenSource _cts;
    public void PutTimerBack(int sessionId)
    {
        _cts = new CancellationTokenSource();
        _ = ComparedTime(sessionId, _cts.Token);
    }
    public event Action<string>? OverTime;

    // 轮询数据库找新消息的时间, 大于5分钟的通知
    public async Task ComparedTime(int sessionId, CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        // WARNING: 当前计时器会用不停歇的运转, 当程序掉线/用户没了会被catch后正常退出
        try
        {
            using (var scope = _scopeFacotry.CreateScope())
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    try
                    {
                        var db = scope.ServiceProvider.GetRequiredService<KeKeContext>();
                        var lastMessage = await db.Messages
                                        .Where(x => x.SessionId == sessionId)
                                        .OrderByDescending(x => x.SendAt)
                                        .FirstOrDefaultAsync(token);

                        if (lastMessage is null)
                        {
                            System.Console.WriteLine($"sessionId {sessionId} 不存在");
                            continue;
                        }

                        var timeSpan = DateTime.Now - lastMessage.SendAt;

                        if (timeSpan.TotalMinutes > 5)
                        {
                            OverTime?.Invoke(timeSpan.ToString(@"dd\-hh\:mm\:ss"));
                        }

                    }
                    catch (Exception err)
                    {
                        System.Console.WriteLine($"定时器错误:{err.Message}");
                        return;
                    }

                }
            }

        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine($"会话{sessionId}离开, 轮询停止");
        }
    }
}

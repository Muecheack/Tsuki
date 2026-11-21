using System;
using System.Collections.Concurrent;
using cs.Repo;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace cs.Mods.Notifation;

public class Shell_TimerEvent(IServiceScopeFactory _scopeFacotry)
{
    private ConcurrentDictionary<int, CancellationTokenSource> _sessionTimers = new();
    public sealed record Infos(
        int sessionId,
        string userName,
        int userId,
        int timeSpan
    );
    public event Func<Infos, CancellationToken, Task>? OverTime;

    // 启动计时器并扔后台运行, ChatPoint方调用
    public void PutTimerBack(int sessionId, int userId, string userName)
    {
        // 之前有Timer就停了再来
        if (_sessionTimers.TryRemove(sessionId, out var oldCts))
        {
            oldCts.Cancel();
            oldCts.Dispose();
        }
        var _cts = new CancellationTokenSource();
        _sessionTimers[sessionId] = _cts;
        _ = ComparedTime(sessionId, userId, userName, _cts.Token);
    }

    // 轮询数据库找新消息的时间, 大于5分钟的通知
    public async Task ComparedTime(int sessionId, int userId, string userName, CancellationToken token)
    {

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        // WARNING: 当前计时器会用不停歇的运转, 当程序掉线/用户没了会被catch后正常退出
        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                using var scope = _scopeFacotry.CreateScope();
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<KeKeContext>();
                    var lastMessage = await db.Messages
                                    .Where(x => x.SessionId == sessionId && x.UserId == userId)
                                    .OrderByDescending(x => x.SendAt)
                                    .FirstOrDefaultAsync(token);

                    if (lastMessage is null)
                    {
                        System.Console.WriteLine($"sessionId {sessionId} 不存在");
                        continue;
                    }

                    var timeSpan = DateTime.Now - lastMessage.SendAt;

                    if (timeSpan.TotalMinutes > 1)
                    {
                        // 发出事件, 为空就直接退出
                        if (OverTime is null) return;
                        var infos = new Infos(sessionId, userName, userId, (int)timeSpan.TotalMinutes);
                        foreach (var f in OverTime.GetInvocationList().Cast<Func<Infos, CancellationToken, Task>>())
                        {
                            await f(infos, token);
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Console.WriteLine($"定时器错误:{err.Message}");
                }

            }

        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine($"会话{sessionId}离开, 轮询停止");
        }
        catch (Exception er)
        {
            System.Console.WriteLine($"出错{er.Message}");
        }
    }
}

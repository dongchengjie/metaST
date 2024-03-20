using System.Diagnostics;
using System.Net;
using Core.Test.Reuslt;
using Util;

namespace Core.Test.Profiler;

public class SpeedProfiler(string url = "https://speed.cloudflare.com/__down?bytes=200000000", int timeout = 3000, int duration = 10000, int rounds = 1) : Profiler<SpeedResult>
{
    private readonly string Url = url;
    private readonly int Timeout = timeout;
    private readonly int Duration = duration;
    private readonly int Rounds = rounds;
    public override Task<SpeedResult> TestAsync(IWebProxy? proxy)
    {
        return Task.Run(() =>
        {
            SpeedResult result = new();
            for (int i = 0; i < Rounds; i++)
            {
                HttpRequest.UsingHttpClient((client) =>
                {
                    try
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        // 打开输入流
                        Task<Stream> task = client.GetStreamAsync(Url);
                        if (Task.WaitAny(task, Task.Delay(Timeout)) != 0)
                        {
                            throw new TimeoutException("SpeedTest open stream timeout");
                        }
                        // 开始下载
                        byte[] buffer = new byte[1000000];
                        long len = -1;
                        long downloaded = 0;
                        using (Stream stream = task.Result)
                        {
                            while (len != 0)
                            {
                                Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                                if (Task.WaitAny(task, Task.Delay(Duration)) != 0)
                                {
                                    throw new TimeoutException("SpeedTest read timeout");
                                }
                                len = readTask.Result;
                                downloaded += len;
                                if (stopwatch.ElapsedMilliseconds > Duration)
                                {
                                    break;
                                }
                            }
                        }
                        stopwatch.Stop();
                        result.bitRates.Add(downloaded * 8 * 1000.0 / stopwatch.ElapsedMilliseconds);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"Erroring Testing speed: {ex.Message}");
                        return false;
                    }
                }, Timeout, proxy);
            }
            return result;
        });
    }
}
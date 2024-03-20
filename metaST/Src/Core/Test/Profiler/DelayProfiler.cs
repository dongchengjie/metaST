using System.Diagnostics;
using System.Net;
using Core.Test.Reuslt;
using Util;

namespace Core.Test.Profiler;

public class DelayProfiler(string url = "https://www.google.com/gen_204", int timeout = 3000, int rounds = 5) : Profiler<DelayResult>
{
    private readonly string Url = url;
    private readonly int Timeout = timeout;
    private readonly int Rounds = rounds;

    public override Task<DelayResult> TestAsync(IWebProxy? proxy)
    {
        return Task.Run(() =>
        {
            DelayResult result = new();
            for (int i = 0; i < Rounds; i++)
            {
                HttpRequest.UsingHttpClient((client) =>
                {
                    try
                    {
                        HttpRequestMessage request = new(HttpMethod.Head, Url);
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        Task<HttpResponseMessage> task = client.SendAsync(request);
                        using HttpResponseMessage response = task.Result;
                        response.EnsureSuccessStatusCode();
                        stopwatch.Stop();
                        result.delays.Add(stopwatch.ElapsedMilliseconds);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"Erroring Testing delay: {ex.Message}");
                        return false;
                    }
                }, Timeout, proxy);
            }
            return result;
        });
    }
}
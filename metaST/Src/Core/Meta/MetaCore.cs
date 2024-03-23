using System.Diagnostics;
using System.Runtime.InteropServices;
using Util;

namespace Core.Meta;

public class MetaCore
{
    // Meta内核解压路径
    private static readonly string metaCorePath = Path.Combine(Constants.WorkSpace, Constants.ExecutableName);
    static MetaCore()
    {
        // 解压内核
        string resourceName = "meta." + Platform.GetPlatform() + "." + Constants.ExecutableName;
        Resources.Extract(resourceName, metaCorePath, true);
        // 文件赋权
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Processes.Start("icacls", metaCorePath + " /grant Everyone:(RX)", (sender, e) => { });
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            File.SetUnixFileMode(metaCorePath, UnixFileMode.OtherExecute | UnixFileMode.GroupExecute | UnixFileMode.UserExecute);
        }
    }

    public static Task<Process> StartProxy(string configPath)
    {
        TaskCompletionSource<bool> tcs = new();
        Task<Process> task = Processes.Start(metaCorePath, "-f " + configPath, (sender, e) =>
        {
            Logger.Trace(e.Data ?? string.Empty);
            if (!tcs.Task.IsCompleted && !string.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("Start initial Compatible provider default"))
                {
                    tcs.TrySetResult(true);
                }
                if (e.Data.Contains("level=fatal"))
                {
                    Logger.Fatal(e.Data);
                    tcs.TrySetResult(false);
                }
                if (e.Data.Contains("level=error"))
                {
                    Logger.Error(e.Data);
                }
            }
        });

        // 注册退出事件
        ExitRegistrar.RegisterAction((type) => task.Result.Kill());

        // 等待代理成功启动，超时60秒
        Logger.Debug("等待代理成功启动...");
        Task finished = Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(60))).Result;
        if (finished == tcs.Task)
        {
            if (!((Task<bool>)finished).Result)
            {
                task.Result.Kill();
                throw new Exception("代理启动失败");
            }
        }
        else
        {
            throw new Exception("代理启动超时");
        }
        Logger.Debug("代理启动成功");
        return task;
    }
}
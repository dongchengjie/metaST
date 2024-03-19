using System.Reflection;
using Core.CommandLine;
using Core.Meta;
using Util;

// 程序信息
[assembly: AssemblyTitle("metaST")]
[assembly: AssemblyDescription("A Clash.Meta-based CLI program for proxy testing.")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace Core.MetaSpeedTest;
public class MetaSpeedTest
{
    public static void Main(CommandLineOptions options)
    {
        // 初始化
        Init(options);
        MetaProxy.Proxy();
    }

    private static void Init(CommandLineOptions options)
    {
        // 日志配置
        Logger.LogLevel = options.Verbose ? LogLevel.trace : LogLevel.info;
        Logger.RefreshInterval = 500;
        Logger.LogPath = Constants.WorkSpace;
        ExitRegistrar.RegisterAction(type => Logger.Terminate());
    }
}
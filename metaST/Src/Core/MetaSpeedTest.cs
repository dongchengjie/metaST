using System.Reflection;
using System.Text;
using Core.CommandLine;
using Core.Meta;
using Core.Meta.Config;
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
        try
        {
            // 初始化
            Init(options);
            // 获取节点列表
            List<ProxyNode> proxies = MetaConfig.GetConfigProxies(options.Config);
            // 节点去重
            proxies = ProxyNode.Distinct(proxies, options.DistinctStrategy);
            // 延迟测试、筛选
            // 下载速度测试、筛选
            // 节点GEO重命名
            proxies = !string.IsNullOrWhiteSpace(options.Tag) || options.GeoLookup ? ProxyNode.Rename(proxies, options) : proxies;
            // 生成配置文件
            string configYaml = MetaConfig.GenerateRegionConfig(proxies, options);
            // 输出到文件
            WriteToFile(configYaml, options);
        }
        finally
        {
            // 程序结束时暂停
            if (options.Pause)
            {
                Console.WriteLine("按任意键继续...");
                Console.ReadKey(true);
            }
        }
    }

    private static void Init(CommandLineOptions options)
    {
        // 清理残余进程
        Processes.FindAndKill(MetaCore.executableName);
        // 日志配置
        Logger.LogLevel = options.Verbose ? LogLevel.trace : LogLevel.info;
        Logger.RefreshInterval = 500;
        Logger.LogPath = Constants.WorkSpace;
        ExitRegistrar.RegisterAction(type => Logger.Terminate());
    }

    private static void WriteToFile(string configContent, CommandLineOptions options)
    {
        string outputPath = options.Output ?? string.Empty;
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            string configFile = options.Config.StartsWith("http") ? Strings.Md5(options.Config) : options.Config;
            configFile = Path.GetFileNameWithoutExtension(configFile);
            outputPath = Path.Combine(Constants.AppPath, configFile + "_result.yaml");
        }
        Files.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(configContent)), outputPath);
        Logger.Info($"输出配置文件: {outputPath}");
    }
}
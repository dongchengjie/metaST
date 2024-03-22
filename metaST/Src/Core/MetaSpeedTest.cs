using System.Reflection;
using System.Text;
using Core.CommandLine;
using Core.Meta;
using Core.Meta.Config;
using Core.Test.Profiler;
using Core.Test.Reuslt;
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
            Initialize(options);
            // 获取节点列表
            List<ProxyNode> proxies = MetaConfig.GetConfigProxies(options.Config);
            // 节点去重
            EnsurePorxiesLeft(proxies);
            proxies = ProxyNode.Distinct(proxies, options.DistinctStrategy);
            // 节点净化(分组 + 二分测试校验)
            EnsurePorxiesLeft(proxies);
            proxies = ProxyNode.Purify(proxies);
            // 延迟测试、筛选
            proxies = DelayTestFilter(proxies, options);
            // 下载速度测试、筛选
            proxies = SpeedTestFilter(proxies, options);
            // 节点GEO重命名
            EnsurePorxiesLeft(proxies);
            proxies = !string.IsNullOrWhiteSpace(options.Tag) || options.GeoLookup ? ProxyNode.Rename(proxies, options) : proxies;
            // 生成配置文件并输出
            EnsurePorxiesLeft(proxies);
            string configYaml = MetaConfig.GenerateRegionConfig(proxies, options);
            WriteToFile(configYaml, options);
        }
        catch (Exception ex)
        {
            Logger.Error($"程序异常退出: {ex.Message}");
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

    private static void Initialize(CommandLineOptions options)
    {
        // 清理残余进程
        Processes.FindAndKill(Constants.ExecutableName);
        // 日志配置
        Logger.LogLevel = options.Verbose ? LogLevel.trace : LogLevel.info;
        Logger.RefreshInterval = 500;
        Logger.LogPath = Constants.WorkSpace;
        Logger.PreProcessor = (message, console) => console ? Emoji.EmojiToShort(message) : message;
        ExitRegistrar.RegisterAction(type => Logger.Terminate());
    }

    private static void EnsurePorxiesLeft(List<ProxyNode> proxies)
    {
        if (proxies == null || proxies.Count == 0)
        {
            throw new InvalidDataException("无满足条件的代理");
        }
    }

    private static List<ProxyNode> DelayTestFilter(List<ProxyNode> proxies, CommandLineOptions options)
    {
        if (options.DelayTestEnable)
        {
            Logger.Info("开始延迟测试...");
            List<ProxyNode> exclueded = [];
            int chunkIndex = 0;
            foreach (ProxyNode[] chunk in proxies.Chunk(Constants.MaxPortsOccupied))
            {
                // 限制延迟测试并行度，提高准确率
                int batchIndex = 0;
                int concurrency = Math.Min(options.DelayTestThreads, Constants.MaxDelayTestThreads);
                foreach (ProxyNode[] batch in chunk.Chunk(concurrency))
                {
                    exclueded.AddRange(MetaService.UsingProxies([.. batch], (proxied) =>
                    {
                        Dictionary<ProxyNode, DelayResult> delayTestResult = proxied.AsParallel()
                            .Select((proxy, index) => new { Index = index, Proxy = proxy })
                            .Select(item =>
                            {
                                DelayProfiler delayProfiler = new(options.DelayTestUrl, options.DelayTestTimeout, options.DelayTestRounds);
                                DelayResult result = delayProfiler.TestAsync(item.Proxy.Mixed).Result;
                                // 输出延迟测试结果
                                int current = chunkIndex * Constants.MaxPortsOccupied + batchIndex * proxied.Count + item.Index + 1;
                                result.Print(Strings.Padding(Emoji.EmojiToShort($"[{current}/{proxies.Count}] {item.Proxy.Name}"), 64));
                                return new { item.Proxy, Result = result };
                            }).ToDictionary(item => item.Proxy, item => item.Result);
                        // 筛选出延迟不满足过滤条件的节点
                        return proxied.Where(proxy =>
                        {
                            if (!delayTestResult.TryGetValue(proxy, out var result) || result == null) return true;
                            return result.Result() > options.DelayTestFilter;
                        }).ToList();
                    }));
                    batchIndex += 1;
                }
                chunkIndex += 1;
            }
            proxies = proxies.Except(exclueded).ToList();
            Logger.Info($"延迟测试完成,排除掉{exclueded.Count}个节点,剩余{proxies.Count}个节点");
        }
        return proxies;
    }

    private static List<ProxyNode> SpeedTestFilter(List<ProxyNode> proxies, CommandLineOptions options)
    {
        if (options.SpeedTestEnable)
        {
            Logger.Info("开始下载速度测试...");
            List<ProxyNode> exclueded = [];
            int chunkIndex = 0;
            foreach (ProxyNode[] chunk in proxies.Chunk(Constants.MaxPortsOccupied))
            {
                Dictionary<ProxyNode, SpeedResult> speedTestResult = [];
                exclueded.AddRange(MetaService.UsingProxies([.. chunk], (proxied) =>
                {
                    Dictionary<ProxyNode, SpeedResult> speedTestResult = proxied
                        .Select((proxy, index) => new { Index = index, Proxy = proxy })
                        .Select(item =>
                        {
                            SpeedProfiler speedProfiler = new(options.SpeedTestUrl, options.SpeedTestTimeout, options.SpeedTestDuration, options.SpeedTestRounds);
                            SpeedResult result = speedProfiler.TestAsync(item.Proxy.Mixed).Result;
                            // 输出下载速度测试结果
                            int current = chunkIndex * Constants.MaxPortsOccupied + item.Index + 1;
                            result.Print(Strings.Padding(Emoji.EmojiToShort($"[{current}/{proxies.Count}] {item.Proxy.Name}"), 64));
                            return new { item.Proxy, Result = result };
                        }).ToDictionary(item => item.Proxy, item => item.Result);
                    return proxied.Where(proxy =>
                    {
                        // 筛选出下载速度不满足过滤条件的节点
                        if (!speedTestResult.TryGetValue(proxy, out var result) || result == null) return true;
                        return result.Result() < options.DelayTestFilter;
                    }).ToList();
                }));
                chunkIndex += 1;
            }
            proxies = proxies.Except(exclueded).ToList();
            Logger.Info($"下载速度测试完成,排除掉{exclueded.Count}个节点,剩余{proxies.Count}个节点");
        }
        return proxies;
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
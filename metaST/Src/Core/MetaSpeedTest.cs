using System.Net;
using System.Reflection;
using System.Text;
using Core.CommandLine;
using Core.CommandLine.Enum;
using Core.Geo;
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

            // 节点重命名
            Rename(proxies, options);
            // 生成配置文件
            string s = MetaConfig.GenerateRegionConfig(proxies, options);
            Files.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(s)), "D:/桌面/aa.yaml");
        }
        finally
        {
            // 程序结束时暂停
            if (options.Pause)
            {
                Console.WriteLine("按任意键继续");
                Console.ReadKey(true);
            }
        }
    }

    private static void Init(CommandLineOptions options)
    {
        // 日志配置
        Logger.LogLevel = options.Verbose ? LogLevel.trace : LogLevel.info;
        Logger.RefreshInterval = 500;
        Logger.LogPath = Constants.WorkSpace;
        ExitRegistrar.RegisterAction(type => Logger.Terminate());
    }

    private static void Rename(List<ProxyNode> proxies, CommandLineOptions options)
    {
        if (proxies != null && proxies.Count > 0)
        {
            // 根据GEO重命名
            if (options.GeoLookup)
            {
                Logger.Info("开始GEO重命名...");
                // 查询GEO信息
                Dictionary<IWebProxy, GeoInfo> infoMap = MetaService.UsingProxies(proxies, proxied =>
                {
                    return proxied
                        .Select(proxy => Task.Run(() => GeoElector.LookupAsnyc(proxy.Mixed)))
                        .Select(task => task.Result)
                        .DistinctBy((info) => info.Proxy)
                        .ToDictionary(info => info.Proxy ??= new WebProxy(), info => info);
                });
                // 分配GEO信息，并重命名
                proxies.ForEach((proxy) =>
                {
                    proxy.GeoInfo = proxy.Mixed != null && infoMap.TryGetValue(proxy.Mixed, out var geoInfo) ? geoInfo : new GeoInfo();
                    proxy.Name = $"{proxy.GeoInfo.Emoji} {proxy.GeoInfo.Country}";
                });
                // 国家名称_序号
                ProxyNode.Distinct(proxies, options.DistinctStrategy);
                Logger.Info("GEO重命名完成");
            }
            // 添加节点前缀
            if (!string.IsNullOrWhiteSpace(options.Tag))
            {
                Logger.Info($"添加Tag: {options.Tag}");
                proxies.ForEach(proxy => proxy.Name = $"{options.Tag}_{proxy.Name}");
                Logger.Info($"添加Tag完成");
            }
        }
    }
}
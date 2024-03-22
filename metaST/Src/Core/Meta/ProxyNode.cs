using System.Collections.Concurrent;
using System.Net;
using Core.CommandLine;
using Core.CommandLine.Enum;
using Core.Geo;
using Util;

namespace Core.Meta;

public class ProxyNode(Dictionary<dynamic, dynamic> info)
{
    public int Id { get; } = info.GetHashCode();

    public Dictionary<dynamic, dynamic> Info { get; set; } = info;

    public string Name
    {
        get { return Info["name"]; }
        set { Info["name"] = value; }
    }

    public string Type
    {
        get { return Info["type"]; }
        set { Info["type"] = value; }
    }
    public string Server
    {
        get { return Info["server"]; }
        set { Info["server"] = value; }
    }

    public int Port
    {
        get { return int.Parse(Info["port"]); }
        set { Info["port"] = value; }
    }

    public IWebProxy? Mixed { get; set; }

    public GeoInfo GeoInfo { get; set; } = new GeoInfo();

    public static List<ProxyNode> Distinct(List<ProxyNode> proxies, DistinctStrategy strategy)
    {
        // 按去重策略去重
        switch (strategy)
        {
            case DistinctStrategy.type_server_port:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Type, proxy.Server, proxy.Port])).ToList();
                break;
            case DistinctStrategy.type_server:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Type, proxy.Server])).ToList();
                break;
            case DistinctStrategy.server_port:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Server, proxy.Port])).ToList();
                break;
            case DistinctStrategy.server:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Server])).ToList();
                break;
        }
        // 名称重复的添加序号后缀
        proxies = proxies
            .GroupBy(p => p.Name)
            .SelectMany(grp => grp.Select((p, i) =>
            {
                p.Name = grp.Count() > 1 ? $"{p.Name}_{i + 1}" : p.Name;
                return p;
            })).ToList();
        return proxies;
    }

    public static List<ProxyNode> Rename(List<ProxyNode> proxies, CommandLineOptions options)
    {
        if (proxies != null && proxies.Count > 0)
        {
            // 根据GEO重命名
            if (options.GeoLookup)
            {
                Logger.Info("开始GEO重命名...");
                ConcurrentDictionary<IWebProxy, GeoInfo> infoMap = [];
                foreach (ProxyNode[] chunk in proxies.Chunk(Constants.MaxPortsOccupied))
                {
                    // 查询GEO信息
                    MetaService.UsingProxies(proxies, proxied =>
                    {
                        return proxied.AsParallel().Select(proxy =>
                        {
                            GeoInfo geoInfo = Task.Run(() => GeoElector.LookupAsnyc(proxy.Mixed)).Result;
                            infoMap.TryAdd(proxy.Mixed ??= new WebProxy(), geoInfo);
                            return geoInfo;
                        }).ToList();
                    });
                }

                // 分配GEO信息，并重命名
                proxies.ForEach((proxy) =>
                {
                    proxy.GeoInfo = proxy.Mixed != null && infoMap.TryGetValue(proxy.Mixed, out var geoInfo) ? geoInfo : new GeoInfo();
                    proxy.Name = $"{proxy.GeoInfo.Emoji} {proxy.GeoInfo.Country}";
                });
                // 国家名称_序号
                Distinct(proxies, options.DistinctStrategy);
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
        return proxies ?? [];
    }
}
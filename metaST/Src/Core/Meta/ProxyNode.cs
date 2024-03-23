using System.Collections.Concurrent;
using System.Net;
using Core.CommandLine;
using Core.Geo;
using Util;

namespace Core.Meta;

public class ProxyNode(Dictionary<dynamic, dynamic> info)
{
    public override bool Equals(object? obj)
    {
        return obj != null && GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ Name.GetHashCode() ^ Port.GetHashCode();
    }

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

    public static List<ProxyNode> Distinct(List<ProxyNode> proxies)
    {
        // 去重（协议类型 + 服务器 + 端口）
        proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Type, proxy.Server, proxy.Port])).ToList();
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

    public static List<ProxyNode> Rename(List<ProxyNode> proxies)
    {
        CommandLineOptions options = Context.Options;
        if (proxies != null && proxies.Count > 0)
        {
            // 根据GEO重命名
            if (options.GeoLookup)
            {
                Logger.Info("开始GEO重命名...");
                ConcurrentDictionary<IWebProxy, GeoInfo> infoMap = [];
                int chunkIndex = 0;
                foreach (ProxyNode[] chunk in proxies.Chunk(Constants.MaxPortsOccupied))
                {
                    // 查询GEO信息
                    MetaService.UsingProxies([.. chunk], proxied =>
                    {
                        return proxied.AsParallel().Select((proxy, index) =>
                        {
                            GeoInfo geoInfo = Task.Run(() => GeoElector.LookupAsnyc(proxy.Mixed)).Result;
                            infoMap.TryAdd(proxy.Mixed ??= new WebProxy(), geoInfo);
                            int current = chunkIndex * Constants.MaxPortsOccupied + index + 1;
                            Logger.Info(Strings.Padding(Emoji.EmojiToShort($"[{current}/{proxies.Count}] {proxy.Name}"), Constants.MaxSubject) + " ==> " + $"{geoInfo.CountryCode}");
                            return geoInfo;
                        }).ToList();
                    });
                    chunkIndex += 1;
                }

                // 分配GEO信息，并重命名
                proxies.ForEach((proxy) =>
                {
                    proxy.GeoInfo = proxy.Mixed != null && infoMap.TryGetValue(proxy.Mixed, out var geoInfo) ? geoInfo : new GeoInfo();
                    proxy.Name = $"{proxy.GeoInfo.Emoji} {proxy.GeoInfo.Country}";
                });
                // 国家名称_序号
                proxies = Distinct(proxies);
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

    public static List<ProxyNode> Purify(List<ProxyNode> proxies)
    {
        List<ProxyNode> purified = [];
        int chunkIndex = 0;
        foreach (ProxyNode[] chunk in proxies.Chunk(Constants.MaxPortsOccupied))
        {
            purified.AddRange(BinaryTest([.. chunk]));
            Logger.Info(Strings.Padding($"[{chunkIndex * Constants.MaxPortsOccupied + chunk.Length}/{proxies.Count}]", Constants.MaxSubject) + " 节点净化...");
            chunkIndex += 1;
        }
        if (purified.Count < proxies.Count)
        {
            Logger.Warn($"节点净化完成,排除{proxies.Count - purified.Count}个节点");
        }
        else
        {
            Logger.Warn("节点净化完成");
        }
        return purified;
    }

    private static List<ProxyNode> BinaryTest(List<ProxyNode> proxies)
    {
        if (proxies != null && proxies.Count > 0)
        {
            try
            {
                return MetaService.UsingProxies(proxies, (proxied) => proxies);
            }
            catch
            {
                return proxies.Count > 1 ? [.. BinaryTest(proxies.Take(proxies.Count / 2).ToList()), .. BinaryTest(proxies.Skip(proxies.Count / 2).ToList())] : [];
            }
        }
        return [];
    }
}
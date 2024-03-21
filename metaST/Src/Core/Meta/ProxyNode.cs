using System.Net;
using Core.CommandLine.Enum;
using Core.Geo;

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
}
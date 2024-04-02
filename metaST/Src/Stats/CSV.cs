using Core.Meta;

namespace Stats;

public class CSV
{
    public static string Generate(List<ProxyNode> proxies)
    {
        Dictionary<string, Func<ProxyNode, string>> cols = new(){
            { "name",       (proxy) => proxy.Name                            },
            { "type",       (proxy) => proxy.Type                            },
            { "delay(ms)",  (proxy) => proxy.DelayResult.Result().ToString() },
            { "speed(bps)", (proxy) => proxy.SpeedResult.Result().ToString() },
            { "ip",         (proxy) => proxy.GeoInfo.Address.ToString()      },
            { "country",    (proxy) => proxy.GeoInfo.CountryCode             },
            { "isp",        (proxy) => proxy.GeoInfo.Organization            },
        };

        string header = string.Join(",", cols.Select(col => col.Key).ToList());
        string body = string.Join(Environment.NewLine,
            proxies.Select(proxy => string.Join(",",
                cols.Select(col => col.Value(proxy))
                .Select(val => $"\"{val.Replace("\"", "\"\"")}\"").ToList()
            )));
        return $"{header}{Environment.NewLine}{body}";
    }
}
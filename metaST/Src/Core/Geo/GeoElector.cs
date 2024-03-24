using System.Net;
using System.Reflection;

namespace Core.Geo;
public class GeoElector
{
    private static readonly IEnumerable<IGeoLookup?> instances;
    // 白名单
    private static readonly IEnumerable<Type?> whiteList = [];
    // 黑名单
    private static readonly IEnumerable<Type?> blackList = [];
    static GeoElector()
    {
        // 查询所有的IGeoLookup实现
        instances = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IGeoLookup)) && !type.IsInterface)
            .Where(type => (!whiteList.Any() || whiteList.Contains(type)) && !blackList.Contains(type))
            .Select(type => (IGeoLookup?)Activator.CreateInstance(type))
            .Where(instance => instance != null).ToList();
        // 设置查询超时
        IGeoLookup.LookupTimout = Context.Options.GeoLookupTimeout;
    }
    public static Task<GeoInfo> LookupAsnyc(IWebProxy? proxy)
    {
        return Task.Run(() =>
        {
            List<GeoInfo?> infos = [];
            // 等待查询结束
            Task.WaitAll(instances.Select((instance) => Task.Run(() => infos.Add(instance?.Lookup(proxy)))).ToArray());
            // 过滤空结果
            infos = infos.Where(info => info != null).ToList();
            if (infos.Count > 0)
            {
                dynamic? address = Elect(infos, (info) => info?.Address);
                dynamic? countryCode = Elect(infos, (info) => info?.CountryCode, key => !"UNKNOWN".Equals(key));
                dynamic? country = Elect(infos, (info) => info?.Country);
                dynamic? organization = Elect(infos, (info) => info?.Organization);
                return new GeoInfo(address, countryCode, country, organization, proxy);
            }
            return new GeoInfo();
        });
    }

    private static dynamic? Elect(IEnumerable<GeoInfo?> infos, Func<GeoInfo?, dynamic?> identifier, Predicate<dynamic>? filter = null)
    {
        dynamic? elected = infos
            .GroupBy(identifier)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .Where(key => filter == null || filter.Invoke(key))
            .FirstOrDefault(key => !string.IsNullOrWhiteSpace(key));
        return elected ?? string.Empty;
    }
}
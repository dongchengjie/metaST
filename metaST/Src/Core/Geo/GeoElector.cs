using System.Net;
using System.Reflection;

namespace Core.Geo;
public class GeoElector
{
    private static readonly IEnumerable<IGeoLookup?> instances;
    static GeoElector()
    {
        instances = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IGeoLookup)) && !type.IsInterface)
            .Select(type => (IGeoLookup?)Activator.CreateInstance(type))
            .Where(instance => instance != null).ToList();
    }
    public static Task<GeoInfo> LookupAsnyc(IWebProxy? proxy)
    {
        return Task.Run(() =>
        {
            List<GeoInfo?> infos = [];
            Task.WaitAny(
                Task.WhenAll(instances.Select((instance) => Task.Run(() => infos.Add(instance?.Lookup(proxy)))).ToArray()),
                Task.Delay(5000)
            );
            infos = infos.Where(value => value != null).ToList();
            if (infos.Count > 0)
            {
                dynamic? address = Elect(infos, (info) => info?.Address);
                dynamic? countryCode = Elect(infos, (info) => info?.CountryCode);
                dynamic? country = Elect(infos, (info) => info?.Country);
                dynamic? organization = Elect(infos, (info) => info?.Organization);
                return new GeoInfo(address, countryCode, country, organization, proxy);
            }
            return new GeoInfo();
        });
    }

    private static dynamic? Elect(IEnumerable<GeoInfo?> infos, Func<GeoInfo?, dynamic?> identifier)
    {
        return infos
            .GroupBy(identifier)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .FirstOrDefault(key => key != null);
    }
}
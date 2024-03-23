using System.Net;

namespace Core.Geo;

public interface IGeoLookup
{
    GeoInfo Lookup(IWebProxy? proxy);

    public static double LookupTimout { get; set; } = 5000;
}
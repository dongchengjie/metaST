using System.Net;

namespace Core.Geo;

public interface IGeoLookup
{
    GeoInfo Lookup(IWebProxy? proxy);

    protected static readonly double LookupTimout = 3000;
}
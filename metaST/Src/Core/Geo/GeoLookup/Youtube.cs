using System.Net;
using System.Text.RegularExpressions;
using Util;

namespace Core.Geo;

public partial class Youtube : IGeoLookup
{
    public GeoInfo? Lookup(IWebProxy? proxy)
    {
        try
        {
            string html = HttpRequest.GetForBody("https://www.youtube.com/", IGeoLookup.LookupTimout, proxy);
            if (!string.IsNullOrWhiteSpace(html))
            {
                if (html.Contains("\"gl\":") || html.Contains("\"remoteHost\""))
                {
                    Match addressMatch = AddressRegex().Match(html);
                    Match countryCodeMatch = CountryCodeRegex().Match(html);
                    return new()
                    {
                        Address = addressMatch.Success ? addressMatch.Groups[1].Value : null,
                        CountryCode = countryCodeMatch.Success ? countryCodeMatch.Groups[1].Value : null,
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Debug($"Error looking up Geo via ${this.GetType().Name}: ${ex.Message}");
        }
        return null;
    }

    [GeneratedRegex(@".*""remoteHost"": ?""(.*?)""")]
    private static partial Regex AddressRegex();

    [GeneratedRegex(@".*""gl"": ?""(.*?)""")]
    private static partial Regex CountryCodeRegex();
}
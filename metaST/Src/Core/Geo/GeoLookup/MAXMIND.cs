namespace Core.Geo;

public class MAXMIND : DBIP
{
    protected override string AddressField { get; } = "traits.ip_address";
    protected override string CountryCodeField { get; } = "country.iso_code";
    protected override string CountryField { get; } = "country.names.zh-CN";
    protected override string OrganizationField { get; } = "traits.isp";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://geoip.maxmind.com/geoip/v2.1/city/me"),
            Headers =
            {
                { "Origin",  "https://www.maxmind.com/" },
                { "Referer", "https://www.maxmind.com/" },
            },
        };
    }

}
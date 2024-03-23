namespace Core.Geo;

public class IPSB : DBIP
{
    protected override string AddressField { get; } = "ip";
    protected override string CountryCodeField { get; } = "country_code";
    protected override string CountryField { get; } = "country";
    protected override string OrganizationField { get; } = "isp";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.ip.sb/geoip"),
            Headers =
            {
                { "Origin",  "https://ip.sb/api" },
                { "Referer", "https://ip.sb/api" },
            },
        }; ;
    }
}
namespace Core.Geo;

public class IPWHOIS : DBIP
{
    protected override string AddressField { get; } = "ip";
    protected override string CountryCodeField { get; } = "country_code";
    protected override string CountryField { get; } = "country";
    protected override string OrganizationField { get; } = "connection.isp";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://ipwhois.io/widget?ip=&lang=en"),
            Headers =
            {
                { "Origin",  "https://ipwhois.io/" },
                { "Referer", "https://ipwhois.io/" }
            }
        };
    }
}
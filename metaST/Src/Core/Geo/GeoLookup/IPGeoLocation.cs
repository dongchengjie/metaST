namespace Core.Geo;

public class IPGeoLocation : DBIP
{
    protected override string AddressField { get; } = "ip";
    protected override string CountryCodeField { get; } = "country_code2";
    protected override string CountryField { get; } = "country_name";
    protected override string OrganizationField { get; } = "isp";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://api.ipgeolocation.io/ipgeo"),
            Headers =
            {
                { "Origin",  "https://ipgeolocation.io" },
                { "Referer", "https://ipgeolocation.io" },
            },
        }; ;
    }
}
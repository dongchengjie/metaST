namespace Core.Geo;

public class IpApiDotCo : DBIP
{
    protected override string AddressField { get; } = "ip";
    protected override string CountryCodeField { get; } = "country";
    protected override string CountryField { get; } = "country_name";
    protected override string OrganizationField { get; } = "org";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://ipapi.co/json"),
            Headers =
            {
                { "Origin",  "https://ipapi.co/" },
                { "Referer", "https://ipapi.co/" },
            },
        }; ;
    }
}
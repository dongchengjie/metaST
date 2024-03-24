namespace Core.Geo;

public class SurfShark : DBIP
{
    protected override string AddressField { get; } = "ip";
    protected override string CountryCodeField { get; } = "countryCode";
    protected override string CountryField { get; } = "country";
    protected override string OrganizationField { get; } = "isp";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://surfshark.com/api/v1/server/user"),
            Headers =
            {
                { "Origin",  "https://surfshark.com/" },
                { "Referer", "https://surfshark.com/" },
            },
        }; ;
    }
}
namespace Core.Geo;

public class IPAPIDotCom : DBIP
{
    protected override string AddressField { get; } = "query";
    protected override string CountryCodeField { get; } = "countryCode";
    protected override string CountryField { get; } = "country";
    protected override string OrganizationField { get; } = "isp";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://ip-api.com/json"),
            Headers =
            {
                { "Origin",  "https://ip-api.com/" },
                { "Referer", "https://ip-api.com/" },
            },
        }; ;
    }
}
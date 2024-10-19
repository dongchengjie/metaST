namespace Core.Geo.Lookup;

public class IPSB : AJsonLookup
{
    public override LookupType Type() => LookupType.BOTH;
    public override double Confidence() => 5.0;

    protected override HttpRequestMessage SelfRequestMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.ip.sb/geoip"),
            Headers =
            {
                { "Accept-Language",  "en-US" },
                { "Referer", "https://ip.sb/api/" }
            }
        };
    }
    protected override ResultMapping SelfResultMapping()
    {
        return new ResultMapping()
        {
            AddressField = "ip",
            CountryCodeField = "country_code",
            CountryField = "country",
            OrganizationField = "organization"
        };
    }

    protected override HttpRequestMessage AddressRequestMessage(string address)
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://api.ip.sb/geoip/{address}"),
            Headers =
            {
                { "Accept-Language",  "en-US" },
                { "Referer", "https://ip.gs/api/" }
            }
        };
    }
    protected override ResultMapping AddressResultMapping()
    {
        return new ResultMapping()
        {
            AddressField = "ip",
            CountryCodeField = "country_code",
            CountryField = "country",
            OrganizationField = "organization"
        };
    }
}
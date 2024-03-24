namespace Core.Geo;

public class BrightVPN : DBIP
{
    protected override string AddressField { get; } = "ip";
    protected override string CountryCodeField { get; } = "country";
    protected override string CountryField { get; } = "_CountryField";
    protected override string OrganizationField { get; } = "asn.org_name";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://brightvpn.com/wp-json/brd/info/general")
        }; ;
    }
}
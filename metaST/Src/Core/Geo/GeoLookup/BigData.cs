namespace Core.Geo;

public class BigData : DBIP
{
    protected override string AddressField { get; } = "_AddressField";
    protected override string CountryCodeField { get; } = "countryCode";
    protected override string CountryField { get; } = "countryName";
    protected override string OrganizationField { get; } = "_OrganizationField";

    protected override HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://api.bigdatacloud.net/data/reverse-geocode-client")
        }; ;
    }
}
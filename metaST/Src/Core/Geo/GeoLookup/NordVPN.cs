namespace Core.Geo;

public class NordVPN : DBIP
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
            RequestUri = new Uri("https://nordvpn.com/wp-admin/admin-ajax.php?action=get_user_info_data"),
            Headers =
            {
                { "Origin",  "https://nordvpn.com/" },
                { "Referer", "https://nordvpn.com/" },
            },
        }; ;
    }
}
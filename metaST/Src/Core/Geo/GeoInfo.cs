using System.Globalization;
using System.Net;

namespace Core.Geo;

public class GeoInfo
{
    public string? Address { get; set; }
    public string? CountryCode { get; set; }
    public string? _country;
    public string? Country
    {
        get
        {
            return string.IsNullOrWhiteSpace(_country) ?
            string.IsNullOrWhiteSpace(CountryCode) || CountryCode.Length != 2 ? "UNKNOWN" : GetCountryByCode(CountryCode)
            : _country;
        }
        set { _country = value; }
    }
    public string? Organization { get; set; }
    public string Emoji
    {
        get
        {
            return string.IsNullOrWhiteSpace(CountryCode) || CountryCode.Length != 2 ?
            "❓" : GetEmojiByCode(CountryCode);
        }
    }
    public IWebProxy? Proxy { get; set; }
    private static string GetCountryByCode(string countryCode)
    {
        try
        {
            return new RegionInfo(countryCode).DisplayName;
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
    }
    private static string GetEmojiByCode(string countryCode)
    {
        countryCode = countryCode.ToUpper();
        const int offset = 0x1F1E6 - 'A';
        return char.ConvertFromUtf32(countryCode[0] + offset) + char.ConvertFromUtf32(countryCode[1] + offset);
    }
}
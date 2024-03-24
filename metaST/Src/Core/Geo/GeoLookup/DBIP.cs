using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Util;

namespace Core.Geo;

public delegate void AttributesApplier<E, V>(E geoInfo, V? val);

public class DBIP : IGeoLookup
{
    protected virtual string AddressField { get; } = "ipAddress";
    protected virtual string CountryCodeField { get; } = "countryCode";
    protected virtual string CountryField { get; } = "countryName";
    protected virtual string OrganizationField { get; } = "_OrganizationField";

    public GeoInfo Lookup(IWebProxy? proxy) => Lookup(proxy, AddressField, CountryCodeField, CountryField, OrganizationField);

    protected virtual HttpRequestMessage GetMessage()
    {
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.db-ip.com/v2/free/self"),
            Headers =
            {
                { "Origin",  "https://db-ip.com/" },
                { "Referer", "https://db-ip.com/" },
            }
        };
    }

    protected GeoInfo Lookup(IWebProxy? proxy, string addressField, string countryCodeField, string countryField, string organizationField)
    {
        GeoInfo geoInfo = new();
        // 读取JSON串
        string? json = HttpRequest.UsingHttpClient((client) =>
        {
            try
            {
                HttpRequestMessage message = GetMessage();
                HttpResponseMessage response = client.SendAsync(message).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error looking up Geo via {GetType().Name}: {ex.Message}");
                return string.Empty;
            }
        }, IGeoLookup.LookupTimout, proxy);
        // 提取JSON串中属性并赋值
        if (!string.IsNullOrWhiteSpace(json))
        {
            return ParseAndApply(json, new Dictionary<string, AttributesApplier<GeoInfo, string?>>()
            {
                { addressField,      (info, val) => info.Address = val ?? string.Empty      },
                { countryCodeField,  (info, val) => info.CountryCode = val ?? "UNKNOWN"     },
                { countryField,      (info, val) => info.Country = val ?? string.Empty      },
                { organizationField, (info, val) => info.Organization = val ?? string.Empty }
            });
        }
        return geoInfo;
    }

    protected static GeoInfo ParseAndApply(string? json, Dictionary<string, AttributesApplier<GeoInfo, string?>> actions)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                JObject? jObject = JsonConvert.DeserializeObject<JObject>(json);
                if (jObject != null)
                {
                    GeoInfo geoInfo = new();
                    foreach (var action in actions)
                    {
                        JToken? tmp = jObject;
                        if (!string.IsNullOrWhiteSpace(action.Key))
                        {
                            string[] keys = action.Key.Split('.');
                            foreach (string key in keys)
                            {
                                tmp = tmp?[key];
                            }
                            string? val = tmp?.ToString();
                            action.Value.Invoke(geoInfo, val);
                        }
                    }
                    return geoInfo;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error parsing Geo JSON: {ex.Message}");
            }
        }
        return new();
    }
}
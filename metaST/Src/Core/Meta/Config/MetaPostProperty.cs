using System.Text.RegularExpressions;
using Core.CommandLine;
using Core.CommandLine.Enum;
using Newtonsoft.Json;
using Util;

namespace Core.Meta.Config;

public partial class MetaPostProperty
{
    public static string Resolve(string config)
    {
        // 读取所有${}占位符
        Regex regex = PlaceholderRegex();
        MatchCollection matchCollection = regex.Matches(config);
        List<GroupCollection> groupCollections = matchCollection.Select(match => match.Groups).DistinctBy(groups => groups[0].Value).ToList();
        // 生成替换映射
        Dictionary<string, string> replacements = groupCollections.ToDictionary(groups => groups[0].Value, groups => ResolvePropertyValue(groups[1].Value));
        // 替换掉占位符
        foreach (KeyValuePair<string, string> entry in replacements)
        {
            config = config.Replace(entry.Key, entry.Value);
        }
        return config;
    }

    private static string ResolvePropertyValue(string property)
    {
        // 处理命令行参数
        if (property.StartsWith("options")) return ResolveOptionValue(property);
        // 处理图标资源
        if (property.StartsWith("icons")) return ResolveIcon(property);
        throw new InvalidDataException($"处理配置属性值错误，未知的属性: {property}");
    }

    private static string ResolveOptionValue(string property)
    {
        CommandLineOptions options = Context.Options;
        string json = JsonConvert.SerializeObject(options);
        Dictionary<string, dynamic>? optionMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        property = property.Replace("options.", string.Empty);
        if (optionMap != null && optionMap.TryGetValue(property, out var value))
        {
            return value.ToString();
        }
        return string.Empty;
    }

    private static string ResolveIcon(string property)
    {
        CommandLineOptions options = Context.Options;
        // http远程Icon
        if (options.IconType.Equals(IconType.http))
        {
            string githubMirror = options.GithubMirror;
            string repository = "https://fastly.jsdelivr.net/gh/dongchengjie/metaST@main/metaST/Resources/";
            string extension = Path.GetExtension(property);
            string iconPath = property.Replace(".", "/").Replace(extension.Replace(".", "/"), extension);
            return $"{repository}{iconPath}";
        }
        // Base64编码Icon
        byte[] bytes = Resources.ReadAsBytes(property);
        bytes = bytes.Length > 0 ? bytes : Resources.ReadAsBytes(property = "icons.unknown.svg");
        string base64 = Convert.ToBase64String(bytes);
        if (property.EndsWith(".jpeg") || property.EndsWith(".jpg"))
        {
            return "data:image/jpeg;base64," + base64;
        }
        if (property.EndsWith(".svg"))
        {
            return "data:image/svg+xml;base64," + base64;
        }
        return "data:image/png;base64," + base64;
    }

    [GeneratedRegex(@"\$\{(.*?)\}")]
    private static partial Regex PlaceholderRegex();
}
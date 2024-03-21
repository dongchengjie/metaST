using System.Text.RegularExpressions;
using Core.CommandLine;
using Newtonsoft.Json;
using Util;

namespace Core.Meta.Config;

public partial class MetaProperty
{
    public static string Resolve(string config, CommandLineOptions options)
    {
        // 读取所有${}占位符
        Regex regex = PlaceholderRegex();
        MatchCollection matchCollection = regex.Matches(config);
        List<GroupCollection> groupCollections = matchCollection.Select(match => match.Groups).DistinctBy(groups => groups[0].Value).ToList();
        // 生成替换映射
        Dictionary<string, string> replacements = groupCollections.ToDictionary(groups => groups[0].Value, groups => GetValue(groups[1].Value, options));
        // 替换掉占位符
        foreach (KeyValuePair<string, string> entry in replacements)
        {
            config = config.Replace(entry.Key, entry.Value);
        }
        return config;
    }

    private static string GetValue(string prorperty, CommandLineOptions options)
    {
        // 处理命令行参数
        if (prorperty.StartsWith("options"))
        {
            string json = JsonConvert.SerializeObject(options);
            Dictionary<string, dynamic>? optionMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            prorperty = prorperty.Replace("options.", string.Empty);
            if (optionMap != null && optionMap.TryGetValue(prorperty, out var value))
            {
                return value.ToString();
            }
        }
        // 处理图标资源
        if (prorperty.StartsWith("icons"))
        {
            return "data:image/svg+xml;base64," + Convert.ToBase64String(Resources.ReadAsBytes(prorperty));
        }
        throw new InvalidDataException($"处理配置属性值错误，未知的属性: {prorperty}");
    }

    [GeneratedRegex(@"\$\{(.*?)\}")]
    private static partial Regex PlaceholderRegex();
}
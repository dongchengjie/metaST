using System.Net;
using System.Text;
using Core.CommandLine;
using Core.CommandLine.Enum;
using Util;

namespace Core.Meta.Config;

public class MetaConfig
{
    public static List<Proxy> GetProxies(string config, bool includeProxies = true, bool includeProviders = true)
    {
        // 不包含任何节点
        if (!includeProxies && !includeProviders) return [];

        // 订阅链接 或 节点池链接
        if (config.StartsWith("http"))
        {
            try
            {
                Logger.Info("下载配置文件：" + config);
                // 下载获取文件内容
                string? content = HttpRequest.UsingHttpClient((client) =>
                {
                    // 设置User-Agent为clash
                    client.DefaultRequestHeaders.Add("User-Agent", "clash");
                    return client.GetAsync(config).Result.Content.ReadAsStringAsync().Result;
                });
                // 文件内容要求不为空
                if (string.IsNullOrWhiteSpace(content)) throw new Exception(config);
                // 需要Base64解码
                if (Strings.IsBase64String(content)) content = Strings.ToBase64String(content, Encoding.UTF8);
                Logger.Info("下载配置文件完成");
                // 保存到临时目录
                string dest = Path.Combine(Constants.WorkSpaceTemp, Strings.Md5(config) + ".yaml");
                try
                {
                    Files.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(content)), dest);
                    return GetProxies(dest, includeProxies, includeProviders);
                }
                finally
                {
                    Files.DeleteFile(dest);
                }
            }
            catch (Exception ex)
            {
                Exception temp = ex;
                while (temp.InnerException != null)
                {
                    temp = temp.InnerException;
                }
                Logger.Warn("下载配置文件失败:" + temp.Message);
                return [];
            }
        }

        // 保存节点列表
        List<Proxy> proxyList = [];
        try
        {
            // 解析配置文件
            string yaml = File.ReadAllText(config);
            Dictionary<dynamic, dynamic> yamlObject = YamlDot.DeserializeObject(yaml);

            // 如果存在proxies
            if (includeProxies && yamlObject.ContainsKey("proxies"))
            {
                List<dynamic> proxies = yamlObject["proxies"];
                proxies.ForEach((proxy) => proxyList.Add(new Proxy(proxy)));
            }

            // 如果存在proxy-providers
            if (includeProviders && yamlObject.ContainsKey("proxy-providers"))
            {
                Dictionary<dynamic, dynamic> providers = yamlObject["proxy-providers"];
                // 读取provider提供的配置
                List<Proxy> proxies = providers
                   .Where(entry => "http".Equals(entry.Value["type"]) && entry.Value["url"]?.StartsWith("http"))
                   .Select((entry) => (List<Proxy>)GetProxies(entry.Value["url"], includeProxies, includeProviders))
                   .SelectMany(list => list).ToList();
                // 添加到节点列表
                proxyList.AddRange(proxies);
            }
            return proxyList;
        }
        catch (Exception ex)
        {
            Logger.Warn("解析配置" + config + "文件出错：" + ex.Message);
        }
        return proxyList;
    }

    public static List<Proxy> Distinct(List<Proxy> proxies, DistinctStrategy strategy)
    {
        // 按去重策略去重
        switch (strategy)
        {
            case DistinctStrategy.type_server_port:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Type, proxy.Server, proxy.Port])).ToList();
                break;
            case DistinctStrategy.type_server:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Type, proxy.Server])).ToList();
                break;
            case DistinctStrategy.server_port:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Server, proxy.Port])).ToList();
                break;
            case DistinctStrategy.server:
                proxies = proxies.DistinctBy((proxy) => string.Join('_', [proxy.Server])).ToList();
                break;
        }
        // 名称重复的添加序号后缀
        proxies = proxies
        .GroupBy(p => p.Name)
        .SelectMany(grp => grp.Select((p, i) =>
        {
            p.Name = grp.Count() > 1 ? $"{p.Name}_{i + 1}" : p.Name;
            return p;
        })).ToList();
        return proxies;
    }

    public class MetaInfo(string config, string configPath, PortManager portManager, List<Proxy> proxies)
    {
        public string Config { get; set; } = config;
        public string ConfigPath { get; set; } = configPath;
        public PortManager PortManager { get; set; } = portManager;
        public List<Proxy> Proxies { get; set; } = proxies;
    }

    public static string GetTemplate(string resourceName) => string.Join(Environment.NewLine, [Resources.ReadAsText("template.common.yaml"), Resources.ReadAsText(resourceName)]);

    public static MetaInfo CreateMixed(List<Proxy> proxies)
    {
        // 读取模板内容
        string yaml = GetTemplate("template.mixed.yaml");

        PortManager portManager = PortManager.Claim(proxies.Count);
        // mixed监听端口
        string listenerList = string.Join(Environment.NewLine, proxies.Select((proxy, index) => $"- name: mixed{index}\n  type: mixed\n  port: {portManager.Get(index)}\n  proxy: {Json.SerializeObject(proxy.Name)}"));
        // mixed出口代理
        string proxyList = string.Join(Environment.NewLine, proxies.Select((proxy, index) => $"  - {Json.SerializeObject(proxy.Info)}"));

        // 生成配置文件
        string config = yaml
            .Replace("listeners: []", $"listeners: \n{listenerList}")
            .Replace("proxies: []", $"proxies: \n{proxyList}");

        // 输出到文件
        string configPath = Path.Combine(Constants.WorkSpaceTemp, "mixed", Guid.NewGuid().ToString() + ".yaml");
        Files.WriteToFile(new MemoryStream(Encoding.UTF8.GetBytes(config)), configPath);

        // 节点配置本地代理
        proxies = proxies.Select((proxy, index) =>
        {
            proxy.Mixed = new WebProxy("127.0.0.1", portManager.Get(index));
            return proxy;
        }).ToList();

        // 返回服务信息
        return new(config, configPath, portManager, proxies);
    }

    public static string CreateStandard(List<Proxy> proxies, CommandLineOptions options)
    {
        // 读取模板内容
        string yaml = GetTemplate("template.standard.yaml");

        // 代理列表
        string proxyList = string.Join(Environment.NewLine, proxies.Select((proxy, index) => $"  - {Json.SerializeObject(proxy.Info)}"));

        // 读取规则集
        string rules = MetaRule.GetRules(options.RuleSet);

        // 生成配置文件
        string config = yaml
            .Replace("proxies: []", $"proxies: \n{proxyList}")
            .Replace("rules: []", $"{rules}");

        // 处理参数
        config = MetaProperty.Resolve(config, options);

        return config;
    }


}
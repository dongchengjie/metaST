using System.Diagnostics;
using Core.Meta.Config;
using Util;

namespace Core.Meta;

public class MetaService
{
    public static T UsingProxies<T>(List<Proxy> proxies, Func<List<Proxy>, T> action)
    {
        // 生成mixed配置文件
        MetaConfig.MetaInfo metaInfo = MetaConfig.CreateMixed(proxies);
        Task<Process>? task = null;
        try
        {
            // 释放所有端口
            metaInfo.PortManager.Dispose();
            // 开启代理
            task = MetaProxy.StartProxy(metaInfo.ConfigPath);
            // 使用代理
            return action.Invoke(metaInfo.Proxies);
        }
        finally
        {
            Processes.Kill(task?.Result);
        }
    }
}
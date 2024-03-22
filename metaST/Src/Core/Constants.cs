using System.Reflection;
using System.Runtime.InteropServices;

namespace Core;
public class Constants
{
    // 内核名称
    public static readonly string MetaCoreName = "mihomo";
    // Meta内核可执行程序名
    public static readonly string ExecutableName = MetaCoreName + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty);
    // 用户目录
    public static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    // 配置目录
    public static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config" + Path.DirectorySeparatorChar + MetaCoreName + Path.DirectorySeparatorChar);
    // 临时文件目录
    public static readonly string TempPath = Path.GetTempPath();
    // 应用程序目录
    public static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;
    // 工作目录
    public static readonly string WorkSpace = Path.Combine(TempPath, "." + Assembly.GetEntryAssembly()?.GetName().Name);
    // 工作目录/临时目录
    public static readonly string WorkSpaceTemp = Path.Combine(WorkSpace, "temp");
    // 最大占用端口数
    public static readonly int MaxPortsOccupied = 500;
    // 最大占用端口数
    public static readonly int MaxDelayTestThreads = 64;
}
using System.Runtime.InteropServices;
using Util;

namespace Core.Meta;

public class MetaProxy
{
    static MetaProxy()
    {
        string fileName = Constants.MetaCoreName + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty);
        string resourceName = "meta." + Platform.GetPlatform() + "." + fileName;
        Resources.Extract(resourceName, Path.Combine(Constants.WorkSpace, fileName), false);
    }
}
using System.Reflection;
using System.Text;

namespace Util;

public class Resources
{
    public static void Extract(string resourceName, string dest, bool deleteIfExists = true)
    {
        string resourcePath = ResourcePath(resourceName);
        if (deleteIfExists && File.Exists(dest)) File.Delete(dest);
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        stream?.CopyTo(new FileStream(dest, FileMode.Create));
    }

    public static string ReadAsText(string resourceName, Encoding? encoding = null)
    {
        string resourcePath = ResourcePath(resourceName);
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream is null) return string.Empty;

        using StreamReader reader = new(stream, encoding ??= Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static string ResourcePath(string resourceName) => Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + resourceName;
}

using System.Security.Cryptography;
using System.Text;

namespace Util;

public class Strings
{
    public static string Padding(string str, int width, char paddingChar, bool chinsesAsDouble = true)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }
        else if (str.Length > width)
        {
            return str[..width];
        }
        int doubleCount = chinsesAsDouble ? str.Count(c => c >= '\u4E00' && c <= '\u9FFF') : 0;
        int paddingCount = width - (str.Length + doubleCount);
        return paddingCount > 0 ? str.PadRight(str.Length + paddingCount, paddingChar) : str;
    }

    public static bool IsBase64String(string base64)
    {
        Span<byte> buffer = new(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }

    public static string ToBase64String(string str, Encoding encoding) => encoding.GetString(Convert.FromBase64String(str));

    public static string Md5(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
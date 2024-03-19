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
}
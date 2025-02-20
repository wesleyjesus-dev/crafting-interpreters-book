namespace Lox;

public static class StringExtensions
{
    public static string JSubstring(this string str, int beginIndex, int endIndex)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));

        if (beginIndex < 0 || endIndex > str.Length || beginIndex > endIndex)
            throw new ArgumentOutOfRangeException("index out of range");

        return str.Substring(beginIndex, endIndex - beginIndex);
    }
}
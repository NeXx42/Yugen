namespace Yugen.Domain.Helpers;

public static class ExtensionMethods
{
    public static void TryParseEnumNullable<T>(this string? inp, out T? val, bool ignoreCase = true) where T : struct
    {
        if (!string.IsNullOrEmpty(inp) && Enum.TryParse(inp, ignoreCase, out T res))
        {
            val = res;
            return;
        }

        val = default;
        return;
    }

    public static T? ParseEnumNullable<T>(this string? inp, bool ignoreCase = true) where T : struct
    {
        if (!string.IsNullOrEmpty(inp) && Enum.TryParse(inp, ignoreCase, out T res))
        {
            return res;
        }

        return default;
    }
}

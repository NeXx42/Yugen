using System.Text.Json;

namespace Yugen.Core.Helpers;

public static class ExtensionMethods
{
    public static void ExtractInt(this JsonElement element, string name, out int? val)
    {
        try
        {
            val = null;

            if (element.TryGetProperty(name, out JsonElement el) && el.TryGetInt32(out int res))
                val = res;
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to parse element - {name}\n\n{e.Message}");
        }
    }

    public static void ExtractString(this JsonElement element, string name, out string? val)
    {
        try
        {
            val = null;

            if (element.TryGetProperty(name, out JsonElement el))
                val = el.GetString();
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to parse element - {name}\n\n{e.Message}");
        }
    }
}

using SmartFormat;

namespace Filedini.Localization;

public static class StringResourceHelper
{
    public static string GetString0(string key)
    {
        return Resources.ResourceManager.GetString(key, Resources.Culture) ?? key;
    }
    
    public static string GetString0(string key, Func<string> defaultValue)
    {
        return Resources.ResourceManager.GetString(key, Resources.Culture) ?? defaultValue();
    }

    public static string GetString1(string key, object a0)
    {
        var s = Resources.ResourceManager.GetString(key, Resources.Culture);
        return s is null ? key : Smart.Format(s, a0);
    }

    public static string GetString2(string key, object a0, object a1)
    {
        var s = Resources.ResourceManager.GetString(key, Resources.Culture);
        return s is null ? key : Smart.Format(s, a0, a1);
    }

    public static string GetString3(string key, object a0, object a1, object a2)
    {
        var s = Resources.ResourceManager.GetString(key, Resources.Culture);
        return s is null ? key : Smart.Format(s, a0, a1, a2);
    }
}
using System;

namespace YoctoHelper.Cores;

public static class StringUtils
{

    public static bool IsNull(this string str)
    {
        return ObjectUtils.IsNull(str);
    }

    public static bool IsNotNull(this string str)
    {
        return ObjectUtils.IsNotNull(str);
    }

    public static bool IsEmpty(this string str)
    {
        return (StringUtils.IsNotNull(str)) && (str.Length <= 0);
    }

    public static bool IsNotEmpty(this string str)
    {
        return (StringUtils.IsNotNull(str)) && (str.Length > 0);
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return (StringUtils.IsNull(str)) || (str.Length <= 0);
    }

    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static bool IsNotWhiteSpace(this string str)
    {
        return (!string.IsNullOrWhiteSpace(str));
    }

    public static bool IsString(this object obj)
    {
        return (obj is string);
    }

    public static void EmptyStringFiller(ref string str, string defaultValue)
    {
        if (StringUtils.IsNullOrEmpty(str))
        {
            str = defaultValue;
        }
    }

    public static string EmptyStringFiller(string str, string defaultValue)
    {
        return (StringUtils.IsNullOrEmpty(str)) ? defaultValue : str;
    }

    public static void WhiteSpaceStringFiller(ref string str, string defaultValue)
    {
        if (StringUtils.IsNullOrWhiteSpace(str))
        {
            str = defaultValue;
        }
    }

    public static string WhiteSpaceStringFiller(string str, string defaultValue)
    {
        return (StringUtils.IsNullOrWhiteSpace(str)) ? defaultValue : str;
    }

    public static string[] Split(this string str, string separator = ",", bool removeEmpty = true, bool trim = true)
    {
        return StringUtils.Split(str, [separator], removeEmpty, trim);
    }

    public static string[] Split(this string str, string[] separator, bool removeEmpty = true, bool trim = true)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new string[0];
        }
        string[] split = str.Split(separator, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        if (trim)
        {
            for (int i = 0; i < split.Length; i++)
            {
                split[i] = split[i].Trim();
            }
        }
        return split;
    }

    public static int CountSubstring(this string str, string find)
    {
        int count = 0;
        for (int i = 0; (i = str.IndexOf(find, i)) != -1; i++)
        {
            count++;
        }
        return count;
    }

    public static string SubStringTilEnd(this string str, int start, int end)
    {
        return str.Substring(start, end - start);
    }

}

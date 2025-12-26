using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public static class FlagUtils
{

    private static readonly string ListRegexPattern = @"\{[\w\d\s,]*\}|[\w\d]+";

    private static readonly string ChooseRegexPattern = @"\{([^}]+:[^}]+)\}";

    private static readonly string RandomRegexPattern = @"\{([^}]+):\s*(\d+(\.\d+)?)\s*\}";

    public static void Add(ref Level level, string flag)
    {
        FlagUtils.Add(ref level.Session.Flags, flag);
    }

    public static void Add(ref Level level, string[] flags)
    {
        FlagUtils.Add(ref level.Session.Flags, flags);
    }

    public static void Add(ref HashSet<string> flagHashSet, string flag)
    {
        flagHashSet.Add(flag);
    }

    public static void Add(ref HashSet<string> flagHashSet, string[] flags)
    {
        if (flags == null)
        {
            return;
        }
        foreach (string flag in flags)
        {
            flagHashSet.Add(flag);
        }
    }

    public static void Remove(ref Level level, string flag)
    {
        FlagUtils.Remove(ref level.Session.Flags, flag);
    }

    public static void Remove(ref Level level, string[] flags)
    {
        FlagUtils.Remove(ref level.Session.Flags, flags);
    }

    public static void Remove(ref HashSet<string> flagHashSet, string flag)
    {
        flagHashSet.Remove(flag);
    }

    public static void Remove(ref HashSet<string> flagHashSet, string[] flags)
    {
        if (flags == null)
        {
            return;
        }
        foreach (string flag in flags)
        {
            flagHashSet.Remove(flag);
        }
    }

    public static void Clear(ref Level level)
    {
        FlagUtils.Clear(ref level.Session.Flags);
    }

    public static void Clear(ref HashSet<string> flagHashSet)
    {
        flagHashSet.Clear();
    }

    public static bool Contains(Level level, string flag) => level.Session.Flags.Contains(flag);

    public static bool Contains(Level level, string[] flags) => level.Session.Flags.Contains(flags);

    public static bool Contains(HashSet<string> flagHashSet, string flag) => flagHashSet.Contains(flag);

    public static bool Contains(this HashSet<string> flagHashSet, string[] flags)
    {
        if (flags == null)
        {
            return false;
        }
        foreach (string flag in flags)
        {
            if (!flagHashSet.Contains(flag))
            {
                return false;
            }
        }
        return true;
    }
    
    public static bool ConfirmFlags(this string conditions)
    {
        string[] _conditions = conditions.Split(',', StringSplitOptions.TrimEntries);

        return ConfirmFlags(_conditions);
    }
    public static bool ConfirmFlags(this ICollection<string> flags)
    {
        if (flags == null)
        {
            return true;
        }
        foreach (string flag in flags)
        {
            bool i = flag.StartsWith("!");
            string name = flag.Trim().TrimStart('!');
            if (i ? name.GetFlag() : !name.GetFlag())
            {
                return false;
            }
        }

        return true;
    }
    
    public static bool ConfirmFlags(params string[] flags)
    {
        if (flags == null)
        {
            return true;
        }
        foreach (string flag in flags)
        {
            bool i = flag.StartsWith("!");
            string name = flag.Trim().TrimStart('!');
            if (i ? name.GetFlag() : !name.GetFlag())
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsCorrectFlag(Level level, string input) => FlagUtils.IsCorrectFlag(level, FlagUtils.Parse(input));

    public static bool IsCorrectFlag(Level level, string[] flags)
    {
        if ((flags == null) || (flags.Length == 0))
        {
            return true;
        }
        return FlagUtils.Contains(level, flags);
    }

    public static string[] Intersect(Level level, string[] flags)
    {
        return FlagUtils.Intersect(level.Session.Flags, flags);
    }

    public static string[] Intersect(HashSet<string> flagHashSet, string[] flags)
    {
        return flagHashSet.Intersect(flags).ToArray();
    }

    public static string[] Parse(string input)
    {
        return (input == null) ? null : StringUtils.Split(input.Trim(), ",");
    }

    public static string[][] ParseList(string input)
    {
        List<string[]> list = new List<string[]>();
        MatchCollection matches = Regex.Matches(input.Trim(), FlagUtils.ListRegexPattern);
        if (matches.Count == 0)
        {
            return [[]];
        }
        foreach (Match match in matches)
        {
            string content = match.Value.Trim();
            if (content[0] == '{' && content[content.Length - 1] == '}')
            {
                list.Add(StringUtils.Split(content.Substring(1, content.Length - 2).Trim(), ",", false));
            }
            else
            {
                list.Add([content]);
            }
        }
        return list.ToArray();
    }

    public static Dictionary<string[], string[]> ParseChoose(string input)
    {
        Dictionary<string[], string[]> dictionary = new Dictionary<string[], string[]>();
        MatchCollection matches = Regex.Matches(input.Trim(), FlagUtils.ChooseRegexPattern);
        foreach (Match match in matches)
        {
            string content = match.Groups[1].Value.Trim();
            string[] parts = StringUtils.Split(content, ":");
            dictionary.Add(StringUtils.Split(parts[0], ","), StringUtils.Split(parts[1], ","));
        }
        return dictionary;
    }

    public static Dictionary<string[], float> ParseRandom(string input)
    {
        Dictionary<string[], float> dictionary = new Dictionary<string[], float>();
        MatchCollection matches = Regex.Matches(input.Trim(), FlagUtils.RandomRegexPattern);
        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value.Trim();
            string valueString = match.Groups[2].Value.Trim();
            if (float.TryParse(valueString, out float value))
            {
                dictionary.Add(StringUtils.Split(key, ","), value);
            }
        }
        return dictionary;
    }

    public static bool CheckCorrectFlags(Level level, string[] flags)
    {
        return (ObjectUtils.IsNotNull(level)) && ((ArrayUtils.IsNullOrEmpty(flags)) || (FlagUtils.Contains(level, flags)));
    }
}

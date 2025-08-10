namespace YoctoHelper.Cores;
using ChroniaHelper.Utils;

public static class FlagUtils
{

    public static void Add(Level level, string flag)
    {
        if ((ObjectUtils.IsNull(level)) || (StringUtils.IsNull(flag)))
        {
            return;
        }
        level.Session.SetFlag(flag, true);
    }

    public static void Add(Level level, string[] flags)
    {
        if ((ObjectUtils.IsNull(level)) || (ArrayUtils.IsNullOrEmpty(flags)))
        {
            return;
        }
        for (int i = 0; i < flags.Length; i++)
        {
            FlagUtils.Add(level, flags[i]);
        }
    }

    public static void Remove(Level level, string flag)
    {
        if ((ObjectUtils.IsNull(level)) || (StringUtils.IsNull(flag)))
        {
            return;
        }
        level.Session.SetFlag(flag, false);
    }

    public static void Remove(Level level, string[] flags)
    {
        if ((ObjectUtils.IsNull(level)) || (ArrayUtils.IsNullOrEmpty(flags)))
        {
            return;
        }
        for (int i = 0; i < flags.Length; i++)
        {
            FlagUtils.Remove(level, flags[i]);
        }
    }

    public static void Clear(Level level)
    {
        if (ObjectUtils.IsNull(level))
        {
            return;
        }
        level.Session.Flags.Clear();
    }

    public static bool Contains(Level level, string flag)
    {
        return (ObjectUtils.IsNotNull(level)) && (StringUtils.IsNotNull(flag)) && (level.Session.Flags.Contains(flag));
    }

    public static bool Contains(Level level, string[] flags)
    {
        if ((ObjectUtils.IsNull(level)) || (ArrayUtils.IsNullOrEmpty(flags)))
        {
            return false;
        }
        for (int i = 0; i < flags.Length; i++)
        {
            if (!FlagUtils.Contains(level, flags[i]))
            {
                return false;
            }
        }
        return true;
    }

    public static bool CheckCorrectFlags(Level level, string[] flags)
    {
        return (ObjectUtils.IsNotNull(level)) && ((ArrayUtils.IsNullOrEmpty(flags)) || (FlagUtils.Contains(level, flags)));
    }

    public static string[] Parse(string input)
    {
        return (StringUtils.IsNullOrWhiteSpace(input)) ? new string[0] : StringUtils.Split(input.Trim());
    }

}

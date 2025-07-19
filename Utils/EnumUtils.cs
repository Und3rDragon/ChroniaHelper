using System;

namespace ChroniaHelper.Utils;

public static class EnumUtils
{

    public static E StringToEnum<E>(string value, E defaultValue) where E : struct
    {
        return Enum.TryParse(value, out E result) ? result : defaultValue;
    }

    public static Classify MatchEnum<Classify>(this string match, Classify defaultMember, bool ignoreCase = false, bool ignoreUnderscore = false) where Classify : struct, Enum
    {
        if (string.IsNullOrEmpty(match) || string.IsNullOrWhiteSpace(match)) { return defaultMember; }
        //if (Enum.GetValues<Classify>().Length == 0) { return null; }

        string arg1 = match.Trim();
        if (ignoreCase) { arg1 = arg1.ToLower(); }
        if (ignoreUnderscore) { arg1 = arg1.RemoveAll("_"); }

        foreach (Classify member in Enum.GetValues<Classify>())
        {
            string arg2 = member.ToString().Trim();
            if (ignoreCase) { arg2 = arg2.ToLower(); }
            if (ignoreUnderscore) { arg2 = arg2.RemoveAll("_"); }

            if (arg1.Equals(arg2)) { return member; }
        }

        return defaultMember;
    }

}

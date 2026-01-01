using System;
using System.Collections.Generic;

namespace ChroniaHelper.Utils;

public static class MapDataUtils
{

    public static List<EntityData> GetMapDataEntities(Level level)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, false);
    }

    public static List<EntityData> GetMapDataTriggers(Level level)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, true);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, (entityData) => true, triggers);
    }

    public static List<EntityData> GetMapDataEntities(Level level, string name)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, false);
    }

    public static List<EntityData> GetMapDataTriggers(Level level, string name)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, true);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, string name, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, (entityData) => entityData.Name == name, triggers);
    }

    public static List<EntityData> GetMapDataEntities(Level level, string name, Func<EntityData, bool> predicate)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, predicate, false);
    }

    public static List<EntityData> GetMapDataTriggers(Level level, string name, Func<EntityData, bool> predicate)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, name, predicate, true);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, string name, Func<EntityData, bool> predicate, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityDataList(level, (entityData) => (entityData.Name == name) && (predicate(entityData)), triggers);
    }

    public static List<EntityData> GetMapDataEntityDataList(Level level, Func<EntityData, bool> predicate, bool triggers = false)
    {
        List<EntityData> entities = new List<EntityData>();
        foreach (LevelData levelData in level.Session.MapData.Levels)
        {
            List<EntityData> entityDataList = (triggers ? levelData.Triggers : levelData.Entities);
            foreach (EntityData entityData in entityDataList)
            {
                if (predicate(entityData))
                {
                    entities.Add(entityData);
                }
            }
        }
        return entities;
    }

    public static EntityData GetMapDataEntity(Level level, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, index, false);
    }

    public static EntityData GetMapDataTrigger(Level level, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, index, true);
    }

    public static EntityData GetMapDataEntityData(Level level, int index = 0, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityData(level, (entityData) => true, index, triggers);
    }

    public static EntityData GetMapDataEntity(Level level, string name, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, index, false);
    }

    public static EntityData GetMapDataTrigger(Level level, string name, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, index, true);
    }

    public static EntityData GetMapDataEntityData(Level level, string name, int index = 0, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityData(level, (entityData) => entityData.Name == name, index, triggers);
    }

    public static EntityData GetMapDataEntity(Level level, string name, Func<EntityData, bool> predicate, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, predicate, index, false);
    }

    public static EntityData GetMapDataTrigger(Level level, string name, Func<EntityData, bool> predicate, int index = 0)
    {
        return MapDataUtils.GetMapDataEntityData(level, name, predicate, index, true);
    }

    public static EntityData GetMapDataEntityData(Level level, string name, Func<EntityData, bool> predicate, int index = 0, bool triggers = false)
    {
        return MapDataUtils.GetMapDataEntityData(level, (entityData) => (entityData.Name == name) && (predicate(entityData)), index, triggers);
    }

    public static EntityData GetMapDataEntityData(Level level, Func<EntityData, bool> predicate, int index = 0, bool triggers = false)
    {
        List<EntityData> entityDataList = MapDataUtils.GetMapDataEntityDataList(level, predicate, triggers);
        if (entityDataList.Count == 0)
        {
            return null;
        }
        return entityDataList[(index < 0 || index >= entityDataList.Count) ? (entityDataList.Count - 1) : index];
    }


    /// <summary>
    /// The function checks your loenn inputs, if the input is empty or if other restraints are true, the function will return the default value, otherwise, it'll try and parse the value you entered
    /// </summary>
    /// <param name="loennInput">The parameters you get from Loenn</param>
    /// <param name="defaultValue">The default value you should set up</param>
    /// <param name="otherRestraints">Are there any restraints defined by boolean results</param>
    /// <returns></returns>
    public static int Fetch(this string loennInput, int defaultValue, bool otherRestraints = false)
    {
        if (string.IsNullOrEmpty(loennInput))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        int parseValue = defaultValue;
        int.TryParse(loennInput, out parseValue);
        return parseValue;
    }

    public static int Fetch(this EntityData data, string tag, int defaultValue, bool otherRestraints = false)
    {
        return otherRestraints ? defaultValue : data.Int(tag, defaultValue);
    }

    public static float Fetch(this string loennInput, float defaultValue, bool otherRestraints = false)
    {
        if (string.IsNullOrEmpty(loennInput))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        float parseValue = defaultValue;
        float.TryParse(loennInput, out parseValue);
        return parseValue;
    }

    public static float Fetch(this EntityData data, string tag, float defaultValue, bool otherRestraints = false)
    {
        return otherRestraints ? defaultValue : data.Float(tag, defaultValue);
    }

    public static double Fetch(this string loennInput, double defaultValue, bool otherRestraints = false)
    {
        if (string.IsNullOrEmpty(loennInput))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        double parseValue = defaultValue;
        double.TryParse(loennInput, out parseValue);
        return parseValue;
    }

    public static double Fetch(this EntityData data, string attrTag, double defaultValue, bool otherRestraints = false)
    {
        if (string.IsNullOrEmpty(attrTag))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        double parseValue = defaultValue;
        double.TryParse(data.Attr(attrTag), out parseValue);
        return parseValue;
    }

    public static bool Fetch(this string loennInput, bool defaultValue, bool otherRestraints = false)
    {
        if (string.IsNullOrEmpty(loennInput))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        bool parseValue = defaultValue;
        bool.TryParse(loennInput, out parseValue);
        return parseValue;
    }

    public static bool Fetch(this EntityData data, string tag, bool defaultValue, bool otherRestraints = false)
    {
        return otherRestraints ? defaultValue : data.Bool(tag, defaultValue);
    }

    public static Color Fetch(this string loennInput, Color defaultValue, bool otherRestraints = false)
    {
        if (string.IsNullOrEmpty(loennInput))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        Color parseValue = defaultValue;
        parseValue = Calc.HexToColor(loennInput);
        return parseValue;
    }

    public static Color Fetch(this EntityData data, string attrTag, Color defaultValue, bool otherRestraints = false)
    {
        if (string.IsNullOrEmpty(attrTag))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        Color parseValue = defaultValue;
        parseValue = Calc.HexToColor(data.Attr(attrTag));
        return parseValue;
    }

    public static Color Fetch(this string loennInput, string defaultHex, bool otherRestraints = false)
    {
        Color defaultValue = Calc.HexToColor(defaultHex);

        if (string.IsNullOrEmpty(loennInput))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        Color parseValue = defaultValue;
        parseValue = Calc.HexToColor(loennInput);
        return parseValue;
    }

    public static Color Fetch(this EntityData data, string attrTag, string defaultHex, bool otherRestraints = false)
    {
        Color defaultValue = Calc.HexToColor(defaultHex);

        if (string.IsNullOrEmpty(attrTag))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        Color parseValue = defaultValue;
        parseValue = Calc.HexToColor(data.Attr(attrTag));
        return parseValue;
    }

    public static Classify Fetch<Classify>(this string loennInput, Classify defaultValue, bool otherRestraints = false, bool ignoreCases = false, bool ignoreUnderscores = false) where Classify : struct, Enum
    {
        if (string.IsNullOrEmpty(loennInput))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        return EnumUtils.MatchEnum<Classify>(loennInput, defaultValue, ignoreCases, ignoreUnderscores);
    }

    public static Classify Fetch<Classify>(this EntityData data, string attrTag, Classify defaultValue, bool otherRestraints = false, bool ignoreCases = false, bool ignoreUnderscores = false) where Classify : struct, Enum
    {
        if (string.IsNullOrEmpty(attrTag))
        {
            return defaultValue;
        }

        if (otherRestraints)
        {
            return defaultValue;
        }

        return EnumUtils.MatchEnum<Classify>(data.Attr(attrTag), defaultValue, ignoreCases, ignoreUnderscores);
    }

    /// <summary>
    /// If there were changes to the loenn option names, filter the changes and try get an existing value.
    /// If all data.Attr(tags) are empty, it returns string.Empty
    /// </summary>
    /// <param name="data"></param>
    /// <param name="tags"></param>
    /// <param name="useLastTagAsPossible"></param>
    /// <returns></returns>
    public static string Filter(this EntityData data, string[] tags, bool useLastTagAsPossible = true)
    {
        if (tags == null || tags.Length == 0) { return string.Empty; }

        int min = tags.Length + 1, max = -1;
        for (int i = 0; i < tags.Length; i++)
        {
            if (!string.IsNullOrEmpty(tags[i]))
            {
                min = Math.Min(min, i);
                max = Math.Max(max, i);
            }
        }
        if (min == tags.Length || max == -1) { return string.Empty; }
        return useLastTagAsPossible ? data.Attr(tags[max]) : data.Attr(tags[min]);
    }
    
    public static List<T> List<T>(this EntityData data, string tag, Func<string, T> convert, 
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        string[] s = data.Attr(tag).Split(separator, split);
        List<T> r = new();
        for(int i = 0; i < s.Length; i++)
        {
            r.Add(convert(s[i]));
        }

        return r;
    }

    public static T[] Array<T>(this EntityData data, string tag, Func<string, T> convert,
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        return List(data, tag, convert, separator, split).ToArray();
    }

    public static List<int> IntList(this EntityData data, string tag, int fallback = 0,
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        string[] s = data.Attr(tag).Split(separator, split);
        List<int> r = new();
        for (int i = 0; i < s.Length; i++)
        {
            r.Add(s[i].ParseInt(fallback));
        }

        return r;
    }

    public static int[] IntArray(this EntityData data, string tag, int fallback = 0,
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        return IntList(data, tag, fallback, separator, split).ToArray();
    }

    public static List<float> FloatList(this EntityData data, string tag, float fallback = 0,
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        string[] s = data.Attr(tag).Split(separator, split);
        List<float> r = new();
        for (int i = 0; i < s.Length; i++)
        {
            r.Add(s[i].ParseFloat(fallback));
        }

        return r;
    }

    public static float[] FloatArray(this EntityData data, string tag, float fallback = 0,
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        return FloatList(data, tag, fallback, separator, split).ToArray();
    }

    public static List<double> DoubleList(this EntityData data, string tag, double fallback = 0,
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        string[] s = data.Attr(tag).Split(separator, split);
        List<double> r = new();
        for (int i = 0; i < s.Length; i++)
        {
            r.Add(s[i].ParseDouble(fallback));
        }

        return r;
    }

    public static double[] DoubleArray(this EntityData data, string tag, double fallback = 0,
        char separator = ',', StringSplitOptions split = StringSplitOptions.TrimEntries)
    {
        return DoubleList(data, tag, fallback, separator, split).ToArray();
    }

    public static List<Vc2> Vector2List(this EntityData data, string tag, Vc2 fallback,
        char primarySeparator = ';', char secondarySeparator = ',', 
        StringSplitOptions split1 = StringSplitOptions.TrimEntries, 
        StringSplitOptions split2 = StringSplitOptions.TrimEntries)
    {
        string[] s = data.Attr(tag).Split(primarySeparator, split1);
        List<Vc2> r = new();
        for (int i = 0; i < s.Length; i++)
        {
            string[] cords = s[i].Split(secondarySeparator, split2);
            
            if (cords.Length == 0) { r.Add(Vc2.Zero); continue; }

            Vc2 member = Vc2.Zero;
            member.X = cords[0].ParseFloat(fallback.X);

            if (cords.Length > 1)
            {
                member.Y = cords[1].ParseFloat(fallback.Y);
            }

            r.Add(member);
        }

        return r;
    }

    public static Vc2[] Vector2Array(this EntityData data, string tag, Vc2 fallback,
        char primarySeparator = ';', char secondarySeparator = ',',
        StringSplitOptions split1 = StringSplitOptions.TrimEntries,
        StringSplitOptions split2 = StringSplitOptions.TrimEntries)
    {
        return Vector2List(data, tag, fallback, primarySeparator, secondarySeparator, split1, split2).ToArray();
    }

}

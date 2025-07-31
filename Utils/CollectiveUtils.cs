using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoHelper.Cores;

namespace ChroniaHelper.Utils;

public static class CollectiveUtils
{
    /// <summary>
    /// Check if the input can be found within a given list
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    /// <param name="input"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool IsIn<Type>(this string input, List<Type> list)
    {
        if (string.IsNullOrEmpty(input.ToString())) { return false; }

        foreach (var item in list)
        {
            if (item.ToString() == input) { return true; }
        }

        return false;
    }

    public static bool IsIn<Type>(this List<Type> list, string input)
    {
        if (string.IsNullOrEmpty(input.ToString())) { return false; }

        foreach (var item in list)
        {
            if (item.ToString() == input) { return true; }
        }

        return false;
    }

    public enum CheckMode { CheckKey, CheckValue, Full }

    public static bool IsIn<TypeA, TypeB>(this string input, Dictionary<TypeA, TypeB> dictionary, CheckMode checkmode)
    {
        if (string.IsNullOrEmpty(input)) { return false; }

        if (checkmode == CheckMode.CheckKey || checkmode == CheckMode.Full)
        {
            foreach (var item in dictionary.Keys)
            {
                if (item.ToString() == input) { return true; }
            }
        }

        if (checkmode == CheckMode.CheckValue || checkmode == CheckMode.Full)
        {
            foreach (var item in dictionary.Values)
            {
                if (item.ToString() == input) { return true; }
            }
        }

        return false;
    }

    public static bool IsIn<TypeA, TypeB>(this Dictionary<TypeA, TypeB> dictionary, string input, CheckMode checkmode)
    {
        if (string.IsNullOrEmpty(input)) { return false; }

        if (checkmode == CheckMode.CheckKey || checkmode == CheckMode.Full)
        {
            foreach (var item in dictionary.Keys)
            {
                if (item.ToString() == input) { return true; }
            }
        }

        if (checkmode == CheckMode.CheckValue || checkmode == CheckMode.Full)
        {
            foreach (var item in dictionary.Values)
            {
                if (item.ToString() == input) { return true; }
            }
        }

        return false;
    }

    public enum Input { normal, onlyAdd, onlyModify }
    public static void Enter<TypeA, TypeB>(this Dictionary<TypeA, TypeB> dictionary, TypeA key, TypeB value, Input modify = Input.normal)
    {
        if (dictionary.ContainsKey(key))
        {
            if (modify != Input.onlyAdd)
            {
                dictionary[key] = value;
            }
        }
        else
        {
            if (modify != Input.onlyModify)
            {
                dictionary.Add(key, value);
            }
        }
    }

    public static void SafeRemove<TypeA, TypeB>(this Dictionary<TypeA, TypeB> dictionary, TypeA key)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary.Remove(key);
        }
    }

    public static void SafeRemove<TypeA>(this List<TypeA> list, TypeA key)
    {
        if (list.Contains(key))
        {
            list.Remove(key);
        }
    }

    public static void Replace<TypeA, TypeB>(this Dictionary<TypeA, TypeB> dictionary, TypeA oldKey, TypeA newKey, TypeB newValue)
    {
        dictionary.SafeRemove(oldKey);
        dictionary.Enter(newKey, newValue);
    }

    public static bool ReplaceKey<TypeA, TypeB>(this Dictionary<TypeA, TypeB> dictionary, TypeA oldKey, TypeA newKey)
    {
        if (!dictionary.ContainsKey(oldKey))
        {
            return false;
        }

        TypeB oldValue = dictionary[oldKey];
        dictionary.SafeRemove(oldKey);
        dictionary.Enter(newKey, oldValue);

        return true;
    }

    public static void Enter<Type>(this List<Type> list, Type item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }

    public static bool ContainsKey<TypeA>(this Dictionary<string, TypeA> dic, string key, bool caseSensitive = true)
    {
        if (caseSensitive)
        {
            return dic.ContainsKey(key);
        }
        else
        {
            foreach (var item in dic.Keys)
            {
                if (key.ToLower() == item.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
    }

    // CreateList
    public static T[] CreateArray<T>(this T value, int length)
    {
        var list = new T[int.Max(1, length)];
        for (int i = 0; i < int.Max(1, length); i++)
        {
            list[i] = value;
        }

        return list;
    }

    public static Color[] CreateColorArray(this Color defaultValue, int length)
    {
        return CreateColorArray(length, defaultValue);
    }

    public static Color[] CreateColorArray(int length, Color? defaultValue = null)
    {
        var list = new Color[int.Max(1, length)];
        if (defaultValue.IsNotNull())
        {
            for (int i = 0; i < int.Max(1, length); i++)
            {
                list[i] = (Color)defaultValue;
            }
        }

        return list;
    }

    public static Color[] CreateColorArray(this string defaultColorHex, int length, bool containAlpha = false)
    {
        return CreateColorArray(length,
            containAlpha ? Calc.HexToColorWithAlpha(defaultColorHex) : Calc.HexToColor(defaultColorHex));
    }

    public static int MaxIndex<T>(this T[] array)
    {
        return array.Length - 1;
    }

    public static T SafeGet<T>(this T[] array, int index)
    {
        if (array.Length == 0) { return default(T); }

        return array[index.Clamp(0, array.MaxIndex())];
    }

    public static T[] SafeSet<T>(ref T[] array, int index, T value)
    {
        if(array.Length == 0) { return Array.Empty<T>(); }

        array[int.Clamp(index, 0, array.Length - 1)] = value;

        return array;
    }
}

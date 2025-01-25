using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace ChroniaHelper.Utils;

public static class ObjectUtils
{

    public static string ObjectToString(this object obj)
    {
        if (obj == null)
        {
            return "null";
        }
        Type type = obj.GetType();
        StringBuilder builder = new StringBuilder($"{type.FullName}{{");
        try
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                builder.Append(field.Name).Append("=").Append(field.GetValue(obj)).Append(", ");
            }
            builder.Remove(builder.Length - 2, 2);
        }
        catch (Exception ex)
        {
            builder.Append(" --- Error: ").Append(ex.Message).Append(" --- ");
        }
        return builder.Append("}").ToString();
    }

    public static string ArrayToString(this Array array)
    {
        if (array == null)
        {
            return "null";
        }
        return "[" + string.Join(", ", array.Cast<object>()) + "]";
    }

    public static string ListToString<T>(this IList<T> list)
    {
        if (list == null)
        {
            return "null";
        }
        return "[" + string.Join(", ", list) + "]";
    }

    public static string SetToString<T>(this ISet<T> set)
    {
        if (set == null)
        {
            return "null";
        }
        return "[" + string.Join(", ", set) + "]";
    }

    public static string DictionaryToString(this IDictionary dictionary)
    {
        if (dictionary == null)
        {
            return "null";
        }
        return "{" + string.Join(", ", dictionary.Cast<DictionaryEntry>().Select(entry => $"{entry.Key}={entry.Value}")) + "}";
    }

    public static List<T> DeepCopyList<T>(this List<T> originalList)
    {
        return [.. originalList];
    }

    public static HashSet<T> DeepCopyHashSet<T>(this HashSet<T> originalSet)
    {
        return [.. originalSet];
    }

}

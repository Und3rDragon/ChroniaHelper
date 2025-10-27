using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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

    public static List<string> ArrayToStringList(this Array array)
    {
        List<string> list = new List<string>();

        if (array == null || array.Length == 0)
        {
            return list;
        }

        foreach (var item in array)
        {
            list.Add(item.ToString());
        }

        return list;
    }

    public static string ListToString<T>(this IList<T> list)
    {
        if (list == null)
        {
            return "null";
        }
        return "[" + string.Join(", ", list) + "]";
    }

    public static List<string> ListToStringList<T>(this IList<T> list)
    {
        List<string> s = new List<string>();
        if(list == null || list.Count == 0)
        {
            return s;
        }

        foreach(var item in list)
        {
            s.Add(item.ToString());
        }

        return s;
    }

    public static string SetToString<T>(this ISet<T> set)
    {
        if (set == null)
        {
            return "null";
        }
        return "[" + string.Join(", ", set) + "]";
    }

    public static List<string> SetToStringList<T>(this ISet<T> set)
    {
        List<string> s = new();
        if (set == null || set.Count == 0)
        {
            return s;
        }

        foreach(var item in set)
        {
            s.Add(item.ToString());
        }

        return s;
    }

    public static string DictionaryToString(this IDictionary dictionary)
    {
        if (dictionary == null)
        {
            return "null";
        }
        return "{" + string.Join(", ", dictionary.Cast<DictionaryEntry>().Select(entry => $"{entry.Key}={entry.Value}")) + "}";
    }

    public static List<string> DictionaryToStringList<TypeA, TypeB>(this Dictionary<TypeA, TypeB> dictionary)
    {
        List<string> s = new();
        if (dictionary == null || dictionary.Keys.Count == 0)
        {
            return s;
        }

        foreach (var item in dictionary)
        {
            s.Add($"{item.Key.ToString()}={item.Value.ToString()}");
        }

        return s;
    }

    public static List<T> DeepCopyList<T>(this List<T> originalList)
    {
        return [.. originalList];
    }

    public static HashSet<T> DeepCopyHashSet<T>(this HashSet<T> originalSet)
    {
        return [.. originalSet];
    }

    public static bool IsNull(this object obj)
    {
        return (obj is null);
    }

    public static bool IsNotNull(this object obj)
    {
        return (!ObjectUtils.IsNull(obj));
    }

    public static bool IsValueType(this object obj)
    {
        return obj.GetType().IsValueType;
    }

    public static bool IsReferenceType(this object obj)
    {
        return (!ObjectUtils.IsValueType(obj));
    }
    
    public static void AssignTo<T>(this T source, bool arg, out T ifTrue, out T ifFalse)
    {
        ifTrue = default;
        ifFalse = default;
        if (arg)
        {
            ifTrue = source;
        }
        else
        {
            ifFalse = source;
        }
    }

    public static void AssignTo<T>(this T source, bool primaryArg, bool argIfPrimaryFalse, out T ifPrimaryTrue, out T ifSecondaryTrue, out T ifSecondaryFalse)
    {
        ifPrimaryTrue = default;
        ifSecondaryTrue = default;
        ifSecondaryFalse = default;
        if (primaryArg)
        {
            ifPrimaryTrue = source;
        }
        else if (argIfPrimaryFalse)
        {
            ifSecondaryTrue = source;
        }
        else
        {
            ifSecondaryFalse = source;
        }
    }
    
    public static void DoUnless(this Action action, bool interfere)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        if (!interfere)
        {
            action();
        }
    }

    public static void DoUnless(this Action action, bool interfere, Action doOtherwise)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (doOtherwise == null) throw new ArgumentNullException(nameof(doOtherwise));
        
        if (!interfere)
        {
            action();
        }
        else
        {
            doOtherwise();
        }
    }

    public static void DoUnless<T>(this T source, Action<T> action, bool interfere)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        if (!interfere)
        {
            action(source);
        }
    }

    public static void DoUnless<T>(this T source, Action<T> action, bool interfere, Action<T> doOtherwise)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (doOtherwise == null) throw new ArgumentNullException(nameof(doOtherwise));
        
        if (!interfere)
        {
            action(source);
        }
        else
        {
            doOtherwise(source);
        }
    }
    
    public static T ForceTo<T>(this object obj)
    {
        if (obj == null)
            return default(T)!; // null 转引用类型或可空值类型

        if (obj is T t)
            return t; // 无需转换

        // 使用 Convert 支持跨数值类型、字符串等
        try
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }
        catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
        {
            throw new InvalidCastException(
                $"Unable to convert object of type '{obj.GetType()}' to type '{typeof(T)}'.", ex);
        }
    }
}

using System;
using System.Collections;
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
    public static bool IsIn<Type>(this string input, ICollection<Type> list)
    {
        if (string.IsNullOrEmpty(input.ToString())) { return false; }

        foreach (var item in list)
        {
            if (item.ToString() == input) { return true; }
        }

        return false;
    }

    public static bool IsIn<Type>(this ICollection<Type> list, string input)
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

    public static void Create<A, B>(this Dictionary<A,B> dic, A key, B value)
    {
        if (!dic.ContainsKey(key))
        {
            dic.Add(key, value);
        }
    }

    public static void SafeRemove<TKey, TValue>(
    this ICollection<KeyValuePair<TKey, TValue>> collection,
    TKey key)
    {
        if (collection is IDictionary<TKey, TValue> dict)
        {
            dict.Remove(key);
            return;
        }

        var itemsToRemove = collection
            .Where(kvp => Equals(kvp.Key, key))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            collection.Remove(item);
        }
    }

    public static void SafeRemove<TypeA>(this ICollection<TypeA> list, TypeA key)
    {
        if (list.Contains(key))
        {
            list.Remove(key);
        }
    }

    public static void Replace<T>(this ICollection<T> dic, T find, T replaceWith)
    {
        foreach (var i in dic)
        {
            if (i.Equals(find))
            {
                dic.Remove(i);
                dic.Add(replaceWith);
            }
        }
    }
    public static void Replace<TypeA, TypeB>(this Dictionary<TypeA, TypeB> dictionary, TypeA oldKey, TypeA newKey, TypeB newValue)
    {
        dictionary.SafeRemove(oldKey);
        dictionary.Enter(newKey, newValue);
    }

    public static bool ReplaceKey<TKey, TValue>(
    this ICollection<KeyValuePair<TKey, TValue>> collection,
    TKey oldKey,
    TKey newKey)
    {
        var found = collection
            .Where(kvp => Equals(kvp.Key, oldKey))
            .ToList();

        if (found.Count == 0) return false;

        foreach (var kvp in found)
        {
            collection.Remove(kvp);
        }

        collection.Add(new KeyValuePair<TKey, TValue>(newKey, found[0].Value));
        return true;
    }

    public static void Enter<TKey, TValue>(
    this ICollection<KeyValuePair<TKey, TValue>> collection,
    TKey key,
    TValue value)
    {
        var itemsToRemove = collection
            .Where(kvp => Equals(kvp.Key, key))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            collection.Remove(item);
        }

        collection.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    public static void Enter<Type>(this ICollection<Type> list, params Type[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            if (!list.Contains(items[i]))
            {
                list.Add(items[i]);
            }
        }
    }
    
    public static void Enter<Type, TypeB>(this ICollection<Type> target, ICollection<TypeB> source, Func<TypeB, Type> translator)
    {
        foreach(var item in source)
        {
            target.Enter(translator(item));
        }
    }

    public static bool ContainsKey<T>(
    this Dictionary<string, T> dic,
    string key,
    bool caseSensitive = true)
    {
        if (caseSensitive)
        {
            return dic.ContainsKey(key);
        }
        else
        {
            return dic.Keys
                .Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
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

    public static T[] SetAll<T>(ref T[] array, T value)
    {
        if (array.Length == 0) { return Array.Empty<T>(); }

        for(int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }

        return array;
    }

    /// <summary>
    /// 将当前集合的元素添加到目标集合中（避免重复）
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="target">目标集合</param>
    public static void AddTo<T, Target>(this IEnumerable<T> source, ref Target target)
        where Target : ICollection<T>
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));

        foreach (var item in source)
        {
            if (!target.Contains(item))
            {
                target.Add(item);
            }
        }
    }

    /// <summary>
    /// 将当前集合的元素添加到目标集合中（允许重复）
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="target">目标集合</param>
    public static void AddAllTo<T, Target>(this IEnumerable<T> source, ref Target target)
        where Target : ICollection<T>
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));

        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    /// <summary>
    /// 对集合中的每个元素执行指定操作
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="action">要执行的操作</param>
    public static void EachDo<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// 对集合中的每个元素执行操作并返回结果集合
    /// </summary>
    /// <typeparam name="TSource">源元素类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="selector">转换函数</param>
    /// <returns>转换后的结果集合</returns>
    public static ResultList EachDo<SourceList, ResultList, TSource, TResult>(
        this SourceList source,
        Func<TSource, TResult> selector)
        where ResultList : ICollection<TResult>, new()
        where SourceList : ICollection<TSource>
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var s = source.Select(selector);
        ResultList r = new();
        foreach (var item in s)
        {
            if (!r.Contains(item))
            {
                r.Add(item);
            }
        }
        return r;
    }
    
    /// <summary>
    /// 对集合中的每个元素执行指定操作, 除非干扰条件满足
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <param name="unless"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void EachDoUnless<T>(this IEnumerable<T> source, Action<T> action, bool unless)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in source)
        {
            item.DoUnless(action, unless);
        }
    }

    public static void EachDoUnless<T>(this IEnumerable<T> source, Action<T> action, bool unless, Action<T> doOtherwise)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (doOtherwise == null) throw new ArgumentNullException(nameof(doOtherwise));

        foreach (var item in source)
        {
            item.DoUnless(action, unless, doOtherwise);
        }
    }

    /// <summary>
    /// 将集合中的每个元素通过转换函数映射为新类型的元素，并返回结果集合。
    /// </summary>
    /// <typeparam name="T1">源元素类型</typeparam>
    /// <typeparam name="T2">目标元素类型</typeparam>
    /// <param name="source">源集合，不可为 null</param>
    /// <param name="transform">转换函数，不可为 null</param>
    /// <returns>包含转换后元素的新 List&lt;T2&gt;</returns>
    /// <exception cref="ArgumentNullException">当 source 或 transform 为 null 时抛出</exception>
    public static ICollection<T2> ReplaceCollectionElements<T1, T2>(
        this ICollection<T1> source,
        Func<T1, T2> transform)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (transform == null) throw new ArgumentNullException(nameof(transform));

        var result = new List<T2>(source.Count); // 预分配容量，提升性能
        foreach (var item in source)
        {
            result.Add(transform(item));
        }

        return result;
    }

    /// <summary>
    /// 使用指定的集合类型进行转换。
    /// eg. hashSet = list.ReplaceCollection(x => x * 2, capacity => new HashSet<int>());
    /// </summary>
    public static TCollection ReplaceCollection<T1, T2, TCollection>(
        this ICollection<T1> source,
        Func<T1, T2> transform,
        Func<int, TCollection> collectionFactory)
        where TCollection : ICollection<T2>
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (transform == null) throw new ArgumentNullException(nameof(transform));
        if (collectionFactory == null) throw new ArgumentNullException(nameof(collectionFactory));

        var result = collectionFactory(source.Count);
        foreach (var item in source)
        {
            result.Add(transform(item));
        }

        return result;
    }
    

    /// <summary>
    /// 对集合中的每个元素执行指定操作，并传递索引
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="action">要执行的操作（包含元素和索引）</param>
    public static void EachDoWithIndex<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        int index = 0;
        foreach (var item in source)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// 对集合中的每个元素执行指定操作，并传递索引
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="action">要执行的操作（包含元素、索引、总计数）</param>
    public static void EachDoWithIndexAndLength<T>(this IEnumerable<T> source, Action<T, int, int> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        int index = 0;
        int L = source.Count();
        foreach (var item in source)
        {
            action(item, index++, L);
        }
    }

    /// <summary>
    /// 对集合中的每个元素执行操作并返回结果（带索引）
    /// </summary>
    /// <typeparam name="T">源元素类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="selector">转换函数（包含元素和索引）</param>
    /// <returns>转换后的结果集合</returns>
    public static ResultType EachDoAndGetResult<T, TResult, SourceType, ResultType>(
        this SourceType source,
        Func<T, int, TResult> selector)
        where SourceType : ICollection<T>
        where ResultType : ICollection<TResult>, new()
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var t = source.Select(selector);
        ResultType r = new();
        foreach(var item in t)
        {
            if (!r.Contains(item))
            {
                r.Add(item);
            }
        }
        return r;
    }

    /// <summary>
    /// 对集合执行操作后返回自身，支持链式调用
    /// </summary>
    /// <typeparam name="TSource">集合类型</typeparam>
    /// <typeparam name="TElement">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="action">要执行的操作</param>
    /// <returns>原集合（支持链式调用）</returns>
    public static TSource EachDoAndReturn<TSource, TElement>(
        this TSource source,
        Action<TElement> action)
        where TSource : IEnumerable<TElement>
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in source)
        {
            action(item);
        }

        return source; // 返回自身，支持链式调用
    }

    public static void Compare<T, ListType, UniqueListA, UniqueListB>(this ICollection<T> source,
        ListType target, out UniqueListA uniqueInSource, out UniqueListB uniqueInTarget) 
        where ListType : ICollection<T>, new()
        where UniqueListA : ICollection<T>, new()
        where UniqueListB : ICollection<T>, new()
    {
        ListType s = new();
        foreach(var item in source)
        {
            s.Add(item);
        }
        ListType t = new();
        foreach (var item in target)
        {
            t.Add(item);
        }
        foreach (var item in source)
        {
            if (t.Contains(item))
            {
                t.Remove(item);
            }
        }
        foreach(var item in target)
        {
            if (s.Contains(item))
            {
                s.Remove(item);
            }
        }

        UniqueListA os = new(); UniqueListB ot = new();
        foreach(var item in s)
        {
            if (!os.Contains(item))
            {
                os.Add(item);
            }
        }
        foreach (var item in t)
        {
            if (!ot.Contains(item))
            {
                ot.Add(item);
            }
        }
        
        uniqueInSource = os; uniqueInTarget = ot;
        return;
    }
    
    /// <summary>
    /// 将一个列表初始化后复制成另一个列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Target"></typeparam>
    /// <param name="source">被复制的列表</param>
    /// <param name="target">目标列表（会被清空并初始化）</param>
    public static void ApplyTo<T, Target>(this ICollection<T> source, out Target target)
        where Target : ICollection<T>, new()
    {
        target = new Target();
        source.AddAllTo(ref target);
    }

    public static void ApplyTo<T>(this ICollection<T> source, out T[] target)
    {
        int c = source.Count;
        target = new T[c];
        source.CopyTo(target, 0);
    }
}

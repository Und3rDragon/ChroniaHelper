using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Triggers;
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

    public static void Create<T>(this List<T> list, int at, T key, T filler)
    {
        if (at < 0)
            at = at.GetAbs();

        if (at < list.Count)
            return;

        // 先填充filler直到at位置
        for (int i = list.Count; i < at; i++)
        {
            list.Add(filler);
        }

        // 在at位置添加key
        list.Add(key);
    }

    public static void Enter<T>(this List<T> dic, int at, T key, T filler)
    {
        if(dic.Count() <= at)
        {
            dic.Create(at, key, filler);
        }
        else
        {
            dic[at] = key;
        }
    }

    public static void Create<A, B>(this Dictionary<A,B> dic, A key, B value)
    {
        if (!dic.ContainsKey(key))
        {
            dic.Add(key, value);
        }
    }

    public static void Create<T>(this ICollection<T> list, T value)
    {
        if (!list.Contains(value))
        {
            list.Add(value);
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

    public static TValue SafeGet<TKey, TValue>(
    this ICollection<KeyValuePair<TKey, TValue>> collection,
    TKey key, TValue defaultValue)
    {
        foreach (var item in collection)
        {
            if (item.Key.Equals(key))
            {
                return item.Value;
            }
        }

        return defaultValue;
    }

    public static TypeA SafeGet<TypeA>(this ICollection<TypeA> list, TypeA key, TypeA ifNotExist)
    {
        if (list.Contains(key))
        {
            return key;
        }

        return ifNotExist;
    }

    public static TypeA SafeGet<TypeA>(this IList<TypeA> list, int at, TypeA ifNotExist)
    {
        if (at < list.Count)
        {
            return list[at];
        }

        return ifNotExist;
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
        for (int i = 0; i < items.Length; i++)
        {
            if (!list.Contains(items[i]))
            {
                list.Add(items[i]);
            }
        }
    }

    public static void Enter<Type, TypeB>(this ICollection<Type> target, ICollection<TypeB> source, Func<TypeB, Type> translator)
    {
        foreach (var item in source)
        {
            target.Enter(translator(item));
        }
    }

    public static void Enter<TypeA, MidType, TypeB>(this IDictionary<TypeA, MidType> target, TypeA key, params TypeB[] items)
        where MidType : ICollection<TypeB>, new()
    {
        if (key.IsNull() || items.IsNull() || items.Length == 0) { return; }

        if (target.ContainsKey(key))
        {
            target[key].Enter(items);
        }
        else
        {
            ;
            target.Enter(key, new());
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
        if (array.Length == 0) { return Array.Empty<T>(); }

        array[int.Clamp(index, 0, array.Length - 1)] = value;

        return array;
    }

    public static T[] SetAll<T>(ref T[] array, T value)
    {
        if (array.Length == 0) { return Array.Empty<T>(); }

        for (int i = 0; i < array.Length; i++)
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
    /// 对集合中的每个元素执行指定操作
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="action">要执行的操作</param>
    public static void EachDoWhen<T>(this IEnumerable<T> source, Predicate<T> predicate, Action<T> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in source)
        {
            if (predicate(item))
            {
                action(item);
            }
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
        foreach (var item in t)
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
        foreach (var item in source)
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
        foreach (var item in target)
        {
            if (s.Contains(item))
            {
                s.Remove(item);
            }
        }

        UniqueListA os = new(); UniqueListB ot = new();
        foreach (var item in s)
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

    /// <summary>
    /// 将一个列表初始化后，通过转换函数映射元素，并复制成另一个列表。
    /// </summary>
    /// <typeparam name="T">源集合元素类型</typeparam>
    /// <typeparam name="U">目标集合元素类型</typeparam>
    /// <typeparam name="Target">目标集合类型，必须实现 ICollection<U> 并有无参构造</typeparam>
    /// <param name="source">被复制并转换的源列表</param>
    /// <param name="target">输出的目标列表（会被清空并初始化）</param>
    /// <param name="transmutor">将 T 转换为 U 的函数</param>
    public static void ApplyTo<T, U, Target>(this ICollection<T> source, out Target target, Func<T, U> transmutor)
        where Target : ICollection<U>, new()
    {
        target = new Target();
        foreach (var item in source)
        {
            target.Add(transmutor(item));
        }
    }

    /// <summary>
    /// 将一个列表通过转换函数映射元素，并复制到一个新的数组中。
    /// </summary>
    /// <typeparam name="T">源集合元素类型</typeparam>
    /// <typeparam name="U">目标数组元素类型</typeparam>
    /// <param name="source">被复制并转换的源列表</param>
    /// <param name="target">输出的目标数组</param>
    /// <param name="transmutor">将 T 转换为 U 的函数</param>
    public static void ApplyTo<T, U>(this ICollection<T> source, out U[] target, Func<T, U> transmutor)
    {
        int count = source.Count;
        target = new U[count];
        int index = 0;
        foreach (var item in source)
        {
            target[index++] = transmutor(item);
        }
    }

    /// <summary>
    /// 将字典拆分为键集合和值集合
    /// </summary>
    public static void SplitDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dic,
        out ICollection<TKey> keys,
        out ICollection<TValue> values)
    {
        if (dic == null)
            throw new ArgumentNullException(nameof(dic));

        keys = new List<TKey>(dic.Keys);      // 或 dic.Select(kvp => kvp.Key).ToList()
        values = new List<TValue>(dic.Values); // 或 dic.Select(kvp => kvp.Value).ToList()
    }

    public static bool CombineDictionary<TKey, TValue>(
        this IList<TKey> keys,
        IList<TValue> values,
        out IDictionary<TKey, TValue> dic,
        bool allowDuplicateKeys = false)
    {
        dic = null;

        if (keys == null || values == null || keys.Count != values.Count || keys.Count == 0)
            return false;

        dic = new Dictionary<TKey, TValue>();

        for (int i = 0; i < keys.Count; i++)
        {
            if (dic.ContainsKey(keys[i]))
            {
                if (!allowDuplicateKeys)
                    return false; // 或抛异常
                continue;
            }
            dic[keys[i]] = values[i];
        }

        return true;
    }

    public static bool TryGet<T, Target>(this ICollection<T> source, Predicate<T> condition, out Target result)
        where Target : ICollection<T>, new()
    {
        bool match = false;
        result = new();
        foreach (var item in source)
        {
            if (condition(item))
            {
                result.Add(item);
                match = true;
            }
        }

        return match;
    }

    public static bool TryGetOrGetFirst<T>(this IList<T> source, int index, out T? result)
        where T : struct
    {
        if (source.IsNull())
        {
            result = null;
            return false;
        }
        if (source.Count == 0)
        {
            result = null;
            return false;
        }
        if (index >= source.Count)
        {
            result = source[0];
            return true;
        }

        result = source[index];
        return true;
    }

    public static bool TryGetOrGetLast<T>(this IList<T> source, int index, out T? result)
        where T : struct
    {
        if (source.IsNull())
        {
            result = null;
            return false;
        }
        if (source.Count == 0)
        {
            result = null;
            return false;
        }
        if (index >= source.Count)
        {
            result = source[source.Count - 1];
            return true;
        }

        result = source[index];
        return true;
    }


    public static bool TryGet<T>(this IList<T> source, int index, out T? result)
        where T : struct
    {
        if (source.IsNull())
        {
            result = null;
            return false;
        }
        if (source.Count == 0)
        {
            result = null;
            return false;
        }
        if (index >= source.Count)
        {
            result = null;
            return false;
        }

        result = source[index];
        return true;
    }

    public static T TryGet<T>(this IList<T> source, int index, T fallback)
    {
        if (source.IsNull())
        {
            return fallback;
        }
        if (source.Count == 0)
        {
            return fallback;
        }
        if (index >= source.Count)
        {
            return fallback;
        }

        return source[index];
    }

    /// <summary>
    /// 深度比较两个对象是否内容相等，支持嵌套 List 和 Dictionary。
    /// </summary>
    public static bool DeepEquals(object x, object y)
    {
        // 引用相等或同时为 null
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        var typeX = x.GetType();
        var typeY = y.GetType();

        // 类型不同直接返回 false
        if (typeX != typeY) return false;

        // 基本类型、string 等直接比较
        if (IsSimpleType(typeX))
            return x.Equals(y);

        // 处理数组（可选）
        if (x is Array ax && y is Array ay)
            return ArraysEqual(ax, ay);

        // 处理 IList（包括 List<T>）
        if (x is IList listX && y is IList listY)
            return ListsEqual(listX, listY);

        // 处理 IDictionary（包括 Dictionary<K,V>）
        if (x is IDictionary dictX && y is IDictionary dictY)
            return DictionariesEqual(dictX, dictY);

        // 其他引用类型：回退到 Equals（或你可抛异常）
        return x.Equals(y);
    }

    private static bool ArraysEqual(Array a, Array b)
    {
        if (a.Length != b.Length) return false;
        var len = a.Length;
        for (int i = 0; i < len; i++)
        {
            if (!Equals(a.GetValue(i), b.GetValue(i)))
                return false;
        }
        return true;
    }

    private static bool ListsEqual(IList a, IList b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (!Equals(a[i], b[i]))
                return false;
        }
        return true;
    }

    private static bool DictionariesEqual(IDictionary a, IDictionary b)
    {
        if (a.Count != b.Count) return false;

        foreach (DictionaryEntry entry in a)
        {
            object key = entry.Key;
            if (!b.Contains(key))
                return false;

            object valueA = entry.Value;
            object valueB = b[key];

            if (!Equals(valueA, valueB))
                return false;
        }
        return true;
    }

    // 用于防止循环引用
    private static readonly ConditionalWeakTable<object, object> _visited = new();

    /// <summary>
    /// 深拷贝一个对象，支持嵌套 List 和 Dictionary。
    /// 注意：资源类型（如 MTexture）仅复制引用。
    /// </summary>
    public static object DeepCopy(object obj)
    {
        if (obj == null) return null;

        var type = obj.GetType();

        // 处理简单类型（值类型 + string）
        if (IsSimpleType(type))
            return obj;

        // 检查是否已拷贝（防循环引用）
        if (_visited.TryGetValue(obj, out object existing))
            return existing;

        object copy;

        // 数组
        if (obj is Array arr)
        {
            var copiedArr = Array.CreateInstance(arr.GetType().GetElementType(), arr.Length);
            _visited.Add(obj, copiedArr);
            for (int i = 0; i < arr.Length; i++)
                copiedArr.SetValue(DeepCopy(arr.GetValue(i)), i);
            copy = copiedArr;
        }
        // IList（如 List<T>）
        else if (obj is IList list)
        {
            var elementType = GetListElementType(type) ?? typeof(object);
            var newList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            _visited.Add(obj, newList);
            foreach (var item in list)
                newList.Add(DeepCopy(item));
            copy = newList;
        }
        // IDictionary（如 Dictionary<K,V>）
        else if (obj is IDictionary dict)
        {
            var kv = GetDictionaryKeyValueType(type);
            if (kv != null)
            {
                var newDict = (IDictionary)Activator.CreateInstance(
                    typeof(Dictionary<,>).MakeGenericType(kv.Value.Item1, kv.Value.Item2));
                _visited.Add(obj, newDict);
                foreach (DictionaryEntry entry in dict)
                {
                    var key = DeepCopy(entry.Key);
                    var value = DeepCopy(entry.Value);
                    newDict.Add(key, value);
                }
                copy = newDict;
            }
            else
            {
                // 非泛型字典，回退
                var newDict = new Hashtable();
                _visited.Add(obj, newDict);
                foreach (DictionaryEntry entry in dict)
                    newDict.Add(DeepCopy(entry.Key), DeepCopy(entry.Value));
                copy = newDict;
            }
        }
        else
        {
            // 其他引用类型：**不深拷！只返回原引用**
            // （适用于 MTexture, Color, Vector2 等资源或结构体包装类）
            copy = obj;
        }

        return copy;
    }

    // 清除循环引用缓存（每次完整拷贝后调用）
    public static T DeepCopy<T>(T obj)
    {
        try
        {
            return (T)DeepCopy((object)obj);
        }
        finally
        {
            _visited.Clear(); // 防止跨调用污染
        }
    }

    private static bool IsSimpleType(Type t)
    {
        return t.IsPrimitive ||
               t == typeof(string) ||
               t == typeof(decimal) ||
               t.IsEnum ||
               // Celeste 常见结构体（不可变或值语义）
               t == typeof(Microsoft.Xna.Framework.Color) ||
               t == typeof(Microsoft.Xna.Framework.Vector2) ||
               t == typeof(Microsoft.Xna.Framework.Rectangle);
    }

    // 获取 List<T> 的 T
    private static Type GetListElementType(Type listType)
    {
        if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
            return listType.GetGenericArguments()[0];

        // 支持 IList<T>
        foreach (var iface in listType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IList<>))
                return iface.GetGenericArguments()[0];
        }
        return null;
    }

    // 获取 Dictionary<K,V> 的 (K, V)
    private static (Type, Type)? GetDictionaryKeyValueType(Type dictType)
    {
        if (dictType.IsGenericType)
        {
            var def = dictType.GetGenericTypeDefinition();
            if (def == typeof(Dictionary<,>) || def == typeof(IDictionary<,>))
            {
                var args = dictType.GetGenericArguments();
                return (args[0], args[1]);
            }
        }

        // 接口支持
        foreach (var iface in dictType.GetInterfaces())
        {
            if (iface.IsGenericType)
            {
                var def = iface.GetGenericTypeDefinition();
                if (def == typeof(IDictionary<,>))
                {
                    var args = iface.GetGenericArguments();
                    return (args[0], args[1]);
                }
            }
        }
        return null;
    }

    public static void As<T1, T2, T3>(this IList<T1> orig, out T3 list, Func<T1, T2> convert)
        where T3 : IList<T2>, new()
    {
        list = new();
        for (int i = 0; i < orig.Count; i++)
        {
            list.Add(convert(orig[i]));
        }
    }
}

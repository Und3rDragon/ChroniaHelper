using System;
using System.Collections.Generic;
using System.Linq;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Utils;

public static class GroupingUtils
{
    /// <summary>
    /// 默认的标签生成器 - 使用分组序号作为标签
    /// </summary>
    /// <typeparam name="T">元素类型（未使用，仅用于匹配委托签名）</typeparam>
    /// <param name="index">分组序号</param>
    /// <param name="item">组内第一个元素</param>
    /// <returns>分组序号</returns>
    public static int DefaultTagging<T>(int index, T item)
    {
        return index;
    }

    /// <summary>
    /// 默认的预处理函数 - 直接返回原元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="item">输入元素</param>
    /// <returns>原元素</returns>
    public static T DefaultPreGrouping<T>(T item)
    {
        return item;
    }

    /// <summary>
    /// 默认的数据保存函数 - 直接返回原元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="item">输入元素</param>
    /// <returns>原元素</returns>
    public static T DefaultSaving<T>(T item)
    {
        return item;
    }

    #region GroupByCondition

    /// <summary>
    /// 基于条件的一对多分组，元素不可重复
    /// </summary>
    /// <typeparam name="T">原始元素类型</typeparam>
    /// <typeparam name="T2">预处理后元素类型</typeparam>
    /// <typeparam name="T3">保存后元素类型</typeparam>
    /// <typeparam name="KeyType">分组标签类型</typeparam>
    /// <param name="source">源列表</param>
    /// <param name="preGrouping">预处理函数（元素分组前进行转换）。默认请使用 DefaultPreGrouping</param>
    /// <param name="condition">分组条件（判断元素是否应该进入某个组）</param>
    /// <param name="saving">数据保存函数（分组后保存数据）。默认请使用 DefaultSaving</param>
    /// <param name="tagging">标签生成器（参数：分组序号, 组内第一个保存后的元素）。默认请使用 DefaultTagging</param>
    /// <returns>分组结果字典</returns>
    public static Dictionary<KeyType, List<T3>> GroupByCondition<T, T2, T3, KeyType>(
        this List<T> source,
        Func<T, T2> preGrouping,
        Predicate<T2> condition,
        Func<T2, T3> saving,
        Func<int, T3, KeyType> tagging)
    {
        if (source == null)
        {
            Log.Error("[GroupingUtils] GroupByCondition: source 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (preGrouping == null)
        {
            Log.Error("[GroupingUtils] GroupByCondition: preGrouping 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (condition == null)
        {
            Log.Error("[GroupingUtils] GroupByCondition: condition 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (saving == null)
        {
            Log.Error("[GroupingUtils] GroupByCondition: saving 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (tagging == null)
        {
            Log.Error("[GroupingUtils] GroupByCondition: tagging 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }

        var result = new Dictionary<KeyType, List<T3>>();
        var used = new HashSet<T>();
        var processedItems = new Dictionary<T, T2>();
        int groupIndex = 0;

        foreach (var item in source)
        {
            if (used.Contains(item)) continue;

            T2 processed;
            if (!processedItems.TryGetValue(item, out processed))
            {
                processed = preGrouping(item);
                processedItems[item] = processed;
            }

            if (!condition(processed)) continue;

            T3 savedItem = saving(processed);
            var group = new List<T3> { savedItem };
            used.Add(item);

            foreach (var other in source)
            {
                if (used.Contains(other)) continue;

                T2 otherProcessed;
                if (!processedItems.TryGetValue(other, out otherProcessed))
                {
                    otherProcessed = preGrouping(other);
                    processedItems[other] = otherProcessed;
                }

                if (condition(otherProcessed))
                {
                    group.Add(saving(otherProcessed));
                    used.Add(other);
                }
            }

            KeyType key = tagging(groupIndex, savedItem);
            result.Enter(key, group);
            groupIndex++;
        }

        return result;
    }

    #endregion

    #region GroupByConditionAllowDuplicate

    /// <summary>
    /// 基于条件的一对多分组，元素可重复
    /// </summary>
    /// <typeparam name="T">原始元素类型</typeparam>
    /// <typeparam name="T2">预处理后元素类型</typeparam>
    /// <typeparam name="T3">保存后元素类型</typeparam>
    /// <typeparam name="KeyType">分组标签类型</typeparam>
    /// <param name="source">源列表</param>
    /// <param name="preGrouping">预处理函数（元素分组前进行转换）。默认请使用 DefaultPreGrouping</param>
    /// <param name="condition">分组条件（判断元素是否应该进入某个组）</param>
    /// <param name="saving">数据保存函数（分组后保存数据）。默认请使用 DefaultSaving</param>
    /// <param name="tagging">标签生成器（参数：分组序号, 组内第一个保存后的元素）。默认请使用 DefaultTagging</param>
    /// <returns>分组结果字典</returns>
    public static Dictionary<KeyType, List<T3>> GroupByConditionAllowDuplicate<T, T2, T3, KeyType>(
        this List<T> source,
        Func<T, T2> preGrouping,
        Predicate<T2> condition,
        Func<T2, T3> saving,
        Func<int, T3, KeyType> tagging)
    {
        if (source == null)
        {
            Log.Error("[GroupingUtils] GroupByConditionAllowDuplicate: source 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (preGrouping == null)
        {
            Log.Error("[GroupingUtils] GroupByConditionAllowDuplicate: preGrouping 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (condition == null)
        {
            Log.Error("[GroupingUtils] GroupByConditionAllowDuplicate: condition 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (saving == null)
        {
            Log.Error("[GroupingUtils] GroupByConditionAllowDuplicate: saving 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (tagging == null)
        {
            Log.Error("[GroupingUtils] GroupByConditionAllowDuplicate: tagging 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }

        var result = new Dictionary<KeyType, List<T3>>();
        var usedSets = new List<HashSet<T3>>();
        var processedItems = new Dictionary<T, T2>();
        int groupIndex = 0;

        foreach (var item in source)
        {
            T2 processed;
            if (!processedItems.TryGetValue(item, out processed))
            {
                processed = preGrouping(item);
                processedItems[item] = processed;
            }

            if (!condition(processed)) continue;

            T3 savedItem = saving(processed);
            var group = new List<T3> { savedItem };
            var groupSet = new HashSet<T3> { savedItem };

            foreach (var other in source)
            {
                if (other.Equals(item)) continue;

                T2 otherProcessed;
                if (!processedItems.TryGetValue(other, out otherProcessed))
                {
                    otherProcessed = preGrouping(other);
                    processedItems[other] = otherProcessed;
                }

                if (condition(otherProcessed))
                {
                    T3 otherSaved = saving(otherProcessed);
                    group.Add(otherSaved);
                    groupSet.Add(otherSaved);
                }
            }

            // 检查是否已存在完全相同的分组
            bool isDuplicate = false;
            foreach (var existingSet in usedSets)
            {
                if (existingSet.SetEquals(groupSet))
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                usedSets.Add(groupSet);
                KeyType key = tagging(groupIndex, savedItem);
                result.Enter(key, group);
                groupIndex++;
            }
        }

        return result;
    }

    #endregion

    #region GroupByInteraction

    /// <summary>
    /// 基于元素间交互的分组，元素不可重复
    /// </summary>
    /// <typeparam name="T">原始元素类型</typeparam>
    /// <typeparam name="T2">预处理后元素类型</typeparam>
    /// <typeparam name="T3">保存后元素类型</typeparam>
    /// <typeparam name="KeyType">分组标签类型</typeparam>
    /// <param name="source">源列表</param>
    /// <param name="preGrouping">预处理函数（元素分组前进行转换）。默认请使用 DefaultPreGrouping</param>
    /// <param name="condition">交互条件（判断两个预处理后的元素是否应该在同一组）</param>
    /// <param name="saving">数据保存函数（分组后保存数据）。默认请使用 DefaultSaving</param>
    /// <param name="tagging">标签生成器（参数：分组序号, 组内第一个保存后的元素）。默认请使用 DefaultTagging</param>
    /// <returns>分组结果字典</returns>
    public static Dictionary<KeyType, List<T3>> GroupByInteraction<T, T2, T3, KeyType>(
        this List<T> source,
        Func<T, T2> preGrouping,
        Func<T2, T2, bool> condition,
        Func<T2, T3> saving,
        Func<int, T3, KeyType> tagging)
    {
        if (source == null)
        {
            Log.Error("[GroupingUtils] GroupByInteraction: source 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (preGrouping == null)
        {
            Log.Error("[GroupingUtils] GroupByInteraction: preGrouping 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (condition == null)
        {
            Log.Error("[GroupingUtils] GroupByInteraction: condition 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (saving == null)
        {
            Log.Error("[GroupingUtils] GroupByInteraction: saving 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (tagging == null)
        {
            Log.Error("[GroupingUtils] GroupByInteraction: tagging 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }

        var result = new Dictionary<KeyType, List<T3>>();
        var used = new HashSet<T>();
        var processedItems = new Dictionary<T, T2>();
        int groupIndex = 0;

        foreach (var item in source)
        {
            if (used.Contains(item)) continue;

            T2 processed;
            if (!processedItems.TryGetValue(item, out processed))
            {
                processed = preGrouping(item);
                processedItems[item] = processed;
            }

            T3 savedItem = saving(processed);
            var group = new List<T3> { savedItem };
            var groupRaw = new List<T> { item };
            used.Add(item);

            bool addedNew;
            do
            {
                addedNew = false;
                foreach (var other in source)
                {
                    if (used.Contains(other)) continue;

                    T2 otherProcessed;
                    if (!processedItems.TryGetValue(other, out otherProcessed))
                    {
                        otherProcessed = preGrouping(other);
                        processedItems[other] = otherProcessed;
                    }

                    bool shouldAdd = false;
                    foreach (var groupItem in groupRaw)
                    {
                        T2 groupProcessed = processedItems[groupItem];
                        if (condition(groupProcessed, otherProcessed) || condition(otherProcessed, groupProcessed))
                        {
                            shouldAdd = true;
                            break;
                        }
                    }

                    if (shouldAdd)
                    {
                        group.Add(saving(otherProcessed));
                        groupRaw.Add(other);
                        used.Add(other);
                        addedNew = true;
                    }
                }
            } while (addedNew);

            KeyType key = tagging(groupIndex, savedItem);
            result.Enter(key, group);
            groupIndex++;
        }

        return result;
    }

    #endregion

    #region GroupByInteractionAllowDuplicate

    /// <summary>
    /// 基于元素间交互的分组，元素可重复
    /// </summary>
    /// <typeparam name="T">原始元素类型</typeparam>
    /// <typeparam name="T2">预处理后元素类型</typeparam>
    /// <typeparam name="T3">保存后元素类型</typeparam>
    /// <typeparam name="KeyType">分组标签类型</typeparam>
    /// <param name="source">源列表</param>
    /// <param name="preGrouping">预处理函数（元素分组前进行转换）。默认请使用 DefaultPreGrouping</param>
    /// <param name="condition">交互条件（判断两个预处理后的元素是否应该在同一组）</param>
    /// <param name="saving">数据保存函数（分组后保存数据）。默认请使用 DefaultSaving</param>
    /// <param name="tagging">标签生成器（参数：分组序号, 组内第一个保存后的元素）。默认请使用 DefaultTagging</param>
    /// <returns>分组结果字典</returns>
    public static Dictionary<KeyType, List<T3>> GroupByInteractionAllowDuplicate<T, T2, T3, KeyType>(
        this List<T> source,
        Func<T, T2> preGrouping,
        Func<T2, T2, bool> condition,
        Func<T2, T3> saving,
        Func<int, T3, KeyType> tagging)
    {
        if (source == null)
        {
            Log.Error("[GroupingUtils] GroupByInteractionAllowDuplicate: source 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (preGrouping == null)
        {
            Log.Error("[GroupingUtils] GroupByInteractionAllowDuplicate: preGrouping 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (condition == null)
        {
            Log.Error("[GroupingUtils] GroupByInteractionAllowDuplicate: condition 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (saving == null)
        {
            Log.Error("[GroupingUtils] GroupByInteractionAllowDuplicate: saving 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }
        if (tagging == null)
        {
            Log.Error("[GroupingUtils] GroupByInteractionAllowDuplicate: tagging 为 null");
            return new Dictionary<KeyType, List<T3>>();
        }

        var result = new Dictionary<KeyType, List<T3>>();
        var usedSets = new List<HashSet<T3>>();
        var processedItems = new Dictionary<T, T2>();
        int groupIndex = 0;

        foreach (var item in source)
        {
            T2 processed;
            if (!processedItems.TryGetValue(item, out processed))
            {
                processed = preGrouping(item);
                processedItems[item] = processed;
            }

            T3 savedItem = saving(processed);
            var group = new List<T3> { savedItem };
            var groupSet = new HashSet<T3> { savedItem };
            var groupRaw = new List<T> { item };

            bool addedNew;
            do
            {
                addedNew = false;
                foreach (var other in source)
                {
                    if (groupRaw.Contains(other)) continue;

                    T2 otherProcessed;
                    if (!processedItems.TryGetValue(other, out otherProcessed))
                    {
                        otherProcessed = preGrouping(other);
                        processedItems[other] = otherProcessed;
                    }

                    bool shouldAdd = false;
                    foreach (var groupItem in groupRaw)
                    {
                        T2 groupProcessed = processedItems[groupItem];
                        if (condition(groupProcessed, otherProcessed) || condition(otherProcessed, groupProcessed))
                        {
                            shouldAdd = true;
                            break;
                        }
                    }

                    if (shouldAdd)
                    {
                        T3 otherSaved = saving(otherProcessed);
                        group.Add(otherSaved);
                        groupSet.Add(otherSaved);
                        groupRaw.Add(other);
                        addedNew = true;
                    }
                }
            } while (addedNew);

            // 检查是否已存在完全相同的分组
            bool isDuplicate = false;
            foreach (var existingSet in usedSets)
            {
                if (existingSet.SetEquals(groupSet))
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                usedSets.Add(groupSet);
                KeyType key = tagging(groupIndex, savedItem);
                result.Enter(key, group);
                groupIndex++;
            }
        }

        return result;
    }

    #endregion

    // ============ 辅助方法 ============

    /// <summary>
    /// 分组结果去重辅助方法 - 使用 SetEquals 检测重复分组
    /// </summary>
    public static List<List<T>> DeduplicateGroups<T>(List<List<T>> groups)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] DeduplicateGroups: groups 为 null");
            return new List<List<T>>();
        }
        if (groups.Count <= 1) return groups;

        var result = new List<List<T>>();
        var usedSets = new List<HashSet<T>>();

        foreach (var group in groups)
        {
            var groupSet = new HashSet<T>(group);
            bool isDuplicate = false;

            foreach (var existingSet in usedSets)
            {
                if (existingSet.SetEquals(groupSet))
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                usedSets.Add(groupSet);
                result.Add(group);
            }
        }

        return result;
    }

    /// <summary>
    /// 将分组结果字典的Key重新映射为自然数序列
    /// </summary>
    public static Dictionary<int, List<T>> RenumberGroups<TKey, T>(Dictionary<TKey, List<T>> groups)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] RenumberGroups: groups 为 null");
            return new Dictionary<int, List<T>>();
        }

        var result = new Dictionary<int, List<T>>();
        int newId = 0;

        foreach (var kvp in groups)
        {
            result.Enter(newId, kvp.Value);
            newId++;
        }

        return result;
    }

    /// <summary>
    /// 合并多个分组结果
    /// </summary>
    public static Dictionary<int, List<T>> MergeGroupResults<T>(params Dictionary<int, List<T>>[] groupsList)
    {
        if (groupsList == null || groupsList.Length == 0)
        {
            if (groupsList == null)
                Log.Error("[GroupingUtils] MergeGroupResults: groupsList 为 null");
            return new Dictionary<int, List<T>>();
        }

        var allGroups = new List<List<T>>();

        foreach (var dict in groupsList)
        {
            if (dict == null)
            {
                Log.Error("[GroupingUtils] MergeGroupResults: 字典为 null，已跳过");
                continue;
            }

            foreach (var kvp in dict)
            {
                allGroups.Add(kvp.Value);
            }
        }

        var deduplicated = DeduplicateGroups(allGroups);
        var result = new Dictionary<int, List<T>>();

        for (int i = 0; i < deduplicated.Count; i++)
        {
            result.Enter(i, deduplicated[i]);
        }

        return result;
    }

    /// <summary>
    /// 对分组结果按组大小排序（降序）
    /// </summary>
    public static Dictionary<TKey, List<T>> SortGroupsBySizeDesc<TKey, T>(Dictionary<TKey, List<T>> groups)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] SortGroupsBySizeDesc: groups 为 null");
            return new Dictionary<TKey, List<T>>();
        }

        return groups
            .OrderByDescending(kvp => kvp.Value.Count)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 对分组结果按组大小排序（升序）
    /// </summary>
    public static Dictionary<TKey, List<T>> SortGroupsBySizeAsc<TKey, T>(Dictionary<TKey, List<T>> groups)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] SortGroupsBySizeAsc: groups 为 null");
            return new Dictionary<TKey, List<T>>();
        }

        return groups
            .OrderBy(kvp => kvp.Value.Count)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 过滤分组结果，只保留满足条件的组
    /// </summary>
    public static Dictionary<TKey, List<T>> FilterGroups<TKey, T>(
        Dictionary<TKey, List<T>> groups,
        Predicate<List<T>> groupCondition)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] FilterGroups: groups 为 null");
            return new Dictionary<TKey, List<T>>();
        }
        if (groupCondition == null)
        {
            Log.Error("[GroupingUtils] FilterGroups: groupCondition 为 null");
            return new Dictionary<TKey, List<T>>();
        }

        var result = new Dictionary<TKey, List<T>>();

        foreach (var kvp in groups)
        {
            if (groupCondition(kvp.Value))
            {
                result.Enter(kvp.Key, kvp.Value);
            }
        }

        return result;
    }

    /// <summary>
    /// 将分组字典转换为只包含组的列表
    /// </summary>
    public static List<List<T>> ToGroupList<TKey, T>(Dictionary<TKey, List<T>> groups)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] ToGroupList: groups 为 null");
            return new List<List<T>>();
        }

        return groups.Values.ToList();
    }

    /// <summary>
    /// 从分组列表中提取所有元素（去重）
    /// </summary>
    public static List<T> ExtractAllElements<T>(List<List<T>> groups)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] ExtractAllElements: groups 为 null");
            return new List<T>();
        }

        var result = new HashSet<T>();
        foreach (var group in groups)
        {
            foreach (var item in group)
            {
                result.Add(item);
            }
        }
        return result.ToList();
    }

    /// <summary>
    /// 从分组字典中提取所有元素（去重）
    /// </summary>
    public static List<T> ExtractAllElements<TKey, T>(Dictionary<TKey, List<T>> groups)
    {
        if (groups == null)
        {
            Log.Error("[GroupingUtils] ExtractAllElements: groups 为 null");
            return new List<T>();
        }

        return ExtractAllElements(groups.Values.ToList());
    }
}
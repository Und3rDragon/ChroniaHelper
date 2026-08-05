using ChroniaHelper.Components.SwimmingFish;
using ChroniaHelper.Cores;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChroniaHelper.Components.SwimmingFish;

public class FishMotionManager : BaseComponent
{
    // 存储所有注册的FishMotion，按GroupTag分组
    private Dictionary<int, List<FishMotion>> groupedFish = new Dictionary<int, List<FishMotion>>();

    // 存储所有FishMotion的扁平列表（GroupTag < 0 的未分组鱼）
    private List<FishMotion> ungroupedFish = new List<FishMotion>();

    // 用于快速查找
    private Dictionary<FishMotion, int> fishToGroupMap = new Dictionary<FishMotion, int>();

    public FishMotionManager()
        : base(active: true, visible: false)
    {
    }

    /// <summary>
    /// 注册一个FishMotion到管理器中
    /// </summary>
    public void Register(FishMotion fish)
    {
        if (fish == null || fishToGroupMap.ContainsKey(fish))
            return;

        int groupTag = fish.GroupTag;
        fishToGroupMap[fish] = groupTag;

        if (groupTag < 0)
        {
            ungroupedFish.Add(fish);
        }
        else
        {
            if (!groupedFish.ContainsKey(groupTag))
            {
                groupedFish[groupTag] = new List<FishMotion>();
            }
            groupedFish[groupTag].Add(fish);
        }
    }

    /// <summary>
    /// 注销一个FishMotion
    /// </summary>
    public void Unregister(FishMotion fish)
    {
        if (fish == null || !fishToGroupMap.ContainsKey(fish))
            return;

        int groupTag = fishToGroupMap[fish];
        fishToGroupMap.Remove(fish);

        if (groupTag < 0)
        {
            ungroupedFish.Remove(fish);
        }
        else if (groupedFish.ContainsKey(groupTag))
        {
            groupedFish[groupTag].Remove(fish);
            if (groupedFish[groupTag].Count == 0)
            {
                groupedFish.Remove(groupTag);
            }
        }
    }

    /// <summary>
    /// 更新FishMotion的分组标签
    /// </summary>
    public void UpdateGroupTag(FishMotion fish, int newGroupTag)
    {
        if (fish == null || !fishToGroupMap.ContainsKey(fish))
            return;

        int oldGroupTag = fishToGroupMap[fish];
        if (oldGroupTag == newGroupTag)
            return;

        // 从旧组中移除
        if (oldGroupTag < 0)
        {
            ungroupedFish.Remove(fish);
        }
        else if (groupedFish.ContainsKey(oldGroupTag))
        {
            groupedFish[oldGroupTag].Remove(fish);
            if (groupedFish[oldGroupTag].Count == 0)
            {
                groupedFish.Remove(oldGroupTag);
            }
        }

        // 添加到新组
        fishToGroupMap[fish] = newGroupTag;
        if (newGroupTag < 0)
        {
            ungroupedFish.Add(fish);
        }
        else
        {
            if (!groupedFish.ContainsKey(newGroupTag))
            {
                groupedFish[newGroupTag] = new List<FishMotion>();
            }
            groupedFish[newGroupTag].Add(fish);
        }
    }

    /// <summary>
    /// 获取指定组的所有FishMotion
    /// </summary>
    public List<FishMotion> GetFishInGroup(int groupTag)
    {
        if (groupTag < 0)
        {
            return new List<FishMotion>(ungroupedFish);
        }

        if (groupedFish.ContainsKey(groupTag))
        {
            return new List<FishMotion>(groupedFish[groupTag]);
        }

        return new List<FishMotion>();
    }

    /// <summary>
    /// 获取所有已注册的FishMotion
    /// </summary>
    public List<FishMotion> GetAllFish()
    {
        var allFish = new List<FishMotion>();
        allFish.AddRange(ungroupedFish);
        foreach (var group in groupedFish.Values)
        {
            allFish.AddRange(group);
        }
        return allFish;
    }

    /// <summary>
    /// 获取指定FishMotion所在组的其他成员（不包括自己）
    /// </summary>
    public List<FishMotion> GetGroupMembers(FishMotion fish)
    {
        if (fish == null || !fishToGroupMap.ContainsKey(fish))
            return new List<FishMotion>();

        int groupTag = fishToGroupMap[fish];
        var allInGroup = GetFishInGroup(groupTag);
        allInGroup.Remove(fish);
        return allInGroup;
    }

    /// <summary>
    /// 获取所有有效的分组标签
    /// </summary>
    public List<int> GetAllGroupTags()
    {
        return groupedFish.Keys.ToList();
    }

    /// <summary>
    /// 获取某个分组的鱼的数量
    /// </summary>
    public int GetGroupCount(int groupTag)
    {
        if (groupTag < 0)
            return ungroupedFish.Count;

        return groupedFish.ContainsKey(groupTag) ? groupedFish[groupTag].Count : 0;
    }

    /// <summary>
    /// 清空所有注册的FishMotion
    /// </summary>
    public void Clear()
    {
        groupedFish.Clear();
        ungroupedFish.Clear();
        fishToGroupMap.Clear();
    }

    /// <summary>
    /// 清理无效的FishMotion（Valid = false的实例）
    /// </summary>
    public void CleanupInvalid()
    {
        var invalidFish = new List<FishMotion>();

        foreach (var fish in fishToGroupMap.Keys)
        {
            if (!fish.Valid)
            {
                invalidFish.Add(fish);
            }
        }

        foreach (var fish in invalidFish)
        {
            Unregister(fish);
        }
    }
}
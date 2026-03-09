using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

using System;
using System.Collections.Generic;

/// <summary>
/// 基于 ID 的 Action 事件管理器
/// 特点：防止重复叠加、支持动态更新逻辑、异常安全
/// </summary>
public class ActionManager
{
    // 核心存储：ID -> Action
    private readonly Dictionary<string, Action> _events = new();

    /// <summary>
    /// 注册或更新一个事件。
    /// 如果 ID 已存在，旧的 Action 会被新的替换（防止 Update 中无限叠加）。
    /// 如果 action 为 null，则视为移除该 ID 的事件。
    /// </summary>
    /// <param name="id">事件的唯一标识符</param>
    /// <param name="action">要执行的动作</param>
    public void Register(string id, Action action)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Event ID cannot be null or empty.", nameof(id));
        }

        if (action == null)
        {
            // 如果传入 null，视为显式移除
            _events.Remove(id);
            return;
        }

        // 直接赋值：
        // 1. 如果 ID 不存在 -> 新增
        // 2. 如果 ID 存在 -> 覆盖旧引用 (完美解决 Update 中重复 += 的问题)
        _events[id] = action;
    }

    /// <summary>
    /// 仅当 ID 不存在时才注册 (保守策略)。
    /// 如果 ID 已存在，则忽略本次注册。
    /// </summary>
    public void RegisterIfNotExists(string id, Action action)
    {
        if (string.IsNullOrEmpty(id) || action == null) return;

        if (!_events.ContainsKey(id))
        {
            _events[id] = action;
        }
    }

    /// <summary>
    /// 移除指定 ID 的事件
    /// </summary>
    public void Unregister(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            _events.Remove(id);
        }
    }

    /// <summary>
    /// 移除所有事件
    /// </summary>
    public void Clear()
    {
        _events.Clear();
    }

    /// <summary>
    /// 触发指定 ID 的事件
    /// </summary>
    /// <param name="id">事件 ID</param>
    /// <param name="ignoreErrors">如果为 true，单个 Action 报错不会影响其他 Action (推荐)</param>
    public void Invoke(string id, bool ignoreErrors = true, bool removeErrorActions = true)
    {
        if (string.IsNullOrEmpty(id) || !_events.TryGetValue(id, out var action))
        {
            return; // 没找到，什么都不做，不报错
        }

        try
        {
            action?.Invoke();
        }
        catch (Exception e)
        {
            if (!ignoreErrors)
            {
                throw; // 如果不需要容错，直接抛出异常
            }

            Log.Error($"Error invoking event '{id}': {e.Message}");
            // 将报错的 Action 从列表中移除，防止下次继续报错
            if (removeErrorActions)
            {
                _events.Remove(id);
            }
        }
    }

    /// <summary>
    /// 触发所有已注册的事件 (慎用，通常用于全局刷新)
    /// </summary>
    public void InvokeAll(bool ignoreErrors = true)
    {
        // 注意：遍历过程中如果 Modify 集合会报错，所以先拷贝 Key 或 Value
        // 这里我们直接遍历 Values，因为 Register 操作可能会修改字典，为了安全最好ToList
        var actionsToInvoke = new List<Action>(_events.Values);

        foreach (var action in actionsToInvoke)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                if (!ignoreErrors) throw;
                Console.WriteLine($"[ActionEventManager] Error invoking an event: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 检查某个 ID 是否存在
    /// </summary>
    public bool HasEvent(string id)
    {
        return !string.IsNullOrEmpty(id) && _events.ContainsKey(id);
    }
}

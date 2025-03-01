using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

// 立即执行协程的组件 避免帧延迟
public class InstantCoroutine : Component
{
    public IEnumerator Current => enumerators.Count > 0 ? enumerators.Peek() : null;
    public bool Completed { get; private set; }
    public Action OnCompleted;

    // 协程堆栈
    private Stack<IEnumerator> enumerators;
    private float waitTimer;
    private bool removeOmCompleted;
    private bool logging;

    private bool needCancel;
    private bool updated;
    private bool isUpdating;
    private bool needExecuteAgain;

    public InstantCoroutine(IEnumerator functionCall, bool removeOmCompleted = true, bool logging = false)
        : base(true, false)
    {
        this.removeOmCompleted = removeOmCompleted;
        this.logging = logging;
        if (logging)
            Logger.SetLogLevel("InstantCoroutine", LogLevel.Verbose);

        enumerators = new Stack<IEnumerator>();
        enumerators.Push(functionCall);

        needCancel = false;
        isUpdating = false;
        needExecuteAgain = false;
    }

    public InstantCoroutine(bool removeOmCompleted = true, bool logging = false)
        : base(true, false)
    {
        this.removeOmCompleted = removeOmCompleted;
        this.logging = logging;
        if (logging)
            Logger.SetLogLevel("InstantCoroutine", LogLevel.Verbose);

        enumerators = new Stack<IEnumerator>();

        needCancel = false;
        isUpdating = false;
        needExecuteAgain = false;
    }

    // 取消当前协程的执行
    public void Cancel()
    {
        Logger.Log(LogLevel.Debug, "InstantCoroutine", "Immediately coroutine canceled");
        Completed = true;
        Active = false;
        waitTimer = 0f;
        enumerators.Clear();

        needExecuteAgain = false;

        if (isUpdating)
            needCancel = true;
    }

    // 替换当前协程为新协程并立即执行
    public void Replace(IEnumerator functionCall)
    {
        Logger.Log(LogLevel.Debug, "InstantCoroutine", $"Current coroutine replaced to {GetCoroutineName(functionCall)}");
        Completed = false;
        Active = true;
        waitTimer = 0f;
        enumerators.Clear();
        enumerators.Push(functionCall);

        if (isUpdating)
            needExecuteAgain = true;
        else if (updated)
            ExecuteCoroutineStack();
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        entity.PreUpdate += PreUpdate;
    }

    public override void Removed(Entity entity)
    {
        base.Removed(entity);
        entity.PreUpdate -= PreUpdate;
    }

    public override void Update()
    {
        base.Update();

        // 协程执行中Replace被调用立刻执行新的协程
        isUpdating = true;
        do
        {
            needExecuteAgain = false;
            ExecuteCoroutineStack();
        } while (needExecuteAgain);
        isUpdating = false;
        updated = true;
    }

    internal void PreUpdate(Entity entity)
    {
        updated = false;
    }

    private void ExecuteCoroutineStack()
    {
        Logger.Log(LogLevel.Verbose, "InstantCoroutine", $"Start processing stack. Depth: {enumerators.Count}. Current coroutine: {GetCoroutineName(enumerators.Peek())}");

        while (enumerators.Count > 0)
        {
            // 等待内部计时器 
            if (waitTimer > 0)
            {
                Logger.Log(LogLevel.Verbose, "InstantCoroutine", $"Waiting remaining: {waitTimer:0.000}s");
                waitTimer -= Engine.DeltaTime;
                break;
            }

            // 尝试推进协程
            IEnumerator current = enumerators.Peek();
            if (!current.MoveNext())
            {
                Logger.Log(LogLevel.Verbose, "InstantCoroutine", $"Coroutine completed: {GetCoroutineName(current)}. Popped");
                enumerators.Pop();
                continue;
            }

            // 处理协程返回值
            var currentReturn = current.Current;
            if (currentReturn is int || currentReturn is float)
            {
                // 返回值是数值则设置内部计时器
                waitTimer = (float)currentReturn;
                Logger.Log(LogLevel.Verbose, "InstantCoroutine", $"Setting wait timer: {waitTimer:0.000}");
            }
            else if (currentReturn is IEnumerator)
            {
                // 返回值是协程则压入堆栈
                enumerators.Push((IEnumerator)currentReturn);
                Logger.Log(LogLevel.Verbose, "InstantCoroutine", $"Pushing inner coroutine: {GetCoroutineName((IEnumerator)currentReturn)}");
            }
            else
            {
                // 其他返回值或 null 中断循环
                break;
            }

            // 立即等待内部计时器 避免帧延迟
            if (waitTimer > 0)
            {
                Logger.Log(LogLevel.Verbose, "InstantCoroutine", $"Waiting remaining: {waitTimer:0.000}s");
                waitTimer -= Engine.DeltaTime;
                break;
            }
        }

        // 需要取消组件则直接返回 避免组件被移除
        if (needCancel)
            return;

        if (enumerators.Count == 0)
        {
            // 处理协程堆栈执行完成
            Logger.Log(LogLevel.Debug, "InstantCoroutine", $"Instant coroutine execute completed");

            Active = false;
            Completed = true;
            OnCompleted?.Invoke();
            if (removeOmCompleted)
            {
                RemoveSelf();

                if (!logging)
                    Logger.SetLogLevel("InstantCoroutine", LogLevel.Info);
            }
        }
        else
        {
            Logger.Log(LogLevel.Debug, "InstantCoroutine", $"Frame execute completed");
        }
    }

    // 获取协程方法名
    private string GetCoroutineName(IEnumerator coroutine)
    {
        if (coroutine == null)
            return "null";

        string typeName = coroutine.GetType().Name;

        int startIndex = typeName.IndexOf('<');
        int endIndex = typeName.IndexOf('>');

        if (startIndex >= 0 && endIndex > startIndex)
            return typeName.Substring(startIndex + 1, endIndex - startIndex - 1);

        return typeName;
    }
}
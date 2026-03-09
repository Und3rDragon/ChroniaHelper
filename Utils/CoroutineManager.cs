using ChroniaHelper.Cores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public class CoroutineManager
{
    private readonly List<CoroutineHolder> _coroutines = new();

    // 用于标记需要移除的协程
    private readonly List<CoroutineHolder> _toRemove = new();

    // 标记管理器是否已停止/销毁
    private bool _isDisposed = false;

    /// <summary>
    /// IEnumerator 处理等待逻辑
    /// </summary>
    private class CoroutineHolder
    {
        public IEnumerator Enumerator;
        public float WaitTimer;
        public bool IsWaiting;
        public bool IsFinished;
        public bool WaitForNextFrame;

        public CoroutineHolder(IEnumerator enumerator)
        {
            Enumerator = enumerator;
            WaitTimer = 0f;
            IsWaiting = false;
            IsFinished = false;
            WaitForNextFrame = false;
        }

        /// <summary>
        /// 更新协程状态
        /// </summary>
        public bool Update(float deltaTime)
        {
            if (IsFinished) return false;

            // 1. 处理等待逻辑
            if (IsWaiting)
            {
                WaitTimer -= deltaTime;
                if (WaitTimer > 0) return true;
                IsWaiting = false;
            }

            if (WaitForNextFrame)
            {
                WaitForNextFrame = false;
                return true;
            }

            // 2. 推动协程
            try
            {
                if (!Enumerator.MoveNext())
                {
                    IsFinished = true;
                    return false;
                }

                object current = Enumerator.Current;

                if (current is float waitTime)
                {
                    IsWaiting = true;
                    WaitTimer = waitTime;
                    return true;
                }

                if (current == null)
                {
                    WaitForNextFrame = true;
                    return true;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Log("CoroutineManager", $"Error in coroutine instance: {e.Message}");
                Logger.Log("CoroutineManager", e.StackTrace);
                IsFinished = true;
                return false;
            }
        }
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    public void Start(IEnumerator coroutine)
    {
        if (_isDisposed)
        {
            Logger.Log("CoroutineManager", "Warning: Trying to start a coroutine on a disposed manager.");
            return;
        }

        if (coroutine == null) return;

        _coroutines.Add(new CoroutineHolder(coroutine));
    }

    /// <summary>
    /// 停止所有协程并清空列表
    /// </summary>
    public void StopAll()
    {
        _coroutines.Clear();
        _toRemove.Clear();
    }

    /// <summary>
    /// 必须在每一帧调用此方法来驱动协程。
    /// 你可以在 Hook 中调用特定实例的这个方法。
    /// </summary>
    public void Update()
    {
        if (_isDisposed || _coroutines.Count == 0) return;

        float deltaTime = Engine.DeltaTime;

        // 1. 遍历并更新
        for (int i = 0; i < _coroutines.Count; i++)
        {
            var holder = _coroutines[i];
            if (!holder.Update(deltaTime))
            {
                _toRemove.Add(holder);
            }
        }

        // 2. 清理
        if (_toRemove.Count > 0)
        {
            foreach (var holder in _toRemove)
            {
                _coroutines.Remove(holder);
            }
            _toRemove.Clear();
        }
    }

    /// <summary>
    /// 销毁管理器，释放资源
    /// </summary>
    public void Dispose()
    {
        StopAll();
        _isDisposed = true;
    }
}
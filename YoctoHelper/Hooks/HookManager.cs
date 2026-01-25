using System;
using System.Collections.Generic;
using System.Reflection;
using ChroniaHelper;
using YoctoHelper.Cores;
using ChroniaHelper.Utils;

namespace YoctoHelper.Hooks;

public class HookManager
{

    private readonly Dictionary<HookId, object> HookDataDefaultValue = new Dictionary<HookId, object>();

    public void Load()
    {
        this.Execute(typeof(Load));
    }

    public void Unload()
    {
        this.Execute(typeof(Unload));
    }

    private void Execute(Type moduleAttribute)
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypesSafe();
        foreach (Type type in types)
        {
            if (type.Namespace != "YoctoHelper.Hooks")
            {
                continue;
            }
            try
            {
                HookRegister hookRegister = type.GetCustomAttribute<HookRegister>();
                if (ObjectUtils.IsNull(hookRegister))
                {
                    continue;
                }
                object obj = Activator.CreateInstance(type);
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    if ((hookRegister.useData) && (ObjectUtils.IsNotNull(method.GetCustomAttribute<DefaultValue>())))
                    {
                        this.HookDataDefaultValue[hookRegister.id] = method.Invoke(obj, null);
                    }
                    if (ObjectUtils.IsNotNull(method.GetCustomAttribute(moduleAttribute)))
                    {
                        method.Invoke(obj, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }

    public void SetHookDataValue<T>(HookId hookId, T value, bool revertOnDeath = true)
    {
        if (revertOnDeath)
        {
            this.SetHookDataRoomValue<T>(hookId, value);
        }
        else
        {
            this.SetHookDataSessionValue<T>(hookId, value);
        }
    }

    private void SetHookDataSessionValue<T>(HookId hookId, T value)
    {
        if (ObjectUtils.IsNull(hookId))
        {
            return;
        }
        if (!Md.Session.HookManagerData.ContainsKey(hookId))
        {
            Md.Session.HookManagerData[hookId] = new HookData();
        }
        Md.Session.HookManagerData[hookId].SetValue(value);
    }

    private void SetHookDataRoomValue<T>(HookId hookId, T value)
    {
        if (ObjectUtils.IsNull(hookId))
        {
            return;
        }
        if (!Md.Session.HookManagerData.ContainsKey(hookId))
        {
            Md.Session.HookManagerData[hookId] = new HookData(this.HookDataDefaultValue[hookId]);
        }
        Md.Session.HookManagerData[hookId].roomValue = value;
    }

    public T GetHookDataValue<T>(HookId hookId)
    {
        if (ObjectUtils.IsNull(hookId))
        {
            return default(T);
        }
        if ((ObjectUtils.IsNotNull(ChroniaHelperModule.Session)) && (Engine.Scene is not Overworld) && (Md.Session.HookManagerData.TryGetValue(hookId, out HookData hookData)))
        {
            if (ObjectUtils.IsNotNull(hookData.roomValue))
            {
                return (T)hookData.roomValue;
            }
            if (ObjectUtils.IsNotNull(hookData.sessionValue))
            {
                return (T)hookData.sessionValue;
            }
        }
        return (this.HookDataDefaultValue.TryGetValue(hookId, out object defaultValue)) ? (T)defaultValue : default(T);
    }

    public void ResetHookDataRoomValue()
    {
        if (DictionaryUtils.IsNullOrEmpty(Md.Session.HookManagerData))
        {
            return;
        }
        foreach (HookId hookId in Md.Session.HookManagerData.Keys)
        {
            Md.Session.HookManagerData[hookId].roomValue = null;
        }
    }

}

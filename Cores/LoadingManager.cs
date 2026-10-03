using System;
using System.Reflection;
using ChroniaHelper.Utils;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Cores;

public class LoadingManager
{
    public static HashSet<Type> forceLoadingHooks { get; private set; } = new();
    public static HashSet<Type> selectiveLoadingHooks { get; private set; } = new();

    /// <summary>
    /// Manually load hooks with [SelectiveLoadHook] labelled
    /// </summary>
    /// <param name="t"></param>
    public static void LoadHook(Type t)
    {
        // if registered and loaded, return
        if (selectiveLoadingHooks.Contains(t))
        {
            return;
        }
        
        // if not, load the hook
        MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        foreach (var method in methods)
        {
            if (method.GetCustomAttribute(typeof(SelectiveLoadHook)) != null)
            {
                object instance = method.IsStatic ? null : Activator.CreateInstance(t);
                method.Invoke(instance, null);
            }
        }
        
        selectiveLoadingHooks.Add(t);
    }

    public static void LoadHook<T>()
    {
        LoadHook(typeof(T));
    }
    
    /// <summary>
    /// Manually unload hooks with [SelectiveUnloadHook] labelled
    /// </summary>
    /// <param name="t"></param>
    public static void UnloadHook(Type t)
    {
        // if unregistered or unloaded, return
        if (!selectiveLoadingHooks.Contains(t))
        {
            return;
        }
        
        // if not, unload the hook
        MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        foreach (var method in methods)
        {
            if (method.GetCustomAttribute(typeof(SelectiveUnloadHook)) != null)
            {
                object instance = method.IsStatic ? null : Activator.CreateInstance(t);
                method.Invoke(instance, null);
            }
        }
        
        selectiveLoadingHooks.Remove(t);
    }

    public static void UnloadHook<T>()
    {
        UnloadHook(typeof(T));
    }

    /// <summary>
    /// Extension method of loading hook directly instead of searching every single methods in the instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="funcs"></param>
    public static void LoadHookWithMethod<T>(T instance, params Action[] funcs)
    {
        Type type = instance.GetType();

        // if registered and loaded, return
        if (selectiveLoadingHooks.Contains(type))
        {
            return;
        }

        foreach(Action func in funcs)
        {
            func?.Invoke();
        }

        selectiveLoadingHooks.Add(type);
    }

    public static void LoadHookWithMethod(Type type, params Action[] funcs)
    {
        // if registered and loaded, return
        if (selectiveLoadingHooks.Contains(type))
        {
            return;
        }

        foreach (Action func in funcs)
        {
            func?.Invoke();
        }

        selectiveLoadingHooks.Add(type);
    }

    public static void LoadHookWithMethod<T>(params Action[] funcs)
    {
        Type type = typeof(T);

        // if registered and loaded, return
        if (selectiveLoadingHooks.Contains(type))
        {
            return;
        }

        foreach (Action func in funcs)
        {
            func?.Invoke();
        }

        selectiveLoadingHooks.Add(type);
    }

    /// <summary>
    /// Extension method of unloading hook directly instead of searching every single methods in the instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="funcs"></param>
    public static void UnloadHookWithMethod<T>(T instance, params Action[] funcs)
    {
        Type type = instance.GetType();

        // if unregistered or unloaded, return
        if (!selectiveLoadingHooks.Contains(type))
        {
            return;
        }

        // if not, unload the hook
        foreach(Action func in funcs)
        {
            func?.Invoke();
        }

        selectiveLoadingHooks.Remove(type);
    }

    public static void UnloadHookWithMethod(Type type, params Action[] funcs)
    {
        // if unregistered or unloaded, return
        if (!selectiveLoadingHooks.Contains(type))
        {
            return;
        }

        // if not, unload the hook
        foreach (Action func in funcs)
        {
            func?.Invoke();
        }

        selectiveLoadingHooks.Remove(type);
    }

    public static void UnloadHookWithMethod<T>(params Action[] funcs)
    {
        Type type = typeof(T);

        // if unregistered or unloaded, return
        if (!selectiveLoadingHooks.Contains(type))
        {
            return;
        }

        // if not, unload the hook
        foreach (Action func in funcs)
        {
            func?.Invoke();
        }

        selectiveLoadingHooks.Remove(type);
    }

    public static void Load()
    {
        Execute(typeof(LoadHook), "ChroniaHelper");
        RegisterAllDecalHandlers();
    }

    public static void Unload()
    {
        Execute(typeof(UnloadHook), "ChroniaHelper");
        
        // do Selective Unload here
        Type[] types = Assembly.GetExecutingAssembly().GetTypesSafe();

        foreach (var t in types)
        {
            if (!t.FullName.StartsWith("ChroniaHelper"))
                continue;

            UnloadHook(t);
        }
    }
    
    private static void Execute(Type attributeType, string targetNamespace = null)
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypesSafe();

        foreach (var t in types)
        {
            if (!string.IsNullOrEmpty(targetNamespace) && !t.FullName.StartsWith(targetNamespace))
                continue;

            MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute(attributeType) != null)
                {
                    if (attributeType == typeof(LoadHook))
                    {
                        forceLoadingHooks.Add(t);
                    }
                    else if (attributeType == typeof(UnloadHook))
                    {
                        forceLoadingHooks.Remove(t);
                    }
                    
                    object instance = method.IsStatic ? null : Activator.CreateInstance(t);
                    method.Invoke(instance, null);
                }
            }
        }
    }
    
    private static void RegisterAllDecalHandlers()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypesSafe();
    
        // 明确指定要获取泛型方法：AddPropertyHandler<T>()
        // 使用 LINQ 表达式来避免字符串硬编码（可选）
        var method = typeof(Celeste.Mod.DecalRegistry).GetMethods()
            .FirstOrDefault(m => m.Name == nameof(Celeste.Mod.DecalRegistry.AddPropertyHandler) 
                                 && m.IsGenericMethod 
                                 && m.GetGenericArguments().Length == 1);
    
        if (method == null)
        {
            Log.Error("Failed to find generic AddPropertyHandler<T> method!");
            return;
        }
    
        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<RegistryHandler>();
            if (attribute == null) continue;
        
            if (type.IsAbstract || type.IsInterface) continue;
        
            try
            {
                // 构造泛型方法 AddPropertyHandler<T>
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(null, null);
            
                Log.Info($"Registered decal handler for: {type.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to register {type.Name}: {ex.Message}");
            }
        }
    }
}
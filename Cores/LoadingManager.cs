using System;
using System.Reflection;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores;

public class LoadingManager
{
    public static void Load()
    {
        Execute(typeof(LoadHook), "ChroniaHelper");
        RegisterAllDecalHandlers();
    }

    public static void Unload()
    {
        Execute(typeof(UnloadHook), "ChroniaHelper");
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
            var attribute = type.GetCustomAttribute<RegistryHandlerAttribute>();
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
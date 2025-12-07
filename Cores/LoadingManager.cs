using System;
using System.Reflection;

namespace ChroniaHelper.Cores;

public class LoadingManager
{
    public static void Load()
    {
        Execute(typeof(LoadHook), "ChroniaHelper");
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
}
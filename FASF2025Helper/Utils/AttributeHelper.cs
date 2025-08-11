using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FASF2025Helper.Utils;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class LoadAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class UnLoadAttribute : Attribute
{

}

public static class AttributeHelper
{
    private readonly static Dictionary<Type, List<MethodInfo>> cache = new Dictionary<Type, List<MethodInfo>>();

    public static void InvokeAll<TAttribute>() where TAttribute : Attribute
    {
        var methods = GetAllMethodWithCustomAttribute<TAttribute>();

        foreach (var method in methods)
            method.Invoke(null, null);
    }

    public static List<MethodInfo> GetAllMethodWithCustomAttribute<TAttribute>() where TAttribute : Attribute
    {
        var attributeType = typeof(TAttribute);

        if (!cache.TryGetValue(attributeType, out List<MethodInfo> cacheEntries))
        {
            cacheEntries = new List<MethodInfo>();
            Assembly assembly = Assembly.GetCallingAssembly();

            foreach (var type in assembly.GetTypesSafe())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.IsDefined(attributeType, false))
                        cacheEntries.Add(method);
                }
            }

            cache[attributeType] = cacheEntries;
        }

        return cacheEntries;
    }
}

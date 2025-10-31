using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FASF2025Helper.Utils;

// 反射工具类
public static class ReflectionHelper
{
    private readonly static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
    private readonly static Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();
    private readonly static Dictionary<string, ConstructorInfo> constructorCache = new Dictionary<string, ConstructorInfo>();
    private readonly static Dictionary<string, FieldInfo> fieldCache = new Dictionary<string, FieldInfo>();
    private readonly static Dictionary<string, PropertyInfo> propertyCache = new Dictionary<string, PropertyInfo>();

    #region 反射获取
    // 获取类型
    public static Type GetType(string typeName)
    {
        if (typeCache.TryGetValue(typeName, out Type type))
            return type;

        type = Type.GetType(typeName);
        if (type != null)
        {
            typeCache[typeName] = type;
            return type;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null)
            {
                typeCache[typeName] = type;
                return type;
            }
        }
        return null;
    }

    // 获取无参方法
    public static MethodInfo GetMethod<TType>(string methodName, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        string key = $"{typeof(TType).FullName}.{methodName}";

        if (methodCache.TryGetValue(key, out MethodInfo method))
            return method;

        method = typeof(TType).GetMethod(methodName, binding);
        if (method != null)
        {
            methodCache[key] = method;
            return method;
        }

        return null;
    }

    // 获取有参方法
    public static MethodInfo GetMethod<TType>(string methodName, Type[] parameterTypes, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        string key = $"{typeof(TType).FullName}.{methodName}({string.Join(",", parameterTypes.Select(t => t.Name))})";

        if (methodCache.TryGetValue(key, out MethodInfo method))
            return method;

        method = typeof(TType).GetMethod(methodName, binding, null, parameterTypes, null);
        if (method != null)
        {
            methodCache[key] = method;
            return method;
        }
        return null;
    }

    // 获取无参构造函数
    public static ConstructorInfo GetConstructor<TType>(BindingFlags binding = Cons.DefaultBindingFlags)
    {
        string key = $"{typeof(TType).FullName}.Constructor";

        if (constructorCache.TryGetValue(key, out ConstructorInfo constructor))
            return constructor;

        constructor = typeof(TType).GetConstructor(binding, null, Type.EmptyTypes, null);
        if (constructor != null)
        {
            constructorCache[key] = constructor;
            return constructor;
        }

        return null;
    }

    // 获取有参构造函数
    public static ConstructorInfo GetConstructor<TType>(Type[] parameterTypes, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        string key = $"{typeof(TType).FullName}.Constructor({string.Join(",", parameterTypes.Select(t => t.Name))})";

        if (constructorCache.TryGetValue(key, out ConstructorInfo constructor))
            return constructor;

        constructor = typeof(TType).GetConstructor(binding, null, parameterTypes, null);
        if (constructor != null)
        {
            constructorCache[key] = constructor;
            return constructor;
        }

        return null;
    }

    // 获取字段
    public static FieldInfo GetField<TType>(string fieldName, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        string key = $"{typeof(TType).FullName}.{fieldName}";

        if (fieldCache.TryGetValue(key, out FieldInfo field))
            return field;

        field = typeof(TType).GetField(fieldName, binding);
        if (field != null)
        {
            fieldCache[key] = field;
            return field;
        }

        return null;
    }

    // 获取属性
    public static PropertyInfo GetProperty<TType>(string propertyName, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        string key = $"{typeof(TType).FullName}.{propertyName}";

        if (propertyCache.TryGetValue(key, out PropertyInfo property))
            return property;

        property = typeof(TType).GetProperty(propertyName, binding);
        if (property != null)
        {
            propertyCache[key] = property;
            return property;
        }

        return null;
    }

    // 获取属性Getter方法
    public static MethodInfo GetPropertyGetter<TType>(string propertyName, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        PropertyInfo property = GetProperty<TType>(propertyName, binding);
        return property?.GetGetMethod(true);
    }

    // 获取属性Setter方法
    public static MethodInfo GetPropertySetter<TType>(string propertyName, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        PropertyInfo property = GetProperty<TType>(propertyName, binding);
        return property?.GetSetMethod(true);
    }
    #endregion

    // 清除缓存
    public static void ClearCaches()
    {
        typeCache.Clear();
        methodCache.Clear();
        constructorCache.Clear();
        fieldCache.Clear();
        propertyCache.Clear();
    }
}

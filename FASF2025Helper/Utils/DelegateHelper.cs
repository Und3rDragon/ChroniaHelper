using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FASF2025Helper.Utils;

// 动态委托生成器
// 利用 DynamicMethod 与 IL 生成动态委托
// 避免反射的开销
public static class DelegateHelper
{
    private readonly static Dictionary<string, Delegate> delegateCache = new Dictionary<string, Delegate>();
    private readonly static Dictionary<string, Func<object[], object>> constructorCache = new Dictionary<string, Func<object[], object>>();
    private readonly static Dictionary<string, Func<object, object>> fieldGetterCache = new Dictionary<string, Func<object, object>>();
    private readonly static Dictionary<string, Action<object, object>> fieldSetterCache = new Dictionary<string, Action<object, object>>();
    private readonly static Dictionary<string, Func<object, object>> propertyGetterCache = new Dictionary<string, Func<object, object>>();
    private readonly static Dictionary<string, Action<object, object>> propertySetterCache = new Dictionary<string, Action<object, object>>();

    public static TDelegate CreateFastDelegate<TDeclaringType, TDelegate>(string methodName, Type[] parameterTypes = null, BindingFlags binding = Cons.DefaultBindingFlags) where TDelegate : Delegate
    {
        // 尝试获取缓存的委托
        string key = parameterTypes == null
            ? $"{typeof(TDeclaringType).FullName}.{methodName}.{typeof(TDelegate).FullName}"
            : $"{typeof(TDeclaringType).FullName}.{methodName}({string.Join(",", parameterTypes.Select(t => t.FullName))}).{typeof(TDelegate).FullName}";
        if (delegateCache.TryGetValue(key, out Delegate cachedDelegate))
            return (TDelegate)cachedDelegate;

        // 获取方法信息
        var method = parameterTypes == null
            ? ReflectionHelper.GetMethod<TDeclaringType>(methodName, binding)
            : ReflectionHelper.GetMethod<TDeclaringType>(methodName, parameterTypes, binding);
        if (method == null)
            throw new ArgumentException($"Method '{methodName}' not found in type '{typeof(TDeclaringType).FullName}'");

        // 创建动态方法
        var declaringType = typeof(TDeclaringType);
        var invokeMethod = typeof(TDelegate).GetMethod("Invoke");
        var delegateParams = invokeMethod.GetParameters();
        var returnType = invokeMethod.ReturnType;
        var dynamicMethod = new DynamicMethod(
            parameterTypes == null
                ? $"DynamicDelegate_{declaringType.Name}_{methodName}"
                : $"DynamicDelegate_{declaringType.Name}_{methodName}_{string.Join(",", parameterTypes.Select(t => t.FullName))}",
            returnType,
            delegateParams.Select(p => p.ParameterType).ToArray(),
            declaringType.Module,
            true
        );

        // 生成 IL 代码
        var il = dynamicMethod.GetILGenerator();
        // 加载参数
        for (int i = 0; i < delegateParams.Length; i++)
            il.Emit(OpCodes.Ldarg, i);
        // 调用方法
        if (method.IsStatic)
            il.Emit(OpCodes.Call, method);
        else
            il.Emit(OpCodes.Callvirt, method);
        // 返回
        il.Emit(OpCodes.Ret);

        // 创建委托实例
        var dynamicDelegate = (TDelegate)dynamicMethod.CreateDelegate(typeof(TDelegate));
        delegateCache[key] = dynamicDelegate;
        return dynamicDelegate;
    }
    public static Delegate CreateFastDelegate(Type declaringType, Type delegateType, string methodName, Type[] parameterTypes = null, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        var method = typeof(DelegateHelper).GetMethod(nameof(CreateFastDelegate), new[] { typeof(string), typeof(Type[]), typeof(BindingFlags) });
        var genericMethod = method.MakeGenericMethod(declaringType, delegateType);
        return (Delegate)genericMethod.Invoke(null, new object[] { methodName, parameterTypes, binding });
    }

    public static TDelegate CreateConstructor<TDeclaringType, TDelegate>(Type[] parameterTypes = null, BindingFlags binding = Cons.DefaultBindingFlags) where TDelegate : Delegate
    {
        string key = parameterTypes == null
            ? $"{typeof(TDeclaringType).FullName}.Constructor.{typeof(TDelegate).FullName}"
            : $"{typeof(TDeclaringType).FullName}.Constructor({string.Join(",", parameterTypes.Select(t => t.FullName))}).{typeof(TDelegate).FullName}";
        if (constructorCache.TryGetValue(key, out Func<object[], object> cachedConstructor))
            return (TDelegate)(object)cachedConstructor;

        var constructor = parameterTypes == null 
            ? ReflectionHelper.GetConstructor<TDeclaringType>(binding)
            : ReflectionHelper.GetConstructor<TDeclaringType>(parameterTypes, binding);
        if (constructor == null)
            throw new ArgumentException($"Constructor not found in type '{typeof(TDeclaringType).FullName}'");

        // 创建动态方法
        var declaringType = typeof(TDeclaringType);
        var constructorParams = constructor.GetParameters();
        var dynamicMethod = new DynamicMethod(
            parameterTypes == null 
                ? $"DynamicConstructor_{declaringType.Name}"
                : $"DynamicConstructor_{declaringType.Name}_{string.Join(",", parameterTypes.Select(t => t.FullName))}",
            typeof(object),
            new Type[] { typeof(object[]) },
            declaringType.Module,
            true
        );

        // object Constructor(object[] args)
        var il = dynamicMethod.GetILGenerator();
        // 加载构造函数参数
        for (int i = 0; i < constructorParams.Length; i++)
        {
            il.Emit(OpCodes.Ldarg_0);             
            il.Emit(OpCodes.Ldc_I4, i);          
            il.Emit(OpCodes.Ldelem_Ref);

            var paramType = constructorParams[i].ParameterType;
            if (paramType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, paramType);
            else
                il.Emit(OpCodes.Castclass, paramType);
        }
        
        // 调用构造函数
        il.Emit(OpCodes.Newobj, constructor);

        // 装箱值类型返回值
        if (declaringType.IsValueType)
            il.Emit(OpCodes.Box, declaringType);

        il.Emit(OpCodes.Ret);

        var constructorDelegate = (Func<object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object[], object>));
        constructorCache[key] = constructorDelegate;
        return (TDelegate)(object)constructorDelegate;
    }
    public static Delegate CreateConstructor(Type declaringType, Type delegateType, Type[] parameterTypes = null, BindingFlags binding = Cons.DefaultBindingFlags)
    {
        var method = typeof(DelegateHelper).GetMethod(nameof(CreateConstructor), new[] { typeof(Type[]), typeof(BindingFlags) });
        var genericMethod = method.MakeGenericMethod(declaringType, delegateType);
        return (Delegate)genericMethod.Invoke(null, new object[] { parameterTypes, binding });
    }

    public static Func<object, object> CreateFieldGetter<TDeclaringType>(string fieldName)
    {
        string key = $"{typeof(TDeclaringType).FullName}.{fieldName}.Getter";
        if (fieldGetterCache.TryGetValue(key, out Func<object, object> cachedGetter))
            return cachedGetter;

        var field = ReflectionHelper.GetField<TDeclaringType>(fieldName);
        if (field == null)
            throw new ArgumentException($"Field '{fieldName}' not found in type '{typeof(TDeclaringType).FullName}'");

        var declaringType = typeof(TDeclaringType);
        var dynamicMethod = new DynamicMethod(
            $"DynamicFieldGetter_{declaringType.Name}_{fieldName}",
            typeof(object),
            new Type[] { typeof(object) },
            declaringType.Module,
            true
        );

        // object FieldGetter(object instance)
        var il = dynamicMethod.GetILGenerator();
        if (field.IsStatic)
        {
            il.Emit(OpCodes.Ldsfld, field);
        }
        else
        {
            il.Emit(OpCodes.Ldarg_0);

            if (declaringType.IsValueType)
                il.Emit(OpCodes.Unbox, declaringType);
            else
                il.Emit(OpCodes.Castclass, declaringType);

            il.Emit(OpCodes.Ldfld, field);
        }
        if (field.FieldType.IsValueType)
            il.Emit(OpCodes.Box, field.FieldType);
        il.Emit(OpCodes.Ret);

        var getter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        fieldGetterCache[key] = getter;
        return getter;
    }
    public static Func<object, object> CreateFieldGetter(Type declaringType, string fieldName)
    {
        var method = typeof(DelegateHelper).GetMethod(nameof(CreateFieldGetter), new[] { typeof(string) });
        var genericMethod = method.MakeGenericMethod(declaringType);
        return (Func<object, object>)genericMethod.Invoke(null, new object[] { fieldName });
    }

    public static Action<object, object> CreateFieldSetter<TDeclaringType>(string fieldName)
    {
        string key = $"{typeof(TDeclaringType).FullName}.{fieldName}.Setter";
        if (fieldSetterCache.TryGetValue(key, out Action<object, object> cachedSetter))
            return cachedSetter;

        var field = ReflectionHelper.GetField<TDeclaringType>(fieldName);
        if (field == null)
            throw new ArgumentException($"Field '{fieldName}' not found in type '{typeof(TDeclaringType).FullName}'");
        if (field.IsInitOnly)
            throw new ArgumentException($"Field '{fieldName}' is readonly");

        var declaringType = typeof(TDeclaringType);
        var dynamicMethod = new DynamicMethod(
            $"DynamicFieldSetter_{declaringType.Name}_{fieldName}",
            typeof(void),
            new[] { typeof(object), typeof(object) },
            declaringType.Module,
            true
        );

        // void FieldSetter(object instance, object value)
        var il = dynamicMethod.GetILGenerator();
        if (field.IsStatic)
        {
            il.Emit(OpCodes.Ldarg_1);
        }
        else
        {
            il.Emit(OpCodes.Ldarg_0);
            if (declaringType.IsValueType)
                il.Emit(OpCodes.Unbox, declaringType);
            else
                il.Emit(OpCodes.Castclass, declaringType);
        }

        if (field.FieldType.IsValueType)
            il.Emit(OpCodes.Unbox_Any, field.FieldType);
        else
            il.Emit(OpCodes.Castclass, field.FieldType);

        if (field.IsStatic)
            il.Emit(OpCodes.Stsfld, field);
        else
            il.Emit(OpCodes.Stfld, field);

        il.Emit(OpCodes.Ret);

        var setter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
        fieldSetterCache[key] = setter;
        return setter;
    }
    public static Action<object, object> CreateFieldSetter(Type declaringType, string fieldName)
    {
        var method = typeof(DelegateHelper).GetMethod(nameof(CreateFieldSetter), new[] { typeof(string) });
        var genericMethod = method.MakeGenericMethod(declaringType);
        return (Action<object, object>)genericMethod.Invoke(null, new object[] { fieldName });
    }

    public static Func<object, object> CreatePropertyGetter<TDeclaringType>(string propertyName)
    {
        string key = $"{typeof(TDeclaringType).FullName}.{propertyName}.Getter";
        if (propertyGetterCache.TryGetValue(key, out Func<object, object> cachedGetter))
            return cachedGetter;

        var propertyGetter = ReflectionHelper.GetPropertyGetter<TDeclaringType>(propertyName);
        if (propertyGetter == null)
            throw new ArgumentException($"Property '{propertyName}' not found in type '{typeof(TDeclaringType).FullName}'");

        var declaringType = typeof(TDeclaringType);
        var dynamicMethod = new DynamicMethod(
            $"DynamicPropertyGetter_{declaringType.Name}_{propertyName}",
            typeof(object),
            new Type[] { typeof(object) },
            declaringType.Module,
            true
        );

        // object PropertyGetter(object instance)
        var il = dynamicMethod.GetILGenerator();
        if (propertyGetter.IsStatic)
        {
            il.Emit(OpCodes.Call, propertyGetter);
        }
        else
        {
            il.Emit(OpCodes.Ldarg_0);
            if (declaringType.IsValueType)
            {
                il.Emit(OpCodes.Unbox, declaringType);
                il.Emit(OpCodes.Call, propertyGetter);
            }
            else
            {
                il.Emit(OpCodes.Castclass, declaringType);
                il.Emit(OpCodes.Callvirt, propertyGetter);
            }
        }
        if (propertyGetter.ReturnType.IsValueType)
            il.Emit(OpCodes.Box, propertyGetter.ReturnType);
        il.Emit(OpCodes.Ret);

        var getter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        propertyGetterCache[key] = getter;
        return getter;
    }
    public static Func<object, object> CreatePropertyGetter(Type declaringType, string propertyName)
    {
        var method = typeof(DelegateHelper).GetMethod(nameof(CreatePropertyGetter), new[] { typeof(string) });
        var genericMethod = method.MakeGenericMethod(declaringType);
        return (Func<object, object>)genericMethod.Invoke(null, new object[] { propertyName });
    }


    public static Action<object, object> CreatePropertySetter<TDeclaringType>(string propertyName)
    {
        string key = $"{typeof(TDeclaringType).FullName}.{propertyName}.Setter";
        if (propertySetterCache.TryGetValue(key, out Action<object, object> cachedSetter))
            return cachedSetter;

        var propertySetter = ReflectionHelper.GetPropertySetter<TDeclaringType>(propertyName);
        if (propertySetter == null)
            throw new ArgumentException($"Property '{propertyName}' not found in type '{typeof(TDeclaringType).FullName}'");

        var declaringType = typeof(TDeclaringType);
        var dynamicMethod = new DynamicMethod(
            $"DynamicPropertySetter_{declaringType.Name}_{propertyName}",
            typeof(void),
            new[] { typeof(object), typeof(object) },
            declaringType.Module,
            true
        );

        // void PropertySetter(object instance, object value)
        var il = dynamicMethod.GetILGenerator();
        if (propertySetter.IsStatic)
        {
            il.Emit(OpCodes.Ldarg_1);
            var paramType = propertySetter.GetParameters()[0].ParameterType;
            if (paramType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, paramType);
            else
                il.Emit(OpCodes.Castclass,paramType);
            il.Emit(OpCodes.Call, propertySetter);
        }
        else
        {
            il.Emit(OpCodes.Ldarg_0);
            if (declaringType.IsValueType)
                il.Emit(OpCodes.Unbox, declaringType);
            else
                il.Emit(OpCodes.Castclass, declaringType);

            il.Emit(OpCodes.Ldarg_1);
            var paramType = propertySetter.GetParameters()[0].ParameterType;
            if (paramType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, paramType);
            else
                il.Emit(OpCodes.Castclass, paramType);

            if (declaringType.IsValueType)
                il.Emit(OpCodes.Call, propertySetter);
            else
                il.Emit(OpCodes.Callvirt, propertySetter);
        }
        il.Emit(OpCodes.Ret);

        var setter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
        propertySetterCache[key] = setter;
        return setter;
    }
    public static Action<object, object> CreatePropertySetter(Type declaringType, string propertyName)
    {
        var method = typeof(DelegateHelper).GetMethod(nameof(CreatePropertySetter), new[] { typeof(string) });
        var genericMethod = method.MakeGenericMethod(declaringType);
        return (Action<object, object>)genericMethod.Invoke(null, new object[] { propertyName });
    }

    public static void ClearCache()
    {
        delegateCache.Clear();
        constructorCache.Clear();
        fieldGetterCache.Clear();
        fieldSetterCache.Clear();
        propertyGetterCache.Clear();
        propertySetterCache.Clear();
    }
}

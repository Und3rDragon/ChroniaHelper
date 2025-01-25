namespace YoctoHelper.Cores;

public static class ObjectUtils
{

    public static bool IsNull(this object obj)
    {
        return (obj is null);
    }

    public static bool IsNotNull(this object obj)
    {
        return (!ObjectUtils.IsNull(obj));
    }

    public static bool IsValueType(this object obj)
    {
        return obj.GetType().IsValueType;
    }

    public static bool IsReferenceType(this object obj)
    {
        return (!ObjectUtils.IsValueType(obj));
    }

}

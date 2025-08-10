using System;
using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

public static class ArrayUtils
{

    public static bool IsNull(this Array array)
    {
        return ObjectUtils.IsNull(array);
    }

    public static bool IsNotNull(this Array array)
    {
        return ObjectUtils.IsNotNull(array);
    }

    public static bool IsEmpty(this Array array)
    {
        return (ArrayUtils.IsNotNull(array)) && (array.Length <= 0);
    }

    public static bool IsNotEmpty(this Array array)
    {
        return (ArrayUtils.IsNotNull(array)) && (array.Length > 0);
    }

    public static bool IsNullOrEmpty(this Array array)
    {
        return (ArrayUtils.IsNull(array)) || (array.Length <= 0);
    }

    public static bool IsArray(this object obj)
    {
        return (obj is Array);
    }

    public static bool IsArraysEquals<T>(this T[] array1, T[] array2)
    {
        if ((ArrayUtils.IsNull(array1)) || (ArrayUtils.IsNull(array2)) || (array1.Length != array2.Length))
        {
            return false;
        }
        for (int i = 0; i < array1.Length; i++)
        {
            if (!(array1[i].Equals(array2[i])))
            {
                return false;
            }
        }
        return true;
    }

}

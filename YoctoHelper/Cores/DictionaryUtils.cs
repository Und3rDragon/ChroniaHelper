using System.Collections;
using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

public static class DictionaryUtils
{

    public static bool IsNull(this IDictionary dictionary)
    {
        return ObjectUtils.IsNull(dictionary);
    }

    public static bool IsNotNull(this IDictionary dictionary)
    {
        return ObjectUtils.IsNotNull(dictionary);
    }

    public static bool IsEmpty(this IDictionary dictionary)
    {
        return (DictionaryUtils.IsNotNull(dictionary)) && (dictionary.Count <= 0);
    }

    public static bool IsNotEmpty(this IDictionary dictionary)
    {
        return (DictionaryUtils.IsNotNull(dictionary)) && (dictionary.Count > 0);
    }

    public static bool IsNullOrEmpty(this IDictionary dictionary)
    {
        return (DictionaryUtils.IsNull(dictionary)) || (dictionary.Count <= 0);
    }

    public static bool IsDictionary(this object obj)
    {
        return (obj is IDictionary);
    }

}

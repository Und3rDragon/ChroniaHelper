namespace ChroniaHelper.Utils;

/// <summary>
/// Enum 与 int/string 的中间包装类
/// </summary>
public readonly struct EnumWrapper<T> where T : struct, Enum
{
    public readonly T Value;

    public EnumWrapper(T value) => Value = value;

    // ============ 静态缓存 ============
    private static readonly Dictionary<int, T> _byInt;
    private static readonly Dictionary<string, T> _byString;
    private static readonly T _defaultValue;

    static EnumWrapper()
    {
        _byInt = new Dictionary<int, T>();
        _byString = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        var values = Enum.GetValues<T>();

        // 第一个成员作为默认值
        _defaultValue = values.Length > 0 ? values[0] : default;

        foreach (T value in values)
        {
            int intValue = Convert.ToInt32(value);
            string name = value.ToString();

            if (!_byInt.ContainsKey(intValue))
                _byInt[intValue] = value;

            if (!_byString.ContainsKey(name))
                _byString[name] = value;
        }
    }

    // ============ 转换方法 ============

    /// <summary>wrapper → int</summary>
    public static implicit operator int(EnumWrapper<T> wrapper)
        => Convert.ToInt32(wrapper.Value);

    /// <summary>wrapper → enum</summary>
    public static implicit operator T(EnumWrapper<T> wrapper)
        => wrapper.Value;

    /// <summary>wrapper → string</summary>
    public static implicit operator string(EnumWrapper<T> wrapper)
        => wrapper.Value.ToString();

    /// <summary>enum → wrapper</summary>
    public static implicit operator EnumWrapper<T>(T value)
        => new(value);

    /// <summary>int → wrapper（无效时返回第一个成员）</summary>
    public static implicit operator EnumWrapper<T>(int value)
        => _byInt.TryGetValue(value, out T result) ? new(result) : new(_defaultValue);

    /// <summary>string → wrapper（无效时返回第一个成员）</summary>
    public static implicit operator EnumWrapper<T>(string value)
        => !string.IsNullOrEmpty(value) && _byString.TryGetValue(value, out T result)
            ? new(result)
            : new(_defaultValue);

    // ============ 运算 ============

    public static EnumWrapper<T> operator +(EnumWrapper<T> a, int b)
        => new((T)Enum.ToObject(typeof(T), Convert.ToInt32(a.Value) + b));

    public static EnumWrapper<T> operator -(EnumWrapper<T> a, int b)
        => new((T)Enum.ToObject(typeof(T), Convert.ToInt32(a.Value) - b));

    public static EnumWrapper<T> operator ++(EnumWrapper<T> a)
        => new((T)Enum.ToObject(typeof(T), Convert.ToInt32(a.Value) + 1));

    public static EnumWrapper<T> operator --(EnumWrapper<T> a)
        => new((T)Enum.ToObject(typeof(T), Convert.ToInt32(a.Value) - 1));

    // ============ 比较 ============

    public static bool operator ==(EnumWrapper<T> a, EnumWrapper<T> b)
        => a.Value.Equals(b.Value);

    public static bool operator !=(EnumWrapper<T> a, EnumWrapper<T> b)
        => !a.Value.Equals(b.Value);

    public static bool operator ==(EnumWrapper<T> a, int b)
        => Convert.ToInt32(a.Value) == b;

    public static bool operator !=(EnumWrapper<T> a, int b)
        => Convert.ToInt32(a.Value) != b;

    public static bool operator ==(int a, EnumWrapper<T> b)
        => a == Convert.ToInt32(b.Value);

    public static bool operator !=(int a, EnumWrapper<T> b)
        => a != Convert.ToInt32(b.Value);

    public static bool operator ==(EnumWrapper<T> a, string b)
        => a.Value.ToString().Equals(b, StringComparison.OrdinalIgnoreCase);

    public static bool operator !=(EnumWrapper<T> a, string b)
        => !a.Value.ToString().Equals(b, StringComparison.OrdinalIgnoreCase);

    public static bool operator ==(string a, EnumWrapper<T> b)
        => b == a;

    public static bool operator !=(string a, EnumWrapper<T> b)
        => b != a;

    // ============ 便捷方法 ============

    /// <summary>尝试从 int 获取 wrapper（无效返回 false）</summary>
    public static bool TryFromInt(int value, out EnumWrapper<T> wrapper)
    {
        if (_byInt.TryGetValue(value, out T result))
        {
            wrapper = new(result);
            return true;
        }
        wrapper = new(_defaultValue);
        return false;
    }

    /// <summary>尝试从 string 获取 wrapper（无效返回 false）</summary>
    public static bool TryFromString(string value, out EnumWrapper<T> wrapper)
    {
        if (!string.IsNullOrEmpty(value) && _byString.TryGetValue(value, out T result))
        {
            wrapper = new(result);
            return true;
        }
        wrapper = new(_defaultValue);
        return false;
    }

    /// <summary>获取默认值（第一个成员）</summary>
    public static EnumWrapper<T> Default => new(_defaultValue);

    /// <summary>获取所有成员</summary>
    public static IEnumerable<T> GetAllValues() => _byInt.Values;

    /// <summary>获取所有名字</summary>
    public static IEnumerable<string> GetAllNames() => _byString.Keys;

    /// <summary>检查 int 是否有效</summary>
    public static bool IsValid(int value) => _byInt.ContainsKey(value);

    /// <summary>检查 string 是否有效</summary>
    public static bool IsValid(string value) => !string.IsNullOrEmpty(value) && _byString.ContainsKey(value);

    // ============ Equals / GetHashCode ============

    public override bool Equals(object obj)
        => obj is EnumWrapper<T> other && Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}

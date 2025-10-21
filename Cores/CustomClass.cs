using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChroniaHelper.Cores;

// ========================================
// IList2<T1, T2> -> .A, .B
// ========================================
public readonly struct List2Item<T1, T2>
{
    public T1 A { get; }
    public T2 B { get; }

    public List2Item(T1 a, T2 b) => (A, B) = (a, b);

    public void Deconstruct(out T1 a, out T2 b) => (a, b) = (A, B);
    public override string ToString() => $"({A}, {B})";
    public override bool Equals(object obj) => obj is List2Item<T1, T2> other && Equals(other);
    public bool Equals(List2Item<T1, T2> other) =>
        EqualityComparer<T1>.Default.Equals(A, other.A) &&
        EqualityComparer<T2>.Default.Equals(B, other.B);
    public override int GetHashCode() => HashCode.Combine(A, B);
}

public class IList2<T1, T2> : IEnumerable<List2Item<T1, T2>>
{
    private readonly List<List2Item<T1, T2>> _items = new();

    public int Count => _items.Count;

    public List2Item<T1, T2> this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(T1 a, T2 b) => _items.Add(new List2Item<T1, T2>(a, b));
    public void RemoveAt(int index) => _items.RemoveAt(index);
    public void Clear() => _items.Clear();

    public IEnumerator<List2Item<T1, T2>> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"IList2[{Count}]";
}


// ========================================
// IList3<T1, T2, T3> -> .A, .B, .C
// ========================================
public readonly struct List3Item<T1, T2, T3>
{
    public T1 A { get; }
    public T2 B { get; }
    public T3 C { get; }

    public List3Item(T1 a, T2 b, T3 c) => (A, B, C) = (a, b, c);

    public void Deconstruct(out T1 a, out T2 b, out T3 c) => (a, b, c) = (A, B, C);
    public override string ToString() => $"({A}, {B}, {C})";
    public override bool Equals(object obj) => obj is List3Item<T1, T2, T3> other && Equals(other);
    public bool Equals(List3Item<T1, T2, T3> other) =>
        EqualityComparer<T1>.Default.Equals(A, other.A) &&
        EqualityComparer<T2>.Default.Equals(B, other.B) &&
        EqualityComparer<T3>.Default.Equals(C, other.C);
    public override int GetHashCode() => HashCode.Combine(A, B, C);
}

public class IList3<T1, T2, T3> : IEnumerable<List3Item<T1, T2, T3>>
{
    private readonly List<List3Item<T1, T2, T3>> _items = new();

    public int Count => _items.Count;

    public List3Item<T1, T2, T3> this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(T1 a, T2 b, T3 c) => _items.Add(new List3Item<T1, T2, T3>(a, b, c));
    public void RemoveAt(int index) => _items.RemoveAt(index);
    public void Clear() => _items.Clear();

    public IEnumerator<List3Item<T1, T2, T3>> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"IList3[{Count}]";
}


// ========================================
// IList4<T1, T2, T3, T4> -> .A, .B, .C, .D
// ========================================
public readonly struct List4Item<T1, T2, T3, T4>
{
    public T1 A { get; }
    public T2 B { get; }
    public T3 C { get; }
    public T4 D { get; }

    public List4Item(T1 a, T2 b, T3 c, T4 d) => (A, B, C, D) = (a, b, c, d);

    public void Deconstruct(out T1 a, out T2 b, out T3 c, out T4 d) => (a, b, c, d) = (A, B, C, D);
    public override string ToString() => $"({A}, {B}, {C}, {D})";
    public override bool Equals(object obj) => obj is List4Item<T1, T2, T3, T4> other && Equals(other);
    public bool Equals(List4Item<T1, T2, T3, T4> other) =>
        EqualityComparer<T1>.Default.Equals(A, other.A) &&
        EqualityComparer<T2>.Default.Equals(B, other.B) &&
        EqualityComparer<T3>.Default.Equals(C, other.C) &&
        EqualityComparer<T4>.Default.Equals(D, other.D);
    public override int GetHashCode() => HashCode.Combine(A, B, C, D);
}

public class IList4<T1, T2, T3, T4> : IEnumerable<List4Item<T1, T2, T3, T4>>
{
    private readonly List<List4Item<T1, T2, T3, T4>> _items = new();

    public int Count => _items.Count;

    public List4Item<T1, T2, T3, T4> this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(T1 a, T2 b, T3 c, T4 d) => _items.Add(new List4Item<T1, T2, T3, T4>(a, b, c, d));
    public void RemoveAt(int index) => _items.RemoveAt(index);
    public void Clear() => _items.Clear();

    public IEnumerator<List4Item<T1, T2, T3, T4>> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"IList4[{Count}]";
}


// ========================================
// IDictionary2<TKey, T1, T2> -> .X, .Y
// ========================================
public readonly struct Dict2Item<T1, T2>
{
    public T1 X { get; }
    public T2 Y { get; }

    public Dict2Item(T1 x, T2 y) => (X, Y) = (x, y);

    public void Deconstruct(out T1 x, out T2 y) => (x, y) = (X, Y);
    public override string ToString() => $"({X}, {Y})";
    public override bool Equals(object obj) => obj is Dict2Item<T1, T2> other && Equals(other);
    public bool Equals(Dict2Item<T1, T2> other) =>
        EqualityComparer<T1>.Default.Equals(X, other.X) &&
        EqualityComparer<T2>.Default.Equals(Y, other.Y);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}

public class IDictionary2<TKey, T1, T2> : IEnumerable<(TKey Key, Dict2Item<T1, T2> Value)>
{
    private readonly Dictionary<TKey, Dict2Item<T1, T2>> _dict = new();

    public int Count => _dict.Count;

    public Dict2Item<T1, T2> this[TKey key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public void Add(TKey key, T1 x, T2 y) => _dict[key] = new Dict2Item<T1, T2>(x, y);

    public bool Remove(TKey key) => _dict.Remove(key);
    public void Clear() => _dict.Clear();

    public bool TryGetValue(TKey key, out Dict2Item<T1, T2> value) => _dict.TryGetValue(key, out value);

    public IEnumerable<TKey> Keys => _dict.Keys;
    public IEnumerable<Dict2Item<T1, T2>> Values => _dict.Values;
    public IEnumerable<(TKey, Dict2Item<T1, T2>)> Entries => _dict.Select(kv => (kv.Key, kv.Value));

    public IEnumerator<(TKey, Dict2Item<T1, T2>)> GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"IDictionary2[{Count}]";
}


// ========================================
// IDictionary3<TKey, T1, T2, T3> -> .X, .Y, .Z
// ========================================
public readonly struct Dict3Item<T1, T2, T3>
{
    public T1 X { get; }
    public T2 Y { get; }
    public T3 Z { get; }

    public Dict3Item(T1 x, T2 y, T3 z) => (X, Y, Z) = (x, y, z);

    public void Deconstruct(out T1 x, out T2 y, out T3 z) => (x, y, z) = (X, Y, Z);
    public override string ToString() => $"({X}, {Y}, {Z})";
    public override bool Equals(object obj) => obj is Dict3Item<T1, T2, T3> other && Equals(other);
    public bool Equals(Dict3Item<T1, T2, T3> other) =>
        EqualityComparer<T1>.Default.Equals(X, other.X) &&
        EqualityComparer<T2>.Default.Equals(Y, other.Y) &&
        EqualityComparer<T3>.Default.Equals(Z, other.Z);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
}

public class IDictionary3<TKey, T1, T2, T3> : IEnumerable<(TKey Key, Dict3Item<T1, T2, T3> Value)>
{
    private readonly Dictionary<TKey, Dict3Item<T1, T2, T3>> _dict = new();

    public int Count => _dict.Count;

    public Dict3Item<T1, T2, T3> this[TKey key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public void Add(TKey key, T1 x, T2 y, T3 z) => _dict[key] = new Dict3Item<T1, T2, T3>(x, y, z);

    public bool Remove(TKey key) => _dict.Remove(key);
    public void Clear() => _dict.Clear();

    public bool TryGetValue(TKey key, out Dict3Item<T1, T2, T3> value) => _dict.TryGetValue(key, out value);

    public IEnumerable<TKey> Keys => _dict.Keys;
    public IEnumerable<Dict3Item<T1, T2, T3>> Values => _dict.Values;
    public IEnumerable<(TKey, Dict3Item<T1, T2, T3>)> Entries => _dict.Select(kv => (kv.Key, kv.Value));

    public IEnumerator<(TKey, Dict3Item<T1, T2, T3>)> GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"IDictionary3[{Count}]";
}


// ========================================
// IDictionary4<TKey, T1, T2, T3, T4> -> .X, .Y, .Z, .W
// ========================================
public readonly struct Dict4Item<T1, T2, T3, T4>
{
    public T1 X { get; }
    public T2 Y { get; }
    public T3 Z { get; }
    public T4 W { get; }

    public Dict4Item(T1 x, T2 y, T3 z, T4 w) => (X, Y, Z, W) = (x, y, z, w);

    public void Deconstruct(out T1 x, out T2 y, out T3 z, out T4 w) => (x, y, z, w) = (X, Y, Z, W);
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
    public override bool Equals(object obj) => obj is Dict4Item<T1, T2, T3, T4> other && Equals(other);
    public bool Equals(Dict4Item<T1, T2, T3, T4> other) =>
        EqualityComparer<T1>.Default.Equals(X, other.X) &&
        EqualityComparer<T2>.Default.Equals(Y, other.Y) &&
        EqualityComparer<T3>.Default.Equals(Z, other.Z) &&
        EqualityComparer<T4>.Default.Equals(W, other.W);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
}

public class IDictionary4<TKey, T1, T2, T3, T4> : IEnumerable<(TKey Key, Dict4Item<T1, T2, T3, T4> Value)>
{
    private readonly Dictionary<TKey, Dict4Item<T1, T2, T3, T4>> _dict = new();

    public int Count => _dict.Count;

    public Dict4Item<T1, T2, T3, T4> this[TKey key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public void Add(TKey key, T1 x, T2 y, T3 z, T4 w) => _dict[key] = new Dict4Item<T1, T2, T3, T4>(x, y, z, w);

    public bool Remove(TKey key) => _dict.Remove(key);
    public void Clear() => _dict.Clear();

    public bool TryGetValue(TKey key, out Dict4Item<T1, T2, T3, T4> value) => _dict.TryGetValue(key, out value);

    public IEnumerable<TKey> Keys => _dict.Keys;
    public IEnumerable<Dict4Item<T1, T2, T3, T4>> Values => _dict.Values;
    public IEnumerable<(TKey, Dict4Item<T1, T2, T3, T4>)> Entries => _dict.Select(kv => (kv.Key, kv.Value));

    public IEnumerator<(TKey, Dict4Item<T1, T2, T3, T4>)> GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"IDictionary4[{Count}]";
}
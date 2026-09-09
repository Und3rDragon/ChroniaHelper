using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.ChroniaHelperModule;

namespace ChroniaHelper.Utils;

/// <summary>
/// 语言名称、数字、ID名中间过渡类
/// </summary>
public class LanguageWrapper
{
    public Language Value { get; }

    private LanguageWrapper(Language lang) => Value = lang;

    // transform
    public static implicit operator LanguageWrapper(Language lang) => new(lang);
    public static implicit operator LanguageWrapper(int number) => new(Languages.Get(number));
    public static implicit operator LanguageWrapper(string id) => new(Languages.Get(id));

    public static implicit operator Language(LanguageWrapper wrapper) => wrapper?.Value ?? Languages.Default;
    public static implicit operator int(LanguageWrapper wrapper) => Languages.GetIndex(wrapper?.Value ?? Languages.Default);
    public static implicit operator string(LanguageWrapper wrapper) => Languages.GetID(wrapper?.Value ?? Languages.Default);

    // equalizer
    public static bool operator ==(LanguageWrapper a, LanguageWrapper b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Value == b.Value;
    }

    public static bool operator !=(LanguageWrapper a, LanguageWrapper b) => !(a == b);

    // LanguageWrapper vs int
    public static bool operator ==(LanguageWrapper a, int b)
    {
        if (a is null) return false;
        return (int)a == b;
    }

    public static bool operator !=(LanguageWrapper a, int b) => !(a == b);
    public static bool operator ==(int a, LanguageWrapper b) => b == a;
    public static bool operator !=(int a, LanguageWrapper b) => !(b == a);

    // LanguageWrapper vs string
    public static bool operator ==(LanguageWrapper a, string b)
    {
        if (a is null) return false;
        return (string)a == b;
    }

    public static bool operator !=(LanguageWrapper a, string b) => !(a == b);
    public static bool operator ==(string a, LanguageWrapper b) => b == a;
    public static bool operator !=(string a, LanguageWrapper b) => !(b == a);

    // integer operations
    public static LanguageWrapper operator +(LanguageWrapper a, int b)
    {
        if (a is null) return null;
        return new LanguageWrapper(Languages.Get((int)a + b));
    }

    public static LanguageWrapper operator +(int a, LanguageWrapper b)
    {
        if (b is null) return null;
        return new LanguageWrapper(Languages.Get(a + (int)b));
    }

    public static LanguageWrapper operator -(LanguageWrapper a, int b)
    {
        if (a is null) return null;
        return new LanguageWrapper(Languages.Get((int)a - b));
    }

    public static LanguageWrapper operator -(int a, LanguageWrapper b)
    {
        if (b is null) return null;
        return new LanguageWrapper(Languages.Get(a - (int)b));
    }

    public static LanguageWrapper operator ++(LanguageWrapper a)
    {
        if (a is null) return null;
        return new LanguageWrapper(Languages.Get((int)a + 1));
    }

    public static LanguageWrapper operator --(LanguageWrapper a)
    {
        if (a is null) return null;
        return new LanguageWrapper(Languages.Get((int)a - 1));
    }

    public static bool operator <(LanguageWrapper a, int b)
    {
        if (a is null) return false;
        return (int)a < b;
    }

    public static bool operator <(int a, LanguageWrapper b)
    {
        if (b is null) return false;
        return a < (int)b;
    }

    public static bool operator >(LanguageWrapper a, int b)
    {
        if (a is null) return false;
        return (int)a > b;
    }

    public static bool operator >(int a, LanguageWrapper b)
    {
        if (b is null) return false;
        return a > (int)b;
    }

    public static bool operator <=(LanguageWrapper a, int b)
    {
        if (a is null) return false;
        return (int)a <= b;
    }

    public static bool operator <=(int a, LanguageWrapper b)
    {
        if (b is null) return false;
        return a <= (int)b;
    }

    public static bool operator >=(LanguageWrapper a, int b)
    {
        if (a is null) return false;
        return (int)a >= b;
    }

    public static bool operator >=(int a, LanguageWrapper b)
    {
        if (b is null) return false;
        return a >= (int)b;
    }

    // string operations
    public static string operator +(LanguageWrapper a, string b)
    {
        if (a is null) return b ?? "";
        return (string)a + b;
    }

    public static string operator +(string a, LanguageWrapper b)
    {
        if (b is null) return a ?? "";
        return a + (string)b;
    }

    // basic overrides
    public override bool Equals(object obj)
    {
        return obj is LanguageWrapper other && Value == other.Value;
    }

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public override string ToString() => (string)this;
}

public static class Languages
{
    public static class Index
    {
        public const int English = 0;
        public const int Brazilian = 1;
        public const int French = 2;
        public const int German = 3;
        public const int Italian = 4;
        public const int Japanese = 5;
        public const int Korean = 6;
        public const int Russian = 7;
        public const int SimplifiedChinese = 8;
        public const int Spanish = 9;
    }

    public static class ID
    {
        public const string English = "english";
        public const string Brazilian = "brazilian";
        public const string French = "french";
        public const string German = "german";
        public const string Italian = "italian";
        public const string Japanese = "japanese";
        public const string Korean = "korean";
        public const string Russian = "russian";
        public const string SimplifiedChinese = "schinese";
        public const string Spanish = "spanish";
    }

    public static Language English => Dialog.Languages[ID.English];
    public static Language Brazilian => Dialog.Languages[ID.Brazilian];
    public static Language French => Dialog.Languages[ID.French];
    public static Language German => Dialog.Languages[ID.German];
    public static Language Italian => Dialog.Languages[ID.Italian];
    public static Language Japanese => Dialog.Languages[ID.Japanese];
    public static Language Korean => Dialog.Languages[ID.Korean];
    public static Language Russian => Dialog.Languages[ID.Russian];
    public static Language SimplifiedChinese => Dialog.Languages[ID.SimplifiedChinese];
    public static Language Spanish => Dialog.Languages[ID.Spanish];

    public static Language Default => English;
    public static Language Current => Dialog.Language;

    private static readonly Dictionary<int, string> _idByIndex = new()
    {
        { Index.English, ID.English },
        { Index.Brazilian, ID.Brazilian },
        { Index.French, ID.French },
        { Index.German, ID.German },
        { Index.Italian, ID.Italian },
        { Index.Japanese, ID.Japanese },
        { Index.Korean, ID.Korean },
        { Index.Russian, ID.Russian },
        { Index.SimplifiedChinese, ID.SimplifiedChinese },
        { Index.Spanish, ID.Spanish },
    };

    private static readonly Dictionary<string, int> _indexById;
    private static readonly Dictionary<int, Language> _langByIndex;
    private static readonly Dictionary<string, Language> _langById;
    private static readonly Dictionary<Language, int> _indexByLang;
    private static readonly Dictionary<Language, string> _idByLang;

    static Languages()
    {
        _indexById = _idByIndex.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        _langByIndex = new();
        _langById = new();
        _indexByLang = new();
        _idByLang = new();

        foreach (var kvp in _idByIndex)
        {
            int index = kvp.Key;
            string id = kvp.Value;
            Language lang = Dialog.Languages[id];

            _langByIndex[index] = lang;
            _langById[id] = lang;
            _indexByLang[lang] = index;
            _idByLang[lang] = id;
        }

        // full name of simplified chinese
        _langById["simplifiedchinese"] = Languages.SimplifiedChinese;
    }

    public static Language Get(int index)
        => _langByIndex.TryGetValue(index, out Language lang) ? lang : Default;

    public static Language Get(string id)
        => !string.IsNullOrEmpty(id) && _langById.TryGetValue(id, out Language lang) ? lang : Default;

    public static Language Get(Language lang)
        => lang ?? Default;

    public static int GetIndex(Language lang)
        => lang is not null && _indexByLang.TryGetValue(lang, out int index) ? index : Index.English;

    public static string GetID(Language lang)
        => lang is not null && _idByLang.TryGetValue(lang, out string id) ? id : ID.English;

    public static int GetIndex(string id)
        => !string.IsNullOrEmpty(id) && _indexById.TryGetValue(id, out int index) ? index : Index.English;

    public static string GetID(int index)
        => _idByIndex.TryGetValue(index, out string id) ? id : ID.English;

    public static bool IsValid(int index) => _idByIndex.ContainsKey(index);
    public static bool IsValid(string id) => !string.IsNullOrEmpty(id) && _indexById.ContainsKey(id);
    public static bool IsValid(Language lang) => lang is not null && _indexByLang.ContainsKey(lang);
}
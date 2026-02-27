using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.ChroniaHelperModule;

namespace ChroniaHelper.Utils;

public enum LanguageIndexs
{
    English = 0,
    Brazilian = 1,
    French = 2,
    German = 3,
    Italian = 4,
    Japanese = 5,
    Korean = 6,
    Russian = 7,
    SimplifiedChinese = 8,
    Spanish = 9,
}

public static class Languages
{
    public static Language English => Dialog.Languages["english"];
    public static Language Brazillian => Dialog.Languages["brazilian"];
    public static Language French => Dialog.Languages["french"];
    public static Language German => Dialog.Languages["german"];
    public static Language Italian => Dialog.Languages["italian"];
    public static Language Japanese => Dialog.Languages["japanese"];
    public static Language Korean => Dialog.Languages["korean"];
    public static Language Russian => Dialog.Languages["russian"];
    public static Language SimplifiedChinese => Dialog.Languages["schinese"];
    public static Language Spanish => Dialog.Languages["spanish"];

    public static Dictionary<LanguageIndexs, string> LanguageID = new()
    {
        { LanguageIndexs.English, "english" },
        { LanguageIndexs.Brazilian, "brazilian" },
        { LanguageIndexs.French, "french" },
        { LanguageIndexs.German, "german" },
        { LanguageIndexs.Italian, "italian" },
        { LanguageIndexs.Japanese, "japanese" },
        { LanguageIndexs.Korean, "korean" },
        { LanguageIndexs.Russian, "russian" },
        { LanguageIndexs.SimplifiedChinese, "schinese" },
        { LanguageIndexs.Spanish, "spanish" },
    };
}

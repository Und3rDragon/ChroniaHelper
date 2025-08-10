using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace ChroniaHelper.Utils;

public enum EaseMode
{
    None = 0,
    Linear = 1,
    SineIn = 2,
    SineOut = 3,
    SineInOut = 4,
    QuadIn = 5,
    QuadOut = 6,
    QuadInOut = 7,
    CubeIn = 8,
    CubeOut = 9,
    CubeInOut = 10,
    QuintIn = 11,
    QuintOut = 12,
    QuintInOut = 13,
    ExpoIn = 14,
    ExpoOut = 15,
    ExpoInOut = 16,
    BackIn = 17,
    BackOut = 18,
    BackInOut = 19,
    BigBackIn = 20,
    BigBackOut = 21,
    BigBackInOut = 22,
    ElasticIn = 23,
    ElasticOut = 24,
    ElasticInOut = 25,
    BounceIn = 26,
    BounceOut = 27,
    BounceInOut = 28
}

public static class EaseUtils
{

    public static Dictionary<EaseMode, Func<float, float>> EaseDictionary = new Dictionary<EaseMode, Func<float, float>>()
    {
        {EaseMode.None, (value) => value },
        {EaseMode.Linear, (value) => Ease.Linear(value) },
        {EaseMode.SineIn, (value) => Ease.SineIn(value) },
        {EaseMode.SineOut, (value) => Ease.SineOut(value) },
        {EaseMode.SineInOut, (value) => Ease.SineInOut(value) },
        {EaseMode.QuadIn, (value) => Ease.QuadIn(value) },
        {EaseMode.QuadOut, (value) => Ease.QuadOut(value) },
        {EaseMode.QuadInOut, (value) => Ease.QuadInOut(value) },
        {EaseMode.CubeIn, (value) => Ease.CubeIn(value) },
        {EaseMode.CubeOut, (value) => Ease.CubeOut(value) },
        {EaseMode.CubeInOut, (value) => Ease.CubeInOut(value) },
        {EaseMode.QuintIn, (value) => Ease.QuintIn(value) },
        {EaseMode.QuintOut, (value) => Ease.QuintOut(value) },
        {EaseMode.QuintInOut, (value) => Ease.QuintInOut(value) },
        {EaseMode.ExpoIn, (value) => Ease.ExpoIn(value) },
        {EaseMode.ExpoOut, (value) => Ease.ExpoOut(value) },
        {EaseMode.ExpoInOut, (value) => Ease.ExpoInOut(value) },
        {EaseMode.BackIn, (value) => Ease.BackIn(value) },
        {EaseMode.BackOut, (value) => Ease.BackOut(value) },
        {EaseMode.BackInOut, (value) => Ease.BackInOut(value) },
        {EaseMode.BigBackIn, (value) => Ease.BigBackIn(value) },
        {EaseMode.BigBackOut, (value) => Ease.BigBackOut(value) },
        {EaseMode.BigBackInOut, (value) => Ease.BigBackInOut(value) },
        {EaseMode.ElasticIn, (value) => Ease.ElasticIn(value) },
        {EaseMode.ElasticOut, (value) => Ease.ElasticOut(value) },
        {EaseMode.ElasticInOut, (value) => Ease.ElasticInOut(value) },
        {EaseMode.BounceIn, (value) => Ease.BounceIn(value) },
        {EaseMode.BounceOut, (value) => Ease.BounceOut(value) },
        {EaseMode.BounceInOut, (value) => Ease.BounceInOut(value) }
    };

    public static float GetEaser(EaseMode easeMode, float value)
    {
        return (EaseUtils.EaseDictionary.TryGetValue(easeMode, out Func<float, float> func)) ? func(value) : 0F;
    }

    public static Dictionary<EaseMode, Ease.Easer> EaseMatch = new Dictionary<EaseMode, Ease.Easer>
    {
        {EaseMode.Linear, Ease.Linear },
        {EaseMode.SineIn, Ease.SineIn },
        {EaseMode.SineOut,  Ease.SineOut },
        {EaseMode.SineInOut,  Ease.SineInOut },
        {EaseMode.QuadIn,  Ease.QuadIn },
        {EaseMode.QuadOut,  Ease.QuadOut },
        {EaseMode.QuadInOut,  Ease.QuadInOut },
        {EaseMode.CubeIn,  Ease.CubeIn },
        {EaseMode.CubeOut,  Ease.CubeOut },
        {EaseMode.CubeInOut,  Ease.CubeInOut },
        {EaseMode.QuintIn,  Ease.QuintIn },
        {EaseMode.QuintOut,  Ease.QuintOut },
        {EaseMode.QuintInOut,  Ease.QuintInOut },
        {EaseMode.ExpoIn,  Ease.ExpoIn },
        {EaseMode.ExpoOut,  Ease.ExpoOut },
        {EaseMode.ExpoInOut,  Ease.ExpoInOut },
        {EaseMode.BackIn,  Ease.BackIn },
        {EaseMode.BackOut,  Ease.BackOut },
        {EaseMode.BackInOut,  Ease.BackInOut },
        {EaseMode.BigBackIn,  Ease.BigBackIn },
        {EaseMode.BigBackOut,  Ease.BigBackOut },
        {EaseMode.BigBackInOut,  Ease.BigBackInOut },
        {EaseMode.ElasticIn,  Ease.ElasticIn },
        {EaseMode.ElasticOut,  Ease.ElasticOut },
        {EaseMode.ElasticInOut,  Ease.ElasticInOut },
        {EaseMode.BounceIn,  Ease.BounceIn },
        {EaseMode.BounceOut,  Ease.BounceOut },
        {EaseMode.BounceInOut,  Ease.BounceInOut }
    };

    public static Ease.Easer StringToEase (string str)
    {
        switch (str.ToLower())
        {
            case "linear":
                return Ease.Linear;
            case "sinein":
                return Ease.SineIn;
            case "sineout":
                return Ease.SineOut;
            case "sineinout":
                return Ease.SineInOut;
            case "quadin":
                return Ease.QuadIn;
            case "quadout":
                return Ease.QuadOut;
            case "quadinout":
                return Ease.QuadInOut;
            case "cubein":
                return Ease.CubeIn;
            case "cubeout":
                return Ease.CubeOut;
            case "cubeinout":
                return Ease.CubeInOut;
            case "quintin":
                return Ease.QuintIn;
            case "quintout":
                return Ease.QuintOut;
            case "quintinout":
                return Ease.QuintInOut;
            case "expoin":
                return Ease.ExpoIn;
            case "expoout":
                return Ease.ExpoOut;
            case "expoinout":
                return Ease.ExpoInOut;
            case "backin":
                return Ease.BackIn;
            case "backout":
                return Ease.BackOut;
            case "backinout":
                return Ease.BackInOut;
            case "bigbackin":
                return Ease.BigBackIn;
            case "bigbackout":
                return Ease.BigBackOut;
            case "bigbackinout":
                return Ease.BigBackInOut;
            case "elasticin":
                return Ease.ElasticIn;
            case "elasticout":
                return Ease.ElasticOut;
            case "elasticinout":
                return Ease.ElasticInOut;
            case "bouncein":
                return Ease.BounceIn;
            case "bounceout":
                return Ease.BounceOut;
            case "bounceinout": 
                return Ease.BounceInOut;
            default:
                return Ease.Linear;
        }
    }

    public static EaseMode StringToEaseMode(string str)
    {
        switch (str.ToLower())
        {
            case "linear":
                return EaseMode.Linear;
            case "sinein":
                return EaseMode.SineIn;
            case "sineout":
                return EaseMode.SineOut;
            case "sineinout":
                return EaseMode.SineInOut;
            case "quadin":
                return EaseMode.QuadIn;
            case "quadout":
                return EaseMode.QuadOut;
            case "quadinout":
                return EaseMode.QuadInOut;
            case "cubein":
                return EaseMode.CubeIn;
            case "cubeout":
                return EaseMode.CubeOut;
            case "cubeinout":
                return EaseMode.CubeInOut;
            case "quintin":
                return EaseMode.QuintIn;
            case "quintout":
                return EaseMode.QuintOut;
            case "quintinout":
                return EaseMode.QuintInOut;
            case "expoin":
                return EaseMode.ExpoIn;
            case "expoout":
                return EaseMode.ExpoOut;
            case "expoinout":
                return EaseMode.ExpoInOut;
            case "backin":
                return EaseMode.BackIn;
            case "backout":
                return EaseMode.BackOut;
            case "backinout":
                return EaseMode.BackInOut;
            case "bigbackin":
                return EaseMode.BigBackIn;
            case "bigbackout":
                return EaseMode.BigBackOut;
            case "bigbackinout":
                return EaseMode.BigBackInOut;
            case "elasticin":
                return EaseMode.ElasticIn;
            case "elasticout":
                return EaseMode.ElasticOut;
            case "elasticinout":
                return EaseMode.ElasticInOut;
            case "bouncein":
                return EaseMode.BounceIn;
            case "bounceout":
                return EaseMode.BounceOut;
            case "bounceinout":
                return EaseMode.BounceInOut;
            default:
                return EaseMode.Linear;
        }
    }

}

using System;
using System.Collections.Generic;

namespace YoctoHelper.Cores;

public static class EaseUtils
{

    public static readonly Dictionary<EaseModes, Func<float, float>> EaseDictionary = new Dictionary<EaseModes, Func<float, float>>()
    {
        {EaseModes.None, (value) => 0 },
        {EaseModes.Linear, (value) => Ease.Linear(value) },
        {EaseModes.SineIn, (value) => Ease.SineIn(value) },
        {EaseModes.SineOut, (value) => Ease.SineOut(value) },
        {EaseModes.SineInOut, (value) => Ease.SineInOut(value) },
        {EaseModes.QuadIn, (value) => Ease.QuadIn(value) },
        {EaseModes.QuadOut, (value) => Ease.QuadOut(value) },
        {EaseModes.QuadInOut, (value) => Ease.QuadInOut(value) },
        {EaseModes.CubeIn, (value) => Ease.CubeIn(value) },
        {EaseModes.CubeOut, (value) => Ease.CubeOut(value) },
        {EaseModes.CubeInOut, (value) => Ease.CubeInOut(value) },
        {EaseModes.QuintIn, (value) => Ease.QuintIn(value) },
        {EaseModes.QuintOut, (value) => Ease.QuintOut(value) },
        {EaseModes.QuintInOut, (value) => Ease.QuintInOut(value) },
        {EaseModes.ExpoIn, (value) => Ease.ExpoIn(value) },
        {EaseModes.ExpoOut, (value) => Ease.ExpoOut(value) },
        {EaseModes.ExpoInOut, (value) => Ease.ExpoInOut(value) },
        {EaseModes.BackIn, (value) => Ease.BackIn(value) },
        {EaseModes.BackOut, (value) => Ease.BackOut(value) },
        {EaseModes.BackInOut, (value) => Ease.BackInOut(value) },
        {EaseModes.BigBackIn, (value) => Ease.BigBackIn(value) },
        {EaseModes.BigBackOut, (value) => Ease.BigBackOut(value) },
        {EaseModes.BigBackInOut, (value) => Ease.BigBackInOut(value) },
        {EaseModes.ElasticIn, (value) => Ease.ElasticIn(value) },
        {EaseModes.ElasticOut, (value) => Ease.ElasticOut(value) },
        {EaseModes.ElasticInOut, (value) => Ease.ElasticInOut(value) },
        {EaseModes.BounceIn, (value) => Ease.BounceIn(value) },
        {EaseModes.BounceOut, (value) => Ease.BounceOut(value) },
        {EaseModes.BounceInOut, (value) => Ease.BounceInOut(value) }
    };

    public static float GetEaser(this EaseModes easeMode, float value)
    {
        return (EaseUtils.EaseDictionary.TryGetValue(easeMode, out Func<float, float> func)) ? func(value) : 0F;
    }

}

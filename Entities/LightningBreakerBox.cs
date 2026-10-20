using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/LightningBreakerBoxWithNoBloomEffect")]
public class LightningBreakerBoxWithNoBloomEffect : LightningBreakerBox
{
    private const float DefaultBloomPulse = 1.2f;

    private const float DefaultBloomBreak = 1.5f;

    private const float DefaultBloomRest = 1f;

    internal static readonly Color DefaultBurst = Calc.HexToColor("FFFC75");

    internal static readonly Color DefaultBurst2 = Calc.HexToColor("6BFFFF");

    internal static readonly Color DefaultFence = Calc.HexToColor("B9FEFE");

    internal static readonly Color DefaultFence2 = Calc.HexToColor("FFF263");

    [SelectiveLoadHook]
    public static void Load()
    {
        On.Celeste.Lightning.SetPulseValue += HookSetPulseValue;
        On.Celeste.Lightning.SetBreakValue += HookSetBreakValue;
        On.Celeste.Lightning.PulseRoutine += HookPulseRoutine;
        On.Celeste.Lightning.RemoveRoutine += HookRemoveRoutine;
    }

    [SelectiveUnloadHook]
    public static void Unload()
    {
        On.Celeste.Lightning.SetPulseValue -= HookSetPulseValue;
        On.Celeste.Lightning.SetBreakValue -= HookSetBreakValue;
        On.Celeste.Lightning.PulseRoutine -= HookPulseRoutine;
        On.Celeste.Lightning.RemoveRoutine -= HookRemoveRoutine;
        Profiles.Clear();
    }

    public LightningBreakerBoxWithNoBloomEffect(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        Ldm.LoadHook(typeof(LightningBreakerBoxWithNoBloomEffect));

        this.bloomPulse = Calc.Clamp(data.Float("bloomPulse", DefaultBloomPulse), 0f, 8f);
        this.bloomBreak = Calc.Clamp(data.Float("bloomBreak", DefaultBloomBreak), 0f, 8f);
        this.bloomRest = Calc.Clamp(data.Float("bloomRest", DefaultBloomRest), 0f, 8f);
        this.BurstColor = ParseColor(data.Attr("burstColor"), DefaultBurst);
        this.BurstColor2 = ParseColor(data.Attr("burstColor2"), DefaultBurst2);
        this.FenceColor = ParseColor(data.Attr("fenceColor"), DefaultFence);
        this.FenceColor2 = ParseColor(data.Attr("fenceColor2"), DefaultFence2);

        int hits = Calc.Clamp(data.Int("hits", 2), 1, 9);
        if (HealthField != null)
        {
            HealthField.SetValue(this, hits);
        }
        else
        {
            Log.Warn("LightningBreakerBox.health not found, hits falls back to vanilla");
        }

        this.ApplyParticleColors();
        this.OnDashCollide = this.TunableDashed;
    }

    public bool HasVanillaColors
    {
        get
        {
            return this.BurstColor == DefaultBurst && this.BurstColor2 == DefaultBurst2
                && this.FenceColor == DefaultFence && this.FenceColor2 == DefaultFence2;
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (particleOwner != null && particleOwner != this && particleOwner.Scene != scene)
        {
            RestoreParticleColors();
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (particleOwner == this)
        {
            RestoreParticleColors();
        }
    }

    private DashCollisionResults TunableDashed(Player player, Vector2 dir)
    {
        if (Scene is Level level)
        {
            Set(level, this.bloomPulse, this.bloomBreak, this.bloomRest);
        }

        return base.Dashed(player, dir);
    }

    /// <summary>
    /// 登记本关的 bloom 参数，供随后的撞击脉冲与破损脉冲使用；只保留当前关卡的配置。
    /// </summary>
    public static void Set(Level level, float pulse, float breakValue, float rest)
    {
        if (level == null)
        {
            return;
        }

        Profiles.Clear();
        Profiles[level] = new Profile(pulse, breakValue, rest);
    }

    private static bool TryGetCustomProfile(Level level, out Profile profile)
    {
        profile = default(Profile);
        return level != null && Profiles.TryGetValue(level, out profile) && !profile.IsBloomVanilla;
    }

    private static void ApplyBloom(Level level, float t, float strength, float glitchMax, float fadeMax)
    {
        Glitch.Value = MathHelper.Lerp(0f, glitchMax, t);
        level.Bloom.Strength = strength;

        LightningRenderer renderer = level.Tracker.GetEntity<LightningRenderer>();
        if (renderer != null)
        {
            renderer.Fade = t * fadeMax;
        }
    }

    /// <summary>
    /// 撞击瞬间的短脉冲：本关已登记该电箱的配置时，用配置的起止值替换默认的 bloom 区间。
    /// </summary>
    private static void HookSetPulseValue(On.Celeste.Lightning.orig_SetPulseValue orig, Level level, float t)
    {
        if (!TryGetCustomProfile(level, out Profile profile))
        {
            orig(level, t);
            return;
        }

        ApplyBloom(level, t, MathHelper.Lerp(profile.Rest, profile.Pulse, t), 0.075f, 0.2f);
    }

    /// <summary>
    /// 电箱破损瞬间的强脉冲：本关已登记该电箱的配置时，用配置的起止值替换默认的 bloom 区间。
    /// </summary>
    private static void HookSetBreakValue(On.Celeste.Lightning.orig_SetBreakValue orig, Level level, float t)
    {
        if (!TryGetCustomProfile(level, out Profile profile))
        {
            orig(level, t);
            return;
        }

        ApplyBloom(level, t, MathHelper.Lerp(profile.Rest, profile.Break, t), 0.15f, 0.6f);
    }

    /// <summary>
    /// 脉冲动画结束后注销本关的临时配置，避免后续房间继续沿用。
    /// </summary>
    private static IEnumerator HookPulseRoutine(On.Celeste.Lightning.orig_PulseRoutine orig, Level level)
    {
        return RunThenUnregister(orig(level), level);
    }

    /// <summary>
    /// 电网消失动画结束后注销本关的临时配置，避免后续房间继续沿用。
    /// </summary>
    private static IEnumerator HookRemoveRoutine(On.Celeste.Lightning.orig_RemoveRoutine orig, Level level, Action onComplete)
    {
        return RunThenUnregister(orig(level, onComplete), level);
    }

    private static IEnumerator RunThenUnregister(IEnumerator routine, Level level)
    {
        while (routine.MoveNext())
        {
            yield return routine.Current;
        }

        Profiles.Remove(level);
    }

    /// <summary>
    /// 按本实体的颜色重建电箱与电网的粒子类型；使用原版默认颜色时保持原样。
    /// </summary>
    private void ApplyParticleColors()
    {
        if (this.HasVanillaColors)
        {
            return;
        }

        if (LightningBreakerBox.P_Smash == null || LightningBreakerBox.P_Sparks == null || Lightning.P_Shatter == null)
        {
            Log.Warn("particle types are not initialized yet, particle colors skipped");
            return;
        }

        if (particleOwner == null || particleOwner == this)
        {
            originalSmash ??= LightningBreakerBox.P_Smash;
            originalSparks ??= LightningBreakerBox.P_Sparks;
            originalShatter ??= Lightning.P_Shatter;
        }

        particleOwner = this;
        LightningBreakerBox.P_Smash = Clone(originalSmash, this.BurstColor, this.BurstColor2);
        LightningBreakerBox.P_Sparks = Clone(originalSparks, this.BurstColor, this.BurstColor2);
        Lightning.P_Shatter = Clone(originalShatter, this.FenceColor, this.FenceColor2);
    }

    internal static void RestoreParticleColors()
    {
        if (particleOwner == null)
        {
            return;
        }

        if (originalSmash != null)
        {
            LightningBreakerBox.P_Smash = originalSmash;
        }

        if (originalSparks != null)
        {
            LightningBreakerBox.P_Sparks = originalSparks;
        }

        if (originalShatter != null)
        {
            Lightning.P_Shatter = originalShatter;
        }

        particleOwner = null;
    }

    private static ParticleType Clone(ParticleType source, Color color, Color color2)
    {
        return new ParticleType(source)
        {
            Color = color,
            Color2 = color2
        };
    }

    /// <summary>
    /// 解析 6 位十六进制颜色（可带 #）；格式不合法时返回 fallback。
    /// </summary>
    internal static Color ParseColor(string value, Color fallback)
    {
        string hex = (value ?? string.Empty).Trim();
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        if (hex.Length != 6)
        {
            return fallback;
        }

        for (int i = 0; i < hex.Length; i++)
        {
            if (!Uri.IsHexDigit(hex[i]))
            {
                return fallback;
            }
        }

        try
        {
            return Calc.HexToColor(hex);
        }
        catch
        {
            return fallback;
        }
    }

    private readonly struct Profile
    {
        public Profile(float pulse, float breakValue, float rest)
        {
            this.Pulse = pulse;
            this.Break = breakValue;
            this.Rest = rest;
        }

        public bool IsBloomVanilla
        {
            get
            {
                return this.Pulse == DefaultBloomPulse && this.Break == DefaultBloomBreak && this.Rest == DefaultBloomRest;
            }
        }

        public readonly float Pulse;

        public readonly float Break;

        public readonly float Rest;
    }

    private static readonly Dictionary<Level, Profile> Profiles = new Dictionary<Level, Profile>();

    private static readonly FieldInfo HealthField = typeof(LightningBreakerBox).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic);

    private static ParticleType originalSmash;

    private static ParticleType originalSparks;

    private static ParticleType originalShatter;

    private static LightningBreakerBoxWithNoBloomEffect particleOwner;

    private readonly float bloomPulse;

    private readonly float bloomBreak;

    private readonly float bloomRest;

    internal readonly Color BurstColor;

    internal readonly Color BurstColor2;

    internal readonly Color FenceColor;

    internal readonly Color FenceColor2;
}

using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/LightningBreakerBoxWithNoBloomEffect")]
public class LightningBreakerBoxWithNoBloomEffect : LightningBreakerBox
{
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

    public bool HasVanillaColors
    {
        get
        {
            return this.BurstColor == LightningBreakerBoxWithNoBloomEffect.DefaultBurst && this.BurstColor2 == LightningBreakerBoxWithNoBloomEffect.DefaultBurst2 && this.FenceColor == LightningBreakerBoxWithNoBloomEffect.DefaultFence && this.FenceColor2 == LightningBreakerBoxWithNoBloomEffect.DefaultFence2;
        }
    }

    public LightningBreakerBoxWithNoBloomEffect(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        Ldm.LoadHook(typeof(LightningBreakerBoxWithNoBloomEffect));

        this.hits = Calc.Clamp(data.Int("hits", 2), 1, 9);
        this.bloomPulse = Calc.Clamp(data.Float("bloomPulse", 1.2f), 0f, 8f);
        this.bloomBreak = Calc.Clamp(data.Float("bloomBreak", 1.5f), 0f, 8f);
        this.bloomRest = Calc.Clamp(data.Float("bloomRest", 1f), 0f, 8f);
        this.BurstColor = LightningBreakerBoxWithNoBloomEffect.ParseColorChecked(data, "burstColor", "FFFC75", LightningBreakerBoxWithNoBloomEffect.DefaultBurst);
        this.BurstColor2 = LightningBreakerBoxWithNoBloomEffect.ParseColorChecked(data, "burstColor2", "6BFFFF", LightningBreakerBoxWithNoBloomEffect.DefaultBurst2);
        this.FenceColor = LightningBreakerBoxWithNoBloomEffect.ParseColorChecked(data, "fenceColor", "B9FEFE", LightningBreakerBoxWithNoBloomEffect.DefaultFence);
        this.FenceColor2 = LightningBreakerBoxWithNoBloomEffect.ParseColorChecked(data, "fenceColor2", "FFF263", LightningBreakerBoxWithNoBloomEffect.DefaultFence2);
        if (LightningBreakerBoxWithNoBloomEffect.HealthField != null)
        {
            LightningBreakerBoxWithNoBloomEffect.HealthField.SetValue(this, this.hits);
        }
        else
        {
            Log.Warn("LightningBreakerBox.health not found, hits falls back to vanilla");
        }
        this.ApplyParticleColors();
        this.OnDashCollide = new DashCollision(this.TunableDashed);
    }

    private static readonly Dictionary<Level, Profile> Profiles = new Dictionary<Level, Profile>();
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
                return this.Pulse == 1.2f && this.Break == 1.5f && this.Rest == 1f;
            }
        }

        public readonly float Pulse;

        public readonly float Break;

        public readonly float Rest;
    }

    private DashCollisionResults TunableDashed(Player player, Vector2 dir)
    {
        Level level = base.Scene as Level;
        if (level != null)
        {
            Set(level, this.bloomPulse, this.bloomBreak, this.bloomRest);
        }
        return base.Dashed(player, dir);
    }

    public static void Set(Level level, float pulse, float breakValue, float rest)
    {
        if (Profiles.Count > 0)
        {
            List<Level> stale = null;
            foreach (Level key in Profiles.Keys)
            {
                if (key != level)
                {
                    List<Level> list;
                    if ((list = stale) == null)
                    {
                        list = (stale = new List<Level>());
                    }
                    list.Add(key);
                }
            }
            if (stale != null)
            {
                foreach (Level key2 in stale)
                {
                    Profiles.Remove(key2);
                }
            }
        }
    }

    /// <summary>
    /// 撞击瞬间的短脉冲：本关已登记该电箱的配置时，用配置的起止值替换默认的 bloom 区间。
    /// </summary>
    private static void HookSetPulseValue(On.Celeste.Lightning.orig_SetPulseValue orig, Level level, float t)
    {
        if (level is null || !Profiles.TryGetValue(level, out Profile profile) || profile.IsBloomVanilla)
        {
            orig(level, t);
            return;
        }

        Glitch.Value = MathHelper.Lerp(0f, 0.075f, t);
        level.Bloom.Strength = MathHelper.Lerp(profile.Rest, profile.Pulse, t);

        LightningRenderer renderer = level.Tracker.GetEntity<LightningRenderer>();
        if (renderer != null)
        {
            renderer.Fade = t * 0.2f;
        }
    }

    /// <summary>
    /// 电箱破损瞬间的强脉冲：本关已登记该电箱的配置时，用配置的起止值替换默认的 bloom 区间。
    /// </summary>
    private static void HookSetBreakValue(On.Celeste.Lightning.orig_SetBreakValue orig, Level level, float t)
    {
        if (level is null || !Profiles.TryGetValue(level, out Profile profile) || profile.IsBloomVanilla)
        {
            orig(level, t);
            return;
        }

        Glitch.Value = MathHelper.Lerp(0f, 0.15f, t);
        level.Bloom.Strength = MathHelper.Lerp(profile.Rest, profile.Break, t);

        LightningRenderer renderer = level.Tracker.GetEntity<LightningRenderer>();
        if (renderer != null)
        {
            renderer.Fade = t * 0.6f;
        }
    }

    /// <summary>
    /// 脉冲动画结束后注销本关的临时配置，避免后续房间继续沿用。
    /// </summary>
    private static IEnumerator HookPulseRoutine(On.Celeste.Lightning.orig_PulseRoutine orig, Level level)
    {
        IEnumerator routine = orig(level);
        while (routine.MoveNext())
        {
            yield return routine.Current;
        }

        Profiles.Remove(level);
    }

    /// <summary>
    /// 电网消失动画结束后注销本关的临时配置，避免后续房间继续沿用。
    /// </summary>
    private static IEnumerator HookRemoveRoutine(On.Celeste.Lightning.orig_RemoveRoutine orig, Level level, Action onComplete)
    {
        IEnumerator routine = orig(level, onComplete);
        while (routine.MoveNext())
        {
            yield return routine.Current;
        }

        Profiles.Remove(level);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (LightningBreakerBoxWithNoBloomEffect.particleOwner != null && LightningBreakerBoxWithNoBloomEffect.particleOwner != this && LightningBreakerBoxWithNoBloomEffect.particleOwner.Scene != scene)
        {
            LightningBreakerBoxWithNoBloomEffect.RestoreParticleColors();
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (LightningBreakerBoxWithNoBloomEffect.particleOwner == this)
        {
            LightningBreakerBoxWithNoBloomEffect.RestoreParticleColors();
        }
    }

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
        if (LightningBreakerBoxWithNoBloomEffect.particleOwner == null || LightningBreakerBoxWithNoBloomEffect.particleOwner == this)
        {
            if (LightningBreakerBoxWithNoBloomEffect.originalSmash == null)
            {
                LightningBreakerBoxWithNoBloomEffect.originalSmash = LightningBreakerBox.P_Smash;
            }
            if (LightningBreakerBoxWithNoBloomEffect.originalSparks == null)
            {
                LightningBreakerBoxWithNoBloomEffect.originalSparks = LightningBreakerBox.P_Sparks;
            }
            if (LightningBreakerBoxWithNoBloomEffect.originalShatter == null)
            {
                LightningBreakerBoxWithNoBloomEffect.originalShatter = Lightning.P_Shatter;
            }
        }
        LightningBreakerBoxWithNoBloomEffect.particleOwner = this;
        LightningBreakerBox.P_Smash = LightningBreakerBoxWithNoBloomEffect.Clone(LightningBreakerBoxWithNoBloomEffect.originalSmash, this.BurstColor, this.BurstColor2);
        LightningBreakerBox.P_Sparks = LightningBreakerBoxWithNoBloomEffect.Clone(LightningBreakerBoxWithNoBloomEffect.originalSparks, this.BurstColor, this.BurstColor2);
        Lightning.P_Shatter = LightningBreakerBoxWithNoBloomEffect.Clone(LightningBreakerBoxWithNoBloomEffect.originalShatter, this.FenceColor, this.FenceColor2);
        
        //LogLevel logLevel = LogLevel.Info;
        //LogLevel logLevel2 = logLevel;
        //string text = "PowerBoxHelper";
        //string text2 = text;
        //bool flag;
        //Logger.LogInterpolatedStringHandler logInterpolatedStringHandler = new Logger.LogInterpolatedStringHandler(44, 4, logLevel, text, out flag);
        //if (flag)
        //{
        //    logInterpolatedStringHandler.AppendLiteral("particle colors applied: burst=#");
        //    logInterpolatedStringHandler.AppendFormatted(LightningBreakerBoxWithNoBloomEffect.Hex(this.BurstColor));
        //    logInterpolatedStringHandler.AppendLiteral("/#");
        //    logInterpolatedStringHandler.AppendFormatted(LightningBreakerBoxWithNoBloomEffect.Hex(this.BurstColor2));
        //    logInterpolatedStringHandler.AppendLiteral(" fence=#");
        //    logInterpolatedStringHandler.AppendFormatted(LightningBreakerBoxWithNoBloomEffect.Hex(this.FenceColor));
        //    logInterpolatedStringHandler.AppendLiteral("/#");
        //    logInterpolatedStringHandler.AppendFormatted(LightningBreakerBoxWithNoBloomEffect.Hex(this.FenceColor2));
        //}
        //Logger.Log(logLevel2, text2, logInterpolatedStringHandler);
    }

    internal static void RestoreParticleColors()
    {
        if (LightningBreakerBoxWithNoBloomEffect.particleOwner == null)
        {
            return;
        }
        if (LightningBreakerBoxWithNoBloomEffect.originalSmash != null)
        {
            LightningBreakerBox.P_Smash = LightningBreakerBoxWithNoBloomEffect.originalSmash;
        }
        if (LightningBreakerBoxWithNoBloomEffect.originalSparks != null)
        {
            LightningBreakerBox.P_Sparks = LightningBreakerBoxWithNoBloomEffect.originalSparks;
        }
        if (LightningBreakerBoxWithNoBloomEffect.originalShatter != null)
        {
            Lightning.P_Shatter = LightningBreakerBoxWithNoBloomEffect.originalShatter;
        }
        LightningBreakerBoxWithNoBloomEffect.particleOwner = null;
    }

    private static ParticleType Clone(ParticleType source, Color color, Color color2)
    {
        return new ParticleType(source)
        {
            Color = color,
            Color2 = color2
        };
    }

    private static string Hex(Color c)
    {
        return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    private static Color ParseColorChecked(EntityData data, string key, string defaultHex, Color fallback)
    {
        string raw = data.Attr(key, defaultHex);
        Color color = LightningBreakerBoxWithNoBloomEffect.ParseColor(raw, fallback);
        string norm = (raw ?? string.Empty).Trim();
        if (norm.StartsWith("#"))
        {
            norm = norm.Substring(1);
        }
        if (color == fallback && norm.Length > 0 && !string.Equals(norm, defaultHex, StringComparison.OrdinalIgnoreCase))
        {
            //LogLevel logLevel = LogLevel.Warn;
            //LogLevel logLevel2 = logLevel;
            //string text = "PowerBoxHelper";
            //string text2 = text;
            //bool flag;
            //Logger.LogInterpolatedStringHandler logInterpolatedStringHandler = new Logger.LogInterpolatedStringHandler(56, 3, logLevel, text, out flag);
            //if (flag)
            //{
            //    logInterpolatedStringHandler.AppendLiteral("invalid color for ");
            //    logInterpolatedStringHandler.AppendFormatted(key);
            //    logInterpolatedStringHandler.AppendLiteral(": '");
            //    logInterpolatedStringHandler.AppendFormatted(raw);
            //    logInterpolatedStringHandler.AppendLiteral("' (expected RRGGBB), using default ");
            //    logInterpolatedStringHandler.AppendFormatted(defaultHex);
            //}
            //Logger.Log(logLevel2, text2, logInterpolatedStringHandler);
        }
        return color;
    }

    internal static Color ParseColor(string value, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }
        string hex = value.Trim();
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }
        if (hex.Length != 6)
        {
            return fallback;
        }
        string text = hex;
        for (int i = 0; i < text.Length; i++)
        {
            if (!Uri.IsHexDigit(text[i]))
            {
                return fallback;
            }
        }
        Color color;
        try
        {
            color = Calc.HexToColor(hex);
        }
        catch
        {
            color = fallback;
        }
        return color;
    }

    internal static readonly Color DefaultBurst = Calc.HexToColor("FFFC75");

    internal static readonly Color DefaultBurst2 = Calc.HexToColor("6BFFFF");

    internal static readonly Color DefaultFence = Calc.HexToColor("B9FEFE");

    internal static readonly Color DefaultFence2 = Calc.HexToColor("FFF263");

    private static readonly FieldInfo HealthField = typeof(LightningBreakerBox).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic);

    private static ParticleType originalSmash;

    private static ParticleType originalSparks;

    private static ParticleType originalShatter;

    private static LightningBreakerBoxWithNoBloomEffect particleOwner;

    private readonly int hits;

    private readonly float bloomPulse;

    private readonly float bloomBreak;

    private readonly float bloomRest;

    internal readonly Color BurstColor;

    internal readonly Color BurstColor2;

    internal readonly Color FenceColor;

    internal readonly Color FenceColor2;
}
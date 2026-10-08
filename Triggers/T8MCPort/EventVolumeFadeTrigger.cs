using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using FMOD.Studio;

namespace ChroniaHelper.Triggers.T8MCPort;

[CustomEntity("T8MC/EventVolumeFadeTrigger")]
[ExA.Credits("youner as original coder from T8MC group")]
public class EventVolumeFadeTrigger : BaseTrigger 
{
    [ExA.SelectiveLoadHook]
    public static void Load()
    {
        On.Celeste.Audio.CreateInstance += CreateInstance;
        On.Celeste.Level.Update += LevelUpdate;
        On.Celeste.Level.LoadLevel += LoadLevel;
        On.Celeste.Level.End += LevelEnd;
    }
    [ExA.SelectiveUnloadHook]
    public static void Unload()
    {
        On.Celeste.Audio.CreateInstance -= CreateInstance;
        On.Celeste.Level.Update -= LevelUpdate;
        On.Celeste.Level.LoadLevel -= LoadLevel;
        On.Celeste.Level.End -= LevelEnd;
        Reset();
    }

    private static EventInstance CreateInstance(On.Celeste.Audio.orig_CreateInstance orig,
        string path, Vector2? position)
    {
        EventInstance instance = orig(path, position);
        if (activeLevel != null && Engine.Scene == activeLevel && path != null &&
            rules.TryGetValue(path, out Rule rule))
        {
            Apply(instance, path, rule.Volume);
        }
        return instance;
    }

    private static void LevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        if (self == activeLevel)
            Refresh();
    }

    private static void LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self,
        Player.IntroTypes intro, bool isFromLoader)
    {
        // Death/respawn and room loads reset all multipliers to the entity's default.
        Reset();
        orig(self, intro, isFromLoader);
    }

    private static void LevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        if (self == activeLevel)
            Reset();
        orig(self);
    }

    private readonly string[] paths;
    private readonly float volumeFrom;
    private readonly float volumeTo;
    private PositionModes positionMode;

    public EventVolumeFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) 
    {
        paths = data.Attr("eventPaths", RedBoosterMove).Split(',',StringSplitOptions.TrimEntries);
        volumeFrom = data.Float("volumeFrom", 1f).Clamp(0f, 1f);
        volumeTo = data.Float("volumeTo", 0f).Clamp(0f, 1f);
        positionMode = data.Enum<PositionModes>("direction", PositionModes.LeftToRight);
        leaveReset = data.Bool("restoreOnLeave", false);

        Ldm.LoadHookWithMethod(this, Load);
    }

    public override void OnEnter(Player player) 
    {
        base.OnEnter(player);

        UpdateVolume(player);
    }

    public override void OnStay(Player player) 
    {
        base.OnStay(player);

        UpdateVolume(player);
    }

    public override void OnLeave(Player player) 
    {
        base.OnLeave(player);

        if (!leaveReset)
        {
            // Sample the exit position too, so crossing the edge reaches 0/1 exactly.
            UpdateVolume(player);
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.LeaveReset(player);

        Restore(this, paths);
    }

    private void UpdateVolume(Player player) {
        float t = GetPositionLerp(player, positionMode);
        Set(level, this, paths, t.LerpValue(0f, 1f, volumeFrom, volumeTo));
    }

    // FMOD Event paths identify sounds, not individual map entities.
    public const string RedBoosterMove = "event:/game/05_mirror_temple/redbooster_move";
    private static Level activeLevel;
    private static readonly Dictionary<string, Rule> rules = new(StringComparer.Ordinal);
    private static readonly Dictionary<IntPtr, TrackedInstance> instances = new();
    private static readonly List<IntPtr> staleHandles = new();

    private sealed class Rule
    {
        public EventVolumeFadeTrigger Owner;
        public float Volume;
        public EventDescription Description;
        public bool TriedResolve;
    }

    private sealed class TrackedInstance
    {
        public EventInstance Instance;
        public string Path;
        public VolumeBaseline Baseline;
    }

    internal static void Set(Level level, EventVolumeFadeTrigger owner, string[] paths, float volume)
    {
        if (activeLevel != level)
        {
            Reset();
            activeLevel = level;
        }
        foreach (string path in paths)
        {
            if (!rules.TryGetValue(path, out Rule rule))
            {
                rule = new Rule();
                rules.Add(path, rule);
            }
            rule.Owner = owner;
            rule.Volume = volume;
        }
        Refresh();
    }

    internal static void Restore(EventVolumeFadeTrigger owner, string[] paths)
    {
        foreach (string path in paths)
        {
            // A later trigger may have taken control of the same Event.
            if (rules.TryGetValue(path, out Rule rule) && rule.Owner == owner)
            {
                rules.Remove(path);
                RestorePath(path);
            }
        }
    }

    private static void Refresh()
    {
        foreach (var pair in rules)
        {
            Rule rule = pair.Value;
            if (!rule.TriedResolve)
            {
                rule.TriedResolve = true;
                try
                {
                    rule.Description = Audio.GetEventDescription(pair.Key);
                    if (rule.Description == null || !rule.Description.isValid())
                        Log.Warn("[T8MCAudioFade]FMOD Event not found: " + pair.Key);
                }
                catch (Exception error)
                {
                    Log.Warn("[T8MCAudioFade]Cannot resolve FMOD Event " + pair.Key + ": " + error.Message);
                }
            }
            // Also catches looping sounds already active before entering the trigger,
            // and instances created directly by FMOD instead of Audio.CreateInstance.
            if (rule.Description != null && rule.Description.isValid() &&
                rule.Description.getInstanceList(out EventInstance[] playing) == FMOD.RESULT.OK &&
                playing != null)
            {
                foreach (EventInstance instance in playing)
                    Apply(instance, pair.Key, rule.Volume);
            }
        }
        staleHandles.Clear();
        foreach (var pair in instances)
        {
            EventInstance instance = pair.Value.Instance;
            if (instance == null || !instance.isValid())
                staleHandles.Add(pair.Key);
        }
        foreach (IntPtr handle in staleHandles)
            instances.Remove(handle);
    }

    private static void Apply(EventInstance instance, string path, float volume)
    {
        if (instance == null || !instance.isValid() ||
            instance.getVolume(out float current, out _) != FMOD.RESULT.OK)
            return;
        IntPtr handle = instance.getRaw();
        if (!instances.TryGetValue(handle, out TrackedInstance tracked) || tracked.Path != path)
        {
            tracked = new TrackedInstance
            {
                Instance = instance,
                Path = path,
                Baseline = new VolumeBaseline(current)
            };
            instances[handle] = tracked;
        }
        instance.setVolume(tracked.Baseline.Apply(current, volume));
    }

    private static void RestoreInstance(TrackedInstance tracked)
    {
        if (tracked.Instance != null && tracked.Instance.isValid() &&
            tracked.Instance.getVolume(out float current, out _) == FMOD.RESULT.OK)
            tracked.Instance.setVolume(tracked.Baseline.Restore(current));
    }

    private static void RestorePath(string path)
    {
        staleHandles.Clear();
        foreach (var pair in instances)
        {
            if (pair.Value.Path == path)
            {
                RestoreInstance(pair.Value);
                staleHandles.Add(pair.Key);
            }
        }
        foreach (IntPtr handle in staleHandles)
            instances.Remove(handle);
    }

    private static void Reset()
    {
        foreach (TrackedInstance instance in instances.Values)
            RestoreInstance(instance);
        instances.Clear();
        rules.Clear();
        activeLevel = null;
    }
}

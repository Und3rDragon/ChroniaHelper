using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Celeste.Mod.ChroniaHelperIndicatorZone;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Cores.Graphical;
using ChroniaHelper.Cores.LiteTeraHelper;
using ChroniaHelper.Entities;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.StopwatchSystem;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;
using YoctoHelper.Components;
using YoctoHelper.Hooks;
using static ChroniaHelper.Entities.CustomBooster;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSession : EverestModuleSession
{

    [YamlIgnore] public readonly Dictionary<HookId, HookData> HookManagerData = new Dictionary<HookId, HookData>();

    [YamlIgnore] public readonly Dictionary<Color, List<DustBunnyEdge>> DustBunnyEdgeColor = new Dictionary<Color, List<DustBunnyEdge>>();

    [YamlIgnore] public Dictionary<string, FlagCarouselTrigger> FlagCarouselDictionary { get; private set; } = new Dictionary<string, FlagCarouselTrigger>();
    
    public bool TeraActive = false;
    
    public TeraType StartTera = TeraType.Any;

    // Frame Count Timer
    
    public int FrameTimer_Hours = 0, FrameTimer_Minutes = 0, FrameTimer_Seconds = 0, FrameTimer_timerFrames = 0, FrameTimer_Frames = 0;
    
    public string FrameTimer_Output;

    // AppleSheep Custom Timer
    public Dictionary<string, long> CustomTimer_TimeRecords { get; set; } = new Dictionary<string, long>();
    
    public long CustomTimer_RawTime;
    
    public long CustomTimer_Time;
    
    public bool CustomTimer_TimerStarted;
    
    public bool CustomTimer_TimerPaused;
    
    public bool CustomTimer_TimerCompleted;

    [YamlIgnore] public List<MTexture> PIZ_Icons { get; set; }
    [YamlIgnore] public List<Vector2> PIZ_IconOffsets { get; set; }
    [YamlIgnore] public List<Color> PIZ_IconColors { get; set; }
    public List<string> PIZ_IconsSave { get; set; }
    public List<Vector2> PIZ_IconOffsetsSave { get; set; }
    public List<string> PIZ_IconColorsSave { get; set; }
    public int PIZ_ZoneDepth { get; set; }

    public void ProcessZoneSaves()
    {
        if (PIZ_Icons is not null) return;
        PIZ_Icons = PIZ_IconsSave?.Select(s => GFX.Game[s]).ToList();
        PIZ_IconOffsets = PIZ_IconOffsetsSave;
        PIZ_IconColors = PIZ_IconColorsSave?.Select(Calc.HexToColor).ToList();
    }

    public void RecordZoneSave(PlayerIndicatorZone zone)
    {
        if (zone is null)
        {
            PIZ_Icons = null;
            PIZ_IconOffsets = null;
            PIZ_IconColors = null;
            PIZ_IconsSave = null;
            PIZ_IconOffsetsSave = null;
            PIZ_IconColorsSave = null;
            return;
        }
        PIZ_Icons = zone.Icons;
        PIZ_IconsSave = zone.Icons.Select(t => t.AtlasPath).ToList();
        PIZ_IconOffsets = zone.IconOffsets;
        PIZ_IconOffsetsSave = zone.IconOffsets;
        PIZ_IconColors = zone.IconColors;
        PIZ_IconColorsSave = zone.IconColors.Select(c => $"{c.R:X2}{c.G:X2}{c.B:X2}").ToList();
        PIZ_ZoneDepth = zone.Depth;
    }

    // Flag Button Data
    // Can be migrated but not necessary?
    public HashSet<string> FlagButtonStates = new();
    public HashSet<string> FlagButtonTargetFlags = new HashSet<string>();
    public Dictionary<int, int> FlagButtonFrameIndex = new Dictionary<int, int>();

    // Flag Carousel Trigger extended
    public Dictionary<string, bool> FlagCarouselState { get; set; } = new Dictionary<string, bool>();

    // Music Trigger Update
    public bool musicReset = false, musicStored = false;
    public OldMusic oldMusic;
    public struct OldMusic
    {
        public string musicTrack;

        public int musicProgress;

        public List<MEP> musicParameters;
    };

    // Sprite Entity saved parameters
    public bool se_DisableControl = false;
    public Dictionary<string, object> se_Variables = new Dictionary<string, object>();

    // Password Keyboard
    public Dictionary<EntityID, int> Passkeyboard_RemainingUses { get; set; } = new();
    public Dictionary<EntityID, int> Passkeyboard_PasswordQueue { get; set; } = new();

    public Dictionary<string,string> Passkeyboard_Passwords { get; set; } = new();

    // Platform Line Controller

    // Vanilla constants
    public static Color sinkLineEdgeColor = Calc.HexToColor("2a1923");
    public static Color sinkLineInnerColor = Calc.HexToColor("160b12");
    public static Color specialMoveLineEdgeColor = Calc.HexToColor("a4464a");
    public static Color specialMoveLineInnerColor = Calc.HexToColor("86354e");
    public static Color moveLineEdgeColor = Calc.HexToColor("2a1923");
    public static Color moveLineInnerColor = Calc.HexToColor("160b12");

    public bool modifySinkingPlatformLine = false, modifyMovingPlatformLine = false;
    public Color platformLine_SP_edgeColor = sinkLineEdgeColor, 
        platformLine_SP_centerColor = sinkLineInnerColor,
        platformLine_MP_edgeColor = moveLineEdgeColor, 
        platformLine_MP_centerColor = moveLineInnerColor;
    public int platformLine_SP_depth = 9001, platformLine_MP_depth = 9001;

    // Omni Zip Mover 2 records
    public Dictionary<int, int> Zipmover_NodeIndex = new();
    public Dictionary<int, bool> Zipmover_NextForward = new();

    // Flag Timer Trigger
    public Dictionary<string, float> FlagTimer = new();

    // GeometryUtils
    public Dictionary<string, HashSet<Rectangle>> Geometry_Rectangles = new();
    public Dictionary<string, List<Vector2>> Geometry_Points = new();
    public Dictionary<string, HashSet<Vector4>> Geometry_FreeRectangles = new();

    // Changed Room Flag Controller
    public HashSet<string> flagsWhenEnter = new();

    // Room Tag
    public List<string> roomTags_Rooms = new();
    public List<string> roomTags = new();
    public bool roomTagLoaded = false;

    // Solid Modifier Component
    [YamlIgnore]
    public SolidModifierComponent currentActiveSolidModifier = null;

    // Stopwatch
    public Dictionary<string, Stopclock> Stopclocks = new();

    // OperationCode Listener
    public Dictionary<int, OperationCodesListener.OperationCodeData> OperationCodeListeners = new();

    // HUD Controller
    public List<bool> HUDPrimaryState = new();
    public bool HUDStateRegistered = false;
    public string HUDStateRegister = string.Empty;

    // Condition Listener
    /// <summary>
    /// item1: condition, item2: constantly or changed, item3: flag operation
    /// item4: expression using, item5: target flag
    /// </summary>
    public struct SessionConditionListener
    {
        public string condition;
        /// <summary>
        /// Constantly = 0, WhenChanged = 1, WhenTrue = 2, WhenFalse = 3
        /// </summary>
        public int constant;
        /// <summary>
        /// On = 0, Off = 1, Switch = 2
        /// </summary>
        public int flagOperation;
        /// <summary>
        /// Flags = 0, ChroniaMathExpression = 1, FrostSessionExpression = 2
        /// </summary>
        public int usingExpression;
        /// <summary>
        /// Delay or Interval
        /// </summary>
        public float time;
        public string flag;
    }
    public Dictionary<string, SessionConditionListener> SessionConditionListeners = new();
    public Dictionary<string, bool> SessionConditionListener_LastState = new();
    public Dictionary<string, bool> SessionConditionListener_TimerState = new();
    public Dictionary<string, float> SessionConditionListener_Timer = new();

    // Fnt Textures
    [YamlIgnore]
    public Dictionary<string, FntData> cachedFntData = new();
    
    // Chronia Points Game
    public HashSet<string> DiscoveredRooms = new();

    // Code Button
    public struct CodeButtonTarget
    {
        public string codeString;
        public string flag;
        public bool needsEnterCheck;
        public bool deactivateFlagWhenNotStaisfied;
        
    }
    public Dictionary<string, CodeButtonTarget> codeButtonTargets = new();
    
    // Settings Override Controller
    public struct SettingsData
    {
        public bool photosensitive; 
        public bool fullScreen; 
        public int windowScale;
        public string language;
        public GrabModes grabMode;
    }
    public SettingsData settingsData = new Ses.SettingsData
    {
        fullScreen = Celeste.Settings.Instance.Fullscreen,
        photosensitive = Celeste.Settings.Instance.DisableFlashes,
        windowScale = Celeste.Settings.Instance.WindowScale,
        language = Celeste.Settings.Instance.Language,
        grabMode = Celeste.Settings.Instance.GrabMode,
    };

    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public HashSet<string> flagsPerRoom = new();
    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public HashSet<string> flagsPerDeath = new();
    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public Dictionary<string, int> countersPerRoom = new();
    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public Dictionary<string, int> countersPerDeath = new();
    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public Dictionary<string, float> slidersPerRoom = new();
    /// <summary>
    /// For Chronia Flag-Counter-Slider System
    /// </summary>
    public Dictionary<string, float> slidersPerDeath = new();

    public Dictionary<string, string> keystrings = new();
}

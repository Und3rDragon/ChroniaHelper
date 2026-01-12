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

    [YamlIgnore] public readonly Dictionary<HookId, HookData> HookData = new Dictionary<HookId, HookData>();

    [YamlIgnore] public readonly Dictionary<Color, List<DustBunnyEdge>> EdgeColorDictionary = new Dictionary<Color, List<DustBunnyEdge>>();

    [YamlIgnore] public Dictionary<string, FlagCarouselTrigger> CarouselDictionary { get; private set; } = new Dictionary<string, FlagCarouselTrigger>();
    
    public bool ActiveTera = false;
    
    public TeraType StartTera = TeraType.Any;

    //timer
    
    public int timerD = 0, timerC = 0, timerB = 0, timerA = 0, timerFrames = 0;
    
    public string timer;

    //AppleSheep Timer
    public Dictionary<string, long> TimeRecords { get; set; } = new Dictionary<string, long>();
    
    public long RawTime;
    
    public long Time;
    
    public bool TimerStarted;
    
    public bool TimerPauseed;
    
    public bool TimerCompleted;

    [YamlIgnore] public List<MTexture> Icons { get; set; }
    [YamlIgnore] public List<Vector2> IconOffsets { get; set; }
    [YamlIgnore] public List<Color> IconColors { get; set; }
    public List<string> IconsSave { get; set; }
    public List<Vector2> IconOffsetsSave { get; set; }
    public List<string> IconColorsSave { get; set; }
    public int ZoneDepth { get; set; }

    public void ProcessZoneSaves()
    {
        if (Icons is not null) return;
        Icons = IconsSave?.Select(s => GFX.Game[s]).ToList();
        IconOffsets = IconOffsetsSave;
        IconColors = IconColorsSave?.Select(Calc.HexToColor).ToList();
    }

    public void RecordZoneSave(PlayerIndicatorZone zone)
    {
        if (zone is null)
        {
            Icons = null;
            IconOffsets = null;
            IconColors = null;
            IconsSave = null;
            IconOffsetsSave = null;
            IconColorsSave = null;
            return;
        }
        Icons = zone.Icons;
        IconsSave = zone.Icons.Select(t => t.AtlasPath).ToList();
        IconOffsets = zone.IconOffsets;
        IconOffsetsSave = zone.IconOffsets;
        IconColors = zone.IconColors;
        IconColorsSave = zone.IconColors.Select(c => $"{c.R:X2}{c.G:X2}{c.B:X2}").ToList();
        ZoneDepth = zone.Depth;
    }

    // Flag Button Data
    // Can be migrated but not necessary?
    public HashSet<string> switchFlag = new();
    public HashSet<string> flagNames = new HashSet<string>();
    public Dictionary<int, int> touchSwitchFrame = new Dictionary<int, int>();

    // Flag Carousel Trigger extended
    public Dictionary<string, bool> CarouselState { get; set; } = new Dictionary<string, bool>();

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
    public Dictionary<EntityID, int> RemainingUses { get; set; } = new();
    public Dictionary<EntityID, int> PasswordQueue { get; set; } = new();

    public Dictionary<string,string> Passwords { get; set; } = new();

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
    public List<string> rooms = new();
    public List<string> roomTags = new();
    public bool roomTagLoaded = false;

    // Solid Modifier Component
    [YamlIgnore]
    public SolidModifierComponent currentActiveSolidModifier = null;

    // Stopwatch
    public Dictionary<string, Stopclock> sessionStopwatches = new();

    // OperationCode Listener
    public Dictionary<int, OperationCodesListener.OperationCodeData> operationCodeListeners = new();

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
    public Dictionary<string, SessionConditionListener> listeningConditions = new();
    public Dictionary<string, bool> listeningConditionLastState = new();
    public Dictionary<string, bool> listeningConditionTimerState = new();
    public Dictionary<string, float> listeningConditionTimer = new();

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

    public Dictionary<string, string> sessionKeys = new();
}

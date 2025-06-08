using YamlDotNet.Serialization;
using System.Collections.Generic;
using Celeste.Mod.ChroniaHelperIndicatorZone;
using System.Linq;
using ChroniaHelper.Cores.LiteTeraHelper;
using ChroniaHelper.Triggers;
using YoctoHelper.Components;
using YoctoHelper.Hooks;
using static ChroniaHelper.Entities.CustomBooster;
using System.Runtime.InteropServices;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSession : EverestModuleSession
{

    [YamlIgnore] public readonly Dictionary<HookId, HookData> HookData = new Dictionary<HookId, HookData>();

    [YamlIgnore] public readonly Dictionary<Color, List<DustBunnyEdge>> EdgeColorDictionary = new Dictionary<Color, List<DustBunnyEdge>>();

    [YamlIgnore] public Dictionary<string, FlagCarouselTrigger> CarouselDictionary { get; private set; } = new Dictionary<string, FlagCarouselTrigger>();

    public bool ActiveTera = false;

    public TeraType StartTera = TeraType.Any;

    //timer
    public static int timerD = 0, timerC = 0, timerB = 0, timerA = 0, timerFrames = 0;
    public static string timer;

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

    public Dictionary<string, bool> switchFlag = new Dictionary<string, bool>();
    public List<string> flagNames = new List<string>();
    public List<string> lastRoom = new List<string>();
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

    // Flag Trigger Temporary setup
    public struct ChroniaFlag
    {
        public string flagID;
        public bool flagState;
        public bool isGlobal;
    }
    public static Dictionary<string, ChroniaFlag> TemporaryFlags = new();

    // Omni Zip Mover 2 records
    public static Dictionary<EntityID, int> Zipmover_NodeIndex = new();
    public static Dictionary<EntityID, bool> Zipmover_NextForward = new();

    // Flag Timer Trigger
    public static Dictionary<string, float> FlagTimer = new();
}

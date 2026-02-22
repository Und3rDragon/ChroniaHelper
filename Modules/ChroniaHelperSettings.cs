using System.Diagnostics;
using System.Reflection.Emit;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSettings : EverestModuleSettings
{
    public bool ChineseCharactersAutoLining { get; set; } = false;
    public bool HUDMainControl { get; set; } = true;
    public enum DisplayPosition { PlayerBased, StaticScreen }
    public enum Aligning { Left, Middle, Right }
    
    public class CommonDisplayer
    {
        public bool enabled { get; set; } = false;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 160;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 168;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    [SettingSubMenu]
    public class StaminaDisplayer : CommonDisplayer
    {
        
    }

    public StaminaDisplayer staminaMeterMenu { get; set; } = new();

    [SettingSubMenu]
    public class DashesDisplayer : CommonDisplayer
    {
        
    }

    public DashesDisplayer dashesCounter { get; set; } = new();

    [SettingSubMenu]
    public class RealTimeClockDisplayer : CommonDisplayer
    {
        public bool hasSeconds { get; set; } = true;
    }

    public RealTimeClockDisplayer realTimeClock { get; set; } = new();

    [SettingSubMenu]
    public class StateMachineDisplayer : CommonDisplayer
    {
        
    }

    public StateMachineDisplayer stateMachineDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class SpeedDisplayer : CommonDisplayer
    {
        public enum SpeedDisplay { speedX, speedY, speedToCoordinates, speedAll }
        public SpeedDisplay speedDisplay { get; set; } = SpeedDisplay.speedAll;
    }

    public SpeedDisplayer speedDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class PlayerSpriteDisplayer : CommonDisplayer
    {
        public enum DisplaySprite { Animation, File, Comparison}
        public DisplaySprite displaySprite { get; set; } = DisplaySprite.Comparison;
    }
    public PlayerSpriteDisplayer playerSpriteDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class PlayerPositionDisplayer : CommonDisplayer
    {
        public enum DisplayCoordinates { X, Y, Both }
        public DisplayCoordinates displayCoordinates { get; set; } = DisplayCoordinates.Both;
        public bool useGlobalCoordinates { get; set; } = true;
    }
    public PlayerPositionDisplayer playerPositionDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class DeathsDisplayer : CommonDisplayer
    {
        
    }

    public DeathsDisplayer deathsDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class TotalDeathsDisplayer : CommonDisplayer
    {
        
    }

    public TotalDeathsDisplayer totalDeathsDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class SaveFileDeathsDisplayer : CommonDisplayer
    {
        
    }

    public SaveFileDeathsDisplayer saveFileDeathsDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class MapNameDisplayer : CommonDisplayer
    {
        public bool prefix { get; set; } = false;
        public bool suffixAuthor { get; set; } = false;
    }
    public MapNameDisplayer mapNameDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class RoomNameDisplayer : CommonDisplayer
    {
        public bool prefix { get; set; } = false;
    }
    public RoomNameDisplayer roomNameDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class MapAuthorNameDisplayer : CommonDisplayer
    {
        public bool prefix { get; set; } = false;
    }
    public MapAuthorNameDisplayer mapAuthorNameDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class LevelBloomDisplayer : CommonDisplayer
    {
        
    }
    public LevelBloomDisplayer levelBloomDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class LevelLightingDisplayer : CommonDisplayer
    {

    }
    public LevelLightingDisplayer levelLightingDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class CameraOffsetDisplayer : CommonDisplayer
    {
        public enum CameraDisplay { CameraX, CameraY, CameraXY }
        public CameraDisplay cameraDisplay { get; set; } = CameraDisplay.CameraXY;
    }

    public CameraOffsetDisplayer cameraOffsetDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class CommandStopclockDisplayer : CommonDisplayer
    {
        [SettingRange(0,6)]
        public int minUnit { get; set; } = 0;
        [SettingRange(0,6)]
        public int maxUnit { get; set; } = 2;
        public bool trimZeros { get; set; } = true;
    }
    public CommandStopclockDisplayer commandStopclockDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class InputDisplayer
    {
        public bool enabled { get; set; } = false;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 4;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 70;
        [SettingRange(0, 160, true)]
        public int letterDistance { get; set; } = 32;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 40;
        public Aligning aligning { get; set; } = Aligning.Left;
        [SettingRange(1, 100, true)]
        public int maxDisplays { get; set; } = 5;
        [SettingRange(-64, 64)]
        public int lineDistance { get; set; } = 2;
        [SettingRange(1, 9)]
        public int overallAligning { get; set; } = 1;

        public List<string> renderTarget = new();
    }
    public InputDisplayer inputDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class MousePositionDisplayer : CommonDisplayer
    {
        public enum DisplayType { HD, LD, Level }
        public DisplayType displayType { get; set; } = DisplayType.HD;
        public bool showMouse { get; set; } = false;
    }
    public MousePositionDisplayer mousePositionDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class EntityInfoDisplayer
    {
        public bool enabled { get; set; } = false;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 4;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 70;
        [SettingRange(-32, 32, true)]
        public int letterDistance { get; set; } = -4;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 40;
        public Aligning aligning { get; set; } = Aligning.Left;
        [SettingRange(-64, 64)]
        public int lineDistance { get; set; } = 2;
        [SettingRange(1, 9)]
        public int overallAligning { get; set; } = 1;

        public List<string> renderTarget = new();

        public bool DisplayEntityInfoInConsole { get; set; } = false;
    }
    public EntityInfoDisplayer entityInfoDisplayer { get; set; } = new();
}

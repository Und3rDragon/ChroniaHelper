using System.Diagnostics;
using System.Reflection.Emit;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSettings : EverestModuleSettings
{
    public enum DisplayPosition { PlayerBased, StaticScreen }
    public enum Aligning { Left, Middle, Right }

    [SettingSubMenu]
    public class StaminaDisplayer
    {
        public bool enabled { get; set; } = false;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.PlayerBased;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 0;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = -16;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public StaminaDisplayer staminaMeterMenu { get; set; } = new();

    [SettingSubMenu]
    public class DashesDisplayer
    {
        public bool enabled { get; set; } = false;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.PlayerBased;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 0;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = -16;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public DashesDisplayer dashesCounter { get; set; } = new();

    [SettingSubMenu]
    public class RealTimeClockDisplayer
    {
        public bool enabled { get; set; } = false;
        public bool hasSeconds { get; set; } = true;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 160;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 172;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public RealTimeClockDisplayer realTimeClock { get; set; } = new();

    [SettingSubMenu]
    public class StateMachineDisplayer
    {
        public bool enabled { get; set; } = false;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 160;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 165;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public StateMachineDisplayer stateMachineDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class SpeedDisplayer
    {
        public bool enabled { get; set; } = false;
        public enum SpeedDisplay { speedX, speedY, speedToCoordinates, speedAll }
        public SpeedDisplay speedDisplay { get; set; } = SpeedDisplay.speedAll;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.PlayerBased;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 0;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = -16;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public SpeedDisplayer speedDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class PlayerSpriteDisplayer
    {
        public bool enabled { get; set; } = false;
        public enum DisplaySprite { Animation, File, Comparison}
        public DisplaySprite displaySprite { get; set; } = DisplaySprite.Comparison;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 160;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 165;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }
    public PlayerSpriteDisplayer playerSpriteDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class PlayerPositionDisplayer
    {
        public bool enabled { get; set; } = false;
        public enum DisplayCoordinates { X, Y, Both }
        public DisplayCoordinates displayCoordinates { get; set; } = DisplayCoordinates.Both;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        public bool useGlobalCoordinates { get; set; } = true;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 160;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 175;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 45;
        public Aligning aligning { get; set; } = Aligning.Middle;
    }
    public PlayerPositionDisplayer playerPositionDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class DeathsDisplayer
    {
        public bool enabled { get; set; } = false;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 318;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 19;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 60;
        public Aligning aligning { get; set; } = Aligning.Right;
    }

    public DeathsDisplayer deathsDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class TotalDeathsDisplayer
    {
        public bool enabled { get; set; } = false;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 318;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 8;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 55;
        public Aligning aligning { get; set; } = Aligning.Right;
    }

    public TotalDeathsDisplayer totalDeathsDisplayer { get; set; } = new();

    [SettingSubMenu]
    public class SaveFileDeathsDisplayer
    {
        public bool enabled { get; set; } = false;
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.StaticScreen;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 318;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = 8;
        [SettingRange(-32, 32)]
        public int letterDistance { get; set; } = 0;
        [SettingRange(0, 1000, true)]
        public int scale { get; set; } = 55;
        public Aligning aligning { get; set; } = Aligning.Right;
    }

    public SaveFileDeathsDisplayer saveFileDeathsDisplayer { get; set; } = new();
}

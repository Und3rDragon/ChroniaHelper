using System.Diagnostics;
using System.Reflection.Emit;
using YamlDotNet.Serialization;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSettings : EverestModuleSettings
{
    [SettingSubMenu]
    public class StaminaDisplayer
    {
        public bool enableStaminaMeter { get; set; } = false;
        public enum DisplayPosition { PlayerBased, StaticScreen }
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.PlayerBased;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 0;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = -16;

        public enum Aligning { Left, Middle, Right }
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public StaminaDisplayer staminaMeterMenu { get; set; } = new();

    [SettingSubMenu]
    public class DashesDisplayer
    {
        public bool enableDashCounter { get; set; } = false;
        public enum DisplayPosition { PlayerBased, StaticScreen }
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.PlayerBased;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 0;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = -16;
        public enum Aligning { Left, Middle, Right }
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public DashesDisplayer dashesCounter { get; set; } = new();

    [SettingSubMenu]
    public class RealTimeClockDisplayer
    {
        public bool enableTimeClock { get; set; } = false;
        public bool hasSeconds { get; set; } = true;
        public enum DisplayPosition { PlayerBased, StaticScreen }
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.PlayerBased;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 0;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = -16;
        public enum Aligning { Left, Middle, Right }
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public RealTimeClockDisplayer realTimeClock { get; set; } = new();

    [SettingSubMenu]
    public class StateMachineDisplayer
    {
        public bool enableStateMachineDisplayer { get; set; } = false;
        public enum DisplayPosition { PlayerBased, StaticScreen }
        public DisplayPosition displayPosition { get; set; } = DisplayPosition.PlayerBased;
        [SettingRange(-1000, 1000, true)]
        public int X { get; set; } = 0;
        [SettingRange(-1000, 1000, true)]
        public int Y { get; set; } = -16;
        public enum Aligning { Left, Middle, Right }
        public Aligning aligning { get; set; } = Aligning.Middle;
    }

    public StateMachineDisplayer stateMachineDisplayer { get; set; } = new();
}

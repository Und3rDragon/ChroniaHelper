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
    }

    public StaminaDisplayer staminaMeterMenu { get; set; } = new();
}

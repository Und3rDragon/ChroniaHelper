global using CColor = ChroniaHelper.Utils.ColorUtils.ChroniaColor;
global using ChroniaColor = ChroniaHelper.Utils.ColorUtils.ChroniaColor;
global using Cons = ChroniaHelper.Utils.Constants;
global using FI = ChroniaHelper.Imports.FrostHelperImports;
global using HSL = ChroniaHelper.Utils.ColorUtils.HSLColor;
global using HSLColor = ChroniaHelper.Utils.ColorUtils.HSLColor;
global using HSV = ChroniaHelper.Utils.ColorUtils.HSVColor;
global using HSVColor = ChroniaHelper.Utils.ColorUtils.HSVColor;
global using MaP = ChroniaHelper.Cores.MapProcessor;
global using Md = ChroniaHelper.ChroniaHelperModule;
global using PUt = ChroniaHelper.Utils.PlayerUtils;
global using Sav = ChroniaHelper.Modules.ChroniaHelperSaveData;
global using Ses = ChroniaHelper.Modules.ChroniaHelperSession;
global using Sts = ChroniaHelper.Modules.ChroniaHelperSettings;
global using Vc2 = Microsoft.Xna.Framework.Vector2;
global using Vc3 = Microsoft.Xna.Framework.Vector3;
global using Sens = ChroniaHelper.Utils.SensitiveFlags.Sensitivity;
using System.Reflection;

namespace ChroniaHelper.Utils;

public static class Constants
{
    public const float piOverEight = MathHelper.PiOver4 / 2f;
    public const float eightPi = 4 * MathHelper.TwoPi;

    public const string BgTileListDynamicDataName = "BgTileList";

    public const float TeraBlockLiftBoostMultipler = 4f;
    
    public const string EntityStringId = "ChroniaHelper/PlayerIndicatorZone";
    public const string EntityStringId2 = "ChroniaHelper/PlayerIndicatorZoneCustom";

    public const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    
}
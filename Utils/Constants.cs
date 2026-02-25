global using CColor = ChroniaHelper.Utils.ColorUtils.ChroniaColor;
global using ChroniaColor = ChroniaHelper.Utils.ColorUtils.ChroniaColor;
global using Cons = ChroniaHelper.Utils.Constants;
global using HSL = ChroniaHelper.Utils.ColorUtils.HSLColor;
global using HSLColor = ChroniaHelper.Utils.ColorUtils.HSLColor;
global using HSV = ChroniaHelper.Utils.ColorUtils.HSVColor;
global using HSVColor = ChroniaHelper.Utils.ColorUtils.HSVColor;
global using MaP = ChroniaHelper.Cores.MapProcessor; // usual
global using Md = ChroniaHelper.ChroniaHelperModule; // usual
global using PUt = ChroniaHelper.Utils.PlayerUtils;
global using Sav = ChroniaHelper.Modules.ChroniaHelperSaveData; // usual
global using Ses = ChroniaHelper.Modules.ChroniaHelperSession; // usual
global using Sts = ChroniaHelper.Modules.ChroniaHelperSettings; // usual
global using Vc2 = Microsoft.Xna.Framework.Vector2;
global using Vc3 = Microsoft.Xna.Framework.Vector3;
global using Sens = ChroniaHelper.Utils.SensitiveFlags.Sensitivity;
global using Prm = ChroniaHelper.Cores.Graphical.GraphicalParams;
global using Clock = ChroniaHelper.Utils.StopwatchSystem.Stopclock;
global using GSav = ChroniaHelper.Modules.ChroniaHelperGlobalSaveData; // usual
global using Lang = ChroniaHelper.ChroniaHelperModule.Languages;

using static ChroniaHelper.Settings.Commands; // usual
using static ChroniaHelper.Cores.NoteAttribute; // marker attributes
using static ChroniaHelper.Cores.WorkingInProgressAttribute; // marker attributes
using static ChroniaHelper.Cores.VersionNoteAttribute; // marker attributes
using static ChroniaHelper.Cores.PrivateForAttribute; // marker attributes
using static ChroniaHelper.Cores.CreditsAttribute; // marker attributes
using static ChroniaHelper.Imports.APIFrostHelper; 
using static ChroniaHelper.Imports.APICommunalHelper;
using static ChroniaHelper.Imports.APISpeedrunTool;
using static ChroniaHelper.References.RefMaxHelpingHand; // usual
using static ChroniaHelper.References.RefXaphanHelper; // usual
using static ChroniaHelper.References.RefCommunalHelper; // usual

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
    
    public const string DisplayFontsReference = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =,_^";

    public const string CommandStopclockID = "ChroniaHelper_Debug_CommandStopclock";

    public const float HDScale = 6f;
}
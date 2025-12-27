using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
[CustomEntity("ChroniaHelper/SettingsOverrideOnFlagController")]
public class SettingsOverrideOnFlagController : BaseEntity
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.End += LevelEnd;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.End -= LevelEnd;
    }

    public static void LevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        Celeste.Settings.Instance.Fullscreen = Md.Session.settingsData.fullScreen;
        Celeste.Settings.Instance.WindowScale = Md.Session.settingsData.windowScale;
        Celeste.Settings.Instance.DisableFlashes = Md.Session.settingsData.photosensitive;

        orig(self);
    }

    public SettingsOverrideOnFlagController(EntityData data, Vector2 offset) : base(data, offset)
    {
        flag = data.Attr("flag", "flag");
        photosensitive = data.Int("photoSensitiveMode", 0);
        fullScreen = data.Int("fullScreen", 0);
        windowScale = data.Attr("windowScale");
    }
    private string flag;
    /// <summary>
    /// Disabled = 0, On = 1, Off = 2
    /// </summary>
    public int photosensitive = 0, fullScreen = 0;
    public string windowScale;
    
    public override void Update()
    {
        base.Update();

        if (flag.IsNullOrEmpty() || !flag.GetFlag()) { return; }
        
        switch (photosensitive)
        {
            case 1:
                Celeste.Settings.Instance.DisableFlashes = true;
                break;
            case 2:
                Celeste.Settings.Instance.DisableFlashes = false;
                break;
            default:
                break;
        }
        
        
        switch (fullScreen)
        {
            case 1:
                Celeste.Settings.Instance.Fullscreen = true;
                break;
            case 2:
                Celeste.Settings.Instance.Fullscreen = false;
                break;
            default:
                break;
        }

        if (windowScale.IsNotNullOrEmpty() && int.TryParse(windowScale, out int n))
        {
            Celeste.Settings.Instance.WindowScale = n;
        }
    }
}

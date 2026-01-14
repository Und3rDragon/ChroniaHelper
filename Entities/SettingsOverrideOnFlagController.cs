using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
[Note("Conflict with HDRender thing, tabbing when Reloading case the screen to be blank")]
//[CustomEntity("ChroniaHelper/SettingsOverrideOnFlagController")]
public class SettingsOverrideOnFlagController : BaseEntity
{
    //[LoadHook]
    public static void Load()
    {
        On.Celeste.Level.End += LevelEnd;
    }
    //[UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.End -= LevelEnd;
    }

    public static void LevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        Celeste.Settings.Instance.Fullscreen = Md.Session.settingsData.fullScreen;
        Celeste.Settings.Instance.WindowScale = Md.Session.settingsData.windowScale;
        Celeste.Settings.Instance.DisableFlashes = Md.Session.settingsData.photosensitive;
        Celeste.Settings.Instance.Language = Md.Session.settingsData.language;
        Celeste.Settings.Instance.GrabMode = Md.Session.settingsData.grabMode;

        Celeste.Settings.Instance.ApplyScreen();
        Celeste.Settings.Instance.ApplyLanguage();

        orig(self);
    }

    public SettingsOverrideOnFlagController(EntityData data, Vector2 offset) : base(data, offset)
    {
        flag = data.Attr("flag", "flag");
        photosensitive = data.Int("photoSensitiveMode", 0);
        fullScreen = data.Int("fullScreen", 0);
        windowScale = data.Attr("windowScale");
        language = data.Int("language", -1);
        grabMode = data.Int("grabMode", 0);
    }
    private string flag;
    /// <summary>
    /// Disabled = 0, On = 1, Off = 2
    /// </summary>
    public int photosensitive = 0, fullScreen = 0;
    public string windowScale;
    public int language = -1;
    /// <summary>
    /// Disabled = 0, Hold = 1, Invert = 2, Toggle = 3
    /// </summary>
    public int grabMode = 0;


    public override void Update()
    {
        base.Update();

        if (flag.IsNullOrEmpty() || !flag.GetFlag()) { return; }
        
        switch (photosensitive)
        {
            case 1:
                if (!Celeste.Settings.Instance.DisableFlashes)
                {
                    Celeste.Settings.Instance.DisableFlashes = true;
                }
                break;
            case 2:
                if (Celeste.Settings.Instance.DisableFlashes)
                {
                    Celeste.Settings.Instance.DisableFlashes = false;
                }
                break;
            default:
                break;
        }
        
        
        switch (fullScreen)
        {
            case 1:
                if (!Celeste.Settings.Instance.Fullscreen)
                {
                    Celeste.Settings.Instance.Fullscreen = true;
                    Celeste.Settings.Instance.ApplyScreen();
                }
                break;
            case 2:
                if (Celeste.Settings.Instance.Fullscreen)
                {
                    Celeste.Settings.Instance.Fullscreen = false;
                    Celeste.Settings.Instance.ApplyScreen();
                }
                break;
            default:
                break;
        }

        switch (grabMode)
        {
            case 1:
                if(Celeste.Settings.Instance.GrabMode != GrabModes.Hold)
                {
                    Celeste.Settings.Instance.GrabMode = GrabModes.Hold;
                }
                break;
            case 2:
                if (Celeste.Settings.Instance.GrabMode != GrabModes.Invert)
                {
                    Celeste.Settings.Instance.GrabMode = GrabModes.Invert;
                }
                break;
            case 3:
                if (Celeste.Settings.Instance.GrabMode != GrabModes.Toggle)
                {
                    Celeste.Settings.Instance.GrabMode = GrabModes.Toggle;
                }
                break;
            default:
                break;
        }

        if (windowScale.IsNotNullOrEmpty() && int.TryParse(windowScale, out int n))
        {
            if (Celeste.Settings.Instance.WindowScale != n)
            {
                Celeste.Settings.Instance.WindowScale = n;
                Celeste.Settings.Instance.ApplyScreen();
            }
        }
        
        if(language >= 0 && language <= 9)
        {
            if (Celeste.Settings.Instance.Language != Md.LanguageID[(Md.Languages)language])
            {
                Celeste.Settings.Instance.Language = Md.LanguageID[(Md.Languages)language];
                Celeste.Settings.Instance.ApplyLanguage();
            }
        }
    }
}

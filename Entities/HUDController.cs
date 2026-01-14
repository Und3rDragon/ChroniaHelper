using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using static ChroniaHelper.Modules.ChroniaHelperSettings;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/HUDController")]
public class HUDController : BaseEntity
{
    public HUDController(EntityData data, Vc2 offset) : base(data, offset)
    {
        Tag = Tags.Global;
        
        condition = data.Attr("condition", "y < 90");
    }
    public string condition;

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

    public static List<CommonDisplayer> displayers = new()
    {
        Md.Settings.dashesCounter,
        Md.Settings.deathsDisplayer,
        Md.Settings.mapAuthorNameDisplayer,
        Md.Settings.mapNameDisplayer,
        Md.Settings.playerPositionDisplayer,
        Md.Settings.playerSpriteDisplayer,
        Md.Settings.realTimeClock,
        Md.Settings.roomNameDisplayer,
        Md.Settings.saveFileDeathsDisplayer,
        Md.Settings.speedDisplayer,
        Md.Settings.staminaMeterMenu,
        Md.Settings.stateMachineDisplayer,
        Md.Settings.totalDeathsDisplayer,
        Md.Settings.levelBloomDisplayer,
        Md.Settings.levelLightingDisplayer,
        Md.Settings.cameraOffsetDisplayer
    };
    
    public static void LevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        for (int i = 0; i < Md.Session.HUDPrimaryState.Count && i < displayers.Count; i++)
        {
            displayers[i].enabled = Md.Session.HUDPrimaryState[i];
        }

        Md.Session.HUDPrimaryState.Clear();

        Md.Session.HUDStateRegistered = false;

        Md.Session.HUDStateRegister = string.Empty;
        
        orig(self);
    }

    protected override void AddedExecute(Scene scene)
    {
        if (Md.Session.HUDStateRegistered && condition == Md.Session.HUDStateRegister)
        {
            return;
        }
        
        Md.Session.HUDPrimaryState.Clear();
        for (int i = 0; i < displayers.Count; i++)
        {
            Md.Session.HUDPrimaryState.Add(displayers[i].enabled);
        }

        for (int i = 0; i < displayers.Count; i++)
        {
            if(condition == "all")
            {
                displayers[i].enabled = false;
                continue;
            }
            
            if (condition.ParseMathExpression((v) =>
            {
                if (v.ToLower() == "x")
                {
                    return displayers[i].X;
                }
                else if (v.ToLower() == "y")
                {
                    return displayers[i].Y;
                }
                else
                {
                    return displayers[i].X;
                }
            }) == 1)
            {
                displayers[i].enabled = false;
            }
        }

        Md.Session.HUDStateRegistered = true;

        Md.Session.HUDStateRegister = condition;
    }
    
}

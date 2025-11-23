using System;
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
        flag = data.Attr("flag");
        condition = data.Attr("condition", "y < 90");

        hasFlag = flag.IsNotNullOrEmpty();
    }
    public string condition, flag;
    public bool hasFlag;
    private bool seal = false;

    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Begin += LevelBegin;
        On.Celeste.Level.End += LevelEnd;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Begin -= LevelBegin;
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
        Md.Settings.totalDeathsDisplayer
    };
    
    public static void LevelBegin(On.Celeste.Level.orig_Begin orig, Celeste.Level self)
    {
        orig(self);

        Md.Session.HUDPrimaryState.Clear();
        for(int i = 0; i < displayers.Count; i++)
        {
            Md.Session.HUDPrimaryState.Add(displayers[i].enabled);
        }
    }
    
    //public static void ScreenWiping(On.Celeste.Level.orig_DoScreenWipe orig, Celeste.Level self, bool wipeIn, Action complete, bool hiResSnow)
    public static void LevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        for (int i = 0; i < displayers.Count; i++)
        {
            displayers[i].enabled = Md.Session.HUDPrimaryState[i];
        }

        //orig(self, wipeIn, complete, hiResSnow);
        orig(self);
    }

    protected override void AddedExecute(Scene scene)
    {
        seal = false;
    }
    protected override void UpdateExecute()
    {
        if(hasFlag && !flag.GetFlag())
        {
            return;
        }

        if (seal)
        {
            return;
        }

        seal = true;
        
        for(int i = 0; i < displayers.Count; i++)
        {
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
    }
}

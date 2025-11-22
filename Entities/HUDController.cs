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
    
    public static void LevelBegin(On.Celeste.Level.orig_Begin orig, Celeste.Level self)
    {
        orig(self);

        Md.Session.HUDPrimaryState.Clear();
        Md.Session.HUDPrimaryState.Add(Md.Settings.dashesCounter.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.deathsDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.mapAuthorNameDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.mapNameDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.playerPositionDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.playerSpriteDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.realTimeClock.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.roomNameDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.saveFileDeathsDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.speedDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.staminaMeterMenu.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.stateMachineDisplayer.enabled);
        Md.Session.HUDPrimaryState.Add(Md.Settings.totalDeathsDisplayer.enabled);
    }
    
    //public static void ScreenWiping(On.Celeste.Level.orig_DoScreenWipe orig, Celeste.Level self, bool wipeIn, Action complete, bool hiResSnow)
    public static void LevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        Md.Settings.dashesCounter.enabled = Md.Session.HUDPrimaryState[0];
        Md.Settings.deathsDisplayer.enabled = Md.Session.HUDPrimaryState[1];
        Md.Settings.mapAuthorNameDisplayer.enabled = Md.Session.HUDPrimaryState[2];
        Md.Settings.mapNameDisplayer.enabled = Md.Session.HUDPrimaryState[3];
        Md.Settings.playerPositionDisplayer.enabled = Md.Session.HUDPrimaryState[4];
        Md.Settings.playerSpriteDisplayer.enabled = Md.Session.HUDPrimaryState[5];
        Md.Settings.realTimeClock.enabled = Md.Session.HUDPrimaryState[6];
        Md.Settings.roomNameDisplayer.enabled = Md.Session.HUDPrimaryState[7];
        Md.Settings.saveFileDeathsDisplayer.enabled = Md.Session.HUDPrimaryState[8];
        Md.Settings.speedDisplayer.enabled = Md.Session.HUDPrimaryState[9];
        Md.Settings.staminaMeterMenu.enabled = Md.Session.HUDPrimaryState[10];
        Md.Settings.stateMachineDisplayer.enabled = Md.Session.HUDPrimaryState[11];
        Md.Settings.totalDeathsDisplayer.enabled = Md.Session.HUDPrimaryState[12];

        //orig(self, wipeIn, complete, hiResSnow);
        orig(self);
    }

    protected override void AddedExecute(Scene scene)
    {
        if (condition.ParseMathExpression((v) =>
        {
            if(v.ToLower() == "x")
            {
                return Md.Settings.dashesCounter.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.dashesCounter.Y;
            }
            else
            {
                return Md.Settings.dashesCounter.X;
            }
        }) == 1)
        {
            Md.Settings.dashesCounter.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.deathsDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.deathsDisplayer.Y;
            }
            else
            {
                return Md.Settings.deathsDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.deathsDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.mapAuthorNameDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.mapAuthorNameDisplayer.Y;
            }
            else
            {
                return Md.Settings.mapAuthorNameDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.mapAuthorNameDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.mapNameDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.mapNameDisplayer.Y;
            }
            else
            {
                return Md.Settings.mapNameDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.mapNameDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.playerPositionDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.playerPositionDisplayer.Y;
            }
            else
            {
                return Md.Settings.playerPositionDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.playerPositionDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.playerSpriteDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.playerSpriteDisplayer.Y;
            }
            else
            {
                return Md.Settings.playerSpriteDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.playerSpriteDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.realTimeClock.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.realTimeClock.Y;
            }
            else
            {
                return Md.Settings.realTimeClock.X;
            }
        }) == 1)
        {
            Md.Settings.realTimeClock.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.roomNameDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.roomNameDisplayer.Y;
            }
            else
            {
                return Md.Settings.roomNameDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.roomNameDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.saveFileDeathsDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.saveFileDeathsDisplayer.Y;
            }
            else
            {
                return Md.Settings.saveFileDeathsDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.saveFileDeathsDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.speedDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.speedDisplayer.Y;
            }
            else
            {
                return Md.Settings.speedDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.speedDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.staminaMeterMenu.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.staminaMeterMenu.Y;
            }
            else
            {
                return Md.Settings.staminaMeterMenu.X;
            }
        }) == 1)
        {
            Md.Settings.staminaMeterMenu.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.stateMachineDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.stateMachineDisplayer.Y;
            }
            else
            {
                return Md.Settings.stateMachineDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.stateMachineDisplayer.enabled = false;
        }

        if (condition.ParseMathExpression((v) =>
        {
            if (v.ToLower() == "x")
            {
                return Md.Settings.totalDeathsDisplayer.X;
            }
            else if (v.ToLower() == "y")
            {
                return Md.Settings.totalDeathsDisplayer.Y;
            }
            else
            {
                return Md.Settings.totalDeathsDisplayer.X;
            }
        }) == 1)
        {
            Md.Settings.totalDeathsDisplayer.enabled = false;
        }
    }
}

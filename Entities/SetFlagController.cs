using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using FMOD;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetFlagController")]
public class SetFlagController : BaseEntity
{
    public SetFlagController(EntityData data, Vc2 offset) : base(data, offset)
    {
        flags = data.Attr("flags").Split(',',StringSplitOptions.TrimEntries);
        paramater = data.Attr("parameters");
        mode = data.Int("mode", 0);
    }
    private string[] flags;
    private string paramater;
    /// <summary>
    /// On Level Load = 0, Always Set = 1, On Scene Start = 2, On Scene End = 3, On Interval = 4
    /// On Player Die = 5, On Player Respawn = 6, On Entity Added = 7, On Entity Removed = 8,
    /// On Flags = 9, On Chronia Expression = 10, On Frost Session Expression = 11
    /// </summary>
    private int mode = 0;

    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += OnLoadLevel;
        On.Celeste.Player.Die += OnPlayerDie;
        On.Celeste.Player.IntroRespawnEnd += OnPlayerRespawned;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= OnLoadLevel;
        On.Celeste.Player.Die -= OnPlayerDie;
        On.Celeste.Player.IntroRespawnEnd -= OnPlayerRespawned;
    }
    
    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool loader)
    {
        orig(self, intro, loader);
        
        self.Tracker.GetEntities<SetFlagController>().As(
            out List<SetFlagController> controllers, (e) => e as SetFlagController);
        
        foreach(var i in controllers)
        {
            if(i.mode == 0)
            {
                i.flags.SetGeneralFlags(",", "!", "*", "?");
            }
        }
    }
    
    public static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        var o = orig(self, direction, evenIfInvincible, registerDeathInStats);

        MaP.level.Tracker.GetEntities<SetFlagController>().As(
            out List<SetFlagController> controllers, (e) => e as SetFlagController);

        foreach (var i in controllers)
        {
            if (i.mode == 5)
            {
                i.flags.SetGeneralFlags(",", "!", "*", "?");
            }
        }
        
        return o;
    }
    
    public static void OnPlayerRespawned(On.Celeste.Player.orig_IntroRespawnEnd orig, Player self)
    {
        orig(self);

        MaP.level.Tracker.GetEntities<SetFlagController>().As(
            out List<SetFlagController> controllers, (e) => e as SetFlagController);

        foreach (var i in controllers)
        {
            if (i.mode == 6)
            {
                i.flags.SetGeneralFlags(",", "!", "*", "?");
            }
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        if(mode == 7)
        {
            flags.SetGeneralFlags(",", "!", "*", "?");
        }
    }

    public override void Removed(Scene scene)
    {
        if(mode == 8)
        {
            flags.SetGeneralFlags(",", "!", "*", "?");
        }
        
        base.Removed(scene);
    }

    private bool _state = false, state = false;
    public override void Update()
    {
        base.Update();
        
        if(mode == 1)
        {
            flags.SetGeneralFlags(",", "!", "*", "?");
        }
        
        if(mode == 4)
        {
            if (Scene.OnInterval(paramater.ParseFloat(0f).GetAbs()))
            {
                flags.SetGeneralFlags(",", "!", "*", "?");
            }
        }
        
        if(mode == 9)
        {
            paramater.Split(",", StringSplitOptions.TrimEntries).ApplyTo(out string[] flags);
            state = true;
            foreach(var flag in flags)
            {
                state.TryNegative(flag.GetGeneralInvertedFlag());
            }
            
            if(_state != state && state)
            {
                flags.SetGeneralFlags(",", "!", "*", "?");
            }
        }
        
        if(mode == 10 || mode == 11)
        {
            if (Md.FrostHelperLoaded && mode == 11)
            {
                state = paramater.getBoolSessionExpressionValue();
            }
            else
            {
                state = paramater.ParseMathExpression() != 0;
            }

            if (_state != state && state)
            {
                flags.SetGeneralFlags(",", "!", "*", "?");
            }
        }

        _state = state;
    }

    public override void SceneBegin(Scene scene)
    {
        base.SceneBegin(scene);
        
        if(mode == 2)
        {
            flags.SetGeneralFlags(",", "!", "*", "?");
        }
    }

    public override void SceneEnd(Scene scene)
    {
        if(mode == 3)
        {
            flags.SetGeneralFlags(",", "!", "*", "?");
        }
        
        base.SceneEnd(scene);
    }
}

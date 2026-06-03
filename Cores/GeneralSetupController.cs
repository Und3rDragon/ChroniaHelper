using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Cores;

public static class GeneralSetupControllerUtils
{
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

        self.Tracker.GetEntities<GeneralSetupController>().As(
            out List<GeneralSetupController> controllers, (e) => e as GeneralSetupController);

        foreach (var i in controllers)
        {
            if (i.mode == 0)
            {
                i.ApplyValue();
            }
        }
    }

    public static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        var o = orig(self, direction, evenIfInvincible, registerDeathInStats);

        MaP.level.Tracker.GetEntities<GeneralSetupController>().As(
            out List<GeneralSetupController> controllers, (e) => e as GeneralSetupController);

        foreach (var i in controllers)
        {
            if (i.mode == 5)
            {
                i.ApplyValue();
            }
        }

        return o;
    }

    public static void OnPlayerRespawned(On.Celeste.Player.orig_IntroRespawnEnd orig, Player self)
    {
        orig(self);

        MaP.level.Tracker.GetEntities<GeneralSetupController>().As(
            out List<GeneralSetupController> controllers, (e) => e as GeneralSetupController);

        foreach (var i in controllers)
        {
            if (i.mode == 6)
            {
                i.ApplyValue();
            }
        }
    }
}

[Tracked(true)]
public abstract class GeneralSetupController : BaseEntity
{
    public GeneralSetupController(EntityData data, Vc2 offset) : base(data, offset)
    {
        paramater = data.Attr("parameters");
        mode = data.Int("mode", 0);
    }
    public string paramater;

    public abstract void ApplyValue();

    /// <summary>
    /// On Level Load = 0, Always Set = 1, On Scene Start = 2, On Scene End = 3, On Interval = 4
    /// On Player Die = 5, On Player Respawn = 6, On Entity Added = 7, On Entity Removed = 8,
    /// On Flags = 9, On Chronia Expression = 10, On Frost Session Expression = 11
    /// </summary>
    public int mode = 0;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (mode == 7)
        {
            ApplyValue();
        }
    }

    public override void Removed(Scene scene)
    {
        if (mode == 8)
        {
            ApplyValue();
        }

        base.Removed(scene);
    }

    private bool _state = false, state = false;
    public override void Update()
    {
        base.Update();

        if (mode == 1)
        {
            ApplyValue();
        }

        if (mode == 4)
        {
            if (Scene.OnInterval(paramater.ParseFloat(0f).GetAbs()))
            {
                ApplyValue();
            }
        }

        if (mode == 9)
        {
            paramater.Split(",", StringSplitOptions.TrimEntries).ApplyTo(out string[] flags);
            state = true;
            foreach (var flag in flags)
            {
                state.TryNegative(flag.GetGeneralInvertedFlag());
            }

            if (_state != state && state)
            {
                ApplyValue();
            }
        }

        if (mode == 10 || mode == 11)
        {
            if (Md.FrostHelperLoaded && mode == 11)
            {
                state = paramater.tryCreateSessionExpression().getBoolSessionExpressionValue();
            }
            else
            {
                state = paramater.ParseMathExpression() != 0;
            }

            if (_state != state && state)
            {
                ApplyValue();
            }
        }

        _state = state;
    }

    public override void SceneBegin(Scene scene)
    {
        base.SceneBegin(scene);

        if (mode == 2)
        {
            ApplyValue();
        }
    }

    public override void SceneEnd(Scene scene)
    {
        if (mode == 3)
        {
            ApplyValue();
        }

        base.SceneEnd(scene);
    }
}

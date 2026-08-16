using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils.LogicExpression;
using YamlDotNet.Core.Tokens;
using static ChroniaHelper.Cores.GeneralSetupController;

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
            if (i.mode == Modes.OnLevelLoad)
            {
                i.SetState(true);

                i.Execute();
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
            if (i.mode == Modes.OnPlayerDie)
            {
                i.SetState(true);

                i.Execute();
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
            if (i.mode == Modes.OnPlayerRespawn)
            {
                i.SetState(true);

                i.Execute();
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
        mode = data.Int("mode", Modes.OnEntityAdded);

        state = false;
    }
    public string paramater;

    public virtual void Execute(){}
    /// <summary>
    /// If the mode supports false state, this will be invoked
    /// </summary>
    public virtual void Revert(){}
    public virtual void ExecuteByUpdateState(bool current, bool last) { }

    /// <summary>
    /// 触发时机 - 使用 struct 包装常量值
    /// </summary>
    public struct Modes
    {
        public const int OnLevelLoad = 0;
        public const int AlwaysSet = 1;
        public const int OnSceneStart = 2;
        public const int OnSceneEnd = 3;
        public const int OnInterval = 4;
        public const int OnPlayerDie = 5;
        public const int OnPlayerRespawn = 6;
        public const int OnEntityAdded = 7;
        public const int OnEntityRemoved = 8;
        public const int OnFlagsEnable = 9;
        public const int OnChroniaExpressionEnable = 10;
        public const int OnFrostSessionExpressionEnable = 11;
        public const int OnChroniaFlagLogicExpressionEnable = 12;
        public const int OnFlagsDisable = 13;
        public const int OnChroniaExpressionDisable = 14;
        public const int OnFrostSessionExpressionDisable = 15;
        public const int OnChroniaFlagLogicExpressionDisable = 16;
        public const int OnFlagsState = 17;
        public const int OnChroniaExpressionState = 18;
        public const int OnFrostSessionExpressionState = 19;
        public const int OnChroniaFlagLogicExpressionState = 20;
    }
    public int mode = Modes.OnEntityAdded;
    public bool constantMode => 
        mode == Modes.AlwaysSet || mode == Modes.OnInterval || 
        mode == Modes.OnFlagsState || mode == Modes.OnChroniaExpressionState || 
        mode == Modes.OnFrostSessionExpressionState || 
        mode == Modes.OnChroniaFlagLogicExpressionState;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (mode == Modes.OnEntityAdded)
        {
            state = true;

            Execute();
        }
    }

    public override void Removed(Scene scene)
    {
        if (mode == Modes.OnEntityRemoved)
        {
            state = true;

            Execute();
        }

        base.Removed(scene);
    }

    private bool _state = false, state = false;
    public override void Update()
    {
        base.Update();

        if (mode == Modes.AlwaysSet)
        {
            state = true;

            Execute();
        }

        if (mode == Modes.OnInterval)
        {
            if (Scene.OnInterval(paramater.ParseFloat(0f).GetAbs()))
            {
                state = true;

                Execute();
            }
            else
            {
                state = false;
                
                Revert();
            }
        }

        if (mode == Modes.OnFlagsEnable || 
            mode == Modes.OnFlagsDisable ||
            mode == Modes.OnFlagsState)
        {
            paramater.Split(",", StringSplitOptions.TrimEntries).ApplyTo(out string[] flags);
            state = true;
            foreach (var flag in flags)
            {
                state.TryNegative(flag.GetGeneralInvertedFlag());
            }

            if (mode == Modes.OnFlagsState)
            {
                if (state)
                {
                    Execute();
                }
                else
                {
                    Revert();
                }
            }

            if (mode == Modes.OnFlagsEnable)
            {
                if (_state != state)
                {
                    if (state)
                    {
                        Execute();
                    }
                    else
                    {
                        Revert();
                    }
                }
            }

            if (mode == Modes.OnFlagsDisable)
            {
                if (_state != state)
                {
                    if (!state)
                    {
                        Execute();
                    }
                    else
                    {
                        Revert();
                    }
                }
            }
        }

        if (mode == Modes.OnChroniaExpressionEnable || 
            mode == Modes.OnFrostSessionExpressionEnable ||
            mode == Modes.OnChroniaExpressionDisable || 
            mode == Modes.OnFrostSessionExpressionDisable || 
            mode == Modes.OnChroniaExpressionState || 
            mode == Modes.OnFrostSessionExpressionState)
        {
            if (Md.FrostHelperLoaded &&
                (mode == Modes.OnFrostSessionExpressionState ||
                 mode == Modes.OnFrostSessionExpressionEnable || 
                 mode == Modes.OnFrostSessionExpressionDisable)
                )
            {
                state = paramater.tryCreateSessionExpression().getBoolSessionExpressionValue();
            }
            else
            {
                state = paramater.ParseMathExpression() != 0;
            }

            if (mode == Modes.OnChroniaExpressionState
                || mode == Modes.OnFrostSessionExpressionState)
            {
                if (state)
                {
                    Execute();
                }
                else
                {
                    Revert();
                }
            }

            if (mode == Modes.OnChroniaExpressionEnable
                || mode == Modes.OnFrostSessionExpressionEnable)
            {
                if (_state != state)
                {
                    if (state)
                    {
                        Execute();
                    }
                    else
                    {
                        Revert();
                    }
                }
            }

            if (mode == Modes.OnChroniaExpressionDisable
                || mode == Modes.OnFrostSessionExpressionDisable)
            {
                if (_state != state)
                {
                    if (!state)
                    {
                        Execute();
                    }
                    else
                    {
                        Revert();
                    }
                }
            }
        }

        if (mode == Modes.OnChroniaFlagLogicExpressionEnable ||
            mode == Modes.OnChroniaFlagLogicExpressionDisable || 
            mode == Modes.OnChroniaFlagLogicExpressionState)
        {
            state = paramater.ParseLogicExpression();
            
            if (mode == Modes.OnChroniaFlagLogicExpressionState)
            {
                if (state)
                {
                    Execute();
                }
                else
                {
                    Revert();
                }
            }

            if (mode == Modes.OnChroniaFlagLogicExpressionEnable)
            {
                if (_state != state)
                {
                    if (state)
                    {
                        Execute();
                    }
                    else
                    {
                        Revert();
                    }
                }
            }

            if (mode == Modes.OnChroniaFlagLogicExpressionDisable)
            {
                if (_state != state)
                {
                    if (!state)
                    {
                        Execute();
                    }
                    else
                    {
                        Revert();
                    }
                }
            }
        }

        ExecuteByUpdateState(state, _state);

        _state = state;
    }

    public override void SceneBegin(Scene scene)
    {
        base.SceneBegin(scene);

        if (mode == Modes.OnSceneStart)
        {
            Execute();
        }
    }

    public override void SceneEnd(Scene scene)
    {
        if (mode == Modes.OnSceneEnd)
        {
            Execute();
        }

        base.SceneEnd(scene);
    }

    public void SetState(bool set)
    {
        state = set;
    }
}

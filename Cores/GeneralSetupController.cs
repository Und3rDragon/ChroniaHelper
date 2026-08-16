using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using ChroniaHelper.Utils.LogicExpression;
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

    private static void ExecuteControllers(Level level, int mode)
    {
        level.Tracker.GetEntities<GeneralSetupController>()
            .OfType<GeneralSetupController>()
            .Where(c => c.Mode == mode)
            .EachDo(c =>
            {
                c.SetState(true);
                c.Execute();
            });
    }

    public static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool loader)
    {
        orig(self, intro, loader);
        ExecuteControllers(self, Modes.OnLevelLoad);
    }

    public static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        var result = orig(self, direction, evenIfInvincible, registerDeathInStats);
        ExecuteControllers(self.SceneAs<Level>(), Modes.OnPlayerDie);
        return result;
    }

    public static void OnPlayerRespawned(On.Celeste.Player.orig_IntroRespawnEnd orig, Player self)
    {
        orig(self);
        ExecuteControllers(self.SceneAs<Level>(), Modes.OnPlayerRespawn);
    }
}

[Tracked(true)]
public abstract class GeneralSetupController : BaseEntity
{
    // ==================== Properties ====================

    public string Parameter;
    public int Mode = Modes.OnEntityAdded;

    private bool CurrentState;
    private bool LastState;

    private float IntervalTimer;

    // ==================== Modes ====================

    public bool IsConstantMode => Mode == Modes.AlwaysSet || 
                                  Mode == Modes.OnInterval ||
                                  IsStateMode;

    public bool IsFlagMode => Mode == Modes.OnFlagsEnable || 
                              Mode == Modes.OnFlagsDisable || 
                              Mode == Modes.OnFlagsState;

    public bool IsChroniaMode => Mode == Modes.OnChroniaExpressionEnable || 
                                 Mode == Modes.OnChroniaExpressionDisable || 
                                 Mode == Modes.OnChroniaExpressionState;

    public bool IsFrostMode => Mode == Modes.OnFrostSessionExpressionEnable || 
                               Mode == Modes.OnFrostSessionExpressionDisable || 
                               Mode == Modes.OnFrostSessionExpressionState;

    public bool IsFlagLogicMode => Mode == Modes.OnChroniaFlagLogicExpressionEnable || 
                                   Mode == Modes.OnChroniaFlagLogicExpressionDisable || 
                                   Mode == Modes.OnChroniaFlagLogicExpressionState;

    public bool IsEnableMode => Mode == Modes.OnFlagsEnable || 
                                Mode == Modes.OnChroniaExpressionEnable ||
                                Mode == Modes.OnFrostSessionExpressionEnable || 
                                Mode == Modes.OnChroniaFlagLogicExpressionEnable;

    public bool IsDisableMode => Mode == Modes.OnFlagsDisable || 
                                 Mode == Modes.OnChroniaExpressionDisable ||
                                 Mode == Modes.OnFrostSessionExpressionDisable || 
                                 Mode == Modes.OnChroniaFlagLogicExpressionDisable;

    public bool IsStateMode => Mode == Modes.OnFlagsState || 
                               Mode == Modes.OnChroniaExpressionState ||
                               Mode == Modes.OnFrostSessionExpressionState || 
                               Mode == Modes.OnChroniaFlagLogicExpressionState;

    public GeneralSetupController(EntityData data, Vc2 offset) : base(data, offset)
    {
        Parameter = data.Attr("parameters");
        Mode = data.Int("mode", Modes.OnEntityAdded);
        CurrentState = false;
        LastState = false;
    }

    public virtual void Execute() { }
    public virtual void Revert() { }
    public virtual void ExecuteByUpdateState(bool current, bool previous) { }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (Mode == Modes.OnEntityAdded)
        {
            CurrentState = true;
            Execute();
        }
    }

    public override void Removed(Scene scene)
    {
        if (Mode == Modes.OnEntityRemoved)
        {
            CurrentState = true;
            Execute();
        }

        base.Removed(scene);
    }

    public override void SceneBegin(Scene scene)
    {
        base.SceneBegin(scene);

        if (Mode == Modes.OnSceneStart)
        {
            CurrentState = true;
            Execute();
        }
    }

    public override void SceneEnd(Scene scene)
    {
        if (Mode == Modes.OnSceneEnd)
        {
            CurrentState = true;
            Execute();
        }

        base.SceneEnd(scene);
    }

    public override void Update()
    {
        base.Update();

        EvaluateState();
        HandleStateExecution();
        
        ExecuteByUpdateState(CurrentState, LastState);
        
        LastState = CurrentState;
    }

    private void EvaluateState()
    {
        switch (Mode)
        {
            case Modes.AlwaysSet:
                CurrentState = true;
                Execute();
                break;

            case Modes.OnInterval:
                EvaluateIntervalMode();
                break;

            case var m when IsFlagMode:
                EvaluateFlagMode();
                break;

            case var m when IsChroniaMode || IsFrostMode:
                EvaluateExpressionMode();
                break;

            case var m when IsFlagLogicMode:
                EvaluateFlagLogicMode();
                break;
        }
    }

    private void EvaluateIntervalMode()
    {
        float interval = Parameter.ParseFloat(0f).GetAbs();
        
        if (Scene.OnInterval(interval))
        {
            CurrentState = true;
            Execute();
        }
        else
        {
            CurrentState = false;
            Revert();
        }
    }

    private void EvaluateFlagMode()
    {
        var flags = Parameter.Split(",", StringSplitOptions.TrimEntries);
        CurrentState = flags.All(flag => flag.GetGeneralInvertedFlag());
    }

    private void EvaluateExpressionMode()
    {
        if (IsFrostMode && Md.FrostHelperLoaded)
        {
            CurrentState = Parameter.tryCreateSessionExpression().getBoolSessionExpressionValue();
        }
        else
        {
            CurrentState = Parameter.ParseMathExpression() != 0;
        }
    }

    private void EvaluateFlagLogicMode()
    {
        CurrentState = Parameter.ParseLogicExpression();
    }

    private void HandleStateExecution()
    {
        if (IsStateMode)
        {
            if (CurrentState)
            {
                Execute();
            }
            else
            {
                Revert();
            }
        }
        else
        {
            if (LastState == CurrentState) return;

            if (IsEnableMode == CurrentState || IsDisableMode == !CurrentState)
                Execute();
            else
                Revert();
        }
    }

    public void SetState(bool state) => CurrentState = state;

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
}
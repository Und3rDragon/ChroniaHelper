using ChroniaHelper.Cores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Pico8;
using ChroniaHelper.Components;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using YamlDotNet.Core.Events;
using Classic = On.Celeste.Pico8.Classic;
using ChroniaHelper.Components.StateListeners;

namespace ChroniaHelper.Entities.FormulaBlocks;

[CustomEntity("ChroniaHelper/FormulaBlockLinear")]
[Tracked]
public class FormulaBlockLinear : GroupedBaseSolid
{
    public FormulaBlockLinear(EntityData data, Vc2 offset) : base(data, offset)
    {
        if (Nodes.Length <= 1)
        {
            direction = Vector2.Zero;
        }
        else
        {
            direction = Nodes[1] - Nodes[0];
        }
        duration = data.Float("duration", 1f).ClampMin(Engine.DeltaTime / 2f);
        maxDuration = data.Float("maxMoveDuration", -1f);
        if (maxDuration > 0)
        {
            maxDuration = maxDuration.ClampMin(duration);
        }
        startDelay = data.Float("startDelay", -1f);
        tileType = data.Char("tiletype", '3');
        Depth = data.Int("depth", Depths.Solids);
        flag = data.Attr("flag", "flag");
        bgTexture = data.Bool("bgTexture", false);
        shouldReturn = data.Bool("returnOnFlagDisable", true);
        returnDuration = data.Float("returnDuration", 0.5f).ClampMin(Engine.DeltaTime / 2f);
        
        Add(new LightOcclude());
        SurfaceSoundIndex = data.Int("surfaceSoundIndex", 8);

        flagListener = new(flag)
        {
            onEnable = OnEnable,
            onDisable = OnDisable,
        };
        Add(flagListener);
        reverse = data.Attr("reverseFlag", "moveReversed");
    }
    public float duration, startDelay = -1f, maxDuration = -1f;
    public bool instantStart => startDelay <= 0f;
    public string flag, reverse;
    public bool bgTexture = false;
    public bool shouldReturn = true;
    public float returnDuration = 0.5f;
    private FlagListener flagListener;

    private Vc2 direction;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        routineState = RoutineStates.None;
    }

    public override void GenerateGrid(bool bg = false)
    {
        base.GenerateGrid(bgTexture);
    }

    public override void PostGroupping()
    {
        // Direction modulation, most of the blocks shouldn't have a direction
        // After groupping, the Group is settled, so we can use the Group to see if the direction should be modified
        foreach (var item in Group)
        {
            if ((item as FormulaBlockLinear).direction != Vc2.Zero)
            {
                direction = (item as FormulaBlockLinear).direction;
            }
            // Unite values, although this is actually not that necessary
            (item as FormulaBlockLinear).maxDuration = (master as FormulaBlockLinear).maxDuration;
            (item as FormulaBlockLinear).duration = (master as FormulaBlockLinear).duration;
            (item as FormulaBlockLinear).startDelay = (master as FormulaBlockLinear).startDelay;
        }
    }

    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        if (other is FormulaBlockLinear block)
        {
            float e = 0.001f;
            return block.flag == flag && 
                   block.Depth == Depth &&
                   block.startDelay.IsBetween(startDelay - e, startDelay + e) &&
                   block.duration.IsBetween(duration - e, duration + e) &&
                   block.maxDuration.IsBetween(maxDuration - e, maxDuration + e);
        }
        
        return false;
    }

    private Coroutine normalRoutine, resetRoutine;

    public void InitiateNormalListener()
    {
        normalRoutine = new(NormalSequence());
    }

    public void InitiateResetRoutine()
    {
        resetRoutine = new(ResetSequence());
    }
    
    private IEnumerator NormalSequence()
    {
        // Shaking
        if (!instantStart)
        {
            ShakeSfx();
            StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return startDelay;
            StopShaking();
        }

        Vc2 start = Position;
        float timer = 0f, progress = 0f, elapsed = 0f;
        float overflowProtection = 1000000f;
        while (true)
        {
            elapsed += Engine.DeltaTime;
            timer += reverse.GetFlag() ? -Engine.DeltaTime : Engine.DeltaTime;
            progress = timer / duration;
            
            MoveTo(start.CalculatePointer(start + direction, progress));

            if (maxDuration > 0f && elapsed >= maxDuration)
            {
                break;
            }

            if (timer >= overflowProtection || elapsed >= overflowProtection ||
                (Position - Nodes[0]).X.GetAbs() > overflowProtection || (Position - Nodes[0]).Y.GetAbs() > overflowProtection)
            {
                break;
            }
            
            yield return null;
        }
    }

    private IEnumerator ResetSequence()
    {
        Vc2 start = Position;
        float timer = 0f;

        while (true)
        {
            timer = Calc.Approach(timer, returnDuration, Engine.DeltaTime);
            
            MoveTo(timer.LerpValue(0f, returnDuration, start, Nodes[0]));

            yield return null;
        }
    }

    public enum RoutineStates
    {
        None, Normal, Reset
    }
    private RoutineStates routineState = RoutineStates.None;
    public void OnEnable()
    {
        InitiateNormalListener();
        routineState = RoutineStates.Normal;
    }

    public void OnDisable()
    {
        InitiateResetRoutine();
        routineState = shouldReturn ? RoutineStates.Reset : RoutineStates.None;
    }

    public override void Update()
    {
        base.Update();

        // Routine Updates
        if (routineState == RoutineStates.Normal)
        {
            normalRoutine.Update();
        }
        else if (routineState == RoutineStates.Reset)
        {
            resetRoutine.Update();
        }
    }

    public bool CheckInside()
    {
        Rectangle levelBound = new(MaP.level.Bounds.X - 32, MaP.level.Bounds.Y - 32,
            MaP.level.Bounds.Width + 64, MaP.level.Bounds.Height + 64);
        Rectangle bound = new(GroupBoundsMin.X, GroupBoundsMin.Y,
            GroupBoundsMax.X - GroupBoundsMin.X,
            GroupBoundsMax.Y - GroupBoundsMin.Y);
        return levelBound.Contains(bound);
    }
}

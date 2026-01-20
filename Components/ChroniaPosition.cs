using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Components;

/// <summary>
/// Adding this Component will make the entity movement locked
/// </summary>
public class ChroniaPosition :  BaseComponent
{
    // Positions

    /// <summary>
    /// The very base starting position (Entity Default Position)
    /// </summary>
    public Vc2 ResetPosition { get; private set; }
    /// <summary>
    /// The calculation starting Base Position.
    /// It's suggested to avoid dynamic chages to it, for dynamic changes,
    /// modulate StoredOffsets as priority.
    /// </summary>
    public Vc2 BasePosition;
    /// <summary>
    /// Current calculated Position
    /// </summary>
    public Vc2 RealPositionWithoutParallax => BasePosition + TotalOffset();
    public Vc2 RealPosition => RealPositionWithoutParallax.InParallax(Parallax, StaticScreenPosition);
    
    public struct PositionLoop
    {
        public float? X1;
        public float? X2;
        public float? Y1;
        public float? Y2;
    }
    public PositionLoop Limitations = new() { 
        X1 = null, X2 = null, Y1 = null, Y2 = null 
    };

    // Offsets

    public List<Vc2> AccumulatedOffsets = new();
    public Dictionary<string, Vc2> StoredOffsets = new();

    private List<Vc2> _accumulatedOffsets = new();
    private Dictionary<string, Vc2> _storedOffsets = new();

    // Speeds

    public Vc2 Speed = Vc2.Zero;
    public List<Vc2> Accelerations = new();
    public Dictionary<string, Vc2> StoredSpeedModulations = new();

    private List<Vc2> _accelerations = new();
    private Dictionary<string, Vc2> _storedSpeedModulations = new();
    // Calculate Speed Displacements, avoiding dynamic changes to the BasePosition
    private Vc2 _speedDisplacement = Vc2.Zero;

    // Parallax
    public Vc2 Parallax = Vc2.One, StaticScreenPosition = new Vc2(160f, 90f);

    public ChroniaPosition(Entity entity)
    {
        Entity = entity;
        ResetPosition = BasePosition = Entity.Position;
        ProtectiveCheck();
    }

    public ChroniaPosition(Vc2 position)
    {
        ResetPosition = BasePosition = position;
        ProtectiveCheck();
    }

    private void ProtectiveCheck()
    {
        if (Entity is null) { return; }

        List<Component> toRemove = new();
        foreach(var component in Entity.Components)
        {
            if(component is ChroniaPosition)
            {
                toRemove.Add(component);
            }
        }
        foreach(var item in toRemove)
        {
            Entity.Components.Remove(item);
        }

        Entity.Add(this);
    }

    public void Reset()
    {
        BasePosition = ResetPosition;
        AccumulatedOffsets.Clear();
        StoredOffsets.Clear();
        Accelerations.Clear();
        StoredSpeedModulations.Clear();
        Speed = Vc2.Zero;
        Entity.Position = ResetPosition;
        _speedDisplacement = Vc2.Zero;
    }

    public void ResetDynamics()
    {
        Accelerations.Clear();
        StoredSpeedModulations.Clear();
        Speed = Vc2.Zero;
    }

    public void ResetOffsets()
    {
        AccumulatedOffsets.Clear();
        StoredOffsets.Clear();
        _speedDisplacement = Vc2.Zero;
    }

    public Vc2 TotalOffset()
    {
        Vc2 displacement = _speedDisplacement;

        foreach (var offset in _accumulatedOffsets)
        {
            displacement += offset;
        }
        foreach (var offset in _storedOffsets.Values)
        {
            displacement += offset;
        }

        return displacement;
    }

    public override void Update()
    {
        if (Entity is null) { return; }

        // Replace lists
        _accelerations = Accelerations;
        _accumulatedOffsets = AccumulatedOffsets;
        _storedOffsets = StoredOffsets;
        _storedSpeedModulations = StoredSpeedModulations;

        if (!RoutineRunning["move_base"] && !RoutineRunning["move_to"])
        {
            // Relocate BasePosition
            Vc2 CurrentSpeed = Speed;
            foreach (var speed in _storedSpeedModulations.Values)
            {
                CurrentSpeed += speed;
            }
            _speedDisplacement += CurrentSpeed * Engine.DeltaTime;
            // Calculate accelerations
            for (int i = 0; i < _accelerations.Count; i++)
            {
                if (i == 0)
                {
                    Speed += _accelerations[i];
                    continue;
                }

                _accelerations[i - 1] += _accelerations[i];
            }
        }

        // Reposition Entity
        Vc2 calculatedPosition = RealPosition;

        if(Limitations.X1 != null && Limitations.X2 != null)
        {
            float min = float.Min(Limitations.X1 ?? 0, Limitations.X2 ?? 0);
            float max = float.Max(Limitations.X1 ?? 0, Limitations.X2 ?? 0);
            float delta = max - min;
            if (delta > 0f && calculatedPosition.X >= min && calculatedPosition.X <= max)
            {
                calculatedPosition.X = NumberUtils.Mod(calculatedPosition.X, delta) + min;
            }
        }

        if (Limitations.Y1 != null && Limitations.Y2 != null)
        {
            float min = float.Min(Limitations.Y1 ?? 0, Limitations.Y2 ?? 0);
            float max = float.Max(Limitations.Y1 ?? 0, Limitations.Y2 ?? 0);
            float delta = max - min;
            if (delta > 0f && calculatedPosition.Y >= min && calculatedPosition.Y <= max)
            {
                calculatedPosition.Y = NumberUtils.Mod(calculatedPosition.Y, delta) + min;
            }
        }

        if(Entity is Platform platform)
        {
            platform.MoveTo(calculatedPosition);
        }
        else
        {
            Entity.Position = calculatedPosition;
        }
    }

    public void Move(Vc2 delta, float duration, Ease.Easer easer)
    {
        if(duration.GetAbs() == 0f)
        {
            BasePosition += delta;
            return;
        }

        int id = Calc.Random.Next();
        RoutineRunning[$"routine_move_{id}"] = true;
        Entity.Add(new Coroutine(MoveRoutine(id, delta, duration.GetAbs(), easer)));
    }

    public void Move(int randomID, Vc2 delta, float duration, Ease.Easer easer)
    {
        if (duration.GetAbs() == 0f)
        {
            BasePosition += delta;
            return;
        }

        RoutineRunning[$"routine_move_{randomID}"] = true;
        Entity.Add(new Coroutine(MoveRoutine(randomID, delta, duration.GetAbs(), easer)));
    }

    public void MoveBaseTo(Vc2 target, float duration, Ease.Easer easer)
    {
        if (duration.GetAbs() == 0f)
        {
            BasePosition = target;
            return;
        }

        if (!RoutineRunning["move_base"])
        {
            RoutineRunning["move_base"] = true; // Signalling in advance for some detection-purposes
            Entity.Add(new Coroutine(MoveBaseToRoutine(target, duration.GetAbs(), easer)));
        }
    }

    public void MoveTo(Vc2 target, float duration, Ease.Easer easer)
    {
        if (duration.GetAbs() == 0f)
        {
            ResetOffsets();
            BasePosition = target;
            return;
        }

        if (!RoutineRunning["move_to"])
        {
            RoutineRunning["move_to"] = true; // Signalling in advance for some detection-purposes
            Entity.Add(new Coroutine(MoveToRoutine(target, duration.GetAbs(), easer)));
        }
    }

    public Dictionary<string, bool> RoutineRunning { get; private set; } = new()
    {
        { "move_base", false },
        { "move_to", false }
    };
    public bool UnfoldableMoveRoutineRunning =>
        RoutineRunning["move_base"] || RoutineRunning["move_to"];

    private IEnumerator MoveRoutine(int id, Vc2 delta, float duration, Ease.Easer easer)
    {
        RoutineRunning[$"routine_move_{id}"] = true;

        float timer = 0f;
        StoredOffsets.Create($"routine_move_{id}", Vc2.Zero);
        Vc2 start = Vc2.Zero;
        Vc2 final = delta;

        while(timer < duration)
        {
            timer = timer.Approach(duration, Engine.DeltaTime);
            Vc2 offset = timer.LerpValue(0f, duration, start, final, EaseUtils.EaseToEaseMode[easer]);
            StoredOffsets[$"routine_move_{id}"] = offset;

            yield return null;
        }

        BasePosition += StoredOffsets[$"routine_move_{id}"];
        StoredOffsets.SafeRemove($"routine_move_{id}");

        RoutineRunning[$"routine_move_{id}"] = false;
    }

    private IEnumerator MoveBaseToRoutine(Vc2 target, float duration, Ease.Easer easer)
    {
        RoutineRunning["move_base"] = true;

        float timer = 0f;
        Vc2 start = BasePosition;
        Vc2 final = target;

        while (timer < duration)
        {
            timer = timer.Approach(duration, Engine.DeltaTime);
            Vc2 pos = timer.LerpValue(0f, duration, start, final, EaseUtils.EaseToEaseMode[easer]);
            BasePosition = pos;

            yield return null;
        }

        RoutineRunning["move_base"] = false;
    }

    private IEnumerator MoveToRoutine(Vc2 target, float duration, Ease.Easer easer)
    {
        RoutineRunning["move_to"] = true;

        float timer = 0f;

        BasePosition = Entity.Position;
        ResetOffsets();

        Vc2 start = BasePosition;
        Vc2 final = target;

        while (timer < duration)
        {
            timer = timer.Approach(duration, Engine.DeltaTime);
            Vc2 pos = timer.LerpValue(0f, duration, start, final, EaseUtils.EaseToEaseMode[easer]);
            BasePosition = pos;

            yield return null;
        }

        RoutineRunning["move_to"] = false;
    }
}

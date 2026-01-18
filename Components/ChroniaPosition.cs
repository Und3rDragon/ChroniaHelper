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
    public Vc2 CurrentPosition => BasePosition + TotalOffset();

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

    public ChroniaPosition()
    {
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
        int count = 0;
        foreach(var component in Entity.Components)
        {
            if(component is ChroniaPosition) { count++; }
        }
        if(count > 0)
        {
            RemoveSelf();
        }
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
        // Replace lists
        _accelerations = Accelerations;
        _accumulatedOffsets = AccumulatedOffsets;
        _storedOffsets = StoredOffsets;
        _storedSpeedModulations = StoredSpeedModulations;

        if (!MoveRoutineRunning)
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
        Entity.Position = CurrentPosition.InParallax(Parallax, StaticScreenPosition);
    }

    public void Move(Vc2 delta, float duration, Ease.Easer easer)
    {
        if(duration.GetAbs() == 0f)
        {
            BasePosition += delta;
            return;
        }

        if (!MoveRoutineRunning)
        {
            Entity.Add(new Coroutine(MoveRoutine(delta, duration.GetAbs(), easer)));
        }
    }

    public void MoveBaseTo(Vc2 target, float duration, Ease.Easer easer)
    {
        if (duration.GetAbs() == 0f)
        {
            BasePosition = target;
            return;
        }

        if (!MoveRoutineRunning)
        {
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

        if (!MoveRoutineRunning)
        {
            Entity.Add(new Coroutine(MoveToRoutine(target, duration.GetAbs(), easer)));
        }
    }

    private bool MoveRoutineRunning = false;
    private IEnumerator MoveRoutine(Vc2 delta, float duration, Ease.Easer easer)
    {
        MoveRoutineRunning = true;

        float timer = 0f;
        Vc2 start = BasePosition;
        Vc2 final = start + delta;

        while(timer < duration)
        {
            timer = timer.Approach(duration, Engine.DeltaTime);
            Vc2 pos = timer.LerpValue(0f, duration, start, final, EaseUtils.EaseToEaseMode[easer]);
            BasePosition = pos;

            yield return null;
        }

        MoveRoutineRunning = false;
    }

    private IEnumerator MoveBaseToRoutine(Vc2 target, float duration, Ease.Easer easer)
    {
        MoveRoutineRunning = true;

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

        MoveRoutineRunning = false;
    }

    private IEnumerator MoveToRoutine(Vc2 target, float duration, Ease.Easer easer)
    {
        MoveRoutineRunning = true;

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

        MoveRoutineRunning = false;
    }
}

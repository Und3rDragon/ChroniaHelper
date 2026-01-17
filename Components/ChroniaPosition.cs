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
    // Offsets
    public Vc2 ResetPosition { get; private set; }
    public Vc2 BasePosition;
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
    }

    public override void Update()
    {
        // Replace lists
        _accelerations = Accelerations;
        _accumulatedOffsets = AccumulatedOffsets;
        _storedOffsets = StoredOffsets;
        _storedSpeedModulations = StoredSpeedModulations;

        // Relocate BasePosition
        Vc2 CurrentSpeed = Speed;
        foreach(var speed in _storedSpeedModulations.Values)
        {
            CurrentSpeed += speed;
        }
        BasePosition += CurrentSpeed * Engine.DeltaTime;
        // Calculate accelerations
        for(int i = 0;  i < _accelerations.Count; i++)
        {
            if(i == 0)
            {
                Speed += _accelerations[i];
                continue;
            }

            _accelerations[i - 1] += _accelerations[i];
        }

        // Reposition Entity
        Vc2 CurrentPosition = BasePosition;
        foreach(var offset in _accumulatedOffsets)
        {
            CurrentPosition += offset;
        }
        foreach(var offset in _storedOffsets.Values)
        {
            CurrentPosition += offset;
        }
        Entity.Position = CurrentPosition;
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

    private bool MoveRoutineRunning = false;
    private IEnumerator MoveRoutine(Vc2 delta, float duration, Ease.Easer easer)
    {
        MoveRoutineRunning = true;

        float timer = 0f;
        if (!StoredOffsets.ContainsKey("MoveRoutine"))
        {
            StoredOffsets.Add("MoveRoutine", Vc2.Zero);
        }
        Vc2 start = StoredOffsets["MoveRoutine"];
        Vc2 final = start + delta;

        while(timer < duration)
        {
            timer = timer.Approach(duration, Engine.DeltaTime);
            Vc2 offset = timer.LerpValue(0f, duration, start, final, EaseUtils.EaseToEaseMode[easer]);
            StoredOffsets["MoveRoutine"] = offset;

            yield return null;
        }

        MoveRoutineRunning = false;
    }
}

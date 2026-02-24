using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/CounterCassetteBlockManager")]
public class CounterCassetteBlockManager :  BaseEntity
{
    public CounterCassetteBlockManager(EntityData data, Vc2 offset) : base(data, offset)
    {
        listener = new FlagListener(data.Attr("flag"));
        Add(listener);

        mode = (Mode)data.Int("mode", 2);

        counter = data.Attr("targetCassetteBlockCounter", "counterCassetteBlockCounter");

        operation = (Operation)data.Int("operation", 0);
        number = data.Int("number", 1);
        if (number == 0) { number = 1; }

        maxCounterIndex = data.Int("maxCassetteBlockIndex", -1);
    }
    private FlagListener listener;
    public enum Mode { OnEnable = 0, OnDisable = 1, OnChange = 2 }
    public Mode mode;
    public string counter;
    public enum Operation { Add = 0, Minus = 1, Multiply = 2, Divide = 3}
    public Operation operation;
    public int number = 1;

    private int maxCounterIndex;
    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (maxCounterIndex > 0) { return; }

        maxCounterIndex = 0;
        foreach (CounterCassetteBlock block in scene.Tracker.GetEntities<CounterCassetteBlock>())
        {
            if(block.counter == counter)
            {
                maxCounterIndex = int.Max(maxCounterIndex, block.Index);
            }
        }
    }

    public int Operate(int current)
    {
        if(operation == Operation.Minus)
        {
            current -= number;
        }
        else if(operation == Operation.Multiply)
        {
            current *= number;
        }
        else if(operation == Operation.Divide)
        {
            current /= number;
        }
        else
        {
            current += number;
        }

        current = CheckRange(current);
        return current;
    }

    public int CheckRange(int current)
    {
        if(maxCounterIndex == 0) { return 0; }

        int actualLoop = maxCounterIndex.GetAbs() + 1;

        current = NumberUtils.Mod(current, actualLoop);

        return current;
    }

    public override void Update()
    {
        base.Update();

        if(mode == Mode.OnEnable)
        {
            listener.onEnable = () =>
            {
                counter.SetCounter(Operate(counter.GetCounter()));
            };
        }
        else if(mode == Mode.OnDisable)
        {
            listener.onDisable = () =>
            {
                counter.SetCounter(Operate(counter.GetCounter()));
            };
        }
        else if(mode == Mode.OnChange)
        {
            listener.onSwitch = () =>
            {
                counter.SetCounter(Operate(counter.GetCounter()));
            };
        }
    }
}

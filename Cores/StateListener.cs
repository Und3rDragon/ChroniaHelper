using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Components.ConditionListener;

namespace ChroniaHelper.Cores;

public class StateListener : BaseComponent
{
    public StateListener() { }
    public StateListener(bool active, bool visible) : base(active, visible) { }

    public Action onTrue, onFalse;
    public Action onEnable, onDisable, onSwitch;

    public bool state { get; private set; } = false;
    public bool lastState { get; private set; } = false;
    public override void Update()
    {
        state = GetState();

        if (state)
        {
            onTrue?.Invoke();
        }
        else
        {
            onFalse?.Invoke();
        }

        if (state != lastState)
        {
            onSwitch?.Invoke();
            
            if (state)
            {
                onEnable?.Invoke();
            }
            else
            {
                onDisable?.Invoke();
            }
        }

        lastState = state;
    }

    protected virtual bool GetState()
    {
        return false;
    }
}

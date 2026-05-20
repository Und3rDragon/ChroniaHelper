using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components.StateListeners;

public class StateListener : BaseComponent
{
    public StateListener() { }
    public StateListener(bool active, bool visible) : base(active, visible) { }

    public Action onTrue, onFalse;
    public Action onEnable, onDisable, onSwitch;

    public bool state { get; private set; } = false;
    public bool lastState { get; private set; } = false;
    public bool InstantState => GetState();
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

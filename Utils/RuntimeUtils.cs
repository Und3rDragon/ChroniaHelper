using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public enum InputTypes
{
    Left,
    Right,
    Up,
    Down,
    Jump,
    Dash,
    Grab,
    Interact,
    CrouchDash,
    Any,
}

public static class RuntimeUtils
{
    // Code from vitellery's Trigger Trigger
    public static bool CheckInput(InputTypes inputType, bool held)
    {
        switch (inputType)
        {
            case InputTypes.Grab:
                return (held ? Input.Grab.Check : Input.Grab.Pressed);
            case InputTypes.Jump:
                return (held ? Input.Jump.Check : Input.Jump.Pressed);
            case InputTypes.Dash:
                return (held ? Input.Dash.Check : Input.Dash.Pressed);
            case InputTypes.Interact:
                return (held ? Input.Talk.Check : Input.Talk.Pressed);
            case InputTypes.CrouchDash:
                return (held ? Input.CrouchDash.Check : Input.CrouchDash.Pressed);
            default:
                Vector2 aim = Input.Aim.Value.FourWayNormal();
                if (held || (aim != Input.Aim.PreviousValue.FourWayNormal()))
                {
                    if (inputType == InputTypes.Left && aim.X < 0f)
                        return true;
                    else if (inputType == InputTypes.Right && aim.X > 0f)
                        return true;
                    else if (inputType == InputTypes.Down && aim.Y > 0f)
                        return true;
                    else if (inputType == InputTypes.Up && aim.Y < 0f)
                        return true;
                }
                break;
        }
        return false;
    }
}

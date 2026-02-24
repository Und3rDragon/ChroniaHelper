using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.References;

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
    ESC,
    QuickRestart,
    Pause,
    ActivateFlagController,
    Any,
}

public static class InputUtils
{
    [Credits("vitellary's Trigger Trigger")]
    public static bool CheckInput(this InputTypes inputType, bool held)
    {
        switch (inputType)
        {
            case InputTypes.ActivateFlagController:
                if (Md.CommunalHelperLoaded)
                {
                    return (held ? 
                        RefCommunalHelper.ActivateFlagController.Check : 
                        RefCommunalHelper.ActivateFlagController.Pressed);
                }
                return false;
            case InputTypes.Left:
                return (held ? Input.MenuLeft.Check : Input.MenuLeft.Pressed);
            case InputTypes.Right:
                return (held ? Input.MenuRight.Check : Input.MenuRight.Pressed);
            case InputTypes.Up:
                return (held ? Input.MenuUp.Check : Input.MenuUp.Pressed);
            case InputTypes.Down:
                return (held ? Input.MenuDown.Check : Input.MenuDown.Pressed);
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
            case InputTypes.ESC:
                return (held ? Input.ESC.Check : Input.ESC.Pressed);
            case InputTypes.QuickRestart:
                return (held ? Input.QuickRestart.Check : Input.QuickRestart.Pressed);
            case InputTypes.Pause:
                return (held ? Input.Pause.Check : Input.Pause.Pressed);
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

    /// <summary>
    /// Get Mouse Position under high definition coordinates
    /// </summary>
    public static Vc2 MousePosition => MInput.Mouse.Position;
    /// <summary>
    /// Get Mouse Position under 320 * 180 Pixels resolution
    /// </summary>
    public static Vc2 MousePositionOnScreen => MInput.Mouse.Position / Cons.HDScale;
    /// <summary>
    /// Get Mouse Position in level
    /// </summary>
    public static Vc2 MouseLevelPosition => MaP.cameraPos + MousePositionOnScreen;
}

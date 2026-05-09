using System.Reflection;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/ForceMovementController")]
[Tracked]
[Credits("Maddie for MaxHelpingHand codes")]
[Note("This entity is basically a reversed project of Maddie's Disable Control Controller")]
public class ForceMovementController : BaseEntity {
    private static Hook hookButtonCheck;
    private static Hook hookButtonPressed;
    private static Hook hookButtonReleased;
    private static Hook hookGrabCheck;
    private static Hook hookDashPressed;
    private static Hook hookCrouchDashPressed;
    
    [LoadHook]
    public static void Load() {
        // break directions
        On.Celeste.Player.Update += breakTheControls;

        // break Input.X.Check, Input.X.Pressed, Input.X.Released with X being Jump, Dash, Grab or CrouchDash
        hookButtonCheck = new Hook(typeof(VirtualButton).GetMethod("get_Check"), typeof(ForceMovementController).GetMethod("hookOnButton", BindingFlags.NonPublic | BindingFlags.Static));
        hookButtonPressed = new Hook(typeof(VirtualButton).GetMethod("get_Pressed"), typeof(ForceMovementController).GetMethod("hookOnButton", BindingFlags.NonPublic | BindingFlags.Static));
        hookButtonReleased = new Hook(typeof(VirtualButton).GetMethod("get_Released"), typeof(ForceMovementController).GetMethod("hookOnButton", BindingFlags.NonPublic | BindingFlags.Static));

        // break Input.GrabCheck and Input.DashPressed
        hookGrabCheck = new Hook(typeof(Input).GetMethod("get_GrabCheck"), typeof(ForceMovementController).GetMethod("modGrabResult", BindingFlags.NonPublic | BindingFlags.Static));
        hookDashPressed = new Hook(typeof(Input).GetMethod("get_DashPressed"), typeof(ForceMovementController).GetMethod("modDashResult", BindingFlags.NonPublic | BindingFlags.Static));
        hookCrouchDashPressed = new Hook(typeof(Input).GetMethod("get_CrouchDashPressed"), typeof(ForceMovementController).GetMethod("modDashResult", BindingFlags.NonPublic | BindingFlags.Static));
    }
    [UnloadHook]
    public static void Unload() {
        On.Celeste.Player.Update -= breakTheControls;

        hookButtonCheck?.Dispose();
        hookButtonPressed?.Dispose();
        hookButtonReleased?.Dispose();
        hookGrabCheck?.Dispose();
        hookDashPressed?.Dispose();
        hookCrouchDashPressed?.Dispose();

        hookButtonCheck = null;
        hookButtonPressed = null;
        hookButtonReleased = null;
        hookGrabCheck = null;
        hookDashPressed = null;
        hookCrouchDashPressed = null;
    }

    private readonly bool up, down, left, right, jump, grab, dash;
    private readonly string onlyIfFlag;
    private float value;

    public ForceMovementController(EntityData data, Vector2 offset)
        : base(data, offset)
    {

        value = data.Float("value", 1f);
        up = data.Bool("up");
        down = data.Bool("down");
        left = data.Bool("left");
        right = data.Bool("right");
        jump = data.Bool("jump");
        grab = data.Bool("grab");
        dash = data.Bool("dash");

        onlyIfFlag = data.Attr("onlyIfFlag");
    }

    private static void breakTheControls(On.Celeste.Player.orig_Update orig, Player self) {
        ForceMovementController c = getDisableControlsControllerInRoomSafely();
        if (c == null) {
            // we don't want to disable controls here!
            orig(self);
            return;
        }

        Vector2 oldAim = Input.Aim;
        int oldMoveX = Input.MoveX.Value;
        int oldMoveY = Input.MoveY.Value;

        Vector2 newAim = Input.Aim;
        int newMoveX = Input.MoveX.Value;
        int newMoveY = Input.MoveY.Value;

        Vector2 aimValue = Vc2.Zero;
        int aimMoveValueX = 0, aimMoveValueY = 0;
        
        if (c.up) {
            // Y would be -1
            aimValue.Y -= c.value;
            aimMoveValueY -= (int)c.value;
        }
        if (c.down) {
            // Y would be 1
            aimValue.Y += c.value;
            aimMoveValueY += (int)c.value;
        }
        if (c.left) {
            // X would be -1
            aimValue.X -= c.value;
            aimMoveValueX -= (int)c.value;
        }
        if (c.right) {
            // X would be 1
            aimValue.X += c.value;
            aimMoveValueX += (int)c.value;
        }

        newAim = aimValue;
        newMoveX = aimMoveValueX;
        newMoveY = aimMoveValueY;

        new DynData<VirtualJoystick>(Input.Aim)["Value"] = newAim;
        Input.MoveX.Value = newMoveX;
        Input.MoveY.Value = newMoveY;

        orig(self);

        new DynData<VirtualJoystick>(Input.Aim)["Value"] = oldAim;
        Input.MoveX.Value = oldMoveX;
        Input.MoveY.Value = oldMoveY;
    }

    private static bool hookOnButton(Func<VirtualButton, bool> orig, VirtualButton self) {
        ForceMovementController c = getDisableControlsControllerInRoomSafely();
        if (c == null) {
            // we don't want to disable controls here!
            return orig(self);
        }

        if (((self == Input.Dash || self == Input.CrouchDash) && c.dash)
            || (self == Input.Jump && c.jump)
            || (self == Input.Grab && c.grab)) {

            return true;
        }

        return orig(self);
    }

    private static bool modGrabResult(Func<bool> orig) {
        ForceMovementController c = getDisableControlsControllerInRoomSafely();
        if (c == null || !c.grab) {
            // we don't want to disable grab
            return orig();
        }

        return true;
    }

    private static bool modDashResult(Func<bool> orig) {
        ForceMovementController c = getDisableControlsControllerInRoomSafely();
        if (c == null || !c.dash) {
            // we don't want to disable dash
            return orig();
        }

        return true;
    }

    private static ForceMovementController getDisableControlsControllerInRoomSafely() {
        // return null if the ForceMovementController type isn't tracked yet. This can happen when the mod is being loaded during runtime
        if (Engine.Scene == null || !Engine.Scene.Tracker.Entities.ContainsKey(typeof(ForceMovementController))) return null;

        ForceMovementController controller = Engine.Scene.Tracker.GetEntity<ForceMovementController>();
        if (controller != null && (string.IsNullOrEmpty(controller.onlyIfFlag) || (Engine.Scene as Level).Session.GetFlag(controller.onlyIfFlag))) {
            return controller;
        }
        return null;
    }
}

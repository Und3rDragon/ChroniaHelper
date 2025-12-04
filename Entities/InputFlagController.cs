using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.CommunalHelper.Entities;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using MonoMod.Cil;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/InputFlagController")]
public class InputFlagController : AbstractInputController
{
    public bool Activated =>
        (activateByGrab && (onlyOnHeld ? Input.Grab.Check : Input.Grab.Pressed)) ||
        (activateByDash && (onlyOnHeld ? Input.Dash.Check : Input.Dash.Pressed)) ||
        (activateByCrouchDash && (onlyOnHeld ? Input.CrouchDash.Check : Input.CrouchDash.Pressed)) ||
        (activateByESC && (onlyOnHeld ? Input.ESC.Check : Input.ESC.Pressed)) ||
        (activateByJump && (onlyOnHeld ? Input.Jump.Check : Input.Jump.Pressed)) ||
        (activateByPause && (onlyOnHeld ? Input.Pause.Check : Input.Pause.Pressed)) ||
        (activateByTalk && (onlyOnHeld ? Input.Talk.Check : Input.Talk.Pressed)) ||
        (activateByDefault && (onlyOnHeld ? CommunalHelperModule.Settings.ActivateFlagController.Check : CommunalHelperModule.Settings.ActivateFlagController.Pressed));

    public string[][] Flags;
    private int flagIndex = 0;

    public bool Toggle;

    public bool ResetFlags;

    public float Delay;
    private float cooldown;

    private string[] conditions;
    private bool noConditions;

    private enum Mode { Toggle = 0, Suffix = 1, Enable = 2}
    private Mode mode;
    private enum Restraint
    {
        No_Restraints = 0,
        Usage_Equals = 1,
        Usage_Lower = 2,
        Usage_Greater = 3,
        Usage_Equals_Or_Lower = 4,
        Usage_Equals_Or_Greater = 5,
        Usage_Within_Range = 6,
    }
    private Restraint restraint;
    private string controllerID;
    private string restraintSetup;

    private bool resetCounter, resetSession;

    private bool activateByDefault = true,
        activateByGrab = false,
        activateByDash = false,
        activateByJump = false,
        activateByTalk = false,
        activateByCrouchDash = false,
        activateByESC = false,
        activateByPause = false,
        onlyOnHeld = false;

    public InputFlagController(EntityData data, Vector2 _)
    {
        Flags = data.Attr("flags").Split(';').Select(str => str.Split(',')).ToArray();
        mode = (Mode)data.Int("mode", 0);
        restraint = (Restraint)data.Int("restraints", 0);
        restraintSetup = data.Attr("restraintValue");

        Toggle = mode == (Mode)0;
        ResetFlags = data.Bool("resetFlags");
        Delay = data.Float("delay");

        conditions = data.Attr("ifFlagCondition").Split(",", StringSplitOptions.TrimEntries);
        noConditions = string.IsNullOrEmpty(data.Attr("ifFlagCondition"));
        controllerID = data.Attr("controllerIDPrefix", "ChroniaHelper_Input_") + data.ID;
        if (MaP.level.Session.GetCounter(controllerID) == 0)
        {
            MaP.level.Session.SetCounter(controllerID, 0);
        }
        staticCounterID = data.Attr("detectCounterID", "inputListenerCounter");
        if(MaP.level.Session.GetCounter(staticCounterID) == 0)
        {
            MaP.level.Session.SetCounter(staticCounterID, 0);
        }

        resetCounter = data.Bool("resetCounter", false);
        resetSession = data.Bool("resetDetectSession", false);

        activateByDefault = data.Bool("activateByDefault",true);
        activateByGrab = data.Bool("activateByGrab", false);
        activateByDash = data.Bool("activateByDash", false);
        activateByJump = data.Bool("activateByJump", false);
        activateByTalk = data.Bool("activateByTalk", false);
        activateByCrouchDash = data.Bool("activateByCrouchDash", false);
        activateByESC = data.Bool("activateByESC", false);
        activateByPause = data.Bool("activateByPause", false);
        onlyOnHeld = data.Bool("onlyOnHeld", false);
    }
    private string staticCounterID;
    public bool CheckFlagCondition()
    {
        if (noConditions) { return true; }

        bool result = true;
        foreach(var item in conditions)
        {
            bool inverted = item.StartsWith('!');
            string name = item.TrimStart('!');

            result.TryNegative(name.GetConditionalInvertedFlag(inverted));
        }

        return result;
    }

    public bool CheckRestraints()
    {
        if(restraint == 0) { return true; }
        bool noSetup = string.IsNullOrEmpty(restraintSetup);
        
        if(restraint >= (Restraint)1 && restraint <= (Restraint)5)
        {
            if (noSetup) { return true; }

            int n = 0;
            int.TryParse(restraintSetup, out n);

            return MaP.level.Session.GetCounter(controllerID).Compare(n, (NumberUtils.Comparator)((int)restraint - 1));
        }

        else if (restraint == (Restraint)6)
        {
            if (noSetup) { return true; }
            string[] r = restraintSetup.Split(",", StringSplitOptions.TrimEntries);

            int m = 0, n = 0;
            int.TryParse(r[0], out m);
            if (r.Length >= 2) { int.TryParse(r[1], out n); }

            return MaP.level.Session.GetCounter(controllerID).Compare(m, (NumberUtils.Comparator)((int)restraint - 1), n);
        }

        return true;
    }

    public override void Update()
    {
        base.Update();

        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaTime;
        }
        else if (Activated)
        {
            Activate();
        }
    }

    public override void FrozenUpdate()
    {
        if (cooldown <= 0 && Activated)
        {
            Activate();
        }
    }

    public void Activate()
    {
        MaP.level.Session.SetCounter(controllerID, MaP.level.Session.GetCounter(controllerID) + 1);
        MaP.level.Session.SetCounter(staticCounterID, MaP.level.Session.GetCounter(staticCounterID) + 1);

        if (!CheckFlagCondition()) { return; }
        if (!CheckRestraints()) { return; }

        if(mode == Mode.Toggle || mode == Mode.Enable)
        {
            if (flagIndex < Flags.Length)
            {
                string[] flagSet = Flags[flagIndex];
                bool value = true;
                foreach (string flag in flagSet)
                {
                    if (Toggle)
                    {
                        value = !flag.GetFlag();
                    }

                    flag.SetFlag(!flag.GetFlag());
                }
                flagIndex++;
                if (Toggle && flagIndex >= Flags.Length)
                { 
                    flagIndex = 0; 
                }
            }
        }

        else if(mode == Mode.Suffix)
        {
            foreach(var item in Flags)
            {
                foreach(var i in item)
                {
                    string regex = $"^{controllerID}_{i}:\\d+$";

                    foreach(var flag in MaP.level.Session.Flags)
                    {
                        if (Regex.IsMatch(flag, regex))
                        {
                            flag.SetFlag(false);
                        }
                    }

                    $"{controllerID}_{i}:{MaP.level.Session.GetCounter(controllerID)}".SetFlag(true);
                }
            }
        }
        
        cooldown = Delay;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (ResetFlags)
        {
            if(mode == Mode.Toggle || mode == Mode.Enable)
            {
                for (int i = 0; i < Flags.Length; i++)
                {
                    foreach (string flag in Flags[i])
                    {
                        flag.SetFlag(false);
                    }
                }
            }
            else if(mode == Mode.Suffix)
            {
                foreach(var sets in Flags)
                {
                    foreach(var i in sets)
                    {
                        string regex = $"^{controllerID}_{i}:\\d+$";

                        foreach (var flag in MaP.level.Session.Flags)
                        {
                            if (Regex.IsMatch(flag, regex))
                            {
                                flag.SetFlag(false);
                            }
                        }
                    }
                }
            }
        }
        if (resetCounter)
        {
            MaP.level.Session.SetCounter(controllerID, 0);
        }
        if (resetSession)
        {
            MaP.level.Session.SetCounter(staticCounterID, 0);
        }
    }
}

[Tracked(true)]
public abstract class AbstractInputController : Entity
{
    public AbstractInputController() : base() { }

    public AbstractInputController(Vector2 position) : base(position) { }

    public abstract void FrozenUpdate();

    [LoadHook]
    internal static void Load()
    {
        IL.Monocle.Engine.Update += Engine_Update;
    }
    [UnloadHook]
    internal static void Unload()
    {
        IL.Monocle.Engine.Update -= Engine_Update;
    }

    private static void Engine_Update(ILContext il)
    {
        ILCursor cursor = new(il);
        /*
        Stick this in the block
        if (FreezeTimer > 0) {
            AbstractController.UpdateControllers(); <--
            ...
        }
        */
        if (cursor.TryGotoNext(instr => instr.MatchLdsfld<Engine>("FreezeTimer"),
            instr => instr.MatchCall<Engine>("get_RawDeltaTime")))
        {
            cursor.EmitDelegate<Action>(UpdateControllers);
        }
    }

    private static void UpdateControllers()
    {
        foreach (AbstractInputController controller in Engine.Scene.Tracker.GetEntities<AbstractInputController>())
        {
            controller.FrozenUpdate();
        }
    }
}

using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/AutoMoveController")]
public class AutoMoveController : BaseEntity
{
    public AutoMoveController(EntityData data, Vector2 offset) : base(data, offset)
    {
        commands = data.Attr("commands");
        flag = data.Attr("playFlag", "autoMove");
        ParseCommands();
        
        Add(new Coroutine(ApplyCommands()));
        
        Tag |= Tags.TransitionUpdate;
    }
    public string commands, flag;

    [Flags]
    public enum Controls
    {
        None = 0,
        Up = 1,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        Jump = 1 << 4,
        Dash = 1 << 5,
        Grab = 1 << 6,
    }
    
    // Either Controls or float
    public List<object> executions = new();
    public void ParseCommands()
    {
        var lines = commands.Split(';', StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            var ops = line.Split(',', StringSplitOptions.TrimEntries);
            Controls control = 0;
            foreach (var op in ops)
            {
                string _op = op.ToLower();

                if (_op == "up" || _op == "w")
                {
                    control |= Controls.Up;
                }
                else if (_op == "down" || _op == "s")
                {
                    control |= Controls.Down;
                }
                else if (_op == "left" || _op == "a")
                {
                    control |= Controls.Left;
                }
                else if (_op == "right" || _op == "d")
                {
                    control |= Controls.Right;
                }
                else if (_op == "jump" || _op == "j" || _op == "c")
                {
                    control |= Controls.Jump;
                }
                else if (_op == "grab" || _op == "g" || _op == "z")
                {
                    control |= Controls.Grab;
                }
                else if (_op == "dash" || _op == "x")
                {
                    control |= Controls.Dash;
                }
                else if (float.TryParse(op, out float t))
                {
                    executions.Add(t);
                    continue;
                }
                
                executions.Add(control);
            }
        }
    }

    private ForceMovementController controller = null;
    private string ControllerFlag => "CutsceneMovementController_Assistive_" + flag;
    public override void Added(Scene scene)
    {
        base.Added(scene);

        controller = new ForceMovementController(new EntityData(), Vc2.Zero)
        {
            up = false,
            down = false,
            left = false,
            right = false,
            jump = false,
            grab = false,
            dash = false,
            onlyIfFlag = ControllerFlag,
        };
        scene.Add(controller);
        controller.Added(scene);
    }

    public override void Removed(Scene scene)
    {
        controller?.Removed(scene);
        scene.Remove(controller);
        
        base.Removed(scene);
    }

    public IEnumerator ApplyCommands()
    {
        while (!flag.GetFlag() || controller is null)
        {
            yield return null;
        }

        ControllerFlag.SetFlag(true);
        
        for (int index = 0; index < executions.Count; index++)
        {
            object package = executions[index];

            if (package is float)
            {
                yield return (float)package;
                continue;
            }
            else if (package is Controls)
            {
                Controls apply = (Controls)package;
                
                controller.up = (apply & Controls.Up) != 0;
                controller.down = (apply & Controls.Down) != 0;
                controller.left = (apply & Controls.Left) != 0;
                controller.right = (apply & Controls.Right) != 0;
                controller.jump = (apply & Controls.Jump) != 0;
                controller.grab = (apply & Controls.Grab) != 0;
                controller.dash = (apply & Controls.Dash) != 0;

                continue;
            }

            yield return null;
        }
        
        ControllerFlag.SetFlag(false);
        controller.up = false;
        controller.down = false;
        controller.left = false;
        controller.right = false;
        controller.jump = false;
        controller.grab = false;
        controller.dash = false;
    }
}
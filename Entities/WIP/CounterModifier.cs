using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Components.Graphical;
using ChroniaHelper.Cores;
using ChroniaHelper.Cores.Graphical;

namespace ChroniaHelper.Entities.WIP;

[Tracked]
[WorkingInProgress]
// [CustomEntity("ChroniaHelper/CounterModifier")]
public class CounterModifier : BaseEntity
{
    public CounterModifier(EntityData d, Vc2 o) : base(d, o)
    {
        text = new();
        text.TargetText = "Hahaha";
        Add(text);
        text.Visible = false;
    }
    private bool interacting;
    private SerialImageComponent text;
    
    public IEnumerator CustomLookRoutine(Player player)
    {
        Level level = this.SceneAs<Level>();

        SandwichLava first = this.Scene.Entities.FindFirst<SandwichLava>();
        if (first != null)
            first.Waiting = true;

        if (player.Holding != null)
            player.Drop();

        player.StateMachine.State = 11;
        yield return player.DummyWalkToExact((int)this.X, cancelOnFall: true);

        if (Math.Abs(this.X - player.X) > 4f || player.Dead || !player.OnGround())
        {
            if (!player.Dead)
                player.StateMachine.State = 0;
            yield break;
        }

        Audio.Play("event:/game/general/lookout_use", this.Position);

        // new operations
        while (!Input.MenuCancel.Pressed
               && !Input.MenuConfirm.Pressed
               && !Input.Dash.Pressed
               && !Input.Jump.Pressed
               && this.interacting)
        {
            text.Visible = true;
            // 在这里写你的每帧逻辑
            yield return null;
        }

        // quitting
        Audio.Play("event:/ui/game/lookout_off");

        this.interacting = false;
        player.StateMachine.State = 0;
    }

}
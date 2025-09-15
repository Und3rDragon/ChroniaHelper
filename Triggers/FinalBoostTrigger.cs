using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FinalBoostTrigger")]
public class FinalBoostTrigger : BaseTrigger
{
    public FinalBoostTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        endLevel = data.Bool("endLevel", false);
        finalCh9Boost = data.Bool("finalCh9Boost", false);
        flash = data.Bool("flash", true);
    }
    private bool endLevel, finalCh9Boost, flash;

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        if (player.Holding != null)
        {
            player.Drop();
        }

        player.StateMachine.State = 11;
        player.DummyAutoAnimate = false;
        player.DummyGravity = false;
        if (player.Inventory.Dashes > 1)
        {
            player.Dashes = 1;
        }
        else
        {
            player.RefillDash();
        }

        player.RefillStamina();
        player.Speed = Vector2.Zero;
        int num = Math.Sign(player.X - X);
        if (num == 0)
        {
            num = -1;
        }

        //BadelineDummy badeline = new BadelineDummy(Position);
        //Scene.Add(badeline);
        //player.Facing = (Facings)(-num);
        //badeline.Sprite.Scale.X = num;
        //Vector2 playerFrom = player.Position;
        //Vector2 playerTo = Position + new Vector2(num * 4, -3f);
        //Vector2 badelineFrom = badeline.Position;
        //Vector2 badelineTo = Position + new Vector2(-num * 4, 3f);
        //for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.2f)
        //{
        //    Vector2 vector = Vector2.Lerp(playerFrom, playerTo, p);
        //    if (player.Scene != null)
        //    {
        //        player.MoveToX(vector.X);
        //    }

        //    if (player.Scene != null)
        //    {
        //        player.MoveToY(vector.Y);
        //    }

        //    badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, p);
        //    yield return null;
        //}

        //Vector2 screenSpaceFocusPoint = new Vector2(Calc.Clamp(player.X - level.Camera.X, 120f, 200f), Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
        //Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.5f, 0.18f)));
        //Engine.TimeRate = 0.5f;

        //badeline.Sprite.Play("boost");
        //yield return 0.1f;
        //if (!player.Dead)
        //{
        //    player.MoveV(5f);
        //}

        //yield return 0.1f;
        //if (endLevel)
        //{
        //    level.TimerStopped = true;
        //    level.RegisterAreaComplete();
        //}

        //if (finalCh9Boost)
        //{
        //    //Scene.Add(new CS10_FinalLaunch(player, this, finalCh9Dialog));
        //    player.Active = false;
        //    badeline.Active = false;
        //    Active = false;
        //    yield return null;
        //    player.Active = true;
        //    badeline.Active = true;
        //}

        //Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
        //{
        //    if (player.Dashes < player.Inventory.Dashes)
        //    {
        //        player.Dashes++;
        //    }

        //    Scene.Remove(badeline);
        //    (Scene as Level).Displacement.AddBurst(badeline.Position, 0.25f, 8f, 32f, 0.5f);
        //}, 0.15f, start: true));
        //(Scene as Level).Shake();
        ////holding = null;
        //if (finalCh9Boost)
        //{
        //    Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part2", Position);
        //}
        
        Engine.FreezeTimer = 0.1f;
        yield return null;
        if (endLevel)
        {
            level.TimerHidden = true;
        }
        
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
        if (flash)
        {
            level.Flash(Color.White * 0.5f, drawPlayerOver: true);
        }
        level.DirectionalShake(-Vector2.UnitY, 0.6f);
        level.Displacement.AddBurst(Center, 0.6f, 8f, 64f, 0.5f);
        //level.ResetZoom();
        player.SummitLaunch(X);
        Engine.TimeRate = 1f;
        Finish();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);

        if (endLevel)
        {
            level.RegisterAreaComplete();
            level.CompleteArea(true, false, false);
            PUt.player.StateMachine.State = 11;
        }
    }

    public void Finish()
    {
        SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
        //SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
        SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
        SceneAs<Level>().CameraOffset = new Vector2(0f, -16f);
    }
}

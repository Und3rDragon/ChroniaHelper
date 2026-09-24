using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Components.Graphical;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/CounterModifier")]
public class CounterModifier : BaseEntity
{
    /// <summary>
    /// The title is drawn in the HUD layer so the text stays crisp,
    /// which means it has to live on its own entity since tags are per-entity.
    /// </summary>
    private class TitleRenderer : Entity
    {
        public TitleRenderer(ActiveFontRenderPack pack)
        {
            Tag = Tags.HUD;
            Depth = int.MinValue;
            Add(pack);
        }
    }

    public CounterModifier(EntityData d, Vc2 o) : base(d, o)
    {
        targetName = d.Attr("targetName", "targetCounter");
        offsetY = d.Float("offsetY", -24f);
        step = d.Int("step", 1);

        Collider = new Hitbox(4f, 4f, -2f, -4f);

        title = new(targetName)
        {
            RelativePosition = Vc2.Zero,
            Rendering = false,
            Outlined = true
        };
        titleHost = new TitleRenderer(title);
        
        value = new()
        {
            RelativePosition = Vc2.Zero,
            Rendering = false,
        };
        Add(value);

        value.Main.distance = -2;

        Add(talk = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vc2(-0.5f, -20f), Interact));
        talk.PlayerMustBeFacing = false;
    }

    public string targetName;
    public float offsetY;
    public int step;

    private bool interacting;
    private TalkComponent talk;
    private ActiveFontRenderPack title;
    private Entity titleHost;
    private SerialImageRenderPack value;

    private const string UseSound = "event:/game/general/lookout_use";
    private const string OnSound = "event:/ui/game/lookout_on";
    private const string OffSound = "event:/ui/game/lookout_off";

    /// <summary>
    /// The scale applied to the value image, it is used to keep the displayed
    /// size of the value as a readable amount as it is drawn in the gameplay layer
    /// </summary>
    private const float ValueScale = 6f;

    /// <summary>
    /// The gap in screen pixels between the value and the title
    /// </summary>
    private const float TitleGap = 10f;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(titleHost);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        scene.Remove(titleHost);

        if (interacting)
        {
            Player player = scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = 0;
            }
        }
    }

    private void Interact(Player player)
    {
        Coroutine coroutine = new Coroutine(ModifyRoutine(player));
        coroutine.RemoveOnComplete = true;
        Add(coroutine);
        interacting = true;
    }

    private void UpdateDisplay()
    {
        SetRendering(true);

        value.RelativePosition = new Vc2(0f, offsetY);

        // Measure the value first, the title is placed right above it
        value.Main.Measure(value.TargetText, value.TextureSelector);

        // The title is drawn in the HUD layer, which uses the 1080p canvas
        // instead of the gameplay resolution, so its position has to be
        // converted from world coordinates to screen coordinates
        Vc2 screenPosition = (Position - SceneAs<Level>().Camera.Position) * Cons.HDScale;
        Vc2 valueScreenPosition = screenPosition + new Vc2(0f, offsetY * Cons.HDScale);

        // The value is scaled up in the gameplay layer, so its measured size
        // already matches its on screen size
        float valueHalfHeight = value.Main.overallSize.Y * 0.5f;
        float titleHalfHeight = ActiveFont.LineHeight * title.Scale.Y * 0.5f;

        title.OverridePosition = valueScreenPosition
            + new Vc2(0f, -valueHalfHeight - TitleGap - titleHalfHeight);
    }

    private void SetRendering(bool visible)
    {
        title.Rendering = visible;
        value.Rendering = visible;
    }

    private IEnumerator ModifyRoutine(Player player)
    {
        Level level = SceneAs<Level>();

        SandwichLava lava = Scene.Entities.FindFirst<SandwichLava>();
        if (lava != null)
        {
            lava.Waiting = true;
        }

        if (player.Holding != null)
        {
            player.Drop();
        }

        player.StateMachine.State = 11;
        yield return player.DummyWalkToExact((int)X, walkBackwards: false, 1f, cancelOnFall: true);

        if (Math.Abs(X - player.X) > 4f || player.Dead || !player.OnGround())
        {
            if (!player.Dead)
            {
                player.StateMachine.State = 0;
            }

            yield break;
        }

        Audio.Play(UseSound, Position);
        Audio.Play(OnSound);

        int lastSign = 0;

        while (!Input.MenuCancel.Pressed
               && !Input.MenuConfirm.Pressed
               && !Input.Dash.Pressed
               && !Input.Jump.Pressed
               && interacting)
        {
            value.TargetText = targetName.GetCounter().ToString();
            UpdateDisplay();

            int sign = Math.Sign(Input.Aim.Value.X);

            // 只在方向键「按下」的那一帧调整数值，按住不会持续变化
            if (sign != 0 && sign != lastSign)
            {
                targetName.SetCounter(targetName.GetCounter() + sign * step);
            }

            lastSign = sign;

            yield return null;
        }

        Audio.Play(OffSound);

        SetRendering(false);
        interacting = false;
        player.StateMachine.State = 0;
    }
}

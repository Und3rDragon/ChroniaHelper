using System.Collections;
using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

public class BaseSolid : Solid
{

    private EntityID id { get; set; }

    protected char tileType { get; private set; }

    protected float lightOcclude { get; private set; }

    private string triggerSound { get; set; }

    private string collapseSound { get; set; }

    private float crumbleDelayOnClimb { get; set; }

    private float crumbleDelayTouchTop { get; set; }

    private float crumbleDelayTouchBottom { get; set; }

    private float crumbleDelayTouchLeft { get; set; }

    private float crumbleDelayTouchRight { get; set; }

    private float crumbleDelayJumpoff { get; set; }

    private bool crumblePermanent { get; set; }

    protected bool blendIn { get; private set; }

    protected Level level { get; private set; }

    private static string DefaultTriggerSound { get; set; }

    private static string DefaultCollapseSound { get; set; }

    private bool destroyAttached { get; set; }

    static BaseSolid()
    {
        BaseSolid.DefaultTriggerSound = "event:/game/general/platform_disintegrate";
        BaseSolid.DefaultCollapseSound = "event:/new_content/game/10_farewell/quake_rockbreak";
    }

    public BaseSolid(Vector2 position, EntityData data, EntityID id) : base(position, data.Width, data.Height, safe: true)
    {
        this.id = id;
        this.tileType = data.Char("tileType", '3');
        this.lightOcclude = data.Float("lightOcclude", 1F, 0F, 1F);
        this.triggerSound = data.String("triggerSound", BaseSolid.DefaultTriggerSound);
        this.collapseSound = data.String("collapseSound", BaseSolid.DefaultCollapseSound);
        this.crumbleDelayOnClimb = data.Float("crumbleDelayOnClimb", -1F, -1F);
        this.crumbleDelayTouchTop = data.Float("crumbleDelayTouchTop", -1F, -1F);
        this.crumbleDelayTouchBottom = data.Float("crumbleDelayTouchBottom", -1F, -1F);
        this.crumbleDelayTouchLeft = data.Float("crumbleDelayTouchLeft", -1F, -1F);
        this.crumbleDelayTouchRight = data.Float("crumbleDelayTouchRight", -1F, -1F);
        this.crumbleDelayJumpoff = data.Float("crumbleDelayJumpoff", -1F, -1F);
        this.crumblePermanent = data.Bool("crumblePermanent", false);
        this.blendIn = data.Bool("blendIn", false);
        this.destroyAttached = data.Bool("destroyAttached", false);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
        base.Add(new Coroutine(this.CrumbleSequence(), true));
    }

    private IEnumerator CrumbleSequence()
    {
        bool flag = false;
        while (true)
        {
            if ((this.crumbleDelayOnClimb >= 0F) && (flag = base.HasPlayerClimbing()))
            {
                Audio.Play(this.triggerSound, base.Position);
                yield return this.crumbleDelayOnClimb;
            }
            else if ((this.GetPlayerOnTouch(out PositionDirections positionDirections) != PositionDirections.None))
            {
                if (((this.crumbleDelayTouchTop >= 0F) || (this.crumbleDelayJumpoff >= 0F)) && (flag = (positionDirections == PositionDirections.Top)))
                {
                    Audio.Play(this.triggerSound, base.Position);
                    if ((this.crumbleDelayTouchTop >= 0F) && (this.crumbleDelayJumpoff >= 0F))
                    {
                        float timer = this.crumbleDelayJumpoff;
                        while ((timer > 0F) && (this.GetPlayerOnTouch() == PositionDirections.Top))
                        {
                            timer -= Engine.RawDeltaTime;
                            yield return null;
                        }
                        yield return (timer <= 0F) ? (this.crumbleDelayTouchTop - timer) : timer;
                    }
                    else
                    {
                        yield return (this.crumbleDelayTouchTop >= 0F) ? this.crumbleDelayTouchTop : this.crumbleDelayJumpoff;
                    }
                }
                else if ((this.crumbleDelayTouchBottom >= 0F) && (flag = (positionDirections == PositionDirections.Bottom)))
                {
                    Audio.Play(this.triggerSound, base.Position);
                    yield return this.crumbleDelayTouchBottom;
                }
                else if ((this.crumbleDelayTouchLeft >= 0F) && (flag = (positionDirections == PositionDirections.Left)))
                {
                    Audio.Play(this.triggerSound, base.Position);
                    yield return this.crumbleDelayTouchLeft;
                }
                else if ((this.crumbleDelayTouchRight >= 0F) && (flag = (positionDirections == PositionDirections.Right)))
                {
                    Audio.Play(this.triggerSound, base.Position);
                    yield return this.crumbleDelayTouchRight;
                }
            }
            if (flag)
            {
                this.CrumbleBreak();
            }
            yield return null;
        }
    }

    private void CrumbleBreak()
    {
        if ((ObjectUtils.IsNull(base.Scene)) || (!base.Collidable))
        {
            return;
        }
        Audio.Play(this.collapseSound, base.Position);
        base.Collidable = false;
        for (int i = 0; i < base.Width / 8F; i++)
        {
            for (int j = 0; j < base.Height / 8F; j++)
            {
                if (!base.Scene.CollideCheck<Solid>(new Rectangle((int)base.X + i * 8, (int)base.Y + j * 8, 8, 8)))
                {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(base.Position + new Vector2(4 + i * 8, 4 + j * 8), this.tileType, true).BlastFrom(base.TopCenter));
                }
            }
        }
        if (destroyAttached)
        {
            base.DestroyStaticMovers();
        }
        if (this.crumblePermanent)
        {
            this.level.Session.DoNotLoad.Add(this.id);
        }
        base.RemoveSelf();
    }

    private PositionDirections GetPlayerOnTouch()
    {
        foreach (Player player in this.level.Tracker.GetEntities<Player>())
        {
            if (base.CollideCheck(player, base.Position - Vector2.UnitY))
            {
                return PositionDirections.Top;
            }
            if (base.CollideCheck(player, base.Position + Vector2.UnitY))
            {
                return PositionDirections.Bottom;
            }
            if ((player.Facing == Facings.Right) && (base.CollideCheck(player, base.Position - Vector2.UnitX)))
            {
                return PositionDirections.Left;
            }
            if ((player.Facing == Facings.Left) && (base.CollideCheck(player, base.Position + Vector2.UnitX)))
            {
                return PositionDirections.Right;
            }
        }
        return PositionDirections.None;
    }

    private PositionDirections GetPlayerOnTouch(out PositionDirections positionDirections)
    {
        return (positionDirections = this.GetPlayerOnTouch());
    }

    private bool HasPlayerOnTouch()
    {
        return (this.GetPlayerOnTouch() != PositionDirections.None);
    }

}

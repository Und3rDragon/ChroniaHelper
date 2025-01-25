using System.Collections;

namespace ChroniaHelper.Utils;

public class BaseSolidBlock : Solid
{

    private EntityID id;

    protected char tileType;

    protected float lightOcclude;

    private string[] crumbleFlag;

    private float crumbleFlagDelay;

    private float crumbleLeftTouchDelay;

    private float crumbleRightTouchDelay;

    private float crumbleOnTopDelay;

    private float crumbleJumpDelay;

    private float crumbleOnBottomDelay;

    private float crumbleClimbDelay;

    private string crumbleSound;

    private bool crumblePermanent;

    protected Level level;

    protected bool enableCrumble;

    private static string CrumbleDefaultSound;

    static BaseSolidBlock()
    {
        BaseSolidBlock.CrumbleDefaultSound = "event:/new_content/game/10_farewell/quake_rockbreak";
    }

    public BaseSolidBlock(Vector2 position, EntityData data, EntityID id, bool enableCrumble = false) : base(position, data.Width, data.Height, true)
    {
        this.id = id;
        this.tileType = data.Char("tileType", '3');
        this.lightOcclude = data.Float("lightOcclude", 1F);
        this.crumbleFlag = FlagUtils.Parse(data.Attr("crumbleFlag", null));
        this.crumbleFlagDelay = data.Float("crumbleFlagDelay", 0F);
        this.crumbleLeftTouchDelay = data.Float("crumbleLeftTouchDelay", 0.1F);
        this.crumbleRightTouchDelay = data.Float("crumbleRightTouchDelay", 0.1F);
        this.crumbleOnTopDelay = data.Float("crumbleOnTopDelay", 1F);
        this.crumbleJumpDelay = data.Float("crumbleJumpDelay", 0.2F);
        this.crumbleOnBottomDelay = data.Float("crumbleOnBottomDelay", 0.1F);
        this.crumbleClimbDelay = data.Float("crumbleClimbDelay", 0.6F);
        this.crumbleSound = data.Attr("crumbleSound", null);
        this.crumblePermanent = data.Bool("crumblePermanent", false);
        this.enableCrumble = enableCrumble;
        if (string.IsNullOrEmpty(this.crumbleSound))
        {
            this.crumbleSound = BaseSolidBlock.CrumbleDefaultSound;
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
        if (this.enableCrumble)
        {
            base.Add(new Coroutine(CrumbleSequence(), true));
        }
    }

    private IEnumerator CrumbleSequence()
    {
        while (true)
        {
            bool flag = this.crumbleFlagDelay < 0;
            /*if ((!flag) && (flag = FlagUtils.IsCorrectFlag(this.level, this.crumbleFlag)))
            {
                yield return this.crumbleFlagDelay;
                if (!this.crumbleUseOnTop && !this.crumbleUseJump && !this.crumbleUseClimb)
                {
                    this.Break();
                    break;
                }
            }
            while (flag)
            {
                if (this.crumbleUseTouch && this.HasPlayerOnTouch())
                {
                    yield return this.crumbleTouchDelay;
                    this.Break();
                    break;
                }
                if ((this.crumbleClimbDelay >= 0) && (base.HasPlayerClimbing()))
                {
                    yield return this.crumbleClimbDelay;
                    this.Break();
                    break;
                }
                if ((this.crumbleUseOnTop || this.crumbleUseJump) && base.HasPlayerOnTop())
                {
                    float timer = this.crumbleUseJump ? this.crumbleJumpDelay : 0F;
                    while ((timer > 0F) && base.HasPlayerOnTop())
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                    if ((this.crumbleUseOnTop || !this.crumbleUseJump) && (timer <= 0F))
                    {
                        timer = this.crumbleOnTopDelay - timer;
                    }
                    yield return timer;
                    this.Break();
                    break;
                }
                yield return null;
            }*/
            yield return null;
        }
    }

    private bool HasPlayerOnTouch()
    {
        foreach (Player player in this.level.Tracker.GetEntities<Player>())
        {
            if (player.Facing == Facings.Left && base.CollideCheck(player, base.Position + Vector2.UnitX))
            {
                return true;
            }
            if (player.Facing == Facings.Right && base.CollideCheck(player, base.Position - Vector2.UnitX))
            {
                return true;
            }
            if (base.CollideCheck(player, base.Position - Vector2.UnitY))
            {
                return true;
            }
            if (base.CollideCheck(player, base.Position + Vector2.UnitY))
            {
                return true;
            }
        }
        return false;
    }

    protected void Break()
    {
        if (!base.Collidable || base.Scene == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(this.crumbleSound))
        {
            Audio.Play(this.crumbleSound, base.Position);
        }
        base.Collidable = false;
        for (int i = 0; i < base.Width / 8F; i++)
        {
            for (int j = 0; j < base.Height / 8F; j++)
            {
                if (!base.Scene.CollideCheck<Solid>(new Rectangle((int) base.X + i * 8, (int) base.Y + j * 8, 8, 8)))
                {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(base.Position + new Vector2(4 + i * 8, 4 + j * 8), this.tileType, true).BlastFrom(base.TopCenter));
                }
            }
        }
        if (this.crumblePermanent)
        {
            base.SceneAs<Level>().Session.DoNotLoad.Add(this.id);
        }
        base.RemoveSelf();
    }

    public int GetPlayerTouch()
    {
        foreach (Player player in this.level.Tracker.GetEntities<Player>())
        {
            if (base.CollideCheck(player, base.Position - Vector2.UnitY))
            {
                return 1;
            }
            if (base.CollideCheck(player, base.Position + Vector2.UnitY))
            {
                return 2;
            }
            if (player.Facing == Facings.Right && base.CollideCheck(player, base.Position - Vector2.UnitX))
            {
                return 3;
            }
            if (player.Facing == Facings.Left && base.CollideCheck(player, base.Position + Vector2.UnitX))
            {
                return 4;
            }
        }
        return 0;
    }

    public float topKillTimer;

    public float bottomKillTimer;

    public float leftKillTimer;

    public float rightKillTimer;

    public float currentKillTimer;

    public void TimedKill()
    {
        int touch = GetPlayerTouch();
        if (touch > 0)
        {
            if (this.topKillTimer == 0 && touch == 1)
            {
                Player player = this.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - base.Position).SafeNormalize());
            }
            else if (this.bottomKillTimer == 0 && touch == 2)
            {
                Player player = this.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - base.Position).SafeNormalize());
            }
            else if (this.leftKillTimer == 0 && touch == 3)
            {
                Player player = this.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - base.Position).SafeNormalize());
            }
            else if (this.rightKillTimer == 0 && touch == 4)
            {
                Player player = this.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - base.Position).SafeNormalize());
            }
            else
            {
                if (this.currentKillTimer > 0)
                {
                    this.currentKillTimer -= Engine.DeltaTime;
                    if (this.currentKillTimer <= 0)
                    {
                        Player player = this.level.Tracker.GetEntity<Player>();
                        if (player == null)
                        {
                            return;
                        }
                        player.Die((player.Position - base.Position).SafeNormalize());
                    }
                }
                else
                {
                    this.currentKillTimer = (touch) switch
                    {
                        1 => this.topKillTimer,
                        2 => this.bottomKillTimer,
                        3 => this.leftKillTimer,
                        4 => this.rightKillTimer,
                        _ => -1
                    };
                }
            }
        }
        else
        {
            this.currentKillTimer = -1;
        }
    }

}

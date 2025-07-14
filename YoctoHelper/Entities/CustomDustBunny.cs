using System;
using Celeste.Mod.Entities;
using YoctoHelper.Components;
using YoctoHelper.Cores;

namespace YoctoHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/CustomDustBunny")]
public class CustomDustBunny : Entity
{

    public Color tintColor;

    public Color eyesColor;

    public Color borderColor;

    public bool hasEyes;

    private bool attached;

    private DustBunnyGraphic sprite;

    private float offset;

    public string baseTexture, overlayTexture, centerTexture;

    public CustomDustBunny(Vector2 position, EntityData data) : base(position)
    {
        this.tintColor = data.HexColor("tintColor", new Color(102, 102, 102));
        this.eyesColor = data.HexColor("eyesColor", Color.Red);
        this.borderColor = data.HexColor("borderColor", Color.White);
        this.hasEyes = data.Bool("hasEyes", true);
        this.attached = data.Bool("attached", false);
        base.Collider = new ColliderList(new Circle(6F), new Hitbox(16F, 4F, -8F, -3F));

        baseTexture = data.Attr("baseTexture", "ChroniaHelper/CustomDustBunny/base");
        overlayTexture = data.Attr("overlayTexture", "ChroniaHelper/CustomDustBunny/overlay");
        centerTexture = data.Attr("centerTexture", "ChroniaHelper/CustomDustBunny/center");
        base.Add(this.sprite = new DustBunnyGraphic(this));
        base.Add(new PlayerCollider(this.OnPlayer));
        base.Add(new HoldableCollider(this.OnHoldable));
        base.Add(new LedgeBlocker());
        if (this.attached)
        {
            base.Add(new StaticMover
            {
                OnShake = this.OnShake,
                SolidChecker = this.IsRiding
            });
        }
        this.offset = Calc.Random.NextFloat();
        base.Depth = Depths.Dust;
    }

    public CustomDustBunny(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    private void OnPlayer(Player player)
    {
        player.Die((player.Position - base.Position).SafeNormalize());
        this.sprite.OnHitPlayer();
    }

    private void OnHoldable(Holdable holdable)
    {
        holdable.HitSpinner(this);
    }

    private void OnShake(Vector2 amount)
    {
        this.sprite.position += amount;
    }

    private bool IsRiding(Solid solid)
    {
        return base.CollideCheck(solid);
    }

    public override void Update()
    {
        base.Update();
        if ((base.Scene.OnInterval(0.05F, this.offset)) && (this.sprite.established))
        {
            Player player = base.Scene.Tracker.GetEntity<Player>();
            if (ObjectUtils.IsNotNull(player))
            {
                base.Collidable = (Math.Abs(player.X - base.X) < 128F) && (Math.Abs(player.Y - base.Y) < 128F);
            }
        }
    }

}

using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/RefillWall")]
public class RefillWall : Refill
{

    private ChroniaColor borderColor;

    private float borderAlpha;

    private ChroniaColor innerColor;

    private float innerAlpha;

    private int respawnHorizontalPointStep;

    private int respawnVerticalPointStep;

    private int respawnHorizontalPointNumber;

    private int respawnVerticalPointNumber;

    public RefillWall(Vector2 position, EntityData data) : base(position, data)
    {
        base.Collider = new Hitbox(data.Width, data.Height);
        this.respawnHorizontalPointStep = 2;
        this.respawnVerticalPointStep = 2;
        this.respawnHorizontalPointNumber = (int) ((base.Collider.Width - 2) / this.respawnHorizontalPointStep);
        this.respawnVerticalPointNumber = (int) ((base.Collider.Height - 2) / this.respawnVerticalPointStep);
        Vector2 centerPosition = base.Collider.Center;
        base.centerPosition = base.Position + centerPosition;
        base.waveMoveOffset = new Vector2(base.Collider.CenterX, base.Collider.CenterY);
        if (!base.single)
        {
            base.idle.Position = (base.flash.Position = centerPosition);
        }
        else
        {
            base.singleSprite.Position = centerPosition;
        }
        base.outline.Position = centerPosition;
        base.bloomPoint.Position = (base.vertexLight.Position = centerPosition);
        this.borderColor = data.GetChroniaColor("borderColor", !base.twoDashes ? Refill.OneDashesParticleShatterColor : Refill.TwoDashesParticleShatterColor);
        this.borderAlpha = data.Float("borderAlpha", 0.2F);
        this.innerColor = data.GetChroniaColor("innerColor", !base.twoDashes ? Refill.OneDashesParticleRegenAndGlowColor : Refill.TwoDashesParticleRegenAndGlowColor);
        this.innerAlpha = data.Float("innerAlpha", 0.1F);
        this.borderColor *= this.borderAlpha;
        this.innerColor *= this.innerAlpha;
    }

    public RefillWall(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    protected override void RenderAfter(float respawnTimer)
    {
        Color borderC = this.borderColor.Parsed(borderAlpha);
        Color innerC = this.innerColor.Parsed(innerAlpha);
        if (respawnTimer <= 0)
        {
            Draw.Line(base.TopLeft, base.TopRight, borderC);
            Draw.Line(base.TopLeft + Vector2.One, base.BottomLeft + Vector2.UnitX, borderC);
            Draw.Line(base.TopRight + Vector2.UnitY, base.BottomRight, borderC);
            Draw.Line(base.BottomLeft + new Vector2(1, -1), base.BottomRight - Vector2.One, borderC);
            Draw.Rect(base.TopLeft.X, base.TopLeft.Y, this.Collider.Width, this.Collider.Height, innerC);
        }
        else
        {
            Draw.Point(base.TopLeft, borderC);
            Draw.Point(base.TopRight - Vector2.UnitX, borderC);
            Draw.Point(base.BottomRight - Vector2.One, borderC);
            Draw.Point(base.BottomLeft - Vector2.UnitY, borderC);
            for (int i = 1; i <= this.respawnHorizontalPointNumber; i++)
            {
                Draw.Point(base.TopLeft + Vector2.UnitX * (i * this.respawnHorizontalPointStep), borderC);
                Draw.Point(base.BottomLeft + Vector2.UnitX * (i * this.respawnHorizontalPointStep) - Vector2.UnitY, borderC);
            }
            for (int i = 1; i <= this.respawnVerticalPointNumber; i++)
            {
                Draw.Point(base.TopLeft + Vector2.UnitY * (i * this.respawnVerticalPointStep), borderC);
                Draw.Point(base.TopRight + Vector2.UnitY * (i * this.respawnVerticalPointStep) - Vector2.UnitX, borderC);
            }
        }
    }

}

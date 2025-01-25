using Celeste.Mod.Entities;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/RefillWall")]
public class RefillWall : Refill
{

    private Color borderColor;

    private float borderAlpha;

    private Color innerColor;

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
        this.borderColor = !string.IsNullOrWhiteSpace(data.Attr("borderColor")) ? data.HexColor("borderColor") : (!base.twoDashes ? Refill.OneDashesParticleShatterColor : Refill.TwoDashesParticleShatterColor);
        this.borderAlpha = data.Float("borderAlpha", 0.2F);
        this.innerColor = !string.IsNullOrWhiteSpace(data.Attr("innerColor")) ? data.HexColor("innerColor") : (!base.twoDashes ? Refill.OneDashesParticleRegenAndGlowColor : Refill.TwoDashesParticleRegenAndGlowColor);
        this.innerAlpha = data.Float("innerAlpha", 0.1F);
        this.borderColor *= this.borderAlpha;
        this.innerColor *= this.innerAlpha;
    }

    public RefillWall(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    protected override void RenderAfter(float respawnTimer)
    {
        if (respawnTimer <= 0)
        {
            Draw.Line(base.TopLeft, base.TopRight, this.borderColor);
            Draw.Line(base.TopLeft + Vector2.One, base.BottomLeft + Vector2.UnitX, this.borderColor);
            Draw.Line(base.TopRight + Vector2.UnitY, base.BottomRight, this.borderColor);
            Draw.Line(base.BottomLeft + new Vector2(1, -1), base.BottomRight - Vector2.One, this.borderColor);
            Draw.Rect(base.TopLeft.X, base.TopLeft.Y, this.Collider.Width, this.Collider.Height, this.innerColor);
        }
        else
        {
            Draw.Point(base.TopLeft, this.borderColor);
            Draw.Point(base.TopRight - Vector2.UnitX, this.borderColor);
            Draw.Point(base.BottomRight - Vector2.One, this.borderColor);
            Draw.Point(base.BottomLeft - Vector2.UnitY, this.borderColor);
            for (int i = 1; i <= this.respawnHorizontalPointNumber; i++)
            {
                Draw.Point(base.TopLeft + Vector2.UnitX * (i * this.respawnHorizontalPointStep), this.borderColor);
                Draw.Point(base.BottomLeft + Vector2.UnitX * (i * this.respawnHorizontalPointStep) - Vector2.UnitY, this.borderColor);
            }
            for (int i = 1; i <= this.respawnVerticalPointNumber; i++)
            {
                Draw.Point(base.TopLeft + Vector2.UnitY * (i * this.respawnVerticalPointStep), this.borderColor);
                Draw.Point(base.TopRight + Vector2.UnitY * (i * this.respawnVerticalPointStep) - Vector2.UnitX, this.borderColor);
            }
        }
    }

}

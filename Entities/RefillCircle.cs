using Celeste.Mod.Entities;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/RefillCircle")]
public class RefillCircle : Refill
{

    private Color borderColor;

    private float borderAlpha;

    private Color innerColor;

    private float innerAlpha;

    private int pointStep;

    private int pointNumber;

    private Vc2[] nodes;
    private float radius;

    private const float pi = (float)Math.PI;

    public RefillCircle(Vector2 position, EntityData data) : base(position, data)
    {
        nodes = data.NodesWithPosition(position);
        base.Collider = new Circle(radius = (nodes[1] - nodes[0]).Length());
        this.pointStep = 2;
        this.pointNumber = (int)(2 * pi * (radius + 2f) / this.pointStep);
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

    public RefillCircle(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    protected override void RenderAfter(float respawnTimer)
    {
        if (respawnTimer <= 0)
        {
            Draw.Circle(base.Position, radius, this.innerColor, 4 * pointNumber);
            Draw.Circle(base.Position, radius + 2, this.borderColor, 4 * pointNumber);
        }
        else
        {
            for (int i = 1; i <= this.pointNumber; i++)
            {
                Vc2 pointPos = Position + new Vc2(radius, 0).Rotate(i * 2 * pi / pointNumber);
                Draw.Point(pointPos, this.borderColor);
            }
        }
    }

}

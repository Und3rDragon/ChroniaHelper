using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/OutlinerCircle")]
public class OutlinerCircle : BaseEntity
{
    public OutlinerCircle(EntityData data, Vc2 offset) : base(data, offset)
    {
        innerStyle = data.Int("innerStyle", 1);
        borderStyle = data.Int("borderStyle", 1);
        radius = (nodes[1] - nodes[0]).Length();
        this.pointStep = 2;
        this.pointNumber = (int)(2 * float.Pi * (radius + 2f) / this.pointStep);

        Collider = new Circle(radius);

        innerColor = data.GetChroniaColor("innerColor", Color.White);
        innerColor.alpha = data.Float("innerAlpha", 0.3f);
        borderColor = data.GetChroniaColor("borderColor", Color.White);
        borderColor.alpha = data.Float("borderAlpha", 1f);

        attached = data.Bool("attached", false);
        if (attached)
        {
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRidingSolid,
                OnMove = OnMove,
                OnEnable = Miscs.EmptyAction,
                OnDisable = Miscs.EmptyAction,
                Visible = true,
            });
        }
    }
    private bool attached;
    private int innerStyle, borderStyle;
    private float radius;
    private int pointStep;
    private int pointNumber;
    private CColor innerColor, borderColor;

    public override void Render()
    {
        base.Render();

        if (innerStyle == 1)
        {
            Draw.Circle(base.Position, radius, this.innerColor.Parsed(), 4 * pointNumber);
        }
        else
        {
            Draw.Circle(base.Position, radius / 2, this.innerColor.Parsed(), radius, 4 * pointNumber);
        }

        if(borderStyle == 1)
        {
            Draw.Circle(base.Position, radius + 2, this.borderColor.Parsed(), 4 * pointNumber);
        }
        else
        {
            float t = 1f;
            t = 1f + (float)Math.Sin((DateTime.Now - Md.Session.LevelStartTime).TotalSeconds);
            Draw.Circle(base.Position, radius + t, this.borderColor.Parsed(), 4 * pointNumber);
        }
    }

    public void OnShake(Vector2 offset)
    {
        Position += offset;
    }

    public void OnMove(Vector2 offset)
    {
        Position += offset;
    }

    public bool IsRidingSolid(Solid solid)
    {
        return CollideCheck(solid);
    }
}

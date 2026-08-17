using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Components.StateListeners;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System.Runtime.InteropServices;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/OutlinerCircle")]
[Credits("pbalint817 for performance improvement")]
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

        visibleFlag = new(data.Attr("visibleFlag"));
        Add(visibleFlag);
        colorFade = new("fadeAlpha", 1f);
        Add(colorFade);
        visibleFade = (EaseMode)data.Int("visibleFade", 1);
        displayFadeTime = data.Float("displayFadeTime", -1f);
        noFade = displayFadeTime <= 0f;
    }
    private bool attached;
    private int innerStyle, borderStyle;
    private float radius;
    private int pointStep;
    private int pointNumber;
    private CColor innerColor, borderColor;
    private FlagsListener visibleFlag;
    private DataPackPreset.Float colorFade;
    private EaseMode visibleFade;
    private float displayFadeTime;
    private bool noFade;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (!visibleFlag.InstantState)
        {
            colorFade.Value = 0f;
        }
    }

    public override void Update()
    {
        base.Update();

        visibleFlag.onEnable = () =>
        {
            float target = 1f;
            if (noFade)
            {
                colorFade.Value = target;
            }
            else
            {
                colorFade.FadeTo(target, displayFadeTime, visibleFade);
            }
        };

        visibleFlag.onDisable = () =>
        {
            float target = 0f;
            if (noFade)
            {
                colorFade.Value = target;
            }
            else
            {
                colorFade.FadeTo(target, displayFadeTime, visibleFade);
            }
        };
    }

    public override void Render()
    {
        base.Render();
        
        var innerTex = innerStyle == 1
            ? CircleTextureCache.GetOutline(radius, 4 * pointNumber, out float innerScale)
            : CircleTextureCache.GetFilled(radius, 4 * pointNumber, out innerScale);
        CircleTextureCache.DrawAt(innerTex, Position, innerColor.Parsed(colorFade.Value), innerScale);

        if (borderStyle == 1)
        {
            var borderTex = CircleTextureCache.GetOutline(radius + 2, 4 * pointNumber, out float borderScale);
            CircleTextureCache.DrawAt(borderTex, Position, borderColor.Parsed(colorFade.Value), borderScale);
        }
        else
        {
            const int BreathKeyframes = 6;

            float t = 1f + (float)Math.Sin((DateTime.Now - Md.Session.LevelStartTime).TotalSeconds);

            float step = 2f / BreathKeyframes;
            float keyframeRadius = radius + 2 + (MathF.Round(t / step) * step - 2f);

            var borderTex = CircleTextureCache.GetOutline(keyframeRadius, 4 * pointNumber, out float drawScale);
            CircleTextureCache.DrawAt(borderTex, Position, borderColor.Parsed(colorFade.Value), drawScale);
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

using Celeste.Mod.Entities;
using ChroniaHelper.Components.Graphical;
using ChroniaHelper.Utils;
using YoctoHelper.Cores;

namespace ChroniaHelper.Triggers.Graphical;

[CustomEntity("ChroniaHelper/EntityTextAdder")]
public class EntityTextAdder : BaseTrigger
{
    public EntityTextAdder(EntityData data, Vc2 offset) : base(data, offset)
    {
        targetText = data.Attr("targetTextID", "dialogID");
        relativePosition = data.Vector2("relativePositionX", "relativePositionY", Vc2.Zero);
        alignment = data.Vector2("alignX", "alignY", Vc2.One * 0.5f);
        scale =  data.Vector2("scaleX", "scaleY", Vc2.One);
        textColor = data.GetChroniaColor("textColor", Color.White);
        stroke = data.Float("stroke", 0f);
        strokeColor = data.GetChroniaColor("strokeColor", Color.White);
        edgeDepth = data.Float("edgeDepth", 0f);
        edgeColor = data.GetChroniaColor("edgeColor", Color.White);
        outlined = data.Bool("outlined", false);
    }
    public string targetText;
    public Vc2 relativePosition;
    public Vc2 alignment;
    public Vc2 scale;
    public CColor textColor;
    public float stroke;
    public CColor strokeColor;
    public float edgeDepth;
    public CColor edgeColor;
    public bool outlined;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        foreach (var entity in MaP.level.Entities)
        {
            if (CollideCheck(entity))
            {
                ActiveFontComponent comp = new(Dialog.Clean(targetText))
                {
                    Entity = entity,
                    RelativePosition = relativePosition,
                    Alignment = alignment,
                    Scale = scale,
                    Color = textColor,
                    Stroke = stroke,
                    StrokeColor = strokeColor,
                    EdgeDepth = edgeDepth,
                    EdgeColor = edgeColor,
                    Outlined = outlined
                };
                entity.Add(comp);
            }
        }
    }
}
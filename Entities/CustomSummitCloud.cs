using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/CustomSummitCloud")]

public class CustomSummitCloud : Entity
{
    public Image image;

    public float diff;

    public Vector2 RenderPosition()
    {
        Vector2 camCenter = (base.Scene as Level).Camera.Position + new Vector2(screenX, screenY);
        
        return camCenter + (Position - camCenter) * diff;// parallax

        // Known paramaters : Position and camCenter
        
    }

    private enum Aligns
    {
        PositionTopLeft, PositionTopCenter, PositionTopRight,
        PositionCenterLeft, PositionCenter, PositionCenterRight,
        PositionBottomLeft, PositionBottomCenter, PositionBottomRight,
        AlignTopLeft, AlignTopCenter, AlignTopRight,
        AlignCenterLeft, AlignCenter, AlignCenterRight,
        AlignBottomLeft, AlignBottomCenter, AlignBottomRight
    }

    private Aligns alignment;

    private float floatiness, parallax, screenX, screenY;
    private string path;
    private bool randomizeParallax;

    public CustomSummitCloud(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        base.Tag = Tags.Persistent;
        base.Depth = data.Int("depth", -10550);
        randomizeParallax = data.Bool("randomizeParallax", true);
        parallax = data.Float("parallax");
        screenX = data.Float("screenPosX", 160f);
        screenY = data.Float("screenPosY", 90f);

        diff = randomizeParallax? Calc.Random.Range(parallax, parallax + 0.1f) : parallax;
        floatiness = data.Float("floatiness", 1f);
        alignment = data.Enum<Aligns>("alignment", Aligns.PositionTopLeft);

        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(path = data.Attr("path", "ChroniaHelper/CustomSummitClouds/cloud"));
        image = new Image(Calc.Random.Choose(atlasSubtextures));
        // Alignments
        float tileL = Width, tileH = Height;
        switch (alignment)
        {
            case Aligns.PositionTopCenter:
                image.CenterOrigin();
                Position.X += tileL / 2;
                break;
            case Aligns.PositionTopRight:
                image.CenterOrigin();
                Position.X += tileL;
                break;
            case Aligns.PositionCenterLeft:
                image.CenterOrigin();
                Position.Y += tileH / 2;
                break;
            case Aligns.PositionCenter:
                image.CenterOrigin();
                Position.Y += tileH / 2; Position.X += tileL / 2;
                break;
            case Aligns.PositionCenterRight:
                image.CenterOrigin();
                Position.X += tileL; Position.Y += tileH / 2;
                break;
            case Aligns.PositionBottomLeft:
                image.CenterOrigin();
                Position.Y += tileH;
                break;
            case Aligns.PositionBottomCenter:
                image.CenterOrigin();
                Position.Y += tileH; Position.X += tileL / 2;
                break;
            case Aligns.PositionBottomRight:
                image.CenterOrigin();
                Position.X += tileL; Position.Y += tileH;
                break;
            case Aligns.AlignTopLeft:
                image.JustifyOrigin(0, 0);
                break;
            case Aligns.AlignTopCenter:
                image.JustifyOrigin(0.5f, 0);
                break;
            case Aligns.AlignTopRight:
                image.JustifyOrigin(1f, 0);
                break;
            case Aligns.AlignCenterLeft:
                image.JustifyOrigin(0, 0.5f);
                break;
            case Aligns.AlignCenter:
                image.JustifyOrigin(0.5f, 0.5f);
                break;
            case Aligns.AlignCenterRight:
                image.JustifyOrigin(1f, 0.5f);
                break;
            case Aligns.AlignBottomLeft:
                image.JustifyOrigin(0, 1f);
                break;
            case Aligns.AlignBottomCenter:
                image.JustifyOrigin(0.5f, 1f);
                break;
            case Aligns.AlignBottomRight:
                image.JustifyOrigin(1f, 1f);
                break;
            default:
                image.CenterOrigin();
                break;
        }

        // Scale Flip
        if(data.Bool("randomFlipX", true))
        {
            image.Scale.X = Calc.Random.Choose(-1, 1);
        }
        if(data.Bool("randomFlipY", false))
        {
            image.Scale.Y = Calc.Random.Choose(-1, 1);
        }

        image.Color = Calc.HexToColor(data.Attr("color", "ffffff"));

        Add(image);
        SineWave sineWave = new SineWave(Calc.Random.Range(0.05f, 0.15f) * floatiness, 0f);
        sineWave.Randomize();
        sineWave.OnUpdate = delegate (float f)
        {
            image.Y = f * 8f;
        };
        Add(sineWave);

        // new params
        base.Collider = new Hitbox(8f, 8f);
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position = RenderPosition();
        base.Render();
        Position = position;
    }


}

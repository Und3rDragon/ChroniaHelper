using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Content;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomSummitCloud2")]

public class CustomSummitCloud2 : Entity
{
    public Image image;

    public float camParallax;

    public Vector2 RenderPosition()
    {
        Vector2 camCenter = (base.Scene as Level).Camera.Position + new Vector2(screenX, screenY);
        
        return camCenter + (Position - camCenter) * camParallax;// parallax

        // Known paramaters : Position and camCenter
    }
    
    private float freq, r_freq, parallax, screenX, screenY, amp;
    private string path;
    private float randomParallax;

    public CustomSummitCloud2(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        base.Tag = Tags.Global;
        base.Depth = data.Int("depth", -10550);
        randomParallax = data.Float("randomParallax", 0.1f).GetAbs();
        parallax = data.Float("parallax", 1f).GetAbs();
        screenX = data.Float("screenPosX", 160f).GetAbs();
        screenY = data.Float("screenPosY", 90f).GetAbs();
        amp = data.Float("floatyAmplitude", 8f).GetAbs();

        camParallax = Calc.Random.Range(parallax - randomParallax, parallax + randomParallax);

        freq = data.Float("floatingFreq", 0.1f).GetAbs();
        r_freq = data.Float("randomFloatingFreq", 0.05f).GetAbs();

        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(path = data.Attr("path", "ChroniaHelper/CustomSummitCloud2s/cloud"));
        image = new Image(Calc.Random.Choose(atlasSubtextures));
        image.CenterOrigin();

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

        bool fPos = freq - r_freq >= 0;
        SineWave sineWave = new SineWave(Calc.Random.Range(fPos? freq - r_freq : 0f, freq + r_freq), 0f);
        sineWave.Randomize();
        sineWave.OnUpdate = delegate (float f)
        {
            image.Y = f * amp;
        };
        Add(sineWave);
        // new params
        base.Collider = new Hitbox(8f, 8f);

        id = data.ID;
    }
    private int id;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        int count = 0;
        foreach (var item in MapProcessor.entities[typeof(CustomSummitCloud2)])
        {
            CustomSummitCloud2 cloud = item as CustomSummitCloud2;
            if (cloud.id == id)
            {
                count++;
            }
        }
        if(count > 1) { RemoveSelf(); }
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position = RenderPosition();
        base.Render();
        Position = position;
    }


}

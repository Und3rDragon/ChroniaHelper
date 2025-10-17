using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.DotNet.Serialized;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Utils.StopwatchSystem;

[Tracked(true)]
[CustomEntity("ChroniaHelper/StopclockRenderer")]
public class StopclockRenderer : Entity
{
    public StopclockRenderer(EntityData d, Vc2 o) : base(d.Position + o)
    {
        Position = d.Position;
        
        source = d.Attr("sourcePath", "ChroniaHelper/StopclockFonts/font");
        textures = GFX.Game.GetAtlasSubtextures(source);

        renderMode = d.Int("renderMode", 0);
        baseAlign = d.Int("positionAlign", 5);
        segmentAlign = d.Int("segmentAlign", 5);
        segmentDistance = d.Float("segmentDistance", 8f);
        clockTag = d.Attr("stopclockTag", "stopclock");
        parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));

        baseLining = AlignUtils.AlignToJustify[(AlignUtils.Aligns)baseAlign];
        segmentLining = AlignUtils.AlignToJustify[(AlignUtils.Aligns)segmentAlign];
        
    }
    private string source;
    private List<MTexture> textures;
    private enum RenderMode { Compact, EqualDistance}
    private int renderMode = 0;
    private int baseAlign = 5, segmentAlign = 5;
    private Vc2 baseLining, segmentLining;
    private float segmentDistance;
    private string clockTag;
    private Vc2 parallax, basePosition, renderPosition;

    public override void Render()
    {
        base.Render();

        if (!clockTag.GetStopclock(out Stopclock clock)) { return; }

        clock.GetTrimmedTimeString(out string renderTarget);

        // Calculate Sizing
        float cal = 0;
        List<float> segmentX = new();
        Vc2 topleft = Vc2.Zero, downright = Vc2.Zero;
        renderTarget.EachDoWithIndexAndLength((c, n, L) =>
        {
            if (renderMode == (int)RenderMode.EqualDistance)
            {
                if (n == 0)
                {
                    topleft = Vc2.Zero - new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * segmentLining;
                    downright = Vc2.Zero + new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * (Vc2.One - segmentLining);
                }
                Vc2 basic = new Vc2(segmentDistance * n, 0);
                Vc2 newtopleft = basic - new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * segmentLining,
                newdownright = basic + new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * (Vc2.One - segmentLining);

                topleft.Y = newtopleft.Y < topleft.Y ? newtopleft.Y : topleft.Y;
                downright.X = newdownright.X > downright.X ? newdownright.X : downright.X;
                downright.Y = newdownright.Y > downright.Y ? newdownright.Y : downright.Y;

                segmentX.Add(segmentDistance * n);
            }
            else
            {
                if (n == 0)
                {
                    topleft = Vc2.Zero - new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * segmentLining;
                    downright = Vc2.Zero + new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * (Vc2.One - segmentLining);

                    segmentX.Add(cal);
                    return;
                }

                cal = cal + textures[$"{renderTarget[n - 1]}".ParseInt(c == ':' ? 10 : 0)].Width * (1f - segmentLining.X) + segmentDistance + textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width * segmentLining.X;

                Vc2 basic = new Vc2(cal, 0);
                Vc2 newtopleft = basic - new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * segmentLining,
                newdownright = basic + new Vc2(textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Width, textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Height) * (Vc2.One - segmentLining);

                topleft.Y = newtopleft.Y < topleft.Y ? newtopleft.Y : topleft.Y;
                downright.X = newdownright.X > downright.X ? newdownright.X : downright.X;
                downright.Y = newdownright.Y > downright.Y ? newdownright.Y : downright.Y;

                segmentX.Add(cal);
            }
        });

        Vc2 size = downright - topleft;

        renderPosition = -size * baseLining;

        // Logging result: Position correct
        Log.Each(segmentX);
        Log.Warn($"size: {size}, topleft: {topleft}, downright: {downright}");
        Log.Warn($"renderPosition: {renderPosition}");
        Log.Warn($"entityPosition: {Position}");
        Log.Warn($"basePosition: {basePosition}");
        Log.Error("_______________________________");
        
        renderTarget.EachDoWithIndexAndLength((c, n, L) =>
        {
            textures[$"{c}".ParseInt(c == ':' ? 10 : 0)].Draw(basePosition + renderPosition + new Vc2(segmentX[n], 0), 
                segmentLining, Color.White, 1f, 0f);
        });
    }

    public override void Update()
    {
        base.Update();

        Vc2 camCenter = MaP.level.Camera.Position + new Vc2(160f, 90f);
        basePosition = camCenter + (Position - camCenter) * parallax;
    }
}

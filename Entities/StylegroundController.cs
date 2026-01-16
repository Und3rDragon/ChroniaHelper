using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
[CustomEntity("ChroniaHelper/StylegroundController")]
public class StylegroundController : BaseEntity
{
    public StylegroundController(EntityData d, Vc2 o) : base(d, o)
    {
        bgTags = d.StringArray("tag");
        bgSpeedSliderX = d.Attr("bgSpeedSliderX");
        bgSpeedSliderY = d.Attr("bgSpeedSliderY");
        bgPosSliderX = d.Attr("bgPosSliderX");
        bgPosSliderY = d.Attr("bgPosSliderY");
        bgAlphaSlider = d.Attr("bgAlphaSlider");
        bgScrollSliderX = d.Attr("bgScrollSliderX");
        bgScrollSliderY = d.Attr("bgScrollSliderY");
    }
    private string[] bgTags;
    private string bgSpeedSliderX, bgSpeedSliderY;
    private string bgPosSliderX, bgPosSliderY;
    private string bgAlphaSlider;
    private string bgScrollSliderX, bgScrollSliderY;

    public struct StylegroundData
    {
        public Vc2 Speed;
        public Vc2 Position;
        public CColor Color;
        public Vc2 Scroll;
    }

    private Dictionary<Backdrop, StylegroundData> stylegroundData = new();

    protected override void AddedExecute(Scene scene)
    {
        foreach(var b in MaP.level.Background.Backdrops)
        {
            if(bgTags.Intersect(b.Tags).Count() > 0){
                StylegroundData d = new()
                {
                    Speed = b.Speed,
                    Position = b.Position,
                    Color = new CColor(b.Color),
                    Scroll = b.Scroll
                };
                stylegroundData.Enter(b, d);
            }
        }

        foreach (var b in MaP.level.Foreground.Backdrops)
        {
            if (bgTags.Intersect(b.Tags).Count() > 0)
            {
                StylegroundData d = new()
                {
                    Speed = b.Speed,
                    Position = b.Position,
                    Color = new CColor(b.Color),
                    Scroll = b.Scroll
                };
                stylegroundData.Enter(b, d);
            }
        }
    }

    public override void Update()
    {
        base.Update();

        foreach (var b in MaP.level.Background.Backdrops)
        {
            if (bgTags.Intersect(b.Tags).Count() > 0)
            {
                if (!bgPosSliderX.IsNullOrEmpty())
                {
                    b.Position.X = stylegroundData[b].Position.X + bgPosSliderX.GetSlider();
                }

                if (!bgPosSliderY.IsNullOrEmpty())
                {
                    b.Position.Y = stylegroundData[b].Position.Y + bgPosSliderY.GetSlider();
                }

                if (!bgAlphaSlider.IsNullOrEmpty())
                {
                    b.Color = stylegroundData[b].Color.Parsed((1f - bgAlphaSlider.GetSlider()).Clamp(0f, 1f));
                }

                if (!bgSpeedSliderX.IsNullOrEmpty())
                {
                    b.Speed.X = stylegroundData[b].Speed.X + bgSpeedSliderX.GetSlider();
                }

                if (!bgSpeedSliderY.IsNullOrEmpty())
                {
                    b.Speed.Y = stylegroundData[b].Speed.Y + bgSpeedSliderY.GetSlider();
                }

                if (!bgScrollSliderX.IsNullOrEmpty())
                {
                    b.Scroll.X = stylegroundData[b].Scroll.X + bgScrollSliderX.GetSlider();
                }

                if (!bgScrollSliderY.IsNullOrEmpty())
                {
                    b.Scroll.Y = stylegroundData[b].Scroll.Y + bgScrollSliderY.GetSlider();
                }
            }
        }

        foreach (var b in MaP.level.Foreground.Backdrops)
        {
            if (bgTags.Intersect(b.Tags).Count() > 0)
            {
                if (!bgPosSliderX.IsNullOrEmpty())
                {
                    b.Position.X = stylegroundData[b].Position.X + bgPosSliderX.GetSlider();
                }

                if (!bgPosSliderY.IsNullOrEmpty())
                {
                    b.Position.Y = stylegroundData[b].Position.Y + bgPosSliderY.GetSlider();
                }

                if (!bgAlphaSlider.IsNullOrEmpty())
                {
                    b.Color = stylegroundData[b].Color.Parsed((1f - bgAlphaSlider.GetSlider()).Clamp(0f, 1f));
                }

                if (!bgSpeedSliderX.IsNullOrEmpty())
                {
                    b.Speed.X = stylegroundData[b].Speed.X + bgSpeedSliderX.GetSlider();
                }

                if (!bgSpeedSliderY.IsNullOrEmpty())
                {
                    b.Speed.Y = stylegroundData[b].Speed.Y + bgSpeedSliderY.GetSlider();
                }

                if (!bgScrollSliderX.IsNullOrEmpty())
                {
                    b.Scroll.X = stylegroundData[b].Scroll.X + bgScrollSliderX.GetSlider();
                }

                if (!bgScrollSliderY.IsNullOrEmpty())
                {
                    b.Scroll.Y = stylegroundData[b].Scroll.Y + bgScrollSliderY.GetSlider();
                }
            }
        }
    }
}

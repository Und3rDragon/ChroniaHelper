using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using IL.MonoMod;
using Microsoft.Build.Framework;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/MessageDisplayer")]
public class MessageDisplayer : Entity
{
    public MessageDisplayer(EntityData d, Vc2 o) : base(d.Position + o)
    {
        base.Depth = d.Int("depth", -100000);

        SerialImage template = new SerialImage(GFX.Game.GetAtlasSubtextures("ChroniaHelper/DisplayFonts/font"));

        template.renderMode = d.Int("renderMode", 0);
        template.origin = new Vc2(d.Float("lineOriginX", 0.5f), d.Float("lineOriginY", 0.5f));
        template.segmentOrigin = Vc2.Zero;
        template.distance = d.Float("letterDistance", 1f);
        template.color = d.GetChroniaColor("fontColor", Color.White);

        renderer = new SerialImageGroup(template, "ChroniaHelper/DisplayFonts/font");
        renderer.groupOrigin = new Vc2(d.Float("originX", 0.5f), d.Float("originY", 0.5f));
        renderer.memberDistance = d.Float("lineDistance", 2f);

        content = d.Attr("message");

        parallax = new Vc2(d.Float("parallaxX", 1f), d.Float("parallaxY", 1f));
        staticScreen = new Vc2(d.Float("screenX", 160f), d.Float("screenY", 90f));
    }
    public SerialImageGroup renderer;
    public string content;
    public Vc2 parallax = Vc2.Zero;
    public Vc2 staticScreen = Vc2.Zero;

    public string reference = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =";

    public List<string> ParseRenderTarget()
    {
        string text = Dialog.Clean(content);
        
        var lines = text.Split(new char[] { '\n', '\r'}, StringSplitOptions.TrimEntries);
        var result = new List<string>();
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                result.Add(trimmed);
            }
        }
        
        return result;
    }

    public int Reflection(char c)
    {
        return reference.Contains(c) ? reference.IndexOf(c) : reference.IndexOf(" ");
    }

    public override void Render()
    {
        base.Render();

        renderer.Render(ParseRenderTarget(),
            (c) => Reflection(c),
            Position.InParallax(parallax, staticScreen));
    }

}

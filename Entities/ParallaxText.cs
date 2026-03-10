using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/ParallaxText")]
public class ParallaxText : BaseEntity {
    private static readonly Vector2 camCenterOffset = new(160f, 90f);
    private static readonly float camScalar = 6f;

    private readonly Vector2 centerPos;
    private readonly string[] text;
    private readonly Vector2 textCenterPos;
    private readonly bool outline;
    private readonly bool noFlag;
    private readonly string flag;
    private readonly Vector2 halfSize;
    private readonly Vector2 zeroAlphaDistance;
    private readonly Vector2 parallaxScalar;
    private readonly Vector2 textScalar;
    private readonly Vector2 zeroParallaxPosition;
    private readonly ChroniaColor textColor;
    private readonly ChroniaColor borderColor;

    private float alpha;

    public ParallaxText(EntityData data, Vector2 offset) 
        : base(data, offset) {
        Tag = Tags.HUD;
        
        Vector2 textSize = Vector2.Zero;
        textScalar = new Vector2(data.Float("textScalarX", 1.25f), data.Float("textScalarY", 1.25f));
        textColor = data.GetChroniaColor("color", Color.White);
        borderColor = data.GetChroniaColor("borderColor", Color.Black);

        //The 'error text' is part of the SC2020 hunt. If you get it unexpectedly, it means
        //		that the text you placed has no 'dialog' attribute, likely an ahorn/lonn integration error
        text = data.Has("dialog") ? 
            Dialog.Clean(data.Attr("dialog", "app_ending")).Split(new char[] { '\n', '\r' }, StringSplitOptions.None) : 
            new string[] { "Extract the zip", "Find 'gymBG'", "Then open it as", "a .txt" };
        foreach (string line in text) {
            Vector2 lineSize = ActiveFont.Measure(line);
            textSize.X += lineSize.X * textScalar.X;
            textSize.Y += lineSize.Y * textScalar.Y;
        }

        textSize.X = 0;

        outline = data.Bool("border");

        halfSize = new Vector2(data.Width / 2, data.Height / 2);

        centerPos = halfSize + Position;
        textCenterPos = nodes[1];
        parallaxScalar = new Vector2(data.Float("parallaxX", 1.75f), data.Float("parallaxY", 1.75f));
        zeroParallaxPosition = new Vc2(data.Float("zeroParallaxPositionX", 960f), data.Float("zeroParallaxPositionY", 540f));
        zeroAlphaDistance = halfSize + new Vc2(data.Float("zeroFadeX", 16f), data.Float("zeroFadeY", 16f));

        flag = data.Attr("flag");
        noFlag = string.IsNullOrEmpty(flag) || string.IsNullOrWhiteSpace(flag);
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        Update();
    }

    public override void Update() {
        Player entity = Scene.Tracker.GetEntity<Player>();
        if (entity != null) {
            alpha = Ease.CubeInOut(
                Calc.ClampedMap(Math.Abs(entity.Center.X - centerPos.X), 
                halfSize.X, zeroAlphaDistance.X, 1f, 0f) 
                * Calc.ClampedMap(Math.Abs(entity.Center.Y - centerPos.Y), 
                halfSize.Y, zeroAlphaDistance.Y, 1f, 0f)
                );
        }

        base.Update();
    }

    public override void Render() {
        if (noFlag || SceneAs<Level>().Session.GetFlag(flag)) {
            //parallax equation
            Vector2 pos = nodes[1].InParallax(parallaxScalar, zeroParallaxPosition) * Cons.HDScale;

            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode) {
                pos.X = 1920f - pos.X;
            }

            foreach (string line in text) {
                if (outline) {
                    ActiveFont.DrawOutline(line, pos, new Vector2(0.5f, 0f), textScalar, textColor.Parsed(alpha), 2f, borderColor.Parsed(alpha * alpha));
                } else {
                    ActiveFont.Draw(line, pos, new Vector2(0.5f, 0f), textScalar, textColor.Parsed(alpha));
                }

                pos += ActiveFont.Measure(line).Y * textScalar.YComp();
            }
        }
    }
}
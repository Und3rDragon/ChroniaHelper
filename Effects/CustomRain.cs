using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.ClutterBlock;
using ChroniaHelper.Utils;
using Celeste.Mod.Backdrops;

/*
    Migrated from VivHelper Repository
    Localised the parameters using ChroniaHelper Utils setups
    Huge thanks to Viv for the open-source code
 */

namespace ChroniaHelper.Effects {
    [CustomBackdrop("ChroniaHelper/CustomRain")]
    public class CustomRain : Backdrop {
        
        public struct Particle {
            public Vector2 Position;

            public Vector2 Speed;

            public float Rotation;

            public Vector2 Scale;
            public Color color;

            public void Init(float rot, float speedMult, Color color, float extX, float extY) {
                Position = new Vector2(-32f + Calc.Random.NextFloat(384f + extX), -32f + Calc.Random.NextFloat(244f + extY));
                Rotation = rot;
                Speed = Calc.AngleToVector(Rotation, Calc.Random.Range(200f * speedMult, 600f * speedMult));
                Scale = new Vector2(4f + (Speed.Length() - 200f) * speedMult / 33.33333f, 1f);
                this.color = color;
            }
        }

        public float speedMult, alpha;
        public Particle[] particles;
        public int count;

        private float visibleFade = 1f;

        private float linearFade = 1f;

        public float extX, extY;
#pragma warning disable CS0612

        public CustomRain(BinaryPacker.Element child)
            :this(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), 
                 child.AttrFloat("angle", 270f), child.AttrFloat("angleDiff", 3f), child.AttrFloat("speedMult", 1f), 
                 child.AttrInt("Amount", 240), child.Attr("Colors", "161933"), child.AttrFloat("alpha"),
                 child.AttrFloat("extendedBorderX", 0f), child.AttrFloat("extendedBorderY", 0f)
                 )
        { }
        public CustomRain(Vector2 scroll, float angle, float angleDiff, float speedMult, int count, string colors, float alpha,
            float extX, float extY
            ) 
        {
            this.Scroll = scroll;
            this.count = count;
            particles = new Particle[count];
            List<Color> _colors = new List<Color>();
            if (string.IsNullOrEmpty(colors))
            {
                _colors = new List<Color>() { Color.White };
            }
            else if (colors.StartsWith("§"))
            {
                string[] colorParams = colors.Substring(1).Split(',', StringSplitOptions.TrimEntries);
                foreach (string p in colorParams) {
                    _colors.Add(Calc.HexToColor(p.Trim().TrimStart('#')));
                }
            }
            else
            {
                string[] colorParams = colors.Split(',', StringSplitOptions.TrimEntries);
                foreach (string p in colorParams)
                {
                    _colors.Add(Calc.HexToColor(p.Trim().TrimStart('#')));
                }
            }
            var _angle = angle * -Calc.DegToRad;
            var _angleDiff = Math.Abs(angleDiff * Calc.DegToRad);
            for (int i = 0; i < count; i++) {
                particles[i].Init(_angle + Calc.Random.Range(-_angleDiff, _angleDiff), speedMult, Calc.Random.Choose<Color>(_colors), extX, extY);
            }
            this.alpha = alpha;

            this.extX = extX;
            this.extY = extY;
        }

#pragma warning restore CS0612
        public override void Update(Scene scene) {
            base.Update(scene);
            bool flag = ((scene as Level).Raining = IsVisible(scene as Level));
            visibleFade = Calc.Approach(visibleFade, flag ? 1 : 0, Engine.DeltaTime * (flag ? 10f : 0.25f));
            if (FadeX != null) {
                linearFade = FadeX.Value((scene as Level).Camera.X + 160f + extX / 2f);
            }
            for (int i = 0; i < count; i++) {
                particles[i].Position += particles[i].Speed * Engine.DeltaTime;
            }
        }

        public override void Render(Scene scene) {
            if (alpha > 0f && visibleFade > 0f && linearFade > 0f) {
                Camera camera = (scene as Level).Camera;
                for (int i = 0; i < particles.Length; i++) {
                    Vector2 position = new Vector2(NumberUtils.Mod(particles[i].Position.X - camera.X, 320f + extX), NumberUtils.Mod(particles[i].Position.Y - camera.Y, 180f + extY));
                    Draw.Pixel.DrawCentered(position, particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);

                    // Useless Codes?

                    //var v = particles[i].Scale.Rotate(particles[i].Rotation);
                    //if (position.Y + v.Y > 180 ) {
                    //    if (position.X + v.X > 320) {
                    //        Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y - 180 ), particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);
                    //    } else {
                    //        Draw.Pixel.DrawCentered(new Vector2(position.X, position.Y - 180 ), particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);
                    //    }
                    //} else if (position.X + v.X > 320) {
                    //    Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y), particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);
                    //}

                }
            }
        }

    }
}

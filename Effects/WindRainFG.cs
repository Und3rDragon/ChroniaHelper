using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

/*
    Migrated from VivHelper Repository
    Localised the parameters using ChroniaHelper Utils setups
    Huge thanks to Viv for the open-source code
 */

namespace ChroniaHelper.Effects {
    public class WindRainFG : Backdrop {
        private struct Particle {
            public Vector2 Position;

            public float Speed;

            public float Rotation;

            public Vector2 Scale;

            public Color Color;

            public void Init() {
                Position = new Vector2(-32f + Calc.Random.NextFloat(384f), -32f + Calc.Random.NextFloat(244f));
                Rotation = MathF.PI / 2f + Calc.Random.Range(-0.05f, 0.05f);
                Speed = Calc.Random.Range(200f, 600f);
                Scale = new Vector2(4f + (Speed - 200f) / 400f * 12f, 1f);
            }
        }

        public float Alpha = 1f;

        private float visibleFade = 1f;

        private float linearFade = 1f;

        private Particle[] particles;

        public Color[] Colors;

        private Level level;

        public float windStrength;

        public Vector2 scroll;
#pragma warning disable CS0612
        public WindRainFG(Vector2 scroll, string colors, float windStrength, int amount, float alpha) {
            this.Scroll = scroll;
            this.windStrength = windStrength;
            particles = new Particle[amount];
            if (string.IsNullOrEmpty(colors)) { this.Colors = new Color[] { Calc.HexToColor("161933") }; }
            else if (colors[0] == '§')
            {
                string[] c = colors.Substring(1).Split(',', StringSplitOptions.TrimEntries);
                this.Colors = new Color[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    c[i].Trim().TrimStart('#');
                    Colors[i] = Calc.HexToColor(c[i]);
                }
            }
            else
            {
                string[] c = colors.Split(',', StringSplitOptions.TrimEntries);
                this.Colors = new Color[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    c[i].Trim().TrimStart('#');
                    Colors[i] = Calc.HexToColor(c[i]);
                }
            }

            for (int i = 0; i < particles.Length; i++) {
                particles[i].Init();
            }
            level = null;

            this.Alpha = alpha;
        }

#pragma warning restore CS0612
        public override void Update(Scene scene) {
            base.Update(scene);
            bool flag = (scene as Level).Raining = IsVisible(scene as Level);
            visibleFade = Calc.Approach(visibleFade, flag ? 1 : 0, Engine.DeltaTime * (flag ? 10f : 0.25f));
            if (FadeX != null) {
                linearFade = FadeX.Value((scene as Level).Camera.X + 160f);
            }
            for (int i = 0; i < particles.Length; i++) {

                particles[i].Position += (Calc.AngleToVector(particles[i].Rotation, particles[i].Speed) + (scene as Level).Wind * windStrength) * Engine.DeltaTime;

            }
        }

        public override void Render(Scene scene) {
            if (!(Alpha <= 0f) && !(visibleFade <= 0f) && !(linearFade <= 0f)) {
                float colFade = Alpha * linearFade * visibleFade;
                Camera camera = (scene as Level).Camera;
                for (int i = 0; i < particles.Length; i++) {
                    float t = (float) Math.Pow((particles[i].Speed - 400) / 400, 1.1);
                    var u = Calc.Angle(Calc.AngleToVector(particles[i].Rotation, (t + 1) * 400) + (scene as Level).Wind * windStrength);
                    //Color color = Colors[(int)(i * NumberUtils.Mod(i * (i + 2) * Math.Abs(i - 7.5f), Math.Abs((2 * i - 1) * i - 3)) * (i + 4.5f)) % Colors.Length];
                    Color color = Colors[i % Colors.Length];
                    Vector2 position = new Vector2(NumberUtils.Mod(particles[i].Position.X - camera.X * Scroll.X, 320f), NumberUtils.Mod(particles[i].Position.Y - camera.Y * Scroll.Y, 180f));
                    Draw.Pixel.DrawCentered(position,
                                            color * colFade,
                                            particles[i].Scale,
                                            u);
                    var v = particles[i].Scale.Rotate(u);
                    if (position.Y + v.Y > 180) {
                        if (position.X + v.X > 320) {
                            Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y - 180),
                                                    color * colFade,
                                                    particles[i].Scale,
                                                    u);
                        } else {
                            Draw.Pixel.DrawCentered(new Vector2(position.X, position.Y - 180),
                                                    color * colFade,
                                                    particles[i].Scale,
                                                    u);
                        }
                    }
                    else if(position.X + v.X > 320) {
                        Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y),
                                                color * colFade,
                                                particles[i].Scale,
                                                u);
                    }

                }
            }
        }
    }
}

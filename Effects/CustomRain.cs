﻿using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.ClutterBlock;
using ChroniaHelper.Utils;

/*
    Migrated from VivHelper Repository
    Localised the parameters using ChroniaHelper Utils setups
    Huge thanks to Viv for the open-source code
 */

namespace ChroniaHelper.Effects {

    public class CustomRain : Backdrop {

        public struct Particle {
            public Vector2 Position;

            public Vector2 Speed;

            public float Rotation;

            public Vector2 Scale;
            public Color color;

            public void Init(float rot, float speedMult, Color color) {
                Position = new Vector2(-32f + Calc.Random.NextFloat(384f), -32f + Calc.Random.NextFloat(244f));
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
#pragma warning disable CS0612
        public CustomRain(Vector2 scroll, float angle, float angleDiff, float speedMult, int count, string colors, float alpha) {
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
                particles[i].Init(_angle + Calc.Random.Range(-_angleDiff, _angleDiff), speedMult, Calc.Random.Choose<Color>(_colors));
            }
            this.alpha = alpha;
        }

#pragma warning restore CS0612
        public override void Update(Scene scene) {
            base.Update(scene);
            bool flag = ((scene as Level).Raining = IsVisible(scene as Level));
            visibleFade = Calc.Approach(visibleFade, flag ? 1 : 0, Engine.DeltaTime * (flag ? 10f : 0.25f));
            if (FadeX != null) {
                linearFade = FadeX.Value((scene as Level).Camera.X + 160f);
            }
            for (int i = 0; i < count; i++) {
                particles[i].Position += particles[i].Speed * Engine.DeltaTime;
            }
        }

        public override void Render(Scene scene) {
            if (alpha > 0f && visibleFade > 0f && linearFade > 0f) {
                Camera camera = (scene as Level).Camera;
                for (int i = 0; i < particles.Length; i++) {
                    Vector2 position = new Vector2(NumberUtils.Mod(particles[i].Position.X - camera.X, 320f), NumberUtils.Mod(particles[i].Position.Y - camera.Y, 180f));
                    Draw.Pixel.DrawCentered(position, particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);
                    var v = particles[i].Scale.Rotate(particles[i].Rotation);
                    if (position.Y + v.Y > 180) {
                        if (position.X + v.X > 320) {
                            Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y - 180), particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);
                        } else {
                            Draw.Pixel.DrawCentered(new Vector2(position.X, position.Y - 180), particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);
                        }
                    } else if (position.X + v.X > 320) {
                        Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y), particles[i].color * alpha * linearFade * visibleFade, particles[i].Scale, particles[i].Rotation);
                    }

                }
            }
        }

    }
}

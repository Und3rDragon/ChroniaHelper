// FrostHelper.CustomSpring
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using Celeste;

// The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
// The original author is ricky06, code modified by UnderDragon

namespace ChroniaHelper.Entities.MigratedNeonHelper
{
    [CustomEntity("ChroniaHelper/PufferBombSpringUp", "ChroniaHelper/PufferBombSpringDown", 
        "ChroniaHelper/PufferBombSpringLeft", "ChroniaHelper/PufferBombSpringRight")]
    public class PufferBombSpring : Spring
    {
        public enum CustomOrientations
        {
            Floor,
            WallLeft,
            WallRight,
            Ceiling
        }

        private float inactiveTimer;

        public new CustomOrientations Orientation;
        public bool RenderOutline;
        public Sprite Sprite;

        private DynData<Spring> dyndata;
        string dir;

        internal Vector2 speedMult;
        private int Version;

        public bool MultiplyPlayerY;

        private static Dictionary<string, CustomOrientations> EntityDataNameToOrientation = new Dictionary<string, CustomOrientations>()
        {
            ["ChroniaHelper/PufferBombSpringDown"] = CustomOrientations.Ceiling,
            ["ChroniaHelper/PufferBombSpringUp"] = CustomOrientations.Floor,
            ["ChroniaHelper/PufferBombSpringRight"] = CustomOrientations.WallLeft,
            ["ChroniaHelper/PufferBombSpringLeft"] = CustomOrientations.WallRight,
        };

        private static Dictionary<CustomOrientations, Orientations> CustomToRegularOrientation = new Dictionary<CustomOrientations, Orientations>()
        {
            [CustomOrientations.WallLeft] = Orientations.WallLeft,
            [CustomOrientations.WallRight] = Orientations.WallRight,
            [CustomOrientations.Floor] = Orientations.Floor,
            [CustomOrientations.Ceiling] = Orientations.Floor,
        };

        public PufferBombSpring(EntityData data, Vector2 offset) : this(data, offset, EntityDataNameToOrientation[data.Name]) { }

        public PufferBombSpring(EntityData data, Vector2 offset, CustomOrientations orientation) : base(data.Position + offset, CustomToRegularOrientation[orientation], data.Bool("playerCanUse", true))
        {
            bool playerCanUse = data.Bool("playerCanUse", true);
            dir = data.Attr("directory", "objects/spring/");
            RenderOutline = data.Bool("renderOutline", true);

            DynData<Spring> self = new DynData<Spring>(this);

            Vector2 position = data.Position + offset;
            DisabledColor = Color.White;
            Orientation = orientation;
            base.Orientation = CustomToRegularOrientation[orientation];
            self.Set("playerCanUse", playerCanUse);
            Remove(Get<PufferCollider>());
            PufferBombCollider pufferCollider = new PufferBombCollider(NewOnPuffer, null);
            Add(pufferCollider);
            dyndata = new DynData<Spring>(this);

            Sprite spr = self.Get<Sprite>("sprite");

            Remove(spr);
            Add(Sprite = new Sprite(GFX.Game, dir));
            Sprite.Add("idle", "", 0f, new int[1]);
            Sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            Sprite.Add("disabled", "white", 0.07f);
            Sprite.Play("idle", false, false);
            Sprite.Origin.X = Sprite.Width / 2f;
            Sprite.Origin.Y = Sprite.Height;

            Depth = -8501;

            Add(Wiggler.Create(1f, 4f, delegate (float v) {
                Sprite.Scale.Y = 1f + v * 0.2f;
            }, false, false));

            StaticMover staticMover2 = self.Get<StaticMover>("staticMover");

            switch (orientation)
            {
                case CustomOrientations.Floor:
                    Collider = new Hitbox(16f, 6f, -8f, -6f);
                    pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                    break;
                case CustomOrientations.WallLeft:
                    Collider = new Hitbox(6f, 16f, 0f, -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
                    Sprite.Rotation = MathHelper.PiOver2;
                    break;
                case CustomOrientations.WallRight:
                    Collider = new Hitbox(6f, 16f, -6f, -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                    Sprite.Rotation = -MathHelper.PiOver2;
                    break;
                case CustomOrientations.Ceiling:
                    Collider = new Hitbox(16f, 6f, -8f, 0);
                    pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -4f);
                    Sprite.Rotation = MathHelper.Pi;
                    staticMover2.SolidChecker = (s) => CollideCheck(s, Position - Vector2.UnitY);
                    staticMover2.JumpThruChecker = (jt) => CollideCheck(jt, Position - Vector2.UnitY);
                    break;
                default:
                    throw new Exception("Orientation not supported!");
            }

            dyndata.Set("sprite", Sprite);
            OneUse = data.Bool("oneUse", false);
            if (OneUse)
            {
                Add(new Coroutine(OneUseParticleRoutine()));
            }
        }


        public void base_Render()
        {
            base.Render();
        }

        public override void Update()
        {
            base.Update();

            inactiveTimer -= Engine.DeltaTime;
        }

        public override void Render()
        {
            if (Collidable && !RenderOutline)
            {
                Sprite.Render();
            }
            else
            {
                base.Render();
            }
        }

        public void TryBreak()
        {
            if (OneUse)
            {
                Add(new Coroutine(BreakRoutine()));
            }
        }

        public IEnumerator BreakRoutine()
        {
            Collidable = false;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Audio.Play("event:/game/general/platform_disintegrate", Center);
            foreach (Image image in Components.GetAll<Image>())
            {
                SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, Position + image.Position, Vector2.One * 3f);
            }

            float t = 1f;
            while (t > 0f)
            {
                foreach (Image image in Components.GetAll<Image>())
                {
                    image.Scale = Vector2.One * t;
                    image.Rotation += Engine.DeltaTime * 4;
                }
                t -= Engine.DeltaTime * 4;
                yield return null;
            }
            Visible = false;
            RemoveSelf();
            yield break;
        }

        private void NewOnPuffer(PufferBomb p)
        {
            if (p.HitSpring(this))
            {
                BounceAnimate();
            }
        }
        private void BounceAnimate()
        {
            Audio.Play("event:/game/general/spring", BottomCenter);
            StaticMover staticMover = dyndata.Get<StaticMover>("staticMover");
            Sprite sprite = dyndata.Get<Sprite>("sprite");
            Wiggler wiggler = dyndata.Get<Wiggler>("wiggler");

            staticMover.TriggerPlatform();
            sprite.Play("bounce", restart: true);
            wiggler.Start();
        }

        private static ParticleType P_Crumble_Up = new ParticleType
        {
            Color = Calc.HexToColor("847E87"),
            FadeMode = ParticleType.FadeModes.Late,
            Size = 1f,
            Direction = MathHelper.PiOver2,
            SpeedMin = -5f,
            SpeedMax = -25f,
            LifeMin = 0.8f,
            LifeMax = 1f,
            Acceleration = Vector2.UnitY * -20f
        };

        private static ParticleType P_Crumble_Down = new ParticleType
        {
            Color = Calc.HexToColor("847E87"),
            FadeMode = ParticleType.FadeModes.Late,
            Size = 1f,
            Direction = MathHelper.PiOver2,
            SpeedMin = 5f,
            SpeedMax = 25f,
            LifeMin = 0.8f,
            LifeMax = 1f,
            Acceleration = Vector2.UnitY * 20f
        };

        private static ParticleType P_Crumble_Left = new ParticleType
        {
            Color = Calc.HexToColor("847E87"),
            FadeMode = ParticleType.FadeModes.Late,
            Size = 1f,
            Direction = 0f,
            SpeedMin = 5f,
            SpeedMax = 25f,
            LifeMin = 0.8f,
            LifeMax = 1f,
            Acceleration = Vector2.UnitY * 20f
        };

        private static ParticleType P_Crumble_Right = new ParticleType
        {
            Color = Calc.HexToColor("847E87"),
            FadeMode = ParticleType.FadeModes.Late,
            Size = 1f,
            Direction = 0f,
            SpeedMin = -5f,
            SpeedMax = -25f,
            LifeMin = 0.8f,
            LifeMax = 1f,
            Acceleration = Vector2.UnitY * -20f
        };

        private IEnumerator OneUseParticleRoutine()
        {
            while (true)
            {
                switch (Orientation)
                {
                    case CustomOrientations.Floor:
                        SceneAs<Level>().Particles.Emit(P_Crumble_Up, 2, Position, new(3f));
                        break;
                    case CustomOrientations.WallRight:
                        SceneAs<Level>().Particles.Emit(P_Crumble_Right, 2, Position, new(2f));
                        break;
                    case CustomOrientations.WallLeft:
                        SceneAs<Level>().Particles.Emit(P_Crumble_Left, 2, Position, new(2f));
                        break;
                    case CustomOrientations.Ceiling:
                        SceneAs<Level>().Particles.Emit(P_Crumble_Down, 2, Position, new(2f));
                        break;
                }
                yield return 0.25f;
            }
        }

        public bool OneUse;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.Entities
{
    [CustomEntity(new string[]
    {
        "ChroniaHelper/BoosterZipper"
    })]
    [Tracked(false)]
    public class BoosterZipper : Booster
    {
        public BoosterZipper(EntityData data, Vector2 offset) : base(data.Position + offset, false)
        {
            this.respawnTime = data.Float("boosterRespawnTime", 1f);
            this.zipperMoveTime = data.Float("zipperMoveTime", 0.5f);
            bool flag = this.zipperMoveTime < 0.017f;
            if (flag)
            {
                this.zipperMoveTime = 0.017f;
            }
            GFX.SpriteBank.CreateOn(this.sprite, "boosterZipper");
            base.Add(new Coroutine(this.Sequence(), true));
            this.sfx.Position = new Vector2(base.Width, base.Height) / 2f;
            base.Add(this.sfx);
            bool flag2 = data.Nodes.Length >= 1;
            if (flag2)
            {
                this.target = data.NodesWithPosition(offset)[1];
            }

            string rc = data.Attr("ropeColor", "663931"),
                rcl = data.Attr("ropeLightColor", "9b6157");
            ropeColor = Calc.HexToColor(rc);
            ropeLightColor = Calc.HexToColor(rcl);
        }
        private Color ropeColor, ropeLightColor;

        public static void Load()
        {
            On.Celeste.Player.BoostCoroutine += Player_BoostCoroutine;
            On.Celeste.Player.BoostUpdate += Player_BoostUpdate;
            On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
            On.Celeste.Booster.PlayerBoosted += Booster_PlayerBoosted;
            On.Celeste.Booster.OnPlayer += Booster_OnPlayer;
            On.Celeste.Booster.Respawn += Booster_Respawn;
        }

        public static void Unload()
        {
            On.Celeste.Player.BoostCoroutine -= Player_BoostCoroutine;
            On.Celeste.Player.BoostUpdate -= Player_BoostUpdate;
            On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
            On.Celeste.Booster.PlayerBoosted -= Booster_PlayerBoosted;
            On.Celeste.Booster.OnPlayer -= Booster_OnPlayer;
            On.Celeste.Booster.Respawn -= Booster_Respawn;
        }

        private static int Player_BoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self)
        {
            return orig.Invoke(self);
        }

        private static IEnumerator Player_BoostCoroutine(On.Celeste.Player.orig_BoostCoroutine orig, Player self)
        {
            bool flag = self.CurrentBooster is BoosterZipper;
            if (flag)
            {
                yield break;
            }
            IEnumerator orig_enum = orig.Invoke(self);
            while (orig_enum.MoveNext())
            {
                object obj = orig_enum.Current;
                yield return obj;
            }
            yield break;
        }

        private static void Booster_PlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
        {
            string currentAnimationID = self.sprite.CurrentAnimationID;
            orig.Invoke(self);
            BoosterZipper boosterZipper = self as BoosterZipper;
            bool flag = boosterZipper != null;
            if (flag)
            {
                self.respawnTimer = boosterZipper.respawnTime;
                bool flag2 = boosterZipper.atEndPoint && currentAnimationID != "spin";
                if (flag2)
                {
                    boosterZipper.sprite.Play("pop_end", false, false);
                }
            }
        }

        private static void Booster_PlayerBoosted(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction)
        {
            orig.Invoke(self, player, direction);
            BoosterZipper boosterZipper = self as BoosterZipper;
            bool flag = boosterZipper != null && boosterZipper.atEndPoint;
            if (flag)
            {
                boosterZipper.sprite.Play("spin_end", false, false);
                Vector2 vector = player.ExplodeLaunch(player.Center - direction, false, false);
                Level level = Engine.Scene as Level;
                if (level != null)
                {
                    level.DirectionalShake(vector, 0.15f);
                }
                Audio.Play("event:/new_content/game/10_farewell/puffer_splode", self.Position);
                player.dashCooldownTimer = 0f;
            }
        }

        private static void Booster_OnPlayer(On.Celeste.Booster.orig_OnPlayer orig, Booster self, Player player)
        {
            orig.Invoke(self, player);
            BoosterZipper boosterZipper = self as BoosterZipper;
            bool flag = boosterZipper != null && boosterZipper.atEndPoint && boosterZipper.sprite.CurrentAnimationID == "inside";
            if (flag)
            {
                boosterZipper.sprite.Play("inside_end", false, false);
            }
        }

        private static void Booster_Respawn(On.Celeste.Booster.orig_Respawn orig, Booster self)
        {
            orig.Invoke(self);
            BoosterZipper boosterZipper = self as BoosterZipper;
            bool flag = boosterZipper != null && boosterZipper.atEndPoint;
            if (flag)
            {
                boosterZipper.sprite.Play("loop_end", true, false);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(this.pathRenderer = new BoosterZipper.ZipMoverPathRenderer(this, base.Center, this.target));
            Image image = this.outline.Components.Get<Image>();
            bool flag = image != null;
            if (flag)
            {
                SpriteBank spriteBank = GFX.SpriteBank;
                string text;
                if (spriteBank == null)
                {
                    text = null;
                }
                else
                {
                    SpriteData spriteData = spriteBank.SpriteData["boosterZipper"];
                    if (spriteData == null)
                    {
                        text = null;
                    }
                    else
                    {
                        List<SpriteDataSource> sources = spriteData.Sources;
                        if (sources == null)
                        {
                            text = null;
                        }
                        else
                        {
                            SpriteDataSource spriteDataSource = sources.First<SpriteDataSource>();
                            if (spriteDataSource == null)
                            {
                                text = null;
                            }
                            else
                            {
                                XmlElement xml = spriteDataSource.XML;
                                text = ((xml != null) ? Calc.Attr(xml, "path") : null);
                            }
                        }
                    }
                }
                string text2 = text;
                bool flag2 = text2 == null || text2.Length == 0;
                if (!flag2)
                {
                    image.Texture = GFX.Game[text2 + "outline"];
                }
            }
        }

        public override void Update()
        {
            base.Update();
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            bool flag = entity != null && entity.CurrentBooster == this;
            if (flag)
            {
                base.BoostingPlayer = true;
                entity.boostTarget = base.Center;
                Vector2 vector = base.Center - entity.Collider.Center + Input.Aim.Value * 3f;
                entity.MoveToX(vector.X, null);
                entity.MoveToY(vector.Y, null);
            }
        }

        private IEnumerator Sequence()
        {
            Player player = base.Scene.Tracker.GetEntity<Player>();
            Vector2 start = this.Position;
            float movementSpeed = 1f / this.zipperMoveTime;
            for (; ; )
            {
                bool boostingPlayer = base.BoostingPlayer;
                if (boostingPlayer)
                {
                    this.sfx.Play("event:/game/01_forsaken_city/zip_mover", null, 0f);
                    Input.Rumble(RumbleStrength.Medium, 0);
                    yield return 0.1f;
                    float at = 0f;
                    while (at < 1f)
                    {
                        yield return null;
                        at = Calc.Approach(at, 1f, movementSpeed * Engine.DeltaTime);
                        this.percent = Ease.SineIn.Invoke(at);
                        Vector2 vector = Vector2.Lerp(start, this.target, this.percent);
                        bool flag = base.Scene.OnInterval(0.1f);
                        if (flag)
                        {
                            this.pathRenderer.CreateSparks();
                        }
                        bool flag2 = at > 0.983f;
                        if (flag2)
                        {
                            this.atEndPoint = true;
                            // Console.WriteLine(this.sprite.CurrentAnimationID);
                            bool flag3 = this.sprite.CurrentAnimationID == "loop";
                            if (flag3)
                            {
                                this.sprite.Play("toEndloop", false, false);
                            }
                            bool flag4 = this.sprite.CurrentAnimationID == "inside";
                            if (flag4)
                            {
                                this.sprite.Play("toEndinside", false, false);
                            }
                            bool flag5 = this.sprite.CurrentAnimationID == "inside_loop";
                            if (flag5)
                            {
                                this.sprite.Play("toEndinsideLoop", false, false);
                            }
                        }
                        this.Position = vector;
                        this.outline.Position = this.Position;
                        vector = default(Vector2);
                    }
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    base.SceneAs<Level>().Shake(0.3f);
                    yield return 0.5f;
                    this.atEndPoint = false;
                    bool flag6 = this.sprite.CurrentAnimationID == "loop_end";
                    if (flag6)
                    {
                        this.sprite.Play("fromEndLoop", false, false);
                    }
                    bool flag7 = this.sprite.CurrentAnimationID == "inside_end";
                    if (flag7)
                    {
                        this.sprite.Play("fromEndinside", false, false);
                    }
                    bool flag8 = this.sprite.CurrentAnimationID == "inside_loop_end";
                    if (flag8)
                    {
                        this.sprite.Play("fromEndinsideLoop", false, false);
                    }
                    at = 0f;
                    while (at < 1f)
                    {
                        yield return null;
                        at = Calc.Approach(at, 1f, movementSpeed / 4f * Engine.DeltaTime);
                        this.percent = 1f - Ease.SineIn.Invoke(at);
                        Vector2 position = Vector2.Lerp(this.target, start, Ease.SineIn.Invoke(at));
                        this.Position = position;
                        this.outline.Position = this.Position;
                        position = default(Vector2);
                    }
                    yield return 0.5f;
                    this.sfx.Stop(true);
                }
                else
                {
                    yield return null;
                }
            }
            yield break;
        }

        private float percent;

        private SoundSource sfx = new SoundSource();

        private BoosterZipper.ZipMoverPathRenderer pathRenderer;

        private Vector2 target;

        public bool atEndPoint = false;

        private float respawnTime = 1f;

        private float zipperMoveTime = 0.5f;

        private class ZipMoverPathRenderer : Entity
        {
            public ZipMoverPathRenderer(BoosterZipper Booster, Vector2 start, Vector2 target)
            {
                base.Depth = 5000;
                this.Booster = Booster;
                this.from = start;
                this.to = target;
                this.sparkAdd = Calc.Perpendicular(Calc.SafeNormalize(this.from - this.to, 5f));
                float num = Calc.Angle(this.from - this.to);
                this.sparkDirFromA = num + 0.3926991f;
                this.sparkDirFromB = num - 0.3926991f;
                this.sparkDirToA = num + 3.1415927f - 0.3926991f;
                this.sparkDirToB = num + 3.1415927f + 0.3926991f;
                this.cog = GFX.Game["objects/zipmover/cog"];
            }

            public void CreateSparks()
            {
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.from + this.sparkAdd + Calc.Range(Calc.Random, -Vector2.One, Vector2.One), this.sparkDirFromA);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.from - this.sparkAdd + Calc.Range(Calc.Random, -Vector2.One, Vector2.One), this.sparkDirFromB);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.to + this.sparkAdd + Calc.Range(Calc.Random, -Vector2.One, Vector2.One), this.sparkDirToA);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.to - this.sparkAdd + Calc.Range(Calc.Random, -Vector2.One, Vector2.One), this.sparkDirToB);
            }

            public override void Render()
            {
                this.DrawCogs(Vector2.UnitY, new Color?(Color.Black));
                this.DrawCogs(Vector2.Zero); //Calc.HexToColor("45bee5")
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null)
            {
                Vector2 vector = Calc.SafeNormalize(this.to - this.from);
                Vector2 vector2 = Calc.Perpendicular(vector) * 3f;
                Vector2 vector3 = -Calc.Perpendicular(vector) * 4f;
                float num = this.Booster.percent * 3.1415927f * 2f;
                Draw.Line(this.from + vector2 + offset, this.to + vector2 + offset, (colorOverride != null) ? colorOverride.Value : Booster.ropeColor);
                Draw.Line(this.from + vector3 + offset, this.to + vector3 + offset, (colorOverride != null) ? colorOverride.Value : Booster.ropeColor);
                for (float num2 = 4f - this.Booster.percent * 3.1415927f * 8f % 4f; num2 < (this.to - this.from).Length(); num2 += 4f)
                {
                    Vector2 vector4 = this.from + vector2 + Calc.Perpendicular(vector) + vector * num2;
                    Vector2 vector5 = this.to + vector3 - vector * num2;
                    Draw.Line(vector4 + offset, vector4 + vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : Booster.ropeLightColor);
                    Draw.Line(vector5 + offset, vector5 - vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : Booster.ropeLightColor);
                }
                this.cog.DrawCentered(this.from + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, num);
                this.cog.DrawCentered(this.to + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, num);
            }

            public BoosterZipper Booster;

            private MTexture cog;

            private Vector2 from;

            private Vector2 to;

            private Vector2 sparkAdd;

            private float sparkDirFromA;

            private float sparkDirFromB;

            private float sparkDirToA;

            private float sparkDirToB;
        }
    }
}
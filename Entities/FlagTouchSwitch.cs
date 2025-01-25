using Celeste.Mod.Entities;
using ChroniaHelper.Modules;
using ChroniaHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChroniaHelper.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities
{
    /// <summary>
    /// A touch switch triggering an arbitrary session flag.
    ///
    /// Attributes:
    /// - flag: the session flag this touch switch sets. Must be the same across the whole touch switch group.
    /// - icon: the name of the icon for the touch switch (relative to objects/MaxHelpingHand/flagTouchSwitch) or "vanilla" for the default one.
    /// - persistent: enable to have the switch stay active even when you die / change rooms.
    /// - inactiveColor / activeColor / finishColor: custom colors for the touch switch.
    /// </summary>
    [CustomEntity("ChroniaHelper/FlagTouchSwitch")]
    [Tracked]
    public class FlagTouchSwitch : Entity
    {
        private static FieldInfo seekerPushRadius = typeof(Seeker).GetField("pushRadius", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo seekerPhysicsHitbox = typeof(Seeker).GetField("physicsHitbox", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo pufferPushRadius = typeof(Puffer).GetField("pushRadius", BindingFlags.NonPublic | BindingFlags.Instance);

        private static FieldInfo dreamSwitchGateIsFlagSwitchGate = null;
        private static MethodInfo dreamSwitchGateTriggeredSetter = null;
        private static MethodInfo dreamSwitchGateFlagGetter = null;

        enum switchClass{
            touchSwitch,
            touchSwitchWall,
        }
        public static void Load()
        {
            On.Celeste.Seeker.RegenerateCoroutine += onSeekerRegenerateCoroutine;
            On.Celeste.Puffer.Explode += onPufferExplode;
        }

        public static void Unload()
        {
            On.Celeste.Seeker.RegenerateCoroutine -= onSeekerRegenerateCoroutine;
            On.Celeste.Puffer.Explode -= onPufferExplode;
        }

        private static IEnumerator onSeekerRegenerateCoroutine(On.Celeste.Seeker.orig_RegenerateCoroutine orig, Seeker self)
        {
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext())
            {
                yield return origEnum.Current;
            }

            // make the seeker check for flag touch switches as well.
            self.Collider = (Collider)seekerPushRadius.GetValue(self);
            turnOnTouchSwitchesCollidingWith(self);
            self.Collider = (Collider)seekerPhysicsHitbox.GetValue(self);
        }

        private static void onPufferExplode(On.Celeste.Puffer.orig_Explode orig, Puffer self)
        {
            orig(self);

            // make the puffer check for flag touch switches as well.
            Collider oldCollider = self.Collider;
            self.Collider = (Collider)pufferPushRadius.GetValue(self);
            turnOnTouchSwitchesCollidingWith(self);
            self.Collider = oldCollider;
        }

        private static void turnOnTouchSwitchesCollidingWith(Entity self)
        {
            foreach (FlagTouchSwitch touchSwitch in self.Scene.Tracker.GetEntities<FlagTouchSwitch>())
            {
                if (self.CollideCheck(touchSwitch))
                {
                    touchSwitch.TurnOn();
                }
            }
        }

        private ParticleType P_RecoloredFire;

        private int id;
        private string flag;
        public string Flag => flag;

        private bool inverted;
        private bool allowDisable;

        // contains all the touch switches in the room
        private List<FlagTouchSwitch> allTouchSwitchesInRoom;
        private List<TouchSwitch> allMovingFlagTouchSwitchesInRoom;

        public bool Activated { get; private set; } = false;
        public bool Finished { get; private set; } = false;

        private SoundSource touchSfx;

        private MTexture border = GFX.Game["objects/touchswitch/container"];

        private Sprite icon;
        private bool persistent;

        private Color inactiveColor;
        private Color activeColor;
        private Color finishColor;

        private bool smoke;

        private float ease;

        private Wiggler wiggler;

        private Vector2 pulse = Vector2.One;

        private float timer = 0f;

        private BloomPoint bloom;

        private string hitSound;
        private string completeSoundFromSwitch;
        private string completeSoundFromScene;

        private string hideIfFlag;

        private Level level => (Level)Scene;

        private float ex, ey, ew, eh, iw, ih;

        private switchClass classify;

        private bool vanilla;

        private float idleAnimTime, spinAnimTime;

        private List<string> vanillanames = new List<string>
        { "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple" };

        public FlagTouchSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.classify = data.Enum<switchClass>("switch", switchClass.touchSwitchWall);

            Depth = 2000;

            id = data.ID;
            flag = data.Attr("flag");
            persistent = data.Bool("persistent", false);

            inactiveColor = Calc.HexToColor(data.Attr("inactiveColor", "5FCDE4"));
            activeColor = Calc.HexToColor(data.Attr("activeColor", "FFFFFF"));
            finishColor = Calc.HexToColor(data.Attr("finishColor", "F141DF"));

            hitSound = data.Attr("hitSound", "event:/game/general/touchswitch_any");
            completeSoundFromSwitch = data.Attr("completeSoundFromSwitch", "event:/game/general/touchswitch_last_cutoff");
            completeSoundFromScene = data.Attr("completeSoundFromScene", "event:/game/general/touchswitch_last_oneshot");

            hideIfFlag = data.Attr("hideIfFlag");

            inverted = data.Bool("inverted", false);
            allowDisable = data.Bool("allowDisable", false);

            smoke = data.Bool("smoke", true);

            string borderTexturePath = data.Attr("borderTexture");
            if (!string.IsNullOrEmpty(borderTexturePath))
            {
                border = GFX.Game[borderTexturePath];
            }

            P_RecoloredFire = new ParticleType(TouchSwitch.P_Fire)
            {
                Color = finishColor
            };

            // set up collision
            ex = data.Position.X; //entityX
            ey = data.Position.Y; //entityY
            ew = data.Width; //entityWidth
            eh = data.Height; //entityHeight

            if(this.classify == switchClass.touchSwitch)
            {
                Collider = new Hitbox(16f, 16f, ew/2 -8f, eh/2 -8f);
                if (data.Bool("playerCanActivate", defaultValue: true))
                {
                    Add(new PlayerCollider(onPlayer, null, new Hitbox(30f, 30f, ew/2 -15f, eh/2 -15f)));
                }
                Add(new HoldableCollider(onHoldable, new Hitbox(20f, 20f, ew/2 -10f, eh/2 -10f)));
                Add(new SeekerCollider(onSeeker, new Hitbox(24f, 24f, ew/2 -12f, eh/2 -12f)));
            }
            else
            {
                Collider = new Hitbox(data.Width, data.Height);
                if (data.Bool("playerCanActivate", defaultValue: true))
                {
                    Add(new PlayerCollider(onPlayer, null, new Hitbox(data.Width, data.Height)));
                }
                Add(new HoldableCollider(onHoldable, new Hitbox(data.Width, data.Height)));
                Add(new SeekerCollider(onSeeker, new Hitbox(data.Width, data.Height)));
            }


            // set up the icon
            idleAnimTime = data.Float("idleAnimDelay",0.1f) < 0f ? 0.1f : data.Float("idleAnimDelay", 0.1f);
            spinAnimTime = data.Float("spinAnimDelay",0.1f) < 0f ? 0.1f : data.Float("spinAnimDelay", 0.1f);
            string iconAttribute = data.Attr("icon", "vanilla");
            if (vanillanames.Contains(iconAttribute))
            {
                vanilla = true;
            }
            else { vanilla = false; }

            if (vanilla) { icon = new Sprite(GFX.Game, iconAttribute == "vanilla" ? "objects/touchswitch/icon" : $"objects/ChroniaHelper/flagTouchSwitch/{iconAttribute}/icon"); }
            else { icon = new Sprite(GFX.Game, iconAttribute); }

            Add(icon);
            if (vanilla)
            {
                icon.Add("idle", "", 0f, default(int));
                icon.Add("spin", "", 0.1f, new Chooser<string>("spin", 1f), 0, 1, 2, 3, 4, 5);
            }
            else
            {
                icon.AddLoop("idle", "", idleAnimTime);
                icon.AddLoop("spin", "", spinAnimTime);
            }

            icon.Play("spin");
            icon.Color = inactiveColor;
            ih = icon.Height;
            iw = icon.Width;
            icon.SetOrigin(-data.Width/2 + icon.Width/2, -data.Height/2 + icon.Height/2);

            Add(bloom = new BloomPoint(new Vector2(data.Width/2, data.Height/2), 0f, 16f)); // original 0, 16
            bloom.Alpha = 0f;

            Add(wiggler = Wiggler.Create(0.5f, 4f, v => {
                pulse = Vector2.One * (1f + v * 0.25f);
            }));

            Add(new VertexLight(Color.White, 0.8f, 16, 32));
            Add(touchSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (inverted != level.Session.GetFlag(flag))
            {
                // start directly finished, since the session flag is already set (or the flag is inverted and unset).
                Activated = true;
                Finished = true;

                icon.Rate = 0.1f;
                icon.Play("idle");
                icon.Color = finishColor;
                ease = 1f;
                bloom.Alpha = 1f;
            }
            else if (level.Session.GetFlag(flag + "_switch" + id))
            {
                // only that switch is activated, not the whole group.
                Activated = true;

                icon.Rate = 4f;
                icon.Color = activeColor;
                ease = 1f;
                bloom.Alpha = 1f;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            // look around for other touch switches that belong to the same group (same flag).
            allTouchSwitchesInRoom = Scene.Tracker.GetEntities<FlagTouchSwitch>()
                .FindAll(touchSwitch => (touchSwitch as FlagTouchSwitch)?.flag == flag).OfType<FlagTouchSwitch>().ToList();
            allMovingFlagTouchSwitchesInRoom = Scene.Entities.OfType<TouchSwitch>()
                .Where(touchSwitch =>
                    touchSwitch.GetType().ToString() == "Celeste.Mod.OutbackHelper.MovingTouchSwitch" &&
                    new DynData<TouchSwitch>(touchSwitch).Data.ContainsKey("flag") &&
                    new DynData<TouchSwitch>(touchSwitch).Get<string>("flag") == flag).ToList();
        }

        private void onPlayer(Player player)
        {
            TurnOn();
        }

        private void onHoldable(Holdable h)
        {
            TurnOn();
        }

        private void onSeeker(Seeker seeker)
        {
            if (SceneAs<Level>().InsideCamera(Position, 10f))
            {
                TurnOn();
            }
        }

        public void TurnOn()
        {
            if (!Activated)
            {
                doEffect(() => touchSfx.Play(hitSound));

                Activated = true;

                // animation
                doEffect(() => {
                    wiggler.Start();
                    for (int i = 0; i < 32; i++)
                    {
                        float num = Calc.Random.NextFloat((float)Math.PI * 2f);
                        level.Particles.Emit(TouchSwitch.P_FireWhite, Position + new Vector2(ew/2, eh/2) + Calc.AngleToVector(num, 6f), num);
                    }
                });
                icon.Rate = 4f;

                HandleCollectedFlagTouchSwitch(flag, inverted, persistent, level, id, allTouchSwitchesInRoom, allMovingFlagTouchSwitchesInRoom, () => doEffect(() => {
                    SoundEmitter.Play(completeSoundFromScene);
                    Add(new SoundSource(completeSoundFromSwitch));
                }));
            }
        }

        // returns true if the entire group was completed.
        internal static bool HandleCollectedFlagTouchSwitch(string flag, bool inverted, bool persistent, Level level, int id,
            List<FlagTouchSwitch> allTouchSwitchesInRoom, List<TouchSwitch> allMovingFlagTouchSwitchesInRoom, Action onFinished)
        {

            if (persistent)
            {
                // this switch is persistent. save its activation in the session.
                level.Session.SetFlag(flag + "_switch" + id, true);
            }

            
            if (MaxHelpingHandMapDataProcessor.FlagTouchSwitches[level.Session.Area.ID][(int)level.Session.Area.Mode][new KeyValuePair<string, bool>(flag, inverted)]
                .All(touchSwitchID => touchSwitchID.Level == level.Session.Level || level.Session.GetFlag(flag + "_switch" + touchSwitchID.ID))
                    && allTouchSwitchesInRoom.All(touchSwitch => touchSwitch.Activated || touchSwitch.isHidden())
                    && allMovingFlagTouchSwitchesInRoom.All(touchSwitch => touchSwitch.Switch.Activated || MovingFlagTouchSwitch.IsHidden(touchSwitch)))
            {

                // all switches in the room are enabled or hidden, and all session flags for switches outside the room are enabled.
                // so, the group is complete.

                foreach (FlagTouchSwitch touchSwitch in allTouchSwitchesInRoom)
                {
                    touchSwitch.finish();
                }
                foreach (TouchSwitch touchSwitch in allMovingFlagTouchSwitchesInRoom)
                {
                    touchSwitch.Switch.Finish();
                }

                onFinished();

                // trigger associated switch gate(s).
                foreach (FlagSwitchGate switchGate in level.Tracker.GetEntities<FlagSwitchGate>().OfType<FlagSwitchGate>())
                {
                    if (switchGate.Flag == flag)
                    {
                        switchGate.Trigger();
                    }
                }

                // trigger associated dream flag switch gate(s) from Communal Helper
                foreach (Entity dreamFlagSwitchGate in level.Entities
                    .Where(entity => entity.GetType().ToString() == "Celeste.Mod.CommunalHelper.DreamSwitchGate")
                    .Where(dreamSwitchGate => {
                        // said dream switch gate should be flag too, but that's a private field.
                        if (dreamSwitchGateIsFlagSwitchGate == null)
                        {
                            dreamSwitchGateIsFlagSwitchGate = dreamSwitchGate.GetType().GetField("isFlagSwitchGate", BindingFlags.NonPublic | BindingFlags.Instance);
                            dreamSwitchGateTriggeredSetter = dreamSwitchGate.GetType().GetMethod("set_Triggered", BindingFlags.NonPublic | BindingFlags.Instance);
                            dreamSwitchGateFlagGetter = dreamSwitchGate.GetType().GetMethod("get_Flag");
                        }
                        return (bool)dreamSwitchGateIsFlagSwitchGate.GetValue(dreamSwitchGate) && dreamSwitchGateFlagGetter.Invoke(dreamSwitchGate, new object[0]).ToString() == flag;
                    }))
                {

                    dreamSwitchGateTriggeredSetter.Invoke(dreamFlagSwitchGate, new object[] { true });
                }


                // set flags for switch gates.
                bool allGatesTriggered = true;
                if (MaxHelpingHandMapDataProcessor.FlagSwitchGates[level.Session.Area.ID][(int)level.Session.Area.Mode].ContainsKey(flag))
                {
                    Dictionary<EntityID, bool> allGates = MaxHelpingHandMapDataProcessor.FlagSwitchGates[level.Session.Area.ID][(int)level.Session.Area.Mode][flag];
                    foreach (KeyValuePair<EntityID, bool> gate in allGates)
                    {
                        if (gate.Value)
                        {
                            // the gate is persistent; set the flag
                            level.Session.SetFlag(flag + "_gate" + gate.Key.ID);
                        }
                        else
                        {
                            // one of the gates is not persistent, so the touch switches shouldn't be forced to persist.
                            allGatesTriggered = false;
                        }
                    }
                }

                // if all the switches OR all the gates are persistent, the flag it's setting is persistent.
                if ((allTouchSwitchesInRoom.All(touchSwitch => touchSwitch.persistent) &&
                    allMovingFlagTouchSwitchesInRoom.All(touchSwitch => new DynData<TouchSwitch>(touchSwitch).Get<bool>("persistent"))) || allGatesTriggered)
                {

                    level.Session.SetFlag(flag, !inverted);
                }

                return true;
            }

            return false;
        }

        private void finish()
        {
            Finished = true;
            ease = 0f;
        }

        public override void Update()
        {
            if (isHidden())
            {
                // disable the entity entirely, and spawn another entity to continue monitoring the flag
                Visible = Active = Collidable = false;
                Scene.Add(new ResurrectOnFlagDisableController { Entity = this, Flag = hideIfFlag });
                return;
            }

            timer += Engine.DeltaTime * 8f;
            ease = Calc.Approach(ease, (Finished || Activated) ? 1f : 0f, Engine.DeltaTime * 2f);

            icon.Color = Color.Lerp(inactiveColor, Finished ? finishColor : activeColor, ease);
            icon.Color *= 0.5f + ((float)Math.Sin(timer) + 1f) / 2f * (1f - ease) * 0.5f + 0.5f * ease;

            bloom.Alpha = ease;
            if (Finished)
            {
                if (icon.Rate > 0.1f)
                {
                    icon.Rate -= 2f * Engine.DeltaTime;
                    if (icon.Rate <= 0.1f)
                    {
                        icon.Rate = 0.1f;
                        wiggler.Start();
                        icon.Play("idle");
                        level.Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                    }
                }
                else if (Scene.OnInterval(0.03f) && smoke)
                {
                    Vector2 position = Position + new Vector2(ew/2, eh/2) + new Vector2(0f, 1f) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
                    level.ParticlesBG.Emit(P_RecoloredFire, position);
                }

                if (allowDisable && inverted == level.Session.GetFlag(flag))
                {
                    // we have to disable the touch switch! aaa
                    if (persistent)
                    {
                        // if the touch switch is persistent, turn its flag off.
                        level.Session.SetFlag(flag + "_switch" + id, false);
                    }

                    Activated = Finished = false;
                    icon.Rate = 1f;
                    icon.Play("spin");

                    // cancel all alarms (deferred animations).
                    foreach (Alarm alarm in Components.OfType<Alarm>().ToList())
                    {
                        alarm.RemoveSelf();
                    }
                }
            }

            base.Update();
        }

        public override void Render()
        {
            
            if (this.classify == switchClass.touchSwitch) {
                border.DrawCentered(Position + new Vector2( ew / 2 , eh / 2) + new Vector2(0f, -1f), Color.Black);
                border.DrawCentered(Position + new Vector2(ew / 2, eh / 2), icon.Color, pulse);
            }
            else
            {
                Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, ColorUtils.ColorCopy(icon.Color, 0.7f));
                Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Lerp(icon.Color, Calc.HexToColor("0a0a0a"), 0.5f) * 0.3f);
            }

            base.Render();
        }

        private void doEffect(Action effect)
        {
            if (allowDisable)
            {
                // do the effect only after 0.05s, in case our touch switch gets disabled in the meantime.
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, effect, 0.05f, true));
            }
            else
            {
                // do the effect right now.
                effect();
            }
        }

        // a tiny entity that will monitor the flag to resurrect the touch switch if it is enabled again.
        internal class ResurrectOnFlagDisableController : Entity
        {
            public Entity Entity { get; set; }
            public string Flag { get; set; }

            public override void Update()
            {
                if (!(Scene as Level).Session.GetFlag(Flag))
                {
                    Entity.Visible = Entity.Active = Entity.Collidable = true;
                    RemoveSelf();
                }
            }
        }

        private bool isHidden()
        {
            return !string.IsNullOrEmpty(hideIfFlag) && (Scene as Level).Session.GetFlag(hideIfFlag);
        }
    }
}
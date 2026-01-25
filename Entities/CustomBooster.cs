using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using System.Reflection;
using ChroniaHelper.Utils;
using ChroniaHelper.Cores;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using YamlDotNet.Core;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomBooster", "ChroniaHelper/CustomBoosterXML")]
public class CustomBooster : Booster
{
    private static ILHook redDashCoroutineHook, greenDashCoroutineHook;
    private static ILHook dashCoroutineHook;

    [LoadHook]
    public static void Load()
    {
        On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
        On.Celeste.Booster.BoostRoutine += Booster_BoostRoutine;
        IL.Celeste.Player.BoostBegin += Player_BoostBegin;

        On.Celeste.Player.Boost += Player_Boost;
        On.Celeste.Player.RedBoost += Player_RedBoost;
        origBoostBeginHook = new ILHook(typeof(Player).GetMethod("orig_BoostBegin", BindingFlags.NonPublic | BindingFlags.Instance), Player_BoostBegin);

        var methodInfo = typeof(Player).GetMethod("BoostCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        dashCoroutineHook = new ILHook(methodInfo, ILHookDashCoroutine);

        IL.Celeste.Player.BoostUpdate += PlayerOnBoostUpdate;

        On.Celeste.Booster.AppearParticles += Booster_AppearParticles;

        On.Celeste.Booster.PlayerBoosted += Booster_PlayerBoosted;

        // Usable but unecessary

        // On.Celeste.Player.RefillDash += RefillD;
        // On.Celeste.Player.RefillStamina += RefillS;
        
        On.Celeste.Player.BoostUpdate += PlayerOnBoostUpdate;

        redDashCoroutineHook = new ILHook(typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), Player_RedDashHook);
        greenDashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), Player_GreenDashHook);
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
        On.Celeste.Booster.BoostRoutine -= Booster_BoostRoutine;
        IL.Celeste.Player.BoostBegin -= Player_BoostBegin;

        On.Celeste.Player.Boost -= Player_Boost;
        On.Celeste.Player.RedBoost -= Player_RedBoost;
        origBoostBeginHook?.Dispose();
        origBoostBeginHook = null;

        dashCoroutineHook.Dispose();

        IL.Celeste.Player.BoostUpdate -= PlayerOnBoostUpdate;

        On.Celeste.Booster.AppearParticles -= Booster_AppearParticles;

        On.Celeste.Booster.PlayerBoosted -= Booster_PlayerBoosted;
        // Usable but unecessary

        // On.Celeste.Player.RefillDash -= RefillD;
        // On.Celeste.Player.RefillStamina -= RefillS;
        On.Celeste.Player.BoostUpdate -= PlayerOnBoostUpdate;

        redDashCoroutineHook.Dispose();
        greenDashCoroutineHook.Dispose();
    }

    private float appearTime, loopTime, insideTime, spinTime, popTime, tr;

    private string outlineDir;

    private int setDash, setStamina;

    private bool setDashes, setupStamina;

    private float hbr, hbx, hby;

    private float moveSpeed, outSpeed;

    private bool setOutSpeed, rememberSpeed;

    private Color color;
    private ParticleType customBurstParticleType, customAppearParticleType;
    private bool burstParticleColorOverride, appearParticleColorOverride;
    private float holdTime = 0.25f;
    public bool DisableFastBubble, allowDashout, onlyOnce;
    
    public bool playerFollow;

    public float redBoostMovingSpeed = 240f, greenBoostMovingSpeed = 240f;

    public CustomBooster(EntityData data, Vector2 position, bool red)
        : base(position, red)
    {
        base.Depth = data.Int("depth", -8500);
        Ch9HubBooster = data.Bool("ch9_hub_booster", false);
        allowDashout = data.Bool("allowDashOutWhenBoosting", true);
        onlyOnce = data.Bool("onlyOnce", false);

        hbr = Math.Abs(data.Float("hitboxRadius", 10f));
        hbx = data.Float("hitboxX", 0f);
        hby = data.Float("hitboxY", 2f);
        base.Collider = new Circle(hbr, hbx, hby);
        this.red = red;
        appearTime = data.Float("appearAnimInterval", 0.08f);
        loopTime = data.Float("loopAnimInterval", 0.1f);
        insideTime = data.Float("insideAnimInterval", 0.1f);
        spinTime = data.Float("spinAnimInterval", 0.06f);
        popTime = data.Float("popAnimInterval", 0.08f);

        // Dashes and Stamina
        setDash = data.Int("dashes", 1);
        setStamina = data.Int("stamina", 110);
        setDashes = data.Bool("setOrRefillDashes", false);
        setupStamina = data.Bool("setOrRefillStamina", false);

        redBoostMovingSpeed = data.Float("redBoostMovingSpeed", 240f);
        greenBoostMovingSpeed = data.Float("greenBoostMovingSpeed", 240f);

        // process old data
        if (!string.IsNullOrEmpty(data.Attr("setOutSpeed")))
        {
            bool setOutSpeed = data.Bool("setOutSpeed", false);
            outSpeed = setOutSpeed ? outSpeed : 1f;
        }
        else
        {
            outSpeed = Math.Abs(data.Float("outSpeedMultiplier", 1f));
        }

        color = Calc.HexToColor(data.Attr("colorOverlay", "ffffff"));
        bool gfxoverride = data.Bool("XMLOverride", false);
        Remove(base.sprite);
        // Sprite shenanigans
        if (!gfxoverride)
        {
            sprite = new Sprite(GFX.Game, data.Attr("directory", "objects/ChroniaHelper/customBoosterPresets/grey").Trim('/') + "/");
            sprite.Justify = new Vector2(0.5f, 0.5f);
            if (red)
            {
                sprite.Add("appear", "appear", appearTime, "loop");
            }

            sprite.AddLoop("loop", "loop", loopTime);
            sprite.AddLoop("inside", "inside", insideTime);
            sprite.AddLoop("spin", "spin", spinTime);
            sprite.Add("pop", "pop", popTime);
        }
        else
        {
            string dir = data.Attr("directory", "Default_booster");
            sprite = GFX.SpriteBank.Create(dir);
        }

        sprite.Color = color;
        Add(sprite);
        sprite.Play("loop");

        // Add(sprite = GFX.SpriteBank.Create(red ? "boosterRed" : "booster"));
        Add(new PlayerCollider(OnPlayer));
        Add(light = new VertexLight(Color.White, 1f, 16, 32));
        Add(bloom = new BloomPoint(0.1f, 16f));
        Add(wiggler = Wiggler.Create(0.5f, 4f, (float f) => { sprite.Scale = Vector2.One * (1f + f * 0.25f); }));
        Add(dashRoutine = new Coroutine(removeOnComplete: false));
        Add(dashListener = new DashListener());
        Add(new MirrorReflection());
        Add(loopingSfx = new SoundSource());
        dashListener.OnDash = OnPlayerDashed;
        particleType = (red ? P_BurstRed : P_Burst);

        tr = data.Float("respawnTime", 1f);
        outlineDir = data.Attr("outlineDirectory", "objects/ChroniaHelper/customBooster/outline");

        burstParticleColorOverride = data.Bool("burstParticleColorOverride");
        if (burstParticleColorOverride)
        {
            customBurstParticleType = new ParticleType(P_Burst);
            customBurstParticleType.Color = data.HexColor("burstParticleColor");
        }

        appearParticleColorOverride = data.Bool("appearParticleColorOverride");
        if (appearParticleColorOverride)
        {
            customAppearParticleType = new ParticleType(P_Appear);
            customAppearParticleType.Color = data.HexColor("appearParticleColor");
        }

        holdTime = data.Float("holdTime", 0.25f);
        DisableFastBubble = data.Bool("disableFastBubble", false);
        playerFollow = data.Bool("playerFollow", false);
        rememberSpeed = data.Bool("keepPlayerSpeed", false);
    }


    public CustomBooster(EntityData data, Vector2 offset)
        : this(data, data.Position + offset, data.Bool("red"))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Image image = new Image(GFX.Game[outlineDir]);
        image.CenterOrigin();
        image.Color = Color.White * 0.75f;
        outline = new Entity(Position);
        outline.Depth = 8999;
        outline.Visible = false;
        outline.Add(image);
        outline.Add(new MirrorReflection());
        scene.Add(outline);
    }

    private static void Booster_PlayerBoosted(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction)
    {
        if(self is CustomBooster booster)
        {
            Audio.Play(booster.red ? "event:/game/05_mirror_temple/redbooster_dash" : "event:/game/04_cliffside/greenbooster_dash", booster.Position);
            if (booster.red)
            {
                booster.loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
                booster.loopingSfx.DisposeOnTransition = false;
            }

            bool tag1 = booster.Ch9HubBooster && direction.Y < 0f,
                tag2 = booster.allowDashout,
                locked = tag1 || !tag2;
            if (locked)
            {
                bool flag = true;
                List<LockBlock> list = booster.Scene.Entities.FindAll<LockBlock>();
                if (list.Count > 0)
                {
                    foreach (LockBlock item in list)
                    {
                        if (!item.UnlockingRegistered)
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    booster.Ch9HubTransition = true;
                    booster.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, [MethodImpl(MethodImplOptions.NoInlining)] () =>
                    {
                        booster.Add(new SoundSource("event:/new_content/timeline_bubble_to_remembered")
                        {
                            DisposeOnTransition = false
                        });
                    }, 2f, start: true));
                }
            }

            booster.BoostingPlayer = true;
            booster.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
            booster.sprite.Play("spin");
            booster.sprite.FlipX = player.Facing == Facings.Left;
            booster.outline.Visible = true;
            booster.wiggler.Start();
            booster.dashRoutine.Replace(booster.BoostRoutine(player, direction));
        }
        else
        {
            orig(self, player, direction);
        }
        
    }

    private static void ILHookDashCoroutine(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(0.25f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(defaultHoldTime =>
            {
                Scene scene = Engine.Scene;
                Player player = scene.Tracker.GetEntity<Player>();
                if (player != null && player.CurrentBooster is CustomBooster booster)
                    return booster.holdTime;
                return defaultHoldTime;
            });
        }
    }

    private static void PlayerOnBoostUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        MethodInfo moveToYMethod = typeof(Actor).GetMethod("MoveToY", BindingFlags.Instance | BindingFlags.Public);
        if (cursor.TryGotoNext(ins => ins.MatchCall(moveToYMethod)
            ))
        {
            cursor.Index += 1;
            ILLabel label = cursor.DefineLabel();

            cursor.EmitDelegate(() =>
            {
                Player player = Engine.Scene.Tracker.GetEntity<Player>();
                if (player != null && player.CurrentBooster is CustomBooster customBooster)
                {
                    return customBooster.DisableFastBubble ? 1 : 0;
                }

                return 0;
            });
            cursor.EmitBrfalse(label);
            cursor.EmitLdcI4(Player.StBoost);
            cursor.EmitRet();

            cursor.MarkLabel(label);
        }
    }

    private static void Player_RedDashHook(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        if(c.TryGotoNext(ins => ins.MatchLdcR4(240f)))
        {
            c.Index += 1;

            c.EmitDelegate<Func<float, float>>(fallback =>
            {
                if(PUt.TryGetPlayer(out var p))
                {
                    if(p.CurrentBooster is CustomBooster b)
                    {
                        return b.redBoostMovingSpeed;
                    }
                }
                return fallback;
            });
        }
    }

    private static void Player_GreenDashHook(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        if (c.TryGotoNext(ins => ins.MatchLdcR4(240f)))
        {
            c.Index += 1;

            c.EmitDelegate<Func<float, float>>(fallback =>
            {
                if (PUt.TryGetPlayer(out var p))
                {
                    if (p.CurrentBooster is CustomBooster b)
                    {
                        return b.greenBoostMovingSpeed;
                    }
                }
                return fallback;
            });
        }
    }

    private static void Player_Boost(On.Celeste.Player.orig_Boost orig, Player self, Booster booster)
    {
        TempCurrentBooster = booster;
        orig(self, booster);
        TempCurrentBooster = null;
    }

    private static void Player_RedBoost(On.Celeste.Player.orig_RedBoost orig, Player self, Booster booster)
    {
        TempCurrentBooster = booster;
        orig(self, booster);
        TempCurrentBooster = null;
    }

    // Code setup from Eevee Helper Patient Booster
    public static Booster TempCurrentBooster = null;
    private static ILHook origBoostBeginHook;

    public Vector2 recordedSpeed = Vector2.Zero;

    private static void Player_BoostBegin(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("RefillStamina")))
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed IL Hook for Booster.BoostBegin (RefillStamina)");
            return;
        }

        var afterRefillsLabel = cursor.MarkLabel();

        if (!cursor.TryGotoPrev(MoveType.AfterLabel, instr => instr.MatchCallvirt<Player>("RefillDash")))
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed IL Hook for Booster.BoostBegin (RefillDash)");
            return;
        }

        var continueLabel = cursor.DefineLabel();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<Player, bool>>(player =>
        {
            if (TempCurrentBooster is CustomBooster myBooster)
            {
                myBooster.recordedSpeed = player.Speed;
                
                // Insert Stamina and Dashes logic here
                if (myBooster.setDashes)
                {
                    player.Dashes = Math.Max(myBooster.setDash, 0);
                }
                else
                {
                    if (player.Dashes < myBooster.setDash)
                    {
                        player.Dashes = myBooster.setDash;
                    }
                    else if (myBooster.setDash < 0)
                    {
                        player.Dashes = Math.Max(player.Dashes - myBooster.setDash, 0);
                    }
                }

                if (myBooster.setupStamina)
                {
                    player.Stamina = Math.Max(myBooster.setStamina, 0);
                }
                else
                {
                    if (player.Stamina < myBooster.setStamina)
                    {
                        player.Stamina = myBooster.setStamina;
                    }
                    else if (myBooster.setStamina < 0)
                    {
                        player.Stamina = Math.Max(player.Stamina - myBooster.setStamina, 0);
                    }
                }

                return true;
            }

            return false;
        });
        cursor.Emit(OpCodes.Brfalse, continueLabel);
        cursor.Emit(OpCodes.Pop);
        cursor.Emit(OpCodes.Br, afterRefillsLabel);
        cursor.MarkLabel(continueLabel);
    }

    private static void Booster_PlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
    {
        orig(self);

        if (self is CustomBooster myBooster)
        {
            myBooster.respawnTimer = myBooster.tr;
            if (myBooster.onlyOnce)
            {
                myBooster.RemoveSelf();
            }
        }
    }

    private static IEnumerator Booster_BoostRoutine(On.Celeste.Booster.orig_BoostRoutine orig, Booster self, Player player, Vector2 dir)
    {
        if (self is CustomBooster myBooster)
        {
            float angle = (-dir).Angle();
            // angle calculation, left is 0, right is PI, topright +, bottomright -
            while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && myBooster.BoostingPlayer)
            {
                myBooster.sprite.RenderPosition = player.Center + playerOffset;
                myBooster.loopingSfx.Position = myBooster.sprite.Position;
                if (myBooster.Scene.OnInterval(0.02f))
                {
                    ParticleType particleType = self is CustomBooster booster && booster.burstParticleColorOverride ? booster.customBurstParticleType : myBooster.particleType;
                    (myBooster.Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
                }

                yield return null;
            }

            myBooster.PlayerReleased();

            if (myBooster.rememberSpeed)
            {
                player.Speed = dir * myBooster.recordedSpeed.Length() * myBooster.outSpeed;
            }
            else
            {
                player.Speed *= myBooster.outSpeed;
            }

            if (player.StateMachine.State == 4)
            {
                myBooster.sprite.Visible = false;
            }

            while (myBooster.SceneAs<Level>().Transitioning)
            {
                yield return null;
            }

            myBooster.Tag = 0;
        }
        else
        {
            yield return new SwapImmediately(orig(self, player, dir));
        }
    }

    public static void Booster_AppearParticles(On.Celeste.Booster.orig_AppearParticles orig, Booster self)
    {
        if(self is CustomBooster booster)
        {
            ParticleSystem particlesBG = MapProcessor.level.ParticlesBG;
            for (int i = 0; i < 360; i += 30)
            {
                particlesBG.Emit(booster.appearParticleColorOverride ? booster.customAppearParticleType : P_Appear, 1, booster.Center, Vector2.One * 2f, (float)i * (MathF.PI / 180f));
            }
        }
        else
        {
            orig(self);
        }
    }
    private static int PlayerOnBoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self)
    {
        if (self.CurrentBooster is CustomBooster { playerFollow: true } booster)
            self.boostTarget = booster.Center;
        return orig(self);
    }

    public override void Render()
    {
        base.Render();

        if (onlyOnce)
        {
            ActiveFont.Draw("!", Position, new Vector2(0.5f, 0.5f), new Vector2(0.35f, 0.35f), Color.Red);
        }
    }

    // Usable but unecessary hooks
    /*
    public static void Booster_Appear(On.Celeste.Booster.orig_Appear orig, Booster self)
    {
        orig(self);
        if(self is CustomBooster myBooster)
        {

        }

    }

    private static bool RefillD(On.Celeste.Player.orig_RefillDash orig, Player self)
    {
        return orig(self);
    }
    private static void RefillS(On.Celeste.Player.orig_RefillStamina orig, Player self)
    {
        orig(self);
    }
    */
}
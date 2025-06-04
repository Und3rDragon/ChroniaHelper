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

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomBooster")]
public class CustomBooster : Booster
{
    public static void Load()
    {
        On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
        On.Celeste.Booster.BoostRoutine += Booster_BoostRoutine;
        IL.Celeste.Player.BoostBegin += Player_BoostBegin;

        On.Celeste.Player.Boost += Player_Boost;
        On.Celeste.Player.RedBoost += Player_RedBoost;
        origBoostBeginHook = new ILHook(typeof(Player).GetMethod("orig_BoostBegin", BindingFlags.NonPublic | BindingFlags.Instance), Player_BoostBegin);

        // Usable but unecessary

        // On.Celeste.Player.RefillDash += RefillD;
        // On.Celeste.Player.RefillStamina += RefillS;
    }

    public static void Unload()
    {
        On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
        On.Celeste.Booster.BoostRoutine -= Booster_BoostRoutine;
        IL.Celeste.Player.BoostBegin -= Player_BoostBegin;

        On.Celeste.Player.Boost -= Player_Boost;
        On.Celeste.Player.RedBoost -= Player_RedBoost;
        origBoostBeginHook?.Dispose();
        origBoostBeginHook = null;

        // Usable but unecessary

        // On.Celeste.Player.RefillDash -= RefillD;
        // On.Celeste.Player.RefillStamina -= RefillS;
    }

    private float appearTime, loopTime, insideTime, spinTime, popTime, tr;

    private string outlineDir;

    private int setDash, setStamina;

    private bool setDashes, setupStamina;

    private float hbr, hbx, hby;

    private float moveSpeed, outSpeed;

    private bool setOutSpeed;

    private Color color;
    private ParticleType customBurstParticleType;
    private bool burstParticleColorOverride;

    public CustomBooster(EntityData data, Vector2 position, bool red)
        : base(position, red)
    {
        base.Depth = data.Int("depth", -8500);
        Ch9HubBooster = data.Bool("ch9_hub_booster", false);

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
        //moveSpeed = Math.Abs(data.Float("moveSpeed", 1f));
        //setOutSpeed = data.Bool("setOutSpeed", false);

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
                // Modify move speed
                //player.Speed *= myBooster.moveSpeed;


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

            player.Speed *= myBooster.outSpeed;

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
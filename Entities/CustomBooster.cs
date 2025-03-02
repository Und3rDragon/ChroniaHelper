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

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomBooster")]

public class CustomBooster : Booster
{
    public static void Load()
    {
        On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
        On.Celeste.Booster.BoostRoutine += Booster_BoostRoutine;
        On.Celeste.Player.BoostBegin += Player_BoostBegin;
        

        // Usable but unecessary

        // On.Celeste.Player.RefillDash += RefillD;
        // On.Celeste.Player.RefillStamina += RefillS;
    }

    public static void Unload()
    {
        On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
        On.Celeste.Booster.BoostRoutine -= Booster_BoostRoutine;
        On.Celeste.Player.BoostBegin += Player_BoostBegin;

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
        outSpeed = Math.Abs(data.Float("outSpeedMultiplier", 1f));
        //setOutSpeed = data.Bool("setOutSpeed", false);
        // process old data
        if (!string.IsNullOrEmpty(data.Attr("setOutSpeed")))
        {
            bool setOutSpeed = data.Bool("setOutSpeed", false);
            outSpeed = setOutSpeed ? outSpeed : 1f;
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
        Add(wiggler = Wiggler.Create(0.5f, 4f, (float f) =>
        {
            sprite.Scale = Vector2.One * (1f + f * 0.25f);
        }));
        Add(dashRoutine = new Coroutine(removeOnComplete: false));
        Add(dashListener = new DashListener());
        Add(new MirrorReflection());
        Add(loopingSfx = new SoundSource());
        dashListener.OnDash = OnPlayerDashed;
        particleType = (red ? P_BurstRed : P_Burst);

        tr = data.Float("respawnTime", 1f);
        outlineDir = data.Attr("outlineDirectory", "objects/ChroniaHelper/customBooster/outline");

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

    private static void Player_BoostBegin(On.Celeste.Player.orig_BoostBegin orig, Player self)
    {
        ChroniaHelperModule.Session.PlayerDashesBeforeEnteringBooster = self.Dashes;
        ChroniaHelperModule.Session.PlayerStaminaBeforeEnteringBooster = self.Stamina;
        orig(self);
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
        if (self is CustomBooster myBooster) {

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
                    (myBooster.Scene as Level).ParticlesBG.Emit(myBooster.particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
                }

                yield return null;
            }

            myBooster.PlayerReleased();

            // Set or Refill Player Dashes or Stamina
            if (myBooster.setDashes)
            {
                player.Dashes = myBooster.setDash;
            }
            else
            {
                if (ChroniaHelperModule.Session.PlayerDashesBeforeEnteringBooster < myBooster.setDash)
                {
                    player.Dashes = myBooster.setDash;
                }
                else
                {
                    player.Dashes = ChroniaHelperModule.Session.PlayerDashesBeforeEnteringBooster;
                }
            }

            if (myBooster.setupStamina)
            {
                player.Stamina = myBooster.setStamina;
            }
            else
            {
                if (ChroniaHelperModule.Session.PlayerStaminaBeforeEnteringBooster < myBooster.setStamina)
                {
                    player.Stamina = myBooster.setStamina;
                }
                else
                {
                    player.Stamina = ChroniaHelperModule.Session.PlayerStaminaBeforeEnteringBooster;
                }
            }

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
    private static void OnPlayer(On.Celeste.Booster.orig_OnPlayer orig, Booster self,Player player)
    {
        if (self is CustomBooster myBooster) {
            
        }
        orig(self, player);
    }

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
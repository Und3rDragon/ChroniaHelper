using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/Refill")]
public class Refill : Entity
{

    private string spritePath;

    private Color spriteColor;

    private float respawnTime;

    private float freeze;

    private FewerMode fewerMode;

    private ResetMode resetMode;

    private int fewerDashes;

    private int rewerDashes;

    private float fewerStamina;

    private float resetStamina;

    private string touchSound;

    private string respawnSound;

    private Color outlineColor;

    private int outlineWidth;

    private float bloomAlpha;

    private float bloomRadius;

    private Color particleShatterColor1;

    private Color particleShatterColor2;

    private Color ParticleRegenColor1;

    private Color particleRegenColor2;

    private Color particleGlowColor1;

    private Color particleGlowColor2;

    private float waveFrequency;

    private float waveRadius;

    private WaveMode waveMode;

    private bool onlyOnce;

    protected bool twoDashes
    {
        get;
        private set;
    }

    private static string RefillSpritePath;

    private static string RefillTwoSpritePath;

    private static string SingleRefillOutlineSpritePath;

    protected Image outline;

    protected Sprite idle;

    protected Sprite flash;

    protected bool single;

    protected Image singleSprite;

    private static string RefillTouchSoundEvent;

    private static string RefillTwoTouchSoundEvent;

    private static string RefillRespawnSoundEvent;

    private static string RefillTwoRespawnSoundEvent;

    protected Vector2 centerPosition;

    private Level level;

    public static Color OneDashesParticleShatterColor;

    public static Color OneDashesParticleRegenAndGlowColor;

    public static Color TwoDashesParticleShatterColor;

    public static Color TwoDashesParticleRegenAndGlowColor;

    private Wiggler wiggler;

    protected BloomPoint bloomPoint;

    protected VertexLight vertexLight;

    private SineWave sineWave;

    protected Vector2 waveMoveOffset;

    private float respawnTimer;

    private ParticleType particleShatter;

    private ParticleType particleRegen;

    private ParticleType particleGlow;

    private float rotateCounter;

    private float rotateSpriteCenterOffset;

    private static Random Random = new Random();

    private float idleDelay, flashDelay;

    private string flagOnCollected;

    static Refill()
    {
        Refill.RefillSpritePath = "objects/refill/";
        Refill.RefillTwoSpritePath = "objects/refillTwo/";
        Refill.SingleRefillOutlineSpritePath = "objects/ChroniaHelper/refill/outline";
        Refill.RefillTouchSoundEvent = "event:/game/general/diamond_touch";
        Refill.RefillTwoTouchSoundEvent = "event:/new_content/game/10_farewell/pinkdiamond_touch";
        Refill.RefillRespawnSoundEvent = "event:/game/general/diamond_return";
        Refill.RefillTwoRespawnSoundEvent = "event:/new_content/game/10_farewell/pinkdiamond_return";
        Refill.OneDashesParticleShatterColor = Calc.HexToColor("d3ffd4");
        Refill.OneDashesParticleRegenAndGlowColor = Calc.HexToColor("a5fff7");
        Refill.TwoDashesParticleShatterColor = Calc.HexToColor("ffd3f9");
        Refill.TwoDashesParticleRegenAndGlowColor = Calc.HexToColor("ffa5aa");
    }

    private enum FewerMode
    {
        None,
        AnyOne,
        Dashes,
        Stamina,
        All
    }

    private enum ResetMode
    {
        None,
        Dashes,
        Stamina,
        All,
        RandomAnyOne,
        RandomDashes,
        RandomStamina,
        RandomAll
    }

    private enum WaveMode
    {
        None,
        Vertical,
        Horizontal,
        ForwardSlash,
        BackSlash,
        Clockwise,
        CounterClockwise
    }

    public Refill(Vector2 position, EntityData data) : base(position)
    {
        this.twoDashes = data.Bool("twoDashes", false);
        this.spritePath = data.Attr("sprite", null);
        this.spriteColor = data.HexColor("spriteColor", Color.White);
        this.respawnTime = data.Float("respawnTime", 2.5F);
        if (this.respawnTime <= 0)
        {
            this.respawnTime = Engine.DeltaTime;
        }
        this.freeze = data.Int("freeze", 3) / 60F;
        this.fewerMode = data.Enum<FewerMode>("fewerMode", FewerMode.AnyOne);
        this.resetMode = data.Enum<ResetMode>("resetMode", ResetMode.All);
        this.fewerDashes = data.Int("fewerDashes", -1);
        if (this.fewerDashes < 0)
        {
            this.fewerDashes = !this.twoDashes ? -1 : 2;
        }
        this.rewerDashes = data.Int("resetDashes", -1);
        if (this.rewerDashes < 0)
        {
            this.rewerDashes = !this.twoDashes ? -1 : 2;
        }
        this.fewerStamina = data.Float("fewerStamina", 20F);
        this.resetStamina = data.Float("resetStamina", 110F);
        this.touchSound = !string.IsNullOrWhiteSpace(data.Attr("touchSound", null)) ? data.Attr("touchSound") : (!this.twoDashes ? Refill.RefillTouchSoundEvent : Refill.RefillTwoTouchSoundEvent);
        this.respawnSound = !string.IsNullOrWhiteSpace(data.Attr("respawnSound", null)) ? data.Attr("respawnSound") : (!this.twoDashes ? Refill.RefillRespawnSoundEvent : Refill.RefillTwoRespawnSoundEvent);
        this.outlineColor = data.HexColor("outlineColor", Color.Black);
        this.outlineWidth = data.Int("outlineWidth", 1);
        this.bloomAlpha = data.Float("bloomAlpha", 0.8F);
        this.bloomRadius = data.Float("bloomRadius", 16F);
        this.particleShatterColor1 = !string.IsNullOrWhiteSpace(data.Attr("particleShatterColor1", null)) ? data.HexColor("particleShatterColor1") : (!this.twoDashes ? Refill.OneDashesParticleShatterColor : Refill.TwoDashesParticleShatterColor);
        this.particleShatterColor2 = !string.IsNullOrWhiteSpace(data.Attr("particleShatterColor2", null)) ? data.HexColor("particleShatterColor2") : (!this.twoDashes ? Refill.OneDashesParticleShatterColor : Refill.TwoDashesParticleShatterColor);
        this.ParticleRegenColor1 = !string.IsNullOrWhiteSpace(data.Attr("particleRegenColor1", null)) ? data.HexColor("particleRegenColor1") : (!this.twoDashes ? Refill.OneDashesParticleRegenAndGlowColor : Refill.TwoDashesParticleShatterColor);
        this.particleRegenColor2 = !string.IsNullOrWhiteSpace(data.Attr("particleRegenColor2", null)) ? data.HexColor("particleRegenColor2") : (!this.twoDashes ? Refill.OneDashesParticleRegenAndGlowColor : Refill.TwoDashesParticleShatterColor);
        this.particleGlowColor1 = !string.IsNullOrWhiteSpace(data.Attr("particleGlowColor1", null)) ? data.HexColor("particleGlowColor1") : (!this.twoDashes ? Refill.OneDashesParticleRegenAndGlowColor : Refill.TwoDashesParticleShatterColor);
        this.particleGlowColor2 = !string.IsNullOrWhiteSpace(data.Attr("particleGlowColor2", null)) ? data.HexColor("particleGlowColor2") : (!this.twoDashes ? Refill.OneDashesParticleRegenAndGlowColor : Refill.TwoDashesParticleShatterColor);
        this.waveFrequency = data.Float("waveFrequency", 0.6F);
        this.waveRadius = data.Float("waveRadius", 2F);
        this.waveMode = data.Enum<WaveMode>("waveMode", WaveMode.Vertical);
        base.Depth = data.Int("depth", -100);
        this.onlyOnce = data.Bool("onlyOnce", false);
        base.Collider = new Hitbox(16F, 16F, -8F, -8F);
        base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
        this.centerPosition = base.Position;
        this.idleDelay = data.Float("idleAnimInterval") <= 0 ? 0.1f : data.Float("idleAnimInterval");
        this.flashDelay = data.Float("flashAnimInterval") <= 0 ? 0.05f : data.Float("flashAnimInterval");
        flagOnCollected = data.Attr("flagOnCollected", "Chronia_Refill_Flag_On_Collected");

        this.Setup();
    }

    public Refill(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    private void Setup()
    {
        if (string.IsNullOrWhiteSpace(this.spritePath))
        {
            this.spritePath = !this.twoDashes ? Refill.RefillSpritePath : Refill.RefillTwoSpritePath;
        }
        this.particleShatter = new ParticleType(global::Celeste.Refill.P_Shatter)
        {
            Color = this.particleShatterColor1,
            Color2 = this.particleShatterColor2
        };
        this.particleRegen = new ParticleType(global::Celeste.Refill.P_Regen)
        {
            Color = this.ParticleRegenColor1,
            Color2 = this.particleRegenColor2
        };
        this.particleGlow = new ParticleType(global::Celeste.Refill.P_Glow)
        {
            Color = this.particleGlowColor1,
            Color2 = this.particleGlowColor2
        };
        if (!(this.single = !this.spritePath.EndsWith("/")))
        {
            base.Add(this.outline = new Image(GFX.Game[spritePath + "outline"]));
            this.outline.CenterOrigin();
            this.outline.Visible = false;
            base.Add(this.idle = new Sprite(GFX.Game, spritePath + "idle"));
            this.idle.AddLoop("idle", "", this.idleDelay);
            this.idle.Play("idle", false, false);
            this.idle.Color = this.spriteColor;
            this.idle.CenterOrigin();
            base.Add(this.flash = new Sprite(GFX.Game, spritePath + "flash"));
            this.flash.Add("flash", "", this.flashDelay);
            this.flash.OnFinish = delegate (string anim)
            {
                this.flash.Visible = false;
            };
            this.flash.CenterOrigin();
            this.rotateSpriteCenterOffset = this.idle.Width / 2F + this.idle.Width / 4F;
        }
        else
        {
            base.Add(this.outline = new Image(GFX.Game[Refill.SingleRefillOutlineSpritePath]));
            this.outline.CenterOrigin();
            this.outline.Visible = false;
            base.Add(this.singleSprite = new Image(GFX.Game[spritePath]));
            this.singleSprite.Color = this.spriteColor;
            this.singleSprite.CenterOrigin();
            this.rotateSpriteCenterOffset = this.singleSprite.Width / 2F + this.singleSprite.Width / 4F;
        }
        base.Add(this.wiggler = Wiggler.Create(1F, 4F, delegate (float v)
        {
            if (!this.single)
            {
                this.idle.Scale = (this.flash.Scale = Vector2.One * (1F + v * 0.02F));
            }
            else
            {
                this.singleSprite.Scale = Vector2.One * (1F + v * 0.02F);
            }
        }, false, false));
        base.Add(new MirrorReflection());
        base.Add(this.bloomPoint = new BloomPoint(this.bloomAlpha, this.bloomRadius));
        base.Add(this.vertexLight = new VertexLight(Color.White, 1F, 16, 48));
        base.Add(this.sineWave = new SineWave(this.waveFrequency, 0F));
        if (this.waveMode != WaveMode.None)
        {
            this.waveMoveOffset = Vector2.One;
            this.sineWave.Randomize();
            this.WaveMove();
        }
        else
        {
            base.Remove(sineWave);
            this.sineWave.Counter = 0F;
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
        if (this.respawnTimer > 0F)
        {
            this.respawnTimer -= Engine.DeltaTime;
        }
        if (this.respawnTimer <= 0F)
        {
            this.Respawn();
            if (base.Scene.OnInterval(0.1F))
            {
                this.level.ParticlesFG.Emit(this.particleGlow, 1, this.centerPosition, Vector2.One * 5F);
            }
        }
        if (this.waveMode != WaveMode.None)
        {
            this.WaveMove();
        }
        this.vertexLight.Alpha = Calc.Approach(this.vertexLight.Alpha, (!this.single ? this.idle.Visible : this.singleSprite.Visible) ? 1F : 0F, 4F * Engine.DeltaTime);
        this.bloomPoint.Alpha = this.vertexLight.Alpha * 0.8F;
        if (base.Scene.OnInterval(2F) && !this.single && this.idle.Visible)
        {
            this.flash.Play("flash", true, false);
            this.flash.Visible = true;
        }
    }

    private void Respawn()
    {
        if (this.Collidable)
        {
            return;
        }
        this.Collidable = true;
        if (!this.single)
        {
            this.idle.Visible = true;
        }
        else
        {
            this.singleSprite.Visible = true;
        }
        this.outline.Visible = false;
        this.wiggler.Start();
        if (!string.IsNullOrEmpty(this.respawnSound))
        {
            Audio.Play(this.respawnSound, this.centerPosition);
        }
        this.level.ParticlesFG.Emit(this.particleRegen, 16, this.centerPosition, Vector2.One * this.waveRadius);
    }

    private void WaveMove()
    {
        float xPosition = this.bloomPoint.X;
        float yPosition = this.bloomPoint.Y;
        float waveValue = this.sineWave.Value * this.waveRadius;
        switch (this.waveMode)
        {
            case WaveMode.Vertical:
                yPosition = waveValue + this.waveMoveOffset.Y;
                break;
            case WaveMode.Horizontal:
                xPosition = waveValue + this.waveMoveOffset.X;
                break;
            case WaveMode.ForwardSlash:
                xPosition = -waveValue + this.waveMoveOffset.X;
                yPosition = waveValue + this.waveMoveOffset.Y;
                break;
            case WaveMode.BackSlash:
                xPosition = waveValue + this.waveMoveOffset.X;
                yPosition = waveValue + this.waveMoveOffset.Y;
                break;
            case WaveMode.Clockwise:
            case WaveMode.CounterClockwise:
                this.rotateCounter += this.waveFrequency * Engine.DeltaTime * (this.waveMode == WaveMode.Clockwise ? -2F : 2F);
                if (this.rotateCounter >= 4)
                {
                    this.rotateCounter = 0;
                }
                double radian = (this.rotateCounter / 180.0) * (270 + this.rotateSpriteCenterOffset);
                xPosition = (float) (this.waveRadius * Math.Sin(radian)) + this.waveMoveOffset.X;
                yPosition = (float) (this.waveRadius * Math.Cos(radian)) + this.waveMoveOffset.Y;
                break;
            default:
                return;
        }
        if (!this.single)
        {
            this.flash.X = (this.idle.X = (this.bloomPoint.X = xPosition));
            this.flash.Y = (this.idle.Y = (this.bloomPoint.Y = yPosition));
        }
        else
        {
            this.singleSprite.X = (this.bloomPoint.X = xPosition);
            this.singleSprite.Y = (this.bloomPoint.Y = yPosition);
        }
    }

    public override void Render()
    {
        if (!this.single && this.idle.Visible)
        {
            this.idle.DrawOutline(this.outlineColor, this.outlineWidth);
        }
        if (this.single && this.singleSprite.Visible)
        {
            this.singleSprite.DrawOutline(this.outlineColor, this.outlineWidth);
        }
        base.Render();
        this.RenderAfter(this.respawnTimer);
    }

    protected virtual void RenderAfter(float respawnTimer)
    {
    }

    private void OnPlayer(Player player)
    {
        if (this.fewerMode == FewerMode.None || this.resetMode == ResetMode.None)
        {
            return;
        }
        if (this.fewerDashes == -1)
        {
            this.fewerDashes = player.MaxDashes;
        }
        if (this.rewerDashes == -1)
        {
            this.rewerDashes = player.MaxDashes;
        }
        if (!this.fewerDictionary[this.fewerMode](player.Dashes, player.Stamina, this.fewerDashes, this.fewerStamina))
        {
            return;
        }
        this.resetDictionary[this.resetMode](ref player.Dashes, ref player.Stamina, this.rewerDashes, this.resetStamina);
        if (!string.IsNullOrEmpty(this.respawnSound))
        {
            Audio.Play(this.touchSound, this.centerPosition);
        }
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        this.Collidable = false;
        global::Celeste.Celeste.Freeze(this.freeze);
        base.Add(new Coroutine(this.RefillRoutine(player), true));
        this.respawnTimer = this.respawnTime;
        SceneAs<Level>().Session.SetFlag(flagOnCollected);
    }

    private IEnumerator RefillRoutine(Player player)
    {
        // Migrated to Line 461
        // global::Celeste.Celeste.Freeze(this.freeze);

        yield return null;
        this.level.Shake(0.3F);
        if (!this.single)
        {
            this.idle.Visible = (this.flash.Visible = false);
        }
        else
        {
            this.singleSprite.Visible = false;
        }
        if (!this.onlyOnce)
        {
            this.outline.Visible = true;
        }
        yield return 0.05F;
        float num = player.Speed.Angle();
        this.level.ParticlesFG.Emit(this.particleShatter, 5, this.centerPosition, Vector2.One * 4F, num - 1.5707964F);
        this.level.ParticlesFG.Emit(this.particleShatter, 5, this.centerPosition, Vector2.One * 4F, num + 1.5707964F);
        SlashFx.Burst(this.centerPosition, num);
        if (this.onlyOnce)
        {
            base.RemoveSelf();
        }
        yield break;
    }

    private delegate bool FewerOperation(int playerDashes, float playerStamina, int fewerDashes, float fewerStamina);

    private delegate void ResetOperation(ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina);

    private Dictionary<FewerMode, FewerOperation> fewerDictionary = new Dictionary<FewerMode, FewerOperation>()
    {
        {
            FewerMode.AnyOne, (int playerDashes, float playerStamina, int fewerDashes, float fewerStamina) => ((playerDashes < fewerDashes) || (playerStamina < fewerStamina))
        },
        {
            FewerMode.Dashes, (int playerDashes, float playerStamina, int fewerDashes, float fewerStamina) => (playerDashes < fewerDashes)
        },
        {
            FewerMode.Stamina, (int playerDashes, float playerStamina, int fewerDashes, float fewerStamina) => (playerStamina < fewerStamina)
        },
        {
            FewerMode.All, (int playerDashes, float playerStamina, int fewerDashes, float fewerStamina) => ((playerDashes < fewerDashes) && (playerStamina < fewerStamina))
        }
    };

    private Dictionary<ResetMode, ResetOperation> resetDictionary = new Dictionary<ResetMode, ResetOperation>()
    {
        {
            ResetMode.Dashes, (ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina) => playerDashes = resetDashes
        },
        {
            ResetMode.Stamina, (ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina) => playerStamina = resetStamina
        },
        {
            ResetMode.All, (ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina) =>
            {
                playerDashes = resetDashes;
                playerStamina = resetStamina;
            }
        },
        {
            ResetMode.RandomAnyOne, (ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina) =>
            {
                if (Refill.Random.Next(0, 2) == 0)
                {
                    playerDashes = Refill.Random.Next(0, resetDashes + 1);
                }
                else
                {
                    playerStamina = Refill.Random.NextFloat() * (resetStamina + 0.01F);
                }
            }
        },
        {
            ResetMode.RandomDashes, (ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina) => playerDashes = Refill.Random.Next(0, resetDashes + 1)
        },
        {
            ResetMode.RandomStamina, (ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina) => playerStamina = Refill.Random.NextFloat() * (resetStamina + 0.01F)
        },
        {
            ResetMode.RandomAll, (ref int playerDashes, ref float playerStamina, int resetDashes, float resetStamina) =>
            {
                playerDashes = Refill.Random.Next(0, resetDashes + 1);
                playerStamina = Refill.Random.NextFloat() * (resetStamina + 0.01F);
            }
        }
    };

}

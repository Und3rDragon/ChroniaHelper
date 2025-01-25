using Celeste.Mod.Entities;
using System.Runtime.CompilerServices;
using System;
using System.Collections;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/DecreaseRefill")]
public class DecreaseRefill : Entity
{
    public static ParticleType P_Shatter;

    public static ParticleType P_Regen;

    public static ParticleType P_Glow;

    public static ParticleType P_ShatterTwo;

    public static ParticleType P_RegenTwo;

    public static ParticleType P_GlowTwo;

    private Sprite sprite;

    private Sprite flash;

    private Image outline;

    private Wiggler wiggler;

    private BloomPoint bloom;

    private VertexLight light;

    private Level level;

    private SineWave sine;

    private bool twoDashes;

    private bool oneUse;

    private ParticleType p_shatter;

    private ParticleType p_regen;

    private ParticleType p_glow;

    private float respawnTimer;

    private float lonnRespawnTimer;

    private float lonnFreezeFrameLength;

    private int lonnDepth;

    private int minusDashes;

    private bool minusStamina;

    private bool minusAllDashes;

    private bool tradeForStamina;


    public DecreaseRefill(Vector2 position, EntityData data) : base(position)
    {
        // Logger.Log(LogLevel.Info, "ChroniaHelper", "测试一下构造函数");
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);

        this.lonnRespawnTimer = data.Float("respawnTimer");
        if (this.lonnRespawnTimer <= 0.02f) { this.lonnRespawnTimer = 0.02f; }
        this.lonnFreezeFrameLength = data.Float("freezeFrameLength");
        if (this.lonnFreezeFrameLength <= 0.02f) { this.lonnFreezeFrameLength = 0.02f; }
        this.lonnDepth = data.Int("depth");
        this.minusDashes = data.Int("dashes");
        this.minusStamina = data.Bool("decreaseStamina");
        this.minusAllDashes = data.Bool("clearAllDashes");
        this.tradeForStamina = data.Bool("tradeDashesToStamina");


        Add(new PlayerCollider(OnPlayer));
        this.twoDashes = data.Bool("twoDashes", false);
        this.oneUse = data.Bool("oneUse", false);
        string text = data.Attr("sprites", "objects/refill/");

        //动画速度提取
        string lonnFPS = data.Attr("frameRates", "12, 12");
        int indexOfFPS = lonnFPS.IndexOf(',');
        string idleRate = lonnFPS.Substring(0, indexOfFPS);
        string flashRate = lonnFPS.Substring(indexOfFPS + 2);
        float idleSpeed;
        float flashSpeed;
        if (float.Parse(idleRate) >= 60f) { idleSpeed = 1 / 60f; }
        else { idleSpeed = 1 / float.Parse(idleRate); }
        if (float.Parse(flashRate) >= 60f) { flashSpeed = 1 / 60f; }
        else { flashSpeed = 1 / float.Parse(flashRate); }

        /*  原始代码
        if (twoDashes)
        {
            text = "objects/refillTwo/";
            p_shatter = Celeste.Refill.P_ShatterTwo;
            p_regen = Celeste.Refill.P_RegenTwo;
            p_glow = Celeste.Refill.P_GlowTwo;
        }
        else
        {
            text = "objects/refill/";
            p_shatter = Celeste.Refill.P_Shatter;
            p_regen = Celeste.Refill.P_Regen;
            p_glow = Celeste.Refill.P_Glow;
        }
        */
        this.p_shatter = Celeste.Refill.P_Shatter;
        this.p_regen = Celeste.Refill.P_Regen;
        this.p_glow = Celeste.Refill.P_Glow;

        Add(outline = new Image(GFX.Game[text + "outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
        Add(sprite = new Sprite(GFX.Game, text + "idle"));
        sprite.AddLoop("idle", "", idleSpeed);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(flash = new Sprite(GFX.Game, text + "flash"));
        flash.Add("flash", "", flashSpeed);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        flash.CenterOrigin();
        Add(wiggler = Wiggler.Create(1f, 4f, [MethodImpl(MethodImplOptions.NoInlining)] (float v) =>
        {
            sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        base.Depth = -100;
    }

    public DecreaseRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {

    }


    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }
    public override void Update()
    {
        //Logger.Log(LogLevel.Info, "Function Detection", "Detected");
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (base.Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
        }

        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        if (base.Scene.OnInterval(2f) && sprite.Visible)
        {
            flash.Play("flash", restart: true);
            flash.Visible = true;
        }
    }
    private void Respawn()
    {
        if (!Collidable)
        {
            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            base.Depth = -100;
            wiggler.Start();
            Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
        }
    }
    private void UpdateY()
    {
        Sprite obj = flash;
        Sprite obj2 = sprite;
        float num2 = (bloom.Y = sine.Value * 2f);
        float y = (obj2.Y = num2);
        obj.Y = y;
    }
    public override void Render()
    {
        if (sprite.Visible)
        {
            sprite.DrawOutline();
        }

        base.Render();
    }


    private void OnPlayer(Player player)
    {
        int num = player.MaxDashes;
        if (twoDashes)
        {
            num = 2;
        }
        if (player.Dashes > 0 || player.Stamina > 0f)
        {
            if (this.tradeForStamina == false)
            {
                //正常消耗
                if (this.minusAllDashes) { player.Dashes = 0; }
                else
                {
                    player.Dashes -= this.minusDashes;
                    if (player.Dashes <= 0) { player.Dashes = 0; }
                }
                if (this.minusStamina) { player.Stamina = 0f; }
            }
            else
            {
                //冲刺数量换体力
                if (this.minusAllDashes) { 
                    player.Dashes = 0; 
                    player.RefillStamina(); 
                }
                else
                {
                    player.Dashes -= this.minusDashes;
                    if (player.Dashes <= 0) { player.Dashes = 0; }
                    player.RefillStamina();
                }
            }
            Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = this.lonnRespawnTimer;
        }
    }
    private IEnumerator RefillRoutine(Player player)
    {
        Celeste.Celeste.Freeze(lonnFreezeFrameLength);
        yield return null;
        level.Shake();
        Sprite obj = sprite;
        Sprite obj2 = flash;
        bool visible = false;
        obj2.Visible = false;
        obj.Visible = visible;
        if (!this.oneUse)
        {
            outline.Visible = true;
        }

        Depth = lonnDepth;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (this.oneUse)
        {
            RemoveSelf();
        }
    }
}
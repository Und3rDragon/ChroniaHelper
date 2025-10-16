using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomResortLantern")]
public class CustomResortLantern : Entity
{

    public Image holder;


    public Sprite lantern;


    public float collideTimer;


    public int mult;


    public Wiggler wiggler;


    public VertexLight light;


    public BloomPoint bloom;


    public float alphaTimer;


    public SoundSource sfx;

    public CustomResortLantern(Vector2 position, EntityData data)
        : base(position)
    {
        base.Collider = new Hitbox(8f, 8f, -4f, -4f);
        base.Depth = data.Int("depth", 2000);
        Add(new PlayerCollider(OnPlayer));

        // generate sprite
        string path = data.Attr("spriteDirectory", "objects/resortLantern/");
        path = string.Concat(path.TrimEnd('/'), "/");
        float speed = data.Float("animationInterval", 0.3f);
        // check if holder exists
        if (GFX.Game.HasAtlasSubtextures($"{path}holder"))
        {
            holder = new Image(GFX.Game[$"{path}holder"]);
            holder.CenterOrigin();
            Add(holder);
        }
        // generate main sprite
        lantern = new Sprite(GFX.Game, path);
        string[] framesData = data.Attr("frames", "0,0,1,2,1").Split(',',StringSplitOptions.TrimEntries);
        int[] frames = new int[framesData.Length];
        for(int i = 0; i < framesData.Length; i++)
        {
            frames[i] = framesData[i].ParseInt(0);
        }
        lantern.AddLoop("light", "lantern", speed, frames);
        lantern.Play("light");
        lantern.Origin = new Vector2(7f, 7f);
        lantern.Position = new Vector2(-1f, -5f);
        Add(lantern);
        lightAlpha = Math.Abs(data.Float("lightAlpha", 0.95f));
        bloomAlpha = Math.Abs(data.Float("bloomAlpha", 0.8f));
        flashAlpha = Math.Abs(data.Float("flashAlpha", 0.05f));
        flashFreq = Math.Abs(data.Float("flashFreqMultiplier", 1f));
        float dur = Math.Abs(data.Float("wiggleDuration", 2.5f)),
            freq = Math.Abs(data.Float("wiggleFrequency", 1.2f)),
            bloomRadius = Math.Abs(data.Float("bloomRadius", 8f)),
            maxWiggle = Math.Abs(data.Float("wiggleMaxAmplitute", 30f));
        Color lightColor = Calc.HexToColor(data.Attr("lightColor", "ffffff"));
        int startFade = Math.Abs(data.Int("lightStartFade", 32)),
            endfade = Math.Abs(data.Int("lightEndFade", 64));
        wiggler = Wiggler.Create(dur, freq, (float v) =>
        {
            lantern.Rotation = v * (float)mult * (MathF.PI / 180f) * maxWiggle;
        });
        wiggler.StartZero = true;
        Add(wiggler);
        Add(light = new VertexLight(lightColor, lightAlpha, startFade, endfade));
        Add(bloom = new BloomPoint(bloomAlpha, bloomRadius));
        Add(sfx = new SoundSource());
    }
    private float bloomAlpha, lightAlpha, flashFreq, flashAlpha;

    public CustomResortLantern(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (CollideCheck<Solid>(Position + Vector2.UnitX * 8f))
        {
            holder.Scale.X = -1f;
            lantern.Scale.X = -1f;
            lantern.X += 2f;
        }
    }

    public override void Update()
    {
        base.Update();
        if (collideTimer > 0f)
        {
            collideTimer -= Engine.DeltaTime;
        }

        alphaTimer += Engine.DeltaTime;
        bloom.Alpha = bloomAlpha + (float)Math.Sin(alphaTimer * flashFreq) * flashAlpha;
        light.Alpha = lightAlpha + (float)Math.Sin(alphaTimer * flashFreq) * flashAlpha;
    }

    public void OnPlayer(Player player)
    {
        if (collideTimer <= 0f)
        {
            if (player.Speed != Vector2.Zero)
            {
                sfx.Play("event:/game/03_resort/lantern_bump");
                collideTimer = 0.5f;
                mult = Calc.Random.Choose(1, -1);
                wiggler.Start();
            }
        }
        else
        {
            collideTimer = 0.5f;
        }
    }
}

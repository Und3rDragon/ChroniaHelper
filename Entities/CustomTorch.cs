using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomTorch")]
public class CustomTorch : Entity
{
    ParticleType P_OnLight2;
    public Color color;
    public float alpha;
    public int startFade, endFade;
    public string FlagName;
    public bool lit;
    public bool startLit;
    public bool unlightOnDeath;
    public VertexLight light;
    public BloomPoint bloom;
    public Sprite sprite;
    public Image image;

    public CustomTorch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        startLit = data.Bool("startLit", false);
        unlightOnDeath = data.Bool("unlightOnDeath", false);
        color = ColorUtils.ColorFix(data.Attr("Color", "Cyan"));
        alpha = data.Float("Alpha", 1f);
        FlagName = "ChroniaHelperTorch_" + id.Key;
        if (alpha < 0 || alpha > 1) { alpha = 1f; }
        startFade = Math.Abs(data.Int("startFade", 48));
        endFade = Math.Abs(data.Int("endFade", 64));
        Collider = new Circle(data.Float("RegisterRadius", 4f));
        sprite = GFX.SpriteBank.Create("CustomTorch");
        if (sprite == null) { throw new Exception("CustomTorch Sprite is missing!"); }
        sprite.Color = ColorUtils.ColorFix(data.Attr("spriteColor", "White"));
        Add(sprite);
        Add(light = new VertexLight(color, 1f, startFade, endFade));
        Add(bloom = new BloomPoint(alpha / 2f, 8f));
        bloom.Visible = false;
        light.Visible = false;
        Add(new PlayerCollider(OnPlayer));
        P_OnLight2 = new ParticleType(Torch.P_OnLight) { Color = color };
        base.Depth = 2000;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        bool lightflag = false; // default off
        if (unlightOnDeath)
        {
            lightflag = false; // off if unlightOnDeath
        }
        lightflag = SceneAs<Level>().Session.GetFlag(FlagName); // get light state
        // Startlit overwrites "unlightOnDeath" and light state
        if (startLit) { lightflag = true; }

        if (lightflag)
        {
            bloom.Visible = (light.Visible = true);
            lit = true;
            Collidable = false;
            sprite.Play("on");
        }
        else
        {
            bloom.Visible = (light.Visible = false);
            lit = false;
            Collidable = true;
        }
        
    }

    private void OnPlayer(Player player)
    {
        if (!lit)
        {
            Audio.Play("event:/game/05_mirror_temple/torch_activate", Position);
            lit = true;
            bloom.Visible = true;
            light.Visible = true;
            Collidable = false;
            sprite.Play("turnOn");
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
            tween.OnUpdate = delegate (Tween t) {
                light.StartRadius = startFade + (1f - t.Eased) * 32f;
                light.EndRadius = endFade + (1f - t.Eased) * 32f;
                bloom.Alpha = alpha + alpha * (1f - t.Eased);
            };
            Add(tween);
            if (!unlightOnDeath)
                SceneAs<Level>().Session.SetFlag(FlagName);
            SceneAs<Level>().ParticlesFG.Emit(P_OnLight2, 12, Position, new Vector2(3f, 3f));
        }
    }

}

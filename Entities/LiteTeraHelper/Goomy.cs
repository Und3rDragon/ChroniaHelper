using Celeste.Mod.Entities;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/goomy")]
public class Goomy : Entity
{
    private Sprite sprite;
    private TeraType tera;
    private Player player;
    private float cryTimer;
    private float idleTimer;
    private float target;
    private int range;
    private float start;
    private bool shiny;
    private Image[] hearts;

    private bool haveFriend { 
        get {
            if (player != null && !player.Dead)
                return player.GetTera() == tera;
            return false;
        }
    }
    private bool haveFood {
        get
        {
            var berries = SceneAs<Level>().Tracker.GetEntities<RoseliBerry>();
            foreach(var berry in berries)
            {
                if ((berry.Position - Position).Length() < 50f)
                    return true;
            }
            return false;
        }
    }
    private bool haveRelaxed {
        get => relaxed > 10;
    }
    private int relaxed;
    private int happiness{
        get
        {
            int joy = 0;
            if (haveFriend)
                joy++;
            if (haveFood)
                joy++;
            if (haveRelaxed)
                joy++;
            return joy;
        }
    }
    private bool activated;

    public Goomy(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        //tera = TeraType.Dragon;
        tera = TeraType.Normal;
        range = data.Int("range", 0);
        start = (data.Position + offset).X;
        Depth = 1000;
        Collider = new Hitbox(39f, 50f, -20f, -50f);
        Add(new PlayerCollider(OnPlayerBounce, new Hitbox(16f, 4f, -8f, -30f)));
        cryTimer = 1f;
        idleTimer = 4f;
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        player = scene.Tracker.GetEntity<Player>();
        shiny = false;
        foreach (Strawberry item in scene.Entities.FindAll<Strawberry>())
        {
            if (item.Golden && item.Follower.Leader != null)
            {
                shiny = true;
                break;
            }
        }
        if (!shiny)
        {
            int roll = 1;
            AreaKey area = SceneAs<Level>().Session.Area;
            var data = SaveData.Instance.Areas_Safe[area.ID];
            roll += data.TotalStrawberries;
            int seed = (int)data.TotalTimePlayed;
            Random random = new Random(seed);
            while (roll > 0)
            {
                roll--;
                var r = random.Next();
                var t = r % 4096;
                if (t == 0)
                {
                    shiny = true;
                    break;
                }
            }
        }
        if (!shiny)
        {
            Add(sprite = GFX.SpriteBank.Create("ChroniaHelper_goomy"));
        }
        else
        {
            Add(sprite = GFX.SpriteBank.Create("ChroniaHelper_goomyShiny"));
            Audio.Play("event:/ChroniaHelper/shiny", player.Position);
        }
        sprite.Position = new Vector2(0f, 2f);
        hearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            hearts[i] = new Image(GFX.Game["ChroniaHelper/objects/tera/Goomy/heart"]);
            hearts[i].Visible = false;
            Add(hearts[i]);
        }
    }
    public override void Update()
    {
        base.Update();
        var rand = new Random((int)(player.Position.X + player.Position.Y + X));
        var joy = happiness;
        if (joy == 3)
            OpenGate();
        if (cryTimer > 0f)
        {
            cryTimer -= Engine.DeltaTime;
            if (cryTimer <= 0f)
            {
                Cry();
                cryTimer = 8f + rand.Next() % 10;
            }
        }
        if (idleTimer > 0f)
        {
            idleTimer -= Engine.DeltaTime;
            if (idleTimer <= 0f)
            {
                var distance = rand.Next();
                var sign = rand.Next() % 2 == 1 ? 1 : -1;
                target = start + (distance % (range+1)) * sign;
                sprite.Scale.X = target > X ? -1 : 1;
            }
        }
        else
        {
            X = Calc.Approach(X, target, 30f * Engine.DeltaTime);
            if (Math.Abs(X - target) < 0.001f)
            {
                X = target;
                idleTimer = 5f + rand.Next() % 15;
            }
        }
        if (CollideCheck<Player>())
        {
            for (int i = 0; i < 3; i++)
                hearts[i].Visible = i < joy;
            if (joy > 0)
            {
                int s = 0;
                int g = 0;
                if (joy == 1)
                {
                    s = -5;
                    g = 0;
                }
                else if (joy == 2)
                {
                    s = -11;
                    g = 13;
                }
                else if (joy == 3)
                {
                    s = -17;
                    g = 13;
                }
                for (int i = 0; i < joy; i++)
                {
                    hearts[i].Position = new Vector2(s + g * i, -50f);
                }
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
                hearts[i].Visible = false;
        }
    }
    private void OnPlayerBounce(Player player)
    {
        Audio.Play("event:/game/general/thing_booped", Position);
        player.Bounce(base.Bottom - 31f);
        relaxed++;
    }
    private void Cry()
    {
        if (PlayerNearBy())
            Audio.Play("event:/ChroniaHelper/cry", Position);
    }
    private bool PlayerNearBy()
    {
        if (player == null && player.Dead)
            return false;
        return (player.Position - Position).Length() <= 50f;
    }
    private void OpenGate()
    {
        if (activated)
            return;
        activated = true;
        foreach(GoomyGate gate in SceneAs<Level>().Tracker.GetEntities<GoomyGate>())
        {
            gate.Open();
        }
    }
}
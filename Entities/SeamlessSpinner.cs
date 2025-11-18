using System.IO;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers.PolygonSeries;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SeamlessSpinner")]
public class SeamlessSpinner : Entity
{
    private class CoreModeListener : Component
    {
        public SeamlessSpinner Parent;
        private Session.CoreModes preCoreMode;

        public CoreModeListener(SeamlessSpinner parent)
            : base(active: true, visible: false)
        {
            Parent = parent;
        }

        public override void Update()
        {
            Level level = Scene as Level;
            if (level.CoreMode == Session.CoreModes.None)
                return;

            if (preCoreMode != level.CoreMode)
                Parent.TrySwitchCoreModeSprite();
            preCoreMode = level.CoreMode;
        }
    }
    
    public static ParticleType P_Move;

    public Dictionary<string, CrystalColor> checkColor = new Dictionary<string, CrystalColor>
    {
        { "red", CrystalColor.Red },
        { "blue", CrystalColor.Blue },
        { "purple", CrystalColor.Purple },
        { "white", CrystalColor.Rainbow },
        { "rainbow", CrystalColor.Rainbow },
    };

    private static Dictionary<CrystalColor, string> fgTextureLookup = new Dictionary<CrystalColor, string>
    {
        {
            CrystalColor.Blue,
            "objects/ChroniaHelper/timedSpinner/blue/fg_blue"
        },
        {
            CrystalColor.Red,
            "objects/ChroniaHelper/timedSpinner/red/fg_red"
        },
        {
            CrystalColor.Purple,
            "objects/ChroniaHelper/timedSpinner/purple/fg_purple"
        },
        {
            CrystalColor.Rainbow,
            "objects/ChroniaHelper/timedSpinner/white/fg_white"
        }
    };

    private static Dictionary<CrystalColor, string> bgTextureLookup = new Dictionary<CrystalColor, string>
    {
        {
            CrystalColor.Blue,
            "objects/ChroniaHelper/timedSpinner/blue/bg_blue"
        },
        {
            CrystalColor.Red,
            "objects/ChroniaHelper/timedSpinner/red/bg_red"
        },
        {
            CrystalColor.Purple,
            "objects/ChroniaHelper/timedSpinner/purple/bg_purple"
        },
        {
            CrystalColor.Rainbow,
            "objects/ChroniaHelper/timedSpinner/white/bg_white"
        }
    };


    public bool AttachToSolid;

    private float offset;

    private bool expanded;

    private int randomSeed;


    private int ID;

    private string imagePath;

    private string bgImagePath;

    public CColor spriteColor, bgSpriteColor;

    private bool noBorder;

    private bool dynamic;
    private float fgAnim, bgAnim;

    private enum Flip
    {
        none,
        flipped,
        random
    }

    private Flip flipX, flipY, bgFlipX, bgFlipY;

    private bool trigger, rainbow;

    private float triggerAnimDelay;

    public CrystalStaticSpinner spinner;

    private bool useCoreModeStyle = false;
    private string coldCoreModeBGSpritePath = "danger/crystal/bg_blue";
    private string hotCoreModeBGSpritePath = "danger/crystal/bg_red";

    private string coldCoreModeSpritePath = "danger/crystal/fg_blue";
    private string hotCoreModeSpritePath = "danger/crystal/fg_red";

    private string coldCoreModeTriggerSpritePath = "objects/ChroniaHelper/timedSpinner/blue/fg_blue_base";
    private string hotCoreModeTriggerSpritePath = "objects/ChroniaHelper/timedSpinner/red/fg_red_base";

    public SeamlessSpinner(Vector2 position, EntityData data) : base(position)
    {
        offset = Calc.Random.NextFloat();
        Tag = Tags.TransitionUpdate;

        //碰撞箱
        string inputHitbox = data.Attr("customHitbox").ToLower();
        string hitboxType = data.Attr("hitboxType");
        SetColliderByHitboxTypeAndData(hitboxType, inputHitbox);
        
        Add(new PlayerCollider(OnPlayer));
        Add(new HoldableCollider(OnHoldable));
        Add(new LedgeBlocker());
        
        Depth = data.Int("depth", -8500);
        AttachToSolid = data.Bool("attachToSolid");

        if (AttachToSolid)
        {
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                OnDestroy = WhenDestroy
            });
        }

        float bloomAlpha = data.Float("bloomAlpha");
        if (bloomAlpha != 0.0f)
        {
            Add(new BloomPoint(Collidable ? Collider.Center : Position + new Vector2(8f, 8f), bloomAlpha, data.Float("bloomRadius")));
        }

        // 路径设置
        imagePath = data.Attr("foreDirectory");
        bgImagePath = data.Attr("backDirectory");

        spriteColor = data.GetChroniaColor("colorOverlay", "ffffff");
        bgSpriteColor = string.IsNullOrEmpty(data.Attr("bgColorOverlay")) ? spriteColor : data.GetChroniaColor("bgColorOverlay", "ffffff");

        List<string> vanillaNames = new List<string> { "blue", "red", "purple", "rainbow", "white" };

        rainbow = imagePath.ToLower() == "rainbow" || data.Bool("rainbow");
        if (vanillaNames.Contains(imagePath.ToLower()))
            imagePath = fgTextureLookup[checkColor[imagePath.ToLower()]];
        if (vanillaNames.Contains(bgImagePath.ToLower()))
            bgImagePath = bgTextureLookup[checkColor[bgImagePath.ToLower()]];
        
        randomSeed = Calc.Random.Next();
        
        noBorder = data.Bool("noBorder");

        dynamic = data.Bool("dynamic");
        float fg1 = data.Float("fgAnimDelay", 0.1f).GetAbs(),
            fg2 = data.Float("fgAnimRandomize").GetAbs(),
            bg1 = data.Float("bgAnimDelay", 0.1f).GetAbs(),
            bg2 = data.Float("bgAnimRandomize").GetAbs();
        fgAnim = Calc.Random.Range(fg1 - fg2 >= Engine.DeltaTime ? fg1 - fg2 : Engine.DeltaTime, fg1 + fg2);
        bgAnim = Calc.Random.Range(bg1 - bg2 >= Engine.DeltaTime ? bg1 - bg2 : Engine.DeltaTime, bg1 + bg2);

        flipX = data.Enum<Flip>("fgFlipX");
        flipY = data.Enum<Flip>("fgFlipY");
        bgFlipX = data.Enum<Flip>("bgFlipX");
        bgFlipY = data.Enum<Flip>("bgFlipY");

        trigger = data.Bool("trigger");
        triggerAnimDelay = data.Float("triggerAnimDelay", 0.05f).GetAbs();
        setTimer = data.Float("triggerDelay", 0.5f).GetAbs();

        useCoreModeStyle = data.Bool("useCoreModeStyle");
        coldCoreModeBGSpritePath = data.Attr("coldCoreModeBGSpritePath");
        hotCoreModeBGSpritePath = data.Attr("hotCoreModeBGSpritePath");
        coldCoreModeSpritePath = data.Attr("coldCoreModeSpritePath");
        hotCoreModeSpritePath = data.Attr("hotCoreModeSpritePath");
        coldCoreModeTriggerSpritePath = data.Attr("coldCoreModeTriggerSpritePath");
        hotCoreModeTriggerSpritePath = data.Attr("hotCoreModeTriggerSpritePath");

        // Sprite Setup
        sprite = new AnimatedImage();
        sprite.color = spriteColor;
        sprite.origin = Vc2.One * 0.5f;
        sprite.flipX = (int)flipX == 2 ? Calc.Random.Range(0, 2).ToBool() : ((int)flipX == 1 ? true : false);
        sprite.flipY = (int)flipY == 2 ? Calc.Random.Range(0, 2).ToBool() : ((int)flipY == 1 ? true : false);
        sprite.textures.Enter("idle", GFX.Game.GetAtlasSubtextures(imagePath));
        sprite.interval.Enter("idle", fgAnim);
        sprite.textures.Enter("load", GFX.Game.GetAtlasSubtextures($"{imagePath}_base"));
        sprite.interval.Enter("load", triggerAnimDelay);
        sprite.loop.Enter("load", false);
        sprite.textures.Enter("idle_hot", GFX.Game.GetAtlasSubtextures(hotCoreModeSpritePath));
        sprite.interval.Enter("idle_hot", fgAnim);
        sprite.textures.Enter("idle_cold", GFX.Game.GetAtlasSubtextures(coldCoreModeSpritePath));
        sprite.interval.Enter("idle_cold", fgAnim);
        sprite.textures.Enter("load_hot", GFX.Game.GetAtlasSubtextures(hotCoreModeTriggerSpritePath));
        sprite.interval.Enter("load_hot", triggerAnimDelay);
        sprite.loop.Enter("load_hot", false);
        sprite.textures.Enter("load_cold", GFX.Game.GetAtlasSubtextures(coldCoreModeTriggerSpritePath));
        sprite.interval.Enter("load_cold", triggerAnimDelay);
        sprite.loop.Enter("load_cold", false);
        
        int totalFrames = sprite.CurrentAnimationLength();
        int randomChoice = Calc.Random.Range(0, totalFrames);
        sprite.currentFrame = randomChoice;
    }

    private void SetColliderByHitboxTypeAndData(string hitboxType, string hitboxData)
    {
        if (hitboxType == "loosened")
        {
            Collider = new ColliderList(new Circle(6f));
        }
        else if (hitboxType == "seamlessSquare")
        {
            Collider = new ColliderList(new Hitbox(16f, 16f, -8f, -8f));
        }
        else if (hitboxType == "seamlessRound")
        {
            Collider = new ColliderList(new Circle(8f));
        }
        else if (hitboxType == "custom")
        {
            Collider = hitboxData.ParseColliderList(this, true);
        }
        else
        {
            Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        }
    }

    public SeamlessSpinner(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
        ID = data.ID;
    }


    private float timer, setTimer;

    public override void Added(Scene scene)
    {
        if (spinner == null)
        {
            spinner = new CrystalStaticSpinner(Vector2.Zero, false, Celeste.CrystalColor.Rainbow);
        }

        spinner.Scene = scene;

        timer = setTimer;
        killPlayer = !trigger;
        triggered = false;
        
        if (useCoreModeStyle)
        {
            string tag = MaP.level.CoreMode == Session.CoreModes.Cold ? "cold" : "hot";
            
            if (trigger)
            {
                sprite.Switch($"load_{tag}");
                triggerAnimTime = triggerAnimDelay * sprite.CurrentAnimationLength();
            }
            else
            {
                sprite.Switch($"idle_{tag}");
                sprite.playing = dynamic;
            }
        }
        else
        {
            if (trigger)
            {
                sprite.Switch("load");
                triggerAnimTime = triggerAnimDelay * sprite.CurrentAnimationLength();
            }
            else
            {
                sprite.Switch("idle");
                sprite.playing = dynamic;
            }
        }

        base.Added(scene);
        
        Add(new CoreModeListener(this));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        bgSprites = new();
        
        foreach (SeamlessSpinner entity in Scene.Tracker.GetEntities<SeamlessSpinner>())
        {
            if (entity.ID > ID && entity.AttachToSolid == AttachToSolid && (entity.Position - Position).LengthSquared() < 576f)
            {
                AddSprite((Position + entity.Position) / 2f - Position);
            }
        }
    }

    private void OnPlayer(Player player)
    {
        // execute once per frame on player interact
        timerActive = true;

        if (killPlayer)
        {
            player.Die((player.Position - Position).SafeNormalize());

            timer = setTimer;
            timerActive = false;
        }
    }

    private bool killPlayer;

    private bool timerActive;
    private float triggerAnimTime;
    private bool triggered = false;
    public override void Update()
    {
        Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
        if (trigger)
        {
            if (timerActive)
            {
                // timer表示从player接触spinner开始到spinner有碰撞需要多久 
                timer -= Engine.DeltaTime;
            }

            if (timer <= triggerAnimTime && timerActive)
            {
                sprite.playing = true;
            }

            if (timer <= 0f && timerActive)
            {
                killPlayer = true;
                triggered = true;
                timerActive = false;

                if (useCoreModeStyle)
                {
                    string tag = SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "cold" : "hot";
                    
                    sprite.Switch($"idle_{tag}");
                    
                    int totalFrames = sprite.CurrentAnimationLength();
                    int randomChoice = Calc.Random.Range(0, totalFrames);
                    sprite.currentFrame = randomChoice;

                    sprite.playing = dynamic;
                }
                else
                {
                    sprite.Switch("idle");
                    int totalFrames = sprite.CurrentAnimationLength();
                    int randomChoice = Calc.Random.Range(0, totalFrames);
                    sprite.currentFrame = randomChoice;

                    sprite.playing = dynamic;
                }
            }
        }

        base.Update();
        if (rainbow && Scene.OnInterval(0.08f, offset))
        {
            UpdateRainbowHue();
        }

        if (Scene.OnInterval(0.25f, offset) && !InView())
        {
            Visible = false;
        }

        if (Scene.OnInterval(0.05f, offset))
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                Collidable = Math.Abs(entity.X - X) < 128f && Math.Abs(entity.Y - Y) < 128f;
            }
        }

        bgSprites.EachDo((image) =>
        {
            image.Update();
        });
        
        sprite.Update();
    }

    private void UpdateRainbowHue()
    {
        sprite.color.color = spinner.GetHue(Position);
        
        foreach(var sprite in bgSprites)
        {
            sprite.color.color = spinner.GetHue(Position);
        }
    }

    private bool InView()
    {
        Camera camera = (Scene as Level).Camera;
        if (X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + 320f + 16f)
        {
            return Y < camera.Y + 180f + 16f;
        }

        return false;
    }

    public AnimatedImage sprite;

    private void TrySwitchCoreModeSprite()
    {
        if (!useCoreModeStyle)
        {
            return;
        }

        // spinner
        string anim = SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "idle_cold" : "idle_hot";
        sprite.Switch(anim);
        bgSprites.EachDo((image) =>
            {
                image.Switch(anim);
                image.playing = dynamic;
            }
        );

        int totalFrames = sprite.CurrentAnimationLength();
        int randomChoice = Calc.Random.Range(0, totalFrames);
        sprite.currentFrame = randomChoice;

        sprite.playing = dynamic;

        if (trigger)
        {
            if (triggered)
            {
                anim = SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "idle_cold" : "idle_hot";
            }
            else
            {
                anim = SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "load_cold" : "load_hot";
            }
            
            sprite.Switch(anim);

            triggerAnimTime = triggerAnimDelay * sprite.CurrentAnimationLength();

            sprite.color = spriteColor;
            if (rainbow)
            {
                sprite.color.color = spinner.GetHue(Position);
            }
        }
    }

    public List<AnimatedImage> bgSprites = new();

    private void AddSprite(Vector2 offset)
    {
        // Create BG Spinner
        AnimatedImage bgsprite = new AnimatedImage();
        bgsprite.textures.Enter("idle", GFX.Game.GetAtlasSubtextures(bgImagePath));
        bgsprite.interval.Enter("idle_hot", bgAnim);
        bgsprite.textures.Enter("idle_hot", GFX.Game.GetAtlasSubtextures(hotCoreModeBGSpritePath));
        bgsprite.interval.Enter("idle_hot", bgAnim);
        bgsprite.textures.Enter("idle_cold", GFX.Game.GetAtlasSubtextures(coldCoreModeBGSpritePath));
        bgsprite.interval.Enter("idle_cold", bgAnim);
        
        bgsprite.origin = Vc2.One * 0.5f;
        bgsprite.offset = offset;

        bgsprite.color = bgSpriteColor;
        if (rainbow)
        {
            bgsprite.color.color = spinner.GetHue(Position + offset);
        }
        
        bgsprite.flipX = (int)bgFlipX == 2 ? Calc.Random.Range(0, 2).ToBool() : ((int)bgFlipX == 1 ? true : false);
        bgsprite.flipY = (int)bgFlipY == 2 ? Calc.Random.Range(0, 2).ToBool() : ((int)bgFlipY == 1 ? true : false);

        if (useCoreModeStyle)
        {
            string anim = SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "idle_cold" : "idle_hot";
            bgsprite.Switch(anim);
        }
        else
        {
            bgsprite.Switch("idle");
        }
        
        int totalFrames = bgsprite.CurrentAnimationLength();
        int randomChoice = Calc.Random.Range(0, totalFrames);
        bgsprite.currentFrame = randomChoice;

        bgsprite.playing = dynamic;

        bgSprites.Add(bgsprite);
    }
    

    private void OnShake(Vector2 pos)
    {
        foreach (Component component in Components)
        {
            if (component is Sprite sprite)
            {
                sprite.Position += pos;
            }
        }
    }

    private bool IsRiding(Solid solid)
    {
        return CollideCheck(solid);
    }

    private void WhenDestroy()
    {
        RemoveSelf();
    }

    private void OnHoldable(Holdable h)
    {
        h.HitSpinner(this);
    }

    public override void Removed(Scene scene)
    {
        
        RemoveSelf();
        
        base.Removed(scene);
    }

    public override void Render()
    {
        base.Render();

        Vc2 levelPos = new Vc2(MaP.level.Bounds.Left, MaP.level.Bounds.Top);

        bgSprites.EachDo((image) =>
        {
            image.Render(Position);
        });
        
        sprite.Render(Position);
        
    }
}
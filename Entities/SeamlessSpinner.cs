using Celeste.Mod.Entities;
using ChroniaHelper.Triggers.PolygonSeries;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked()]
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

    public class Border : Entity
    {
        public CColor imageColor = new CColor(Color.Black);
        private Entity[] drawing = new Entity[2];

        public Border(Entity parent, Entity filler)
        {
            drawing[0] = parent;
            drawing[1] = filler;
            // Border Depth, unknown use, bugfix in CH 1.19.17
            Depth = parent.Depth + 10;
        }

        public override void Render()
        {
            if (drawing[0].Visible)
            {
                DrawBorder(drawing[0]);
                DrawBorder(drawing[1]);
            }
        }

        private void DrawBorder(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            foreach (Component component in entity.Components)
            {
                if (component is Image image)
                {
                    Color color = image.Color;
                    Vector2 position = image.Position;
                    image.Color = imageColor.Parsed();
                    image.Position = position + new Vector2(0f, -1f);
                    image.Render();
                    image.Position = position + new Vector2(0f, 1f);
                    image.Render();
                    image.Position = position + new Vector2(-1f, 0f);
                    image.Render();
                    image.Position = position + new Vector2(1f, 0f);
                    image.Render();
                    image.Color = color;
                    image.Position = position;
                }
            }
        }
    }

    public class Inner : Entity
    {
        private Entity main;

        public Inner(Entity parent)
        {
            main = parent;

            Depth = parent.Depth + 5;
        }

        public override void Render()
        {
            base.Render();
            Position = main.Position;
            Visible = main.Visible;
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

    public Inner filler;

    public Border border;

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

    public bool trigger, rainbow;

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

        Visible = false;
        Add(new PlayerCollider(OnPlayer));
        Add(new HoldableCollider(OnHoldable));
        Add(new LedgeBlocker());

        Depth = data.Int("depth", -8500);
        AttachToSolid = data.Bool("attachToSolid");

        // 新增变量
        imagePath = data.Attr("foreDirectory");
        bgImagePath = data.Attr("backDirectory");

        List<string> vanillaNames = new List<string> { "blue", "red", "purple", "rainbow", "white" };

        rainbow = imagePath.ToLower() == "rainbow" || data.Bool("rainbow");
        if (vanillaNames.Contains(imagePath.ToLower()))
            imagePath = fgTextureLookup[checkColor[imagePath.ToLower()]];
        if (vanillaNames.Contains(bgImagePath.ToLower()))
            bgImagePath = bgTextureLookup[checkColor[bgImagePath.ToLower()]];


        if (AttachToSolid)
        {
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                OnDestroy = WhenDestroy
            });
        }

        randomSeed = Calc.Random.Next();

        float bloomAlpha = data.Float("bloomAlpha");
        if (bloomAlpha != 0.0f)
        {
            Add(new BloomPoint(Collidable ? Collider.Center : Position + new Vector2(8f, 8f), bloomAlpha, data.Float("bloomRadius")));
        }

        spriteColor = data.GetChroniaColor("colorOverlay", "ffffff");
        bgSpriteColor = string.IsNullOrEmpty(data.Attr("bgColorOverlay")) ? spriteColor : data.GetChroniaColor("bgColorOverlay", "ffffff");

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

        childMode = data.Attr("childMode");
    }

    public string childMode;
    public DangerBubbler childModeParent = null;
    
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

    public override void Awake(Scene scene)
    {
        if (childMode.IsNotNullOrEmpty())
        {
            var bubblers = (scene as Level).Tracker.GetEntities<DangerBubbler>();
            foreach (var bubbler in bubblers)
            {
                //Log.Info(bubbler.SourceData.ID);
                if (bubbler.SourceData.ID.ToString() == childMode)
                {
                    childModeParent = bubbler as DangerBubbler;
                }
            }
        }
        
        if (spinner == null)
        {
            spinner = new CrystalStaticSpinner(Vector2.Zero, false, Celeste.CrystalColor.Rainbow);
        }

        spinner.Scene = scene;

        timer = setTimer;
        killPlayer = !trigger;

        base.Awake(scene);
        Add(new CoreModeListener(this));


        if (InView())
        {
            CreateSprites();
        }
    }

    private void OnPlayer(Player player)
    {
        // execute once per frame on player interact
        timerActive = true;

        if (killPlayer)
        {
            if (childModeParent.IsNotNull())
            {
                childModeParent.PlayerActivated(player);
            }
            else
            {
                player.Die((player.Position - Position).SafeNormalize());
            }

            timer = setTimer;
            timerActive = false;
        }
    }

    private bool killPlayer;

    private bool timerActive;
    private float triggerAnimTime;

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

            if (timer <= triggerAnimTime && timer > 0f)
            {
                sprite.Rate = 1f;
            }

            if (timer <= 0f && timerActive)
            {
                killPlayer = true;
                timerActive = false;

                sprite.Play("idle");

                if (useCoreModeStyle)
                {
                    sprite.Play(SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "idle_cold" : "idle_hot");
                }

                int totalFrames = sprite.CurrentAnimationTotalFrames;
                int randomChoice = Calc.Random.Range(0, totalFrames);
                sprite.SetAnimationFrame(randomChoice);

                if (!dynamic)
                {
                    sprite.Rate = 0f;
                }
            }
        }

        if (!Visible)
        {
            Collidable = false;

            if (InView())
            {
                Visible = true;
                if (!expanded)
                {
                    CreateSprites();
                }

                if (rainbow)
                {
                    UpdateRainbowHue();
                }
            }
        }
        else
        {
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
        }

        if (filler != null)
        {
            filler.Position = Position;
            filler.Visible = Visible;
        }
    }

    public void UpdateRainbowHue()
    {
        foreach (Component component in Components)
        {
            if (component is Sprite sprite)
            {
                sprite.Color = spinner.GetHue(Position + sprite.Position);
            }
        }

        if (filler == null)
        {
            return;
        }

        foreach (Component component2 in filler.Components)
        {
            if (component2 is Sprite sprite2)
            {
                sprite2.Color = spinner.GetHue(Position + sprite2.Position);
            }
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

    public Sprite sprite;

    private void TrySwitchCoreModeSprite()
    {
        if (!useCoreModeStyle)
        {
            return;
        }

        // spinner
        sprite.AddLoop("idle_cold", coldCoreModeSpritePath, fgAnim);
        sprite.AddLoop("idle_hot", hotCoreModeSpritePath, fgAnim);
        if(sprite.CurrentAnimationID.StartsWith("idle"))
        {
            sprite.Play(SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "idle_cold" : "idle_hot");

            int totalFrames = sprite.CurrentAnimationTotalFrames;
            int randomChoice = Calc.Random.Range(0, totalFrames);
            sprite.SetAnimationFrame(randomChoice);

            if (!dynamic) { sprite.Rate = 0f; }
        }

        // bg
        foreach (Sprite bgSprite in bgSprites)
        {
            bgSprite.AddLoop("idle_cold", coldCoreModeBGSpritePath, bgAnim);
            bgSprite.AddLoop("idle_hot", hotCoreModeBGSpritePath, bgAnim);

            if (bgSprite.CurrentAnimationID.StartsWith("idle"))
            {
                bgSprite.Play(SceneAs<Level>().coreMode == Session.CoreModes.Cold ? "idle_cold" : "idle_hot");

                int totalBGFrames = bgSprite.CurrentAnimationTotalFrames;
                int randomBGChoice = Calc.Random.Range(0, totalBGFrames);
                bgSprite.SetAnimationFrame(randomBGChoice);

                if (!dynamic) { bgSprite.Rate = 0f; }
            }
        }
        
    }

    private void CreateSprites()
    {
        // Create FG spinner

        if (expanded)
        {
            return;
        }

        Calc.PushRandom(randomSeed);

        sprite = new Sprite(GFX.Game, "");

        sprite.AddLoop("idle", imagePath, fgAnim);
        sprite.Add("load", imagePath + "_base", triggerAnimDelay);

        Add(sprite);
        sprite.Justify = Vector2.One / 2;
        sprite.Color = spriteColor.Parsed();
        if (rainbow)
        {
            sprite.Color = spinner.GetHue(Position);
        }

        sprite.Play("idle");

        Vector2 scale = Vector2.One;
        if (flipX == Flip.flipped)
        {
            scale.X = -1;
        }
        else if (flipX == Flip.random)
        {
            scale.X = Calc.Random.Choose(1f, -1f);
        }

        if (flipY == Flip.flipped)
        {
            scale.Y = -1;
        }
        else if (flipY == Flip.random)
        {
            scale.Y = Calc.Random.Choose(1f, -1f);
        }

        sprite.Scale = scale;

        if (!dynamic)
        {
            int totalFrames = sprite.CurrentAnimationTotalFrames;
            int randomChoice = Calc.Random.Range(0, totalFrames);
            sprite.Rate = 0f;
            sprite.SetAnimationFrame(randomChoice);
        }

        if (trigger)
        {
            sprite.Play("load");
            sprite.Rate = 0f;
            sprite.SetAnimationFrame(0);
            
            triggerAnimTime = triggerAnimDelay * sprite.CurrentAnimationTotalFrames;
        }

        foreach (SeamlessSpinner entity in Scene.Tracker.GetEntities<SeamlessSpinner>())
        {
            if (entity.ID > ID && entity.AttachToSolid == AttachToSolid && (entity.Position - Position).LengthSquared() < 576f)
            {
                AddSprite((Position + entity.Position) / 2f - Position);
            }
        }

        if (!noBorder)
        {
            Scene.Add(border = new Border(this, filler));
        }

        expanded = true;
        Calc.PopRandom();
    }

    public List<Sprite> bgSprites = new();

    private void AddSprite(Vector2 offset)
    {
        if (filler == null)
        {
            Scene.Add(filler = new Inner(this));
        }

        // Create BG Spinner
        Sprite bgsprite = new Sprite(GFX.Game, "");
        bgSprites.Add(bgsprite);

        bgsprite.AddLoop("idle", bgImagePath, bgAnim);

        filler.Add(bgsprite);

        bgsprite.Justify = Vector2.One / 2;
        bgsprite.Position = offset;

        bgsprite.Color = bgSpriteColor.Parsed();
        if (rainbow)
        {
            bgsprite.Color = spinner.GetHue(Position + offset);
        }

        bgsprite.Play("idle");

        Vector2 scale = Vector2.One;
        if (bgFlipX == Flip.flipped)
        {
            scale.X = -1;
        }
        else if (bgFlipX == Flip.random)
        {
            scale.X = Calc.Random.Choose(1f, -1f);
        }

        if (bgFlipY == Flip.flipped)
        {
            scale.Y = -1;
        }
        else if (bgFlipY == Flip.random)
        {
            scale.Y = Calc.Random.Choose(1f, -1f);
        }

        bgsprite.Scale = scale;

        if (!dynamic)
        {
            int totalFrames = bgsprite.GetFrames("").Length;
            int randomChoice = Calc.Random.Range(0, totalFrames);
            bgsprite.Rate = 0f;
            bgsprite.SetAnimationFrame(randomChoice);
        }
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
        if (filler != null)
        {
            filler.RemoveSelf();
        }

        filler = null;

        if (border != null)
        {
            border.RemoveSelf();
        }

        border = null;

        RemoveSelf();
    }

    private void OnHoldable(Holdable h)
    {
        h.HitSpinner(this);
    }

    public override void Removed(Scene scene)
    {
        if (filler != null && filler.Scene == scene)
        {
            filler.RemoveSelf();
        }

        if (border != null && border.Scene == scene)
        {
            border.RemoveSelf();
        }

        RemoveSelf();

        base.Removed(scene);
    }
}
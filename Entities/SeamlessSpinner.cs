using Celeste.Mod.Entities;
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
            if (level.Session.CoreMode == Session.CoreModes.None)
                return;

            if (preCoreMode != level.Session.CoreMode)
                Parent.TrySwitchCoreModeSprite();
            preCoreMode = level.Session.CoreMode;
        }
    }

    private class Border : Entity
    {
        private Entity[] drawing = new Entity[2];

        public Border(Entity parent, Entity filler)
        {
            drawing[0] = parent;
            drawing[1] = filler;
            // Border Depth, unknown use, bugfix in CH 1.19.17
            Depth = 100000;
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
                    image.Color = Color.Black;
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

    private class Inner : Entity
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

    private Inner filler;

    private Border border;

    private float offset;

    private bool expanded;

    private int randomSeed;


    private int ID;

    private string imagePath;

    private string bgImagePath;

    public int depth;

    private Color spriteColor, bgSpriteColor;

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
        string[] hitboxData = inputHitbox.Split(';', StringSplitOptions.TrimEntries);
        SetColliderByHitboxTypeAndData(hitboxType, hitboxData);

        Visible = false;
        Add(new PlayerCollider(OnPlayer));
        Add(new HoldableCollider(OnHoldable));
        Add(new LedgeBlocker());
        depth = data.Int("depth", -8500);
        Depth = depth;
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

        spriteColor = Calc.HexToColor(data.Attr("colorOverlay", "ffffff"));
        bgSpriteColor = string.IsNullOrEmpty(data.Attr("bgColorOverlay")) ? spriteColor : Calc.HexToColor(data.Attr("bgColorOverlay", "ffffff"));

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
        coldCoreModeSpritePath = data.Attr("coldCoreModeSpritePath");
        hotCoreModeSpritePath = data.Attr("hotCoreModeSpritePath");
        coldCoreModeTriggerSpritePath = data.Attr("coldCoreModeTriggerSpritePath");
        hotCoreModeTriggerSpritePath = data.Attr("hotCoreModeTriggerSpritePath");
    }

    private void SetColliderByHitboxTypeAndData(string hitboxType, string[] hitboxData)
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
            //对于每组数据
            ColliderList CL = new ColliderList();
            for (int i = 0; i < hitboxData.Length; i++)
            {
                //首先分割并去空
                string[] hb = hitboxData[i].Split(",", StringSplitOptions.TrimEntries);
                //淘汰length为0
                if (hb.Length == 0)
                {
                    break;
                }

                //length !=0 , 检查第一位
                if (hb[0] == "" || (hb[0] != "c" && hb[0] != "r"))
                {
                    break;
                }

                //第一位稳定，开始记录
                float p1 = 0f, p2 = 0f, p3 = 0f, p4 = 0f;
                if (hb.Length >= 2)
                {
                    p1 = hb[1].ParseFloat();
                }

                if (hb.Length >= 3)
                {
                    p2 = hb[2].ParseFloat();
                }

                if (hb.Length >= 4)
                {
                    p3 = hb[3].ParseFloat();
                }

                if (hb.Length >= 5)
                {
                    p4 = hb[4].ParseFloat();
                }

                if (hb[0] == "r")
                {
                    if (p1 <= 0)
                    {
                        break;
                    }

                    if (p2 <= 0)
                    {
                        break;
                    }

                    CL.Add(new Hitbox(p1, p2, p3, p4));
                }
                else
                {
                    if (p1 <= 0)
                    {
                        break;
                    }

                    CL.Add(new Circle(p1, p2, p3));
                }
            }

            Collider = CL;
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
            player.Die((player.Position - Position).SafeNormalize());

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

            if (timer <= triggerAnimTime)
            {
                loadSprite.Rate = 1f;
            }

            if (timer <= 0f)
            {
                killPlayer = true;
                timerActive = false;

                if (loadSprite.CurrentAnimationFrame == loadSprite.GetFrames("").Length - 1)
                {
                    loadSprite.Visible = false;

                    sprite.Visible = true;
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

    private void UpdateRainbowHue()
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

    private Sprite sprite, loadSprite;

    private void TrySwitchCoreModeSprite()
    {
        if (!useCoreModeStyle)
            return;

        // spinner
        string path = SceneAs<Level>().coreMode == Session.CoreModes.Cold ? coldCoreModeSpritePath : hotCoreModeSpritePath;
        sprite.Reset(GFX.Game, path);
        sprite.AddLoop("idle", "", fgAnim);
        sprite.Play("idle");

        int totalFrames = sprite.GetFrames("").Length;
        int randomChoice = Calc.Random.Range(0, totalFrames);
        sprite.SetAnimationFrame(randomChoice);

        // loadSprite
        // 因为如果play完了, currentAnimation就变成null, 所以这里也要判断一下
        if (loadSprite == null || loadSprite.currentAnimation == null)
            return;
        path = SceneAs<Level>().coreMode == Session.CoreModes.Cold ? coldCoreModeTriggerSpritePath : hotCoreModeTriggerSpritePath;
        loadSprite.Path = path;
        loadSprite.currentAnimation.Frames = loadSprite.GetFrames("");
        // 重置一下当前帧
        loadSprite.SetFrame(loadSprite.animations["load"].Frames[loadSprite.CurrentAnimationFrame]);
    }

    private void CreateSprites()
    {
        // Create FG spinner

        if (expanded)
        {
            return;
        }

        Calc.PushRandom(randomSeed);

        sprite = new Sprite(GFX.Game, imagePath);

        sprite.AddLoop("idle", "", fgAnim);

        Add(sprite);
        sprite.Justify = Vector2.One / 2;
        sprite.Color = spriteColor;
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
            int totalFrames = sprite.GetFrames("").Length;
            int randomChoice = Calc.Random.Range(0, totalFrames);
            sprite.Rate = 0f;
            sprite.SetAnimationFrame(randomChoice);
        }

        if (trigger)
        {
            sprite.Visible = false;

            loadSprite = new Sprite(GFX.Game, $"{imagePath}_base");
            loadSprite.Add("load", "", triggerAnimDelay);
            triggerAnimTime = triggerAnimDelay * loadSprite.GetFrames("").Length;
            loadSprite.Justify = Vector2.One / 2;
            loadSprite.Color = spriteColor;
            if (rainbow)
            {
                loadSprite.Color = spinner.GetHue(Position);
            }

            Vector2 loadScale = Vector2.One;
            if (flipX == Flip.flipped)
            {
                loadScale.X = -1;
            }
            else if (flipX == Flip.random)
            {
                loadScale.X = Calc.Random.Choose(1f, -1f);
            }

            if (flipY == Flip.flipped)
            {
                loadScale.Y = -1;
            }
            else if (flipY == Flip.random)
            {
                loadScale.Y = Calc.Random.Choose(1f, -1f);
            }

            loadSprite.Scale = loadScale;

            Add(loadSprite);
            loadSprite.Play("load");
            loadSprite.SetAnimationFrame(0);
            loadSprite.Rate = 0f;
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

    private Sprite bgsprite;

    private void AddSprite(Vector2 offset)
    {
        if (filler == null)
        {
            Scene.Add(filler = new Inner(this));
        }

        // Create BG Spinner
        bgsprite = new Sprite(GFX.Game, bgImagePath);

        bgsprite.AddLoop("idle", "", bgAnim);

        filler.Add(bgsprite);

        bgsprite.Justify = Vector2.One / 2;
        bgsprite.Position = offset;

        bgsprite.Color = bgSpriteColor;
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


    private void ClearSprites()
    {
        foreach (Sprite item in Components.GetAll<Sprite>())
        {
            item.RemoveSelf();
        }

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

        expanded = false;
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

        base.Removed(scene);
    }
}
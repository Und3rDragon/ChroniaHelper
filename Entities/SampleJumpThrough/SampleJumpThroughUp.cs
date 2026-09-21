using Celeste.Mod.Entities;
using ChroniaHelper.Cores.SampleJumpThrough;
using MonoMod.Utils;

namespace ChroniaHelper.Entities;

/// <summary>
/// 朝上的跳穿板，可选择粘连到相邻实体上随其一同移动。
/// 未启用粘连时行为与原版跳穿板一致。
/// </summary>
[CustomEntity("ChroniaHelper/SampleJumpThroughUp")]
[Tracked(false)]
[ExA.Credits("Maddie for Sideways and Upside-down Jumpthru codes" +
    "pixelator for VortexHelper attached jumpthru codes")]
public class SampleJumpThroughUp : JumpThru
{
    private readonly int columns;

    private readonly int overrideSoundIndex = -1;
    private readonly string overrideTexture;
    private readonly float animationDelay;

    private readonly bool attached;

    private readonly StaticMover staticMover;
    private Platform Platform;

    private Vector2 imageOffset;

    public Color EnabledColor = Color.White;
    public Color DisabledColor = Color.White;
    public bool VisibleWhenDisabled;

    public SampleJumpThroughUp(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Attr("texture", "default"),
               data.Int("surfaceIndex", -1), data.Float("animationDelay", 0f),
               data.Bool("attached", defaultValue: false)) { }

    public SampleJumpThroughUp(Vector2 position, int width, string overrideTexture, int overrideSoundIndex = -1, float animationDelay = 0f, bool attached = false)
        : base(position, width, safe: false)
    {
        this.columns = width / 8;
        this.Depth = Depths.Dust - 10;

        this.overrideTexture = overrideTexture;
        this.overrideSoundIndex = overrideSoundIndex;
        this.animationDelay = animationDelay;
        this.attached = attached;
        
        if (!attached)
        {
            return;
        }

        this.staticMover = new StaticMover
        {
            OnAttach = p => this.Depth = p.Depth + 1,
        };

        Add(new StaticMoverWithLiftSpeed
        {
            OnMove = OnMove,
            OnShake = OnShake,
            SolidChecker = IsRiding,
            OnEnable = OnEnable,
            OnDisable = OnDisable,
            OnSetLiftSpeed = OnSetLiftSpeed
        });
    }

    public override void Awake(Scene scene)
    {

        base.Awake(scene);

        var areaData = AreaData.Get(scene);
        string jumpthru = areaData.Jumpthru;
        areaData.Jumpthru = jumpthru;

        if (!string.IsNullOrEmpty(this.overrideTexture) && !this.overrideTexture.Equals("default"))
            areaData.Jumpthru = this.overrideTexture;

        this.SurfaceSoundIndex = this.overrideSoundIndex > 0
            ? this.overrideSoundIndex
            : jumpthru.ToLower() switch
            {
                "dream" => SurfaceIndex.AuroraGlass,
                "temple" or "templeb" => SurfaceIndex.Brick,
                "core" => SurfaceIndex.Dirt,
                _ => SurfaceIndex.Wood,
            };

        // 贴图名可能带编号序列：不带编号时直接命中，带编号时取第一帧
        MTexture[] atlasFrames = GFX.Game.GetAtlasSubtextures("objects/jumpthru/" + areaData.Jumpthru).ToArray();
        MTexture mTexture = atlasFrames.Length > 0
            ? atlasFrames[0]
            : GFX.Game["objects/jumpthru/" + areaData.Jumpthru];

        int blockColumns = mTexture.Width / 8;

        // 逐格决定使用图集中的哪一块，动画与静态绘制共用同一套取块规则
        Point[] blocks = new Point[this.columns];

        for (int i = 0; i < this.columns; i++)
        {
            int tx, ty;
            if (i == 0)
            {
                tx = 0;
                ty = CollideCheck<Solid>(this.Position + new Vector2(-1f, 0f)) ? 0 : 1;
            }
            else if (i == this.columns - 1)
            {
                tx = blockColumns - 1;
                ty = CollideCheck<Solid>(this.Position + new Vector2(1f, 0f)) ? 0 : 1;
            }
            else
            {
                tx = 1 + Calc.Random.Next(blockColumns - 2);
                ty = Calc.Random.Choose(0, 1);
            }

            blocks[i] = new Point(tx, ty);
        }

        // 指定动画速度时按图集序列逐帧切换，否则静态绘制
        if (animationDelay > 0f)
        {
            if (atlasFrames.Length > 0)
            {
                Add(new JumpThroughAnimator(
                    atlasFrames, this.columns, horizontal: true, animationDelay, blocks)
                {
                    Depth = this.Depth
                });
            }
        }
        else
        {
            for (int i = 0; i < this.columns; i++)
            {
                Add(new Image(mTexture.GetSubtexture(blocks[i].X * 8, blocks[i].Y * 8, 8, 8))
                {
                    X = i * 8
                });
            }
        }

        if (!attached)
        {
            return;
        }

        foreach (StaticMover component in scene.Tracker.GetComponents<StaticMover>())
        {
            if (component.IsRiding(this) && component.Platform is null)
            {
                this.staticMovers.Add(component);
                component.Platform = this;
                component.OnAttach?.Invoke(this);
            }
        }
    }

    private bool IsRiding(Solid solid)
    {
        if (!CollideCheck(solid, this.Position + Vector2.UnitX) && !CollideCheck(solid, this.Position - Vector2.UnitX))
            return false;

        this.staticMover.Platform = this.Platform = solid;
        if (this.VisibleWhenDisabled = solid is CassetteBlock)
            this.DisabledColor = Color.Lerp(Color.Black, Color.White, .4f);
        return true;
    }

    public override void Render()
    {
        Vector2 position = this.Position;
        this.Position += this.imageOffset;
        base.Render();
        this.Position = position;
    }

    private void OnSetLiftSpeed(Vector2 liftSpeed) => this.LiftSpeed = liftSpeed;

    // Fix weird tp glitch with MoveBlocks
    private void OnMove(Vector2 amount)
    {
        float moveH = amount.X;
        float moveV = amount.Y;

        if (this.Platform is MoveBlock && new DynData<MoveBlock>(this.Platform as MoveBlock).Get<float>("speed") == 0f)
        {
            MoveHNaive(moveH);
            MoveVNaive(moveV);
        }
        else
        {
            MoveH(moveH, this.LiftSpeed.X);
            MoveV(moveV, this.LiftSpeed.Y);
        }
    }

    public override void OnShake(Vector2 amount)
    {
        this.imageOffset += amount;
        ShakeStaticMovers(amount);
    }

    private void OnDisable()
    {
        this.Active = this.Collidable = false;
        DisableStaticMovers();

        if (this.VisibleWhenDisabled)
            SetColor(this.DisabledColor);
        else
            this.Visible = false;
    }

    private void SetColor(Color color)
    {
        foreach (Component component in this.Components)
            if (component is Image image)
                image.Color = color;
    }

    private void OnEnable()
    {
        EnableStaticMovers();
        this.Active = this.Visible = this.Collidable = true;
        SetColor(Color.White);
    }

    public override void OnStaticMoverTrigger(StaticMover sm) => TriggerPlatform();

    public override void Update()
    {
        base.Update();

        if (!attached)
        {
            return;
        }

        Player playerRider = GetPlayerRider();
        if (playerRider is not null && playerRider.Speed.Y >= 0f)
            TriggerPlatform();
    }

    private void TriggerPlatform() => this.Platform?.OnStaticMoverTrigger(this.staticMover);

    // edited so that attached jumpthrus provide horizontal liftspeed
    public override void MoveHExact(int move)
    {
        if (!attached)
        {
            base.MoveHExact(move);
            return;
        }

        if (this.Collidable)
        {
            foreach (Actor entity in this.Scene.Tracker.GetEntities<Actor>())
            {
                if (entity.IsRiding(this))
                {
                    this.Collidable = false;
                    if (entity.TreatNaive)
                    {
                        entity.NaiveMove(Vector2.UnitX * move);
                    }
                    else
                    {
                        entity.MoveHExact(move);
                    }
                    entity.LiftSpeed = this.LiftSpeed;
                    this.Collidable = true;
                }
            }
        }

        this.X += move;
        MoveStaticMovers(Vector2.UnitX * move);
    }
}

using System.Collections.Generic;
using System;
using Celeste.Mod.Entities;
using System.Linq;
using YoctoHelper.Cores;

namespace YoctoHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/MoonBoostBlock")]
public class MoonBoostBlock : BaseSolid
{

    private EaseModes dashAnimEase;

    private float dashMomentum;

    private EaseModes sinkAnimEase;

    private float sinkMomentum;

    private float waveRange;

    private float waveFrequency;

    private bool spawnOffset;

    private bool upSpringReactionForce;

    private float sineWave;

    private bool awake;

    private bool hasGroup;

    private bool masterOfGroup;

    private Dictionary<Platform, Vector2> moves;

    private List<MoonBoostBlock> group;

    private List<JumpThru> jumpthrus;

    private Point groupBoundsMin;

    private Point groupBoundsMax;

    private TileGrid tiles;

    private MoonBoostBlock master;

    private float dashEaseValue;

    private Vector2 dashDirection;

    private float yLerp;

    private float sinkTimer;

    public MoonBoostBlock(Vector2 position, EntityData data, EntityID id) : base(position, data, id)
    {
        this.dashAnimEase = data.Enum<EaseModes>("dashAnimEase", EaseModes.QuadIn);
        this.dashMomentum = data.Float("dashMomentum", 1F);
        this.sinkAnimEase = data.Enum<EaseModes>("sinkAnimEase", EaseModes.SineInOut);
        this.sinkMomentum = data.Float("sinkMomentum", 1F);
        this.waveRange = data.Float("waveRange", 4F);
        this.waveFrequency = data.Float("waveFrequency", 1F);
        this.spawnOffset = data.Bool("spawnOffset", false);
        this.upSpringReactionForce = data.Bool("upSpringReactionForce", false);
        this.Old(data);
        base.SurfaceSoundIndex = SurfaceIndex.TileToIndex[base.tileType];
        base.Add(new LightOcclude(base.lightOcclude));
        this.sineWave = (this.spawnOffset ? 0F : Calc.Random.NextFloat((float)Math.PI * 2F));
        base.Depth = Depths.Solids;
    }

    private void Old(EntityData data)
    {
        this.dashAnimEase = data.Enum<EaseModes>("dashEase"); // EaseModes.QuadIn);
        this.sinkAnimEase = data.Enum<EaseModes>("sinkingEase"); // EaseModes.SineInOut);
        this.sinkMomentum = data.Float("sinkingMomentum"); // 1F);
        this.waveRange = data.Float("waveRange"); // 4F);
        this.waveFrequency = data.Float("waveFrequency"); // 1F);
        this.spawnOffset = data.Bool("spawnOffset"); // false);
        this.upSpringReactionForce = data.Bool("upSpringMomentum"); // false);
    }

    public MoonBoostBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data, id)
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        this.awake = true;
        if (!this.hasGroup)
        {
            this.masterOfGroup = true;
            this.moves = new Dictionary<Platform, Vector2>();
            this.group = new List<MoonBoostBlock>();
            this.jumpthrus = new List<JumpThru>();
            this.groupBoundsMin = new Point((int)base.X, (int)base.Y);
            this.groupBoundsMax = new Point((int)base.Right, (int)base.Bottom);
            this.AddToGroupAndFindChildren(this);
            Rectangle rectangle = new Rectangle(this.groupBoundsMin.X / 8, this.groupBoundsMin.Y / 8, (this.groupBoundsMax.X - this.groupBoundsMin.X) / 8 + 1, (this.groupBoundsMax.Y - this.groupBoundsMin.Y) / 8 + 1);
            VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
            foreach (MoonBoostBlock moonBoostBlock in this.group)
            {
                int intersectWidth = (int)(moonBoostBlock.X / 8F) - rectangle.X;
                int intersectHeight = (int)(moonBoostBlock.Y / 8F) - rectangle.Y;
                int blockWidth = (int)(moonBoostBlock.Width / 8F);
                int blockHeight = (int)(moonBoostBlock.Height / 8F);
                for (int i = intersectWidth; i < intersectWidth + blockWidth; i++)
                {
                    for (int j = intersectHeight; j < intersectHeight + blockHeight; j++)
                    {
                        virtualMap[i, j] = base.tileType;
                    }
                }
            }
            this.tiles = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour
            {
                EdgesExtend = false,
                EdgesIgnoreOutOfLevel = false,
                PaddingIgnoreOutOfLevel = false
            }).TileGrid;
            this.tiles.Position = new Vector2(this.groupBoundsMin.X - base.X, this.groupBoundsMin.Y - base.Y);
            base.Add(this.tiles);
        }
        this.TryToInitPosition();
    }

    private void AddToGroupAndFindChildren(MoonBoostBlock from)
    {
        if (from.X < this.groupBoundsMin.X)
        {
            this.groupBoundsMin.X = (int)from.X;
        }
        if (from.Y < this.groupBoundsMin.Y)
        {
            this.groupBoundsMin.Y = (int)from.Y;
        }
        if (from.Right > this.groupBoundsMax.X)
        {
            this.groupBoundsMax.X = (int)from.Right;
        }
        if (from.Bottom > this.groupBoundsMax.Y)
        {
            this.groupBoundsMax.Y = (int)from.Bottom;
        }
        from.hasGroup = true;
        from.OnDashCollide = this.OnDash;
        this.group.Add(from);
        this.moves.Add(from, from.Position);
        if (from != this)
        {
            from.master = this;
        }
        foreach (JumpThru jumpThru in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height)))
        {
            if (!this.jumpthrus.Contains(jumpThru))
            {
                this.AddJumpThru(jumpThru);
            }
        }
        foreach (JumpThru jumpThru in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2)))
        {
            if (!this.jumpthrus.Contains(jumpThru))
            {
                this.AddJumpThru(jumpThru);
            }
        }
        foreach (MoonBoostBlock moonBoostBlock in base.Scene.Tracker.GetEntities<MoonBoostBlock>().Cast<MoonBoostBlock>())
        {
            if (!moonBoostBlock.hasGroup && moonBoostBlock.tileType == base.tileType && (base.Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), moonBoostBlock) || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), moonBoostBlock)))
            {
                this.AddToGroupAndFindChildren(moonBoostBlock);
            }
        }
    }

    private void AddJumpThru(JumpThru jumpThru)
    {
        jumpThru.OnDashCollide = this.OnDash;
        this.jumpthrus.Add(jumpThru);
        this.moves.Add(jumpThru, jumpThru.Position);
        foreach (MoonBoostBlock moonBoostBlock in base.Scene.Tracker.GetEntities<MoonBoostBlock>())
        {
            if (!moonBoostBlock.hasGroup && moonBoostBlock.tileType == base.tileType && base.Scene.CollideCheck(new Rectangle((int)jumpThru.X - 1, (int)jumpThru.Y, (int)jumpThru.Width + 2, (int)jumpThru.Height), moonBoostBlock))
            {
                this.AddToGroupAndFindChildren(moonBoostBlock);
            }
        }
    }

    private DashCollisionResults OnDash(Player player, Vector2 direction)
    {
        if (this.masterOfGroup && this.dashEaseValue <= 0.2F)
        {
            this.dashEaseValue = 1F;
            this.dashDirection = direction;
        }
        return DashCollisionResults.NormalOverride;
    }

    private void TryToInitPosition()
    {
        if (this.masterOfGroup)
        {
            foreach (MoonBoostBlock moonBoostBlock in this.group)
            {
                if (!moonBoostBlock.awake)
                {
                    return;
                }
            }
            this.MoveToTarget();
        }
        else
        {
            this.master.TryToInitPosition();
        }
    }

    private void MoveToTarget()
    {
        float waveValue = (float)Math.Sin(this.sineWave) * this.waveRange;
        Vector2 momentum = Calc.YoYo(EaseUtils.GetEaser(this.dashAnimEase, this.dashEaseValue)) * this.dashDirection * 8 * this.dashMomentum;
        for (int i = 0; i < 2; i++)
        {
            foreach (KeyValuePair<Platform, Vector2> move in this.moves)
            {
                Platform key = move.Key;
                bool flag = false;
                JumpThru jumpThru = key as JumpThru;
                Solid solid = key as Solid;
                if ((jumpThru != null && jumpThru.HasRider()) || (solid != null && solid.HasRider()))
                {
                    flag = true;
                }
                if ((flag || i != 0) && (!flag || i != 1))
                {
                    Vector2 value = move.Value;
                    float lerp = MathHelper.Lerp(value.Y, value.Y + 12F * this.sinkMomentum, EaseUtils.GetEaser(this.sinkAnimEase, this.yLerp)) + waveValue;
                    key.MoveToY(lerp + momentum.Y);
                    key.MoveToX(value.X + momentum.X);
                }
            }
        }
    }

    public override void OnStaticMoverTrigger(StaticMover staticMover)
    {
        if (staticMover.Entity is Spring)
        {
            switch ((staticMover.Entity as Spring).Orientation)
            {
                case Spring.Orientations.Floor:
                    this.sinkTimer = 0.5F;
                    if (this.upSpringReactionForce)
                    {
                        this.dashEaseValue = 1F;
                        this.dashDirection = Vector2.UnitY;
                    }
                    break;
                case Spring.Orientations.WallLeft:
                    this.dashEaseValue = 1F;
                    this.dashDirection = -Vector2.UnitX;
                    break;
                case Spring.Orientations.WallRight:
                    this.dashEaseValue = 1F;
                    this.dashDirection = Vector2.UnitX;
                    break;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (this.masterOfGroup)
        {
            bool flag = false;
            foreach (MoonBoostBlock moonBoostBlock in this.group)
            {
                if (moonBoostBlock.HasPlayerRider())
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                foreach (JumpThru jumpthru in this.jumpthrus)
                {
                    if (jumpthru.HasPlayerRider())
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                this.sinkTimer = 0.3F;
            }
            else if (this.sinkTimer > 0F)
            {
                this.sinkTimer -= Engine.RawDeltaTime;
            }
            this.yLerp = Calc.Approach(this.yLerp, (this.sinkTimer > 0F) ? 1F : 0F, 1F * Engine.RawDeltaTime);
            this.sineWave += Engine.RawDeltaTime * this.waveFrequency;
            this.dashEaseValue = Calc.Approach(this.dashEaseValue, 0F, Engine.RawDeltaTime * 1.5F);
            this.MoveToTarget();
        }
        base.LiftSpeed = Vector2.Zero;
    }

    public override void OnShake(Vector2 amount)
    {
        if (!this.masterOfGroup)
        {
            return;
        }
        base.OnShake(amount);
        this.tiles.Position += amount;
        foreach (JumpThru jumpthru in this.jumpthrus)
        {
            foreach (Component component in jumpthru.Components)
            {
                if (component is Image image)
                {
                    image.Position += amount;
                }
            }
        }
    }

}

using ChroniaHelper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Cores;

[Tracked(true)]
public class GroupedBaseSolid : Solid
{
    public GroupedBaseSolid(EntityData data, Vc2 offset, bool safe = false) : base(data.Position + offset, data.Width, data.Height, safe)
    {
        Nodes = data.NodesWithPosition(offset);
    }
    public int PlayerTouch;
    public Vc2[] Nodes;
    #region Groupping
    public List<GroupedBaseSolid> Group = new();
    public List<JumpThru> Jumpthrus = new();
    public Point GroupBoundsMin;
    public Point GroupBoundsMax;
    public int GroupTag = 0;
    public bool MasterOfGroup = false;
    public bool climbFall, standFall;
    public bool Triggered = false, canTrigger = false;
    public ParticleType FallDustA = Celeste.FallingBlock.P_FallDustA;
    public ParticleType FallDustB = Celeste.FallingBlock.P_FallDustB;
    public ParticleType LandDust = Celeste.FallingBlock.P_LandDust;
    public char tileType;
    public GroupedBaseSolid master;
    public TileGrid grid;
    public bool bgTexture = false;
    public bool blendIn = false;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        AddToGroupAndFindChildren();

        PostGroupping();

        GenerateGrid();

        PostGridGenerating();
    }

    public virtual void PostGroupping() { }

    public virtual void PostGridGenerating() { }

    public virtual void GenerateGrid()
    {
        if (MasterOfGroup)
        {
            // After finding group, the GroupBoundMin and GroupBoundMax are modified
            // Start building tilemap
            Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax - GroupBoundsMin).X / 8 + 1, (GroupBoundsMax - GroupBoundsMin).Y / 8 + 1);
            VirtualMap<char> charMap = new(rectangle.Width, rectangle.Height, '0');
            foreach (var item in Group)
            {
                int num = (int)(item.X / 8f) - rectangle.X; // Start X
                int num2 = (int)(item.Y / 8f) - rectangle.Y; // Start Y
                int num3 = (int)(item.Width / 8f); // Width
                int num4 = (int)(item.Height / 8f); // Height
                // Generate Tile Map
                for (int i = num; i < num + num3; i++)
                {
                    for (int j = num2; j < num2 + num4; j++)
                    {
                        charMap[i, j] = tileType;
                    }
                }
            }

            // Start generating tiles
            // Setting up tiling behaviour
            Autotiler.Behaviour tilingBehaviour = new()
            {
                EdgesExtend = false,
                EdgesIgnoreOutOfLevel = false,
                PaddingIgnoreOutOfLevel = false,
            };

            if (blendIn)
            {
                tilingBehaviour = new()
                {
                    EdgesExtend = true,
                    EdgesIgnoreOutOfLevel = true,
                    PaddingIgnoreOutOfLevel = true,
                };
            }
            grid = bgTexture
                ? GFX.BGAutotiler.GenerateMap(charMap, tilingBehaviour).TileGrid
                : GFX.FGAutotiler.GenerateMap(charMap, tilingBehaviour).TileGrid;
            // Grid Position is relative to the entity
            grid.Position = new Vc2(GroupBoundsMin.X - X, GroupBoundsMin.Y - Y);
            Add(grid);
            Add(new TileInterceptor(grid, false));
        }
    }

    public virtual bool PlayerFallCheck()
    {
        foreach (GroupedBaseSolid block in Group)
        {
            if (block.climbFall)
            {
                if (!block.standFall && block.HasPlayerOnTop())
                {
                    return false;
                }
                if (block.HasPlayerRider())
                {
                    return true;
                }
            }
            else if (this.standFall)
            {
                if (block.HasPlayerOnTop())
                {
                    return true;
                }
            }
        }
        foreach (JumpThru jp in Jumpthrus)
        {
            if (jp.HasPlayerRider())
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool PlayerWaitCheck()
    {
        if (Triggered && this.canTrigger)
        {
            return true;
        }
        if (PlayerFallCheck())
        {
            return true;
        }
        if (climbFall)
        {
            foreach (GroupedBaseSolid block in Group)
            {
                if (!block.CollideCheck<Player>(Position - Vector2.UnitX))
                {
                    if (CollideCheck<Player>(Position + Vector2.UnitX))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
        if (this.standFall)
        {
            foreach (GroupedBaseSolid block in Group)
            {
                if (!block.CollideCheck<Player>(base.Position - Vector2.UnitY))
                {
                    if (base.CollideCheck<Player>(base.Position + Vector2.UnitY))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void LandParticles()
    {
        foreach (GroupedBaseSolid block in Group)
        {
            for (int i = 2; i <= block.Width; i += 4)
            {
                foreach (Solid solid in Scene.Tracker.GetEntities<Solid>())
                {
                    if (solid is GroupedKillerBlock) continue;
                    if (block.CollideCheck(solid, block.Position + new Vector2(i, 3f)))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(FallDustA, 1, new Vector2(block.X + i, block.Bottom), Vector2.One * 4f, (float)-Math.PI / 2f);
                        float direction = (!(i < block.Width / 2f)) ? 0f : ((float)Math.PI);
                        SceneAs<Level>().ParticlesFG.Emit(LandDust, 1, new Vector2(block.X + i, block.Bottom), Vector2.One * 4f, direction);
                    }
                }
            }
        }
    }

    public void ShakeSfx()
    {
        Vector2 center = GetGroupCenter();
        if (tileType == '3')
        {
            Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", center);
        }
        else if (tileType == '9')
        {
            Audio.Play("event:/game/03_resort/fallblock_wood_shake", center);
        }
        else if (tileType == 'g')
        {
            Audio.Play("event:/game/06_reflection/fallblock_boss_shake", center);
        }
        else
        {
            Audio.Play("event:/game/general/fallblock_shake", center);
        }
    }

    public void ImpactSfx()
    {
        Vector2 bottomCenter = GetGroupBottomCenter();
        if (tileType == '3')
        {
            Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", bottomCenter);
        }
        else if (tileType == '9')
        {
            Audio.Play("event:/game/03_resort/fallblock_wood_impact", bottomCenter);
        }
        else if (tileType == 'g')
        {
            Audio.Play("event:/game/06_reflection/fallblock_boss_impact", bottomCenter);
        }
        else
        {
            Audio.Play("event:/game/general/fallblock_impact", bottomCenter);
        }
    }

    public Vector2 GetGroupCenter()
    {
        float area = 0;
        float sumX = 0;
        float sumY = 0;
        foreach (GroupedBaseSolid block in Group)
        {
            float blockArea = block.Width * block.Height;
            float distX = block.CenterX - CenterX;
            float distY = block.CenterY - CenterY;
            area += blockArea;
            sumX += distX * blockArea;
            sumY += distY * blockArea;
        }
        return Center + new Vector2(sumX / area, sumY / area);
    }

    public Vector2 GetGroupBottomCenter()
    {
        float area = 0;
        float sumX = 0;
        float maxY = float.MinValue;
        foreach (GroupedBaseSolid block in Group)
        {
            float blockArea = block.Width * block.Height;
            float distX = block.CenterX - CenterX;
            area += blockArea;
            sumX += distX * blockArea;
            if (block.Bottom > maxY)
            {
                maxY = block.Bottom;
            }
        }
        return new Vector2(CenterX + sumX / area, maxY);
    }


    public void Trigger()
    {
        if (MasterOfGroup)
        {
            Triggered = true;
        }
        else
        {
            master.Triggered = true;
        }
    }

    public void TriggerAll()
    {
        foreach(var i in Group)
        {
            i.Triggered = true;
        }
    }
    
    public void AddToGroupAndFindChildren()
    {
        // From the 2nd block loading in Awake, this function can be ignored
        if (GroupTag != 0)
        {
            return;
        }
        
        // Prepare to look for members
        MasterOfGroup = true;
        master = this;
        Group.Clear();
        Jumpthrus.Clear();
        GroupBoundsMin = new((int)X, (int)Y);
        GroupBoundsMax = new((int)Right, (int)Bottom);
        
        // Finding matches, including self
        // Note that each group of the blocks would only execute this once
        Group.Add(this);
        int c = 0;
        List<GroupedBaseSolid> solidsToAdd = new();
        while (c != Group.Count)
        {
            c = Group.Count;
            foreach (GroupedBaseSolid item in Group)
            {
                foreach (GroupedBaseSolid entity in MaP.level.Tracker.GetEntities<GroupedBaseSolid>())
                {
                    if (Group.Contains(entity))
                    {
                        continue;
                    }
                    
                    bool arg1 = entity.GroupTag == 0;
                    bool arg2 = entity.tileType == this.tileType;
                    if (!arg1 || !arg2 || !item.ShouldAddIntoGroup(entity))
                    {
                        continue;
                    }
                    
                    bool arg = Scene.CollideCheck(
                                   new Rectangle((int)item.X - 1, (int)item.Y,
                                       (int)item.Width + 2, (int)item.Height),
                                   entity)
                               || base.Scene.CollideCheck(
                                   new Rectangle((int)item.X, (int)item.Y - 1,
                                       (int)item.Width, (int)item.Height + 2),
                                   entity);

                    if (!arg)
                    {
                        continue;
                    }
                    
                    solidsToAdd.Add(entity);
                    // Adjusting bounds
                    if (entity.X < (float)GroupBoundsMin.X)
                    {
                        GroupBoundsMin.X = (int)entity.X;
                    }
                    if (entity.Y < (float)GroupBoundsMin.Y)
                    {
                        GroupBoundsMin.Y = (int)entity.Y;
                    }
                    if (entity.Right > (float)GroupBoundsMax.X)
                    {
                        GroupBoundsMax.X = (int)entity.Right;
                    }
                    if (entity.Bottom > (float)GroupBoundsMax.Y)
                    {
                        GroupBoundsMax.Y = (int)entity.Bottom;
                    }
                }
            }

            foreach (var item in solidsToAdd)
            {
                Group.Add(item);
            }
            Group = Group.Distinct().ToList();
        }
        
        // Finished groupping then...
        foreach (var solid in Group)
        {
            // Adding jumpthrus
            foreach (JumpThru item in Scene.CollideAll<JumpThru>(new Rectangle((int)solid.X - 1, (int)solid.Y, (int)solid.Width + 2, (int)solid.Height)))
            {
                Jumpthrus.Enter(item);
            }
            foreach (JumpThru item2 in Scene.CollideAll<JumpThru>(new Rectangle((int)solid.X, (int)solid.Y - 1, (int)solid.Width, (int)solid.Height + 2)))
            {
                Jumpthrus.Enter(item2);
            }
        }
        
        string s = $"Group of {this.GetID()}:";
        foreach (var item in Group)
        {
            s += $" {item.GetID()}";
        }
        //Log.Info(s);
        //Log.Info($"Master of {this.GetID()}: {master.GetID()}");
        
        // Set Group master
        // Reassign attributes
        foreach (var item in Group)
        {
            if (item == this)
            {
                continue;
            }
            item.GroupTag = 0;
            item.MasterOfGroup = false;
        }
       // Group.Sort((block, otherBlock) =>
       //      otherBlock.SourceData.ID - block.SourceData.ID);
        // Group.First().MasterOfGroup = true;
        // Group setup
        int id = RandomUtils.RandomInt();
        foreach (var item in Group)
        {
            item.GroupTag = id;
            item.master = this;
            item.GroupBoundsMin = GroupBoundsMin;
            item.GroupBoundsMax = GroupBoundsMax;
            item.Group = Group;
            item.Jumpthrus = Jumpthrus;
        }
    }
    
    public virtual bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        return true;
    }

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        Trigger();
    }

    public override void OnShake(Vector2 amount)
    {
        if (MasterOfGroup)
        {
            base.OnShake(amount);
            grid.Position += amount;
            foreach (JumpThru jumpthru in Jumpthrus)
            {
                foreach (Component component in jumpthru.Components)
                {
                    Image image = component as Image;
                    if (image != null)
                    {
                        image.Position += amount;
                    }
                }
            }
        }
    }
    #endregion

    #region Touch Getter and Killer Setups
    public float topKillTimer = -1f;
    public float bottomKillTimer = -1f;
    public float leftKillTimer = -1f;
    public float rightKillTimer = -1f;
    public float currentKillTimer = -1f;
    public int GetPlayerTouch()
    {
        foreach (Player player in MaP.level.Tracker.GetEntities<Player>())
        {
            if (CollideCheck(player, Position - Vector2.UnitY))
            {
                return 1; // up
            }
            if (CollideCheck(player, Position + Vector2.UnitY))
            {
                return 2; // down
            }
            if (player.Facing == Facings.Right && CollideCheck(player, Position - Vector2.UnitX))
            {
                return 3; // left
            }
            if (player.Facing == Facings.Left && CollideCheck(player, Position + Vector2.UnitX))
            {
                return 4; // right
            }
        }
        return 0;
    }

    public void TimedKill()
    {
        PlayerTouch = GetPlayerTouch();
        if (PlayerTouch > 0)
        {
            if (topKillTimer == 0 && PlayerTouch == 1)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (bottomKillTimer == 0 && PlayerTouch == 2)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (leftKillTimer == 0 && PlayerTouch == 3)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else if (rightKillTimer == 0 && PlayerTouch == 4)
            {
                Player player = MaP.level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    return;
                }
                player.Die((player.Position - Position).SafeNormalize());
            }
            else
            {
                if (currentKillTimer > 0)
                {
                    currentKillTimer -= Engine.DeltaTime;
                    if (currentKillTimer <= 0)
                    {
                        Player player = MaP.level.Tracker.GetEntity<Player>();
                        if (player == null)
                        {
                            return;
                        }
                        player.Die((player.Position - Position).SafeNormalize());
                    }
                }
                else
                {
                    currentKillTimer = PlayerTouch switch
                    {
                        1 => topKillTimer,
                        2 => bottomKillTimer,
                        3 => leftKillTimer,
                        4 => rightKillTimer,
                        _ => -1
                    };
                }
            }
        }
        else
        {
            currentKillTimer = -1;
        }
    }
    #endregion

    #region 组内同步
    /// <summary>
    /// 设置组内所有成员的可见性与碰撞开关。
    /// </summary>
    public void SetGroupVisibility(bool value)
    {
        foreach (GroupedBaseSolid block in Group)
        {
            block.Collidable = block.Visible = value;
        }
    }

    /// <summary>
    /// 把组内成员统一为指定配置。
    /// </summary>
    public void SyncGroup<T>(Action<T> apply) where T : GroupedBaseSolid
    {
        foreach (GroupedBaseSolid block in Group)
        {
            if (block is T target)
            {
                apply(target);
            }
        }
    }

    /// <summary>
    /// 按移动方向排序组内成员：沿移动方向「最靠前」的排在前面，
    /// 使其先让开位置，其余成员移动时不会与同组成员互相误判为碰撞。
    /// </summary>
    public void SortGroupByMoveDirection(Vc2 moveDir)
    {
        Group.Sort((a, b) =>
            Vc2.Dot(b.Center, moveDir).CompareTo(Vc2.Dot(a.Center, moveDir)));
    }
    #endregion

    #region 组内移动
    /// <summary>
    /// 记录各分组内各骑乘者最近一次被带动的时间，用于保证同一组在一帧内
    /// 只会带动同一骑乘者一次，避免玩家同时位于多块交界处时位移翻倍。
    /// </summary>
    private static readonly Dictionary<(int, Actor), float> carriedStamp = new();

    /// <summary>
    /// 判断当前骑乘者是否应由本组带动：本帧尚未被本组带动过则返回 true。
    /// 独立方块不存在重复带动的问题，始终返回 true。
    /// </summary>
    private bool TryCarryRider(Actor actor)
    {
        if (Group.Count <= 1)
        {
            return true;
        }

        float now = Scene == null ? 0f : Scene.TimeActive;

        if (carriedStamp.TryGetValue((GroupTag, actor), out float last) && last == now)
        {
            return false;
        }

        carriedStamp[(GroupTag, actor)] = now;
        return true;
    }

    /// <summary>
    /// 竖直方向精确移动；骑乘者每帧每组只被带动一次。
    /// </summary>
    public override void MoveVExact(int move)
    {
        MoveGroupExact(move, true);
    }

    /// <summary>
    /// 水平方向精确移动；骑乘者每帧每组只被带动一次。
    /// </summary>
    public override void MoveHExact(int move)
    {
        MoveGroupExact(move, false);
    }

    /// <summary>
    /// 组内方块的精确移动：沿指定轴移动自身并同步带动骑乘者，
    /// 同时把移动路径上的其他角色推开。
    /// </summary>
    protected virtual void MoveGroupExact(int move, bool vertical)
    {
        Vc2 dir = vertical ? Vc2.UnitY : Vc2.UnitX;

        GetRiders();

        float top = Top;
        float bottom = Bottom;
        float left = Left;
        float right = Right;

        Position += dir * move;
        MoveStaticMovers(dir * move);


        if (!Collidable)
        {
            return;
        }

        foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
        {
            if (!entity.AllowPushing)
            {
                continue;
            }

            bool collidable = entity.Collidable;
            entity.Collidable = true;

            if (!entity.TreatNaive && CollideCheck(entity, Position))
            {

                Collidable = false;

                if (vertical)
                {
                    int moveV = (move <= 0) ? (move - (int)(entity.Bottom - top)) : (move - (int)(entity.Top - bottom));
                    entity.MoveVExact(moveV, entity.SquishCallback, this);
                }
                else
                {
                    int moveH = (move <= 0) ? (move - (int)(entity.Right - left)) : (move - (int)(entity.Left - right));
                    entity.MoveHExact(moveH, entity.SquishCallback, this);
                }

                entity.LiftSpeed = LiftSpeed;
                Collidable = true;
            }
            else if (riders.Contains(entity))
            {
                bool carry = TryCarryRider(entity);

                if (!carry)
                {
                    entity.Collidable = collidable;
                    continue;
                }

                // 玩家可能同时位于组内多块的交界处，带动他时需让这些方块暂时不参与碰撞，
                // 否则他会在移动途中被相邻的同组方块挡住而无法跟随。
                List<GroupedBaseSolid> ridingBlocks = new();

                foreach (GroupedBaseSolid block in Group)
                {
                    if (block.Collidable && entity.IsRiding(block))
                    {
                        ridingBlocks.Add(block);
                        block.Collidable = false;
                    }
                }

                Collidable = false;

                if (entity.TreatNaive)
                {
                    entity.NaiveMove(dir * move);
                }
                else if (vertical)
                {
                    entity.MoveVExact(move);
                }
                else
                {
                    entity.MoveHExact(move);
                }

                entity.LiftSpeed = LiftSpeed;

                Collidable = true;

                foreach (GroupedBaseSolid block in ridingBlocks)
                {
                    block.Collidable = true;
                }
            }

            entity.Collidable = collidable;
        }

        riders.Clear();
    }
    #endregion
}

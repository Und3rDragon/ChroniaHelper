using Celeste.Mod.CommunalHelper.Entities;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Entities.FormulaBlocks;
using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ChroniaHelper.Entities;

/// <summary>
/// 属性相同且相互贴合的冲刺方块，会自动连接成一组；
/// 玩家冲刺撞到组内任意一块时，整组会一同破碎。
/// </summary>
[Tracked]
[CustomEntity("ChroniaHelper/ConnectedDashBlock")]
public class ConnectedDashBlock : GroupedBaseSolid
{
    public bool permanent;

    public bool blendIn;

    public bool canDash;

    public ConnectedDashBlock(EntityData data, Vc2 offset) : base(data, offset)
    {
        permanent = data.Bool("permanent", true);
        blendIn = data.Bool("blendin", false);
        canDash = data.Bool("canDash", true);
        tileType = data.Char("tiletype", '3');
        bgTexture = data.Bool("bgTexture", false);

        Safe = true;
        Depth = data.Int("depth", blendIn ? -10501 : -12999);
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
        OnDashCollide = OnDashed;
    }

    public override void PostGroupping()
    {
        // 组确定后，把母体属性同步给组内每个成员，并记录整组标识。
        foreach (ConnectedDashBlock block in Group)
        {
            block.permanent = (master as ConnectedDashBlock).permanent;
            block.canDash = (master as ConnectedDashBlock).canDash;
            block.blendIn = (master as ConnectedDashBlock).blendIn;
            block.tileType = (master as ConnectedDashBlock).tileType;
            block.Depth = (master as ConnectedDashBlock).Depth;
        }

        bool collided = false;
        foreach(var block in Group)
        {
            if (block.CollideCheck<Player>())
            {
                collided = true;
            }
        }
        if (collided)
        {
            foreach (var block in Group)
            {
                block.RemoveSelf();
            }
        }
    }

    public override void GenerateGrid()
    {
        if (blendIn && MasterOfGroup)
        {
            GenerateBlendInGrid();
            return;
        }

        base.GenerateGrid();
    }

    /// <summary>
    /// 生成融合进场景实体的贴图网格。
    /// 将整组方块形状与场景实体数据合成同一张网格数据，
    /// 使分组后保持原有轮廓，并让边缘与周围地形自然衔接。
    /// </summary>
    private void GenerateBlendInGrid()
    {
        Level level = SceneAs<Level>();
        Rectangle tileBounds = level.Session.MapData.TileBounds;
        VirtualMap<char> solidsData = level.SolidsData;

        Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax - GroupBoundsMin).X / 8 + 1, (GroupBoundsMax - GroupBoundsMin).Y / 8 + 1);
        VirtualMap<char> charMap = new(rectangle.Width, rectangle.Height, '0');

        // 铺上场景实体数据，让边缘判定能看到周围地形
        for (int i = 0; i < rectangle.Width; i++)
        {
            for (int j = 0; j < rectangle.Height; j++)
            {
                charMap[i, j] = solidsData[rectangle.X + i - tileBounds.Left, rectangle.Y + j - tileBounds.Top];
            }
        }

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

        Autotiler.Behaviour behaviour = default(Autotiler.Behaviour);

        grid = bgTexture
            ? GFX.BGAutotiler.Generate(charMap, 0, 0, rectangle.Width, rectangle.Height, forceSolid: false, '0', behaviour).TileGrid
            : GFX.FGAutotiler.Generate(charMap, 0, 0, rectangle.Width, rectangle.Height, forceSolid: false, '0', behaviour).TileGrid;

        // 抹去不属于本组方块的部分，只保留方块自身的贴图
        for (int i = 0; i < rectangle.Width; i++)
        {
            for (int j = 0; j < rectangle.Height; j++)
            {
                int num = rectangle.X + i;
                int num2 = rectangle.Y + j;
                bool inGroup = false;
                foreach (var item in Group)
                {
                    if (num >= (int)(item.X / 8f) && num < (int)(item.X / 8f) + (int)(item.Width / 8f)
                        && num2 >= (int)(item.Y / 8f) && num2 < (int)(item.Y / 8f) + (int)(item.Height / 8f))
                    {
                        inGroup = true;
                        break;
                    }
                }
                if (!inGroup)
                {
                    grid.Tiles[i, j] = null;
                }
            }
        }

        grid.Position = new Vc2(rectangle.X * 8 - X, rectangle.Y * 8 - Y);
        Add(grid);
        Add(new TileInterceptor(grid, false));
        Add(new EffectCutout());
    }

    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        if (other is ConnectedDashBlock block)
        {
            return block.tileType == tileType
                && block.canDash == canDash
                && block.blendIn == blendIn
                && block.Depth == Depth
                && block.permanent == permanent;
        }

        return false;
    }

    /// <summary>
    /// 玩家冲刺撞到方块时的处理，整组一同破碎。
    /// </summary>
    public DashCollisionResults OnDashed(Player player, Vc2 direction)
    {
        if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10)
        {
            return DashCollisionResults.NormalCollision;
        }

        if (MasterOfGroup)
        {
            foreach (ConnectedDashBlock block in Group)
            {
                block.Break(player.Center, direction);
            }
        }
        else
        {
            (master as ConnectedDashBlock).OnDashed(player, direction);
        }

        return DashCollisionResults.Rebound;
    }

    /// <summary>
    /// 破碎方块，播放音效并在每个格子生成碎屑。
    /// </summary>
    public void Break(Vc2 from, Vc2 direction, bool playSound = true, bool playDebrisSound = true)
    {
        if (playSound)
        {
            if (tileType == '1')
            {
                Audio.Play("event:/game/general/wall_break_dirt", Position);
            }
            else if (tileType == '3')
            {
                Audio.Play("event:/game/general/wall_break_ice", Position);
            }
            else if (tileType == '9')
            {
                Audio.Play("event:/game/general/wall_break_wood", Position);
            }
            else
            {
                Audio.Play("event:/game/general/wall_break_stone", Position);
            }
        }

        for (int i = 0; i < Width / 8f; i++)
        {
            for (int j = 0; j < Height / 8f; j++)
            {
                Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vc2(4 + i * 8, 4 + j * 8), tileType, playDebrisSound).BlastFrom(from));
            }
        }

        Collidable = false;
        if (permanent)
        {
            RemoveAndFlagAsGone();
        }
        else
        {
            RemoveSelf();
        }
    }

    /// <summary>
    /// 移除方块并记录其标识，使其不再随房间重置而恢复。
    /// </summary>
    public void RemoveAndFlagAsGone()
    {
        RemoveSelf();
        Level level = SceneAs<Level>();
        foreach (var item in Group)
        {
            level.Session.DoNotLoad.Add(item.GetEID());
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Celeste.Celeste.Freeze(0.05f);
    }
}

using ChroniaHelper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChroniaHelper.Utils;

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
    public List<GroupedBaseSolid> Group;
    public List<JumpThru> Jumpthrus;
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

    public bool PlayerFallCheck()
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

    public bool PlayerWaitCheck()
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
        Group = new();
        Jumpthrus = new();
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
        if (MasterOfGroup)
        {
            Triggered = true;
        }
        else
        {
            master.Triggered = true;
        }
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
}

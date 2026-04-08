using ChroniaHelper.Cores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Pico8;
using ChroniaHelper.Components;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using YamlDotNet.Core.Events;
using Classic = On.Celeste.Pico8.Classic;

namespace ChroniaHelper.WIPs.Entities;

[WorkingInProgress]
[CustomEntity("ChroniaHelper/ClockworkBlock")]
[Tracked]
public class ClockworkBlock : GroupedBaseSolid
{
    public ClockworkBlock(EntityData data, Vc2 offset) : base(data, offset)
    {
        if (Nodes.Length <= 1)
        {
            direction = Vector2.Zero;
        }
        else
        {
            direction = Nodes[1] - Nodes[0];
        }
        duration = data.Float("duration", 1f).ClampMin(Engine.DeltaTime / 2f);
        maxDuration = data.Float("maxMoveDuration", -1f);
        if (maxDuration > 0)
        {
            maxDuration = maxDuration.ClampMin(duration);
        }
        startDelay = data.Float("startDelay", -1f);
        tileType = data.Char("tiletype", '3');
        Depth = data.Int("depth", Depths.Solids);
        flag = data.Attr("flag", "flag");
        bgTexture = data.Bool("bgTexture", false);
        shouldReturn = data.Bool("returnOnFlagDisable", true);
        returnDuration = data.Float("returnDuration", 0.5f).ClampMin(Engine.DeltaTime / 2f);
        
        Add(new LightOcclude());
        SurfaceSoundIndex = data.Int("surfaceSoundIndex", 8);

        flagListener = new(flag)
        {
            onEnable = OnEnable,
            onDisable = OnDisable,
        };
        Add(flagListener);
    }
    public float duration, startDelay = -1f, maxDuration = -1f;
    public bool instantStart => startDelay <= 0f;
    public string flag;
    public bool bgTexture = false;
    public bool shouldReturn = true;
    public float returnDuration = 0.5f;
    private FlagListener flagListener;

    private Vc2 direction;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        routineState = RoutineStates.None;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        AddToGroupAndFindChildren();
        // Direction modulation, most of the blocks shouldn't have a direction
        // After groupping, the Group is settled, so we can use the Group to see if the direction should be modified
        foreach (var item in Group)
        {
            if ((item as ClockworkBlock).direction != Vc2.Zero)
            {
                direction = (item as ClockworkBlock).direction;
            }
            // Unite values, although this is actually not that necessary
            (item as ClockworkBlock).maxDuration = (master as ClockworkBlock).maxDuration;
            (item as ClockworkBlock).duration = (master as ClockworkBlock).duration;
            (item as ClockworkBlock).startDelay = (master as ClockworkBlock).startDelay;
        }
        Point delta = GroupBoundsMax - GroupBoundsMin;
        if (MasterOfGroup)
        {
            // After finding group, the GroupBoundMin and GroupBoundMax are modified
            // Start building tilemap
            Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, delta.X / 8 + 1, delta.Y / 8 + 1);
            VirtualMap<char> charMap = new(rectangle.Width, rectangle.Height, '0');
            foreach (var item in Group)
            {
                int num = (int) (item.X / 8f) - rectangle.X; // Start X
                int num2 = (int) (item.Y / 8f) - rectangle.Y; // Start Y
                int num3 = (int) (item.Width / 8f); // Width
                int num4 = (int) (item.Height / 8f); // Height
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
            grid = bgTexture
                ? GFX.BGAutotiler.GenerateMap(charMap, tilingBehaviour).TileGrid
                : GFX.FGAutotiler.GenerateMap(charMap, tilingBehaviour).TileGrid;
            // Grid Position is relative to the entity
            grid.Position = new Vc2(GroupBoundsMin.X - X, GroupBoundsMin.Y - Y);
            Add(grid);
            Add(new TileInterceptor(grid, false));
        }
    }

    public override bool ShouldAddIntoGroup(GroupedBaseSolid other)
    {
        if (other is ClockworkBlock block)
        {
            float e = 0.001f;
            return block.flag == flag && 
                   block.Depth.IsBetween(Depth -10, Depth + 10) &&
                   block.startDelay.IsBetween(startDelay - e, startDelay + e) &&
                   block.duration.IsBetween(duration - e, duration + e) &&
                   block.maxDuration.IsBetween(maxDuration - e, maxDuration + e);
        }
        
        return false;
    }

    private Coroutine normalRoutine, resetRoutine;

    public void InitiateNormalListener()
    {
        normalRoutine = new(NormalSequence());
    }

    public void InitiateResetRoutine()
    {
        resetRoutine = new(ResetSequence());
    }
    
    private IEnumerator NormalSequence()
    {
        // Shaking
        if (!instantStart)
        {
            ShakeSfx();
            StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return startDelay;
            StopShaking();
        }

        Vc2 start = Position;
        float timer = 0f, progress = 0f;
        while (true)
        {
            timer += Engine.DeltaTime;
            progress = timer / duration;
            
            MoveTo(start.CalculatePointer(start + direction, progress));

            if (maxDuration > 0f && timer >= maxDuration)
            {
                break;
            }
            yield return null;
        }
    }

    private IEnumerator ResetSequence()
    {
        Vc2 start = Position;
        float timer = 0f, progress = 0f;

        while (true)
        {
            timer += Engine.DeltaTime;
            progress = timer / 0.5f;
            
            MoveTo(progress.LerpValue(0f, 1f, start, Nodes[0]));

            yield return null;
        }
    }

    public enum RoutineStates
    {
        None, Normal, Reset
    }
    private RoutineStates routineState = RoutineStates.None;
    public void OnEnable()
    {
        InitiateNormalListener();
        routineState = RoutineStates.Normal;
    }

    public void OnDisable()
    {
        InitiateResetRoutine();
        routineState = shouldReturn ? RoutineStates.Reset : RoutineStates.None;
    }

    public override void Update()
    {
        base.Update();

        // Routine Updates
        if (routineState == RoutineStates.Normal)
        {
            normalRoutine.Update();
        }
        else if (routineState == RoutineStates.Reset)
        {
            resetRoutine.Update();
        }
    }

    public bool CheckInside()
    {
        Rectangle levelBound = new(MaP.level.Bounds.X - 32, MaP.level.Bounds.Y - 32,
            MaP.level.Bounds.Width + 64, MaP.level.Bounds.Height + 64);
        Rectangle bound = new(GroupBoundsMin.X, GroupBoundsMin.Y,
            GroupBoundsMax.X - GroupBoundsMin.X,
            GroupBoundsMax.Y - GroupBoundsMin.Y);
        return levelBound.Contains(bound);
    }
}

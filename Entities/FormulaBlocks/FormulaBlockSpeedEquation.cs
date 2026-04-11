using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Entities.FormulaBlocks;

[CustomEntity("ChroniaHelper/FormulaBlockSpeedEquation")]
[Tracked]
[Credits("Thanks to AppleSheep for delegation help")]
public class FormulaBlockSpeedEquation : GroupedBaseSolid
{
    public FormulaBlockSpeedEquation(EntityData data, Vc2 offset) : base(data, offset)
    {
        functionX = data.Attr("functionVX");
        if (!functionX.HasValidContent())
        {
            functionX = "0";
        }
        functionY = data.Attr("functionVY");
        if (!functionY.HasValidContent())
        {
            functionY = "0";
        }
        startDelay = data.Float("startDelay", -1f);
        tileType = data.Char("tiletype", '3');
        normalRoutine = new Coroutine(NormalSequence());
        flag = new(data.Attr("flag", "flag"));
        flag.onTrue = RoutineUpdate;
        Add(flag);
        bgTexture = data.Bool("bgTexture", false);
        Add(new LightOcclude());
        SurfaceSoundIndex = data.Int("surfaceSoundIndex", 8);
        Depth = data.Int("depth", Depths.Solids);
        maxMoveDuration = data.Float("maxMoveDuration", -1f);
        expressionType = data.Int("expressionType", 1);
    }
    public string functionX, functionY;
    public float startDelay = -1f, maxMoveDuration = -1f;
    public bool instantStart => startDelay > 0f;
    public bool bgTexture;
    public int surfaceSoundIndex;
    public FlagListener flag;
    public int expressionType = 1;

    private float elapsed = 0f;
    
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        AddToGroupAndFindChildren();
        foreach (var item in Group)
        {
            (item as FormulaBlockSpeedEquation).startDelay = (master as FormulaBlockSpeedEquation).startDelay;
            (item as FormulaBlockSpeedEquation).maxMoveDuration = (master as FormulaBlockSpeedEquation).maxMoveDuration;
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
        if (other is FormulaBlockSpeedEquation block)
        {
            float e = 0.001f;
            //Log.Info($"[flag {block.GetID()} VS {this.GetID()}] {block.flag.Flag} : {this.flag.Flag}");
            //Log.Info($"[depth {block.GetID()} VS {this.GetID()}] {block.Depth} : {this.Depth}");
            //Log.Info($"[startDelay {block.GetID()} VS {this.GetID()}] {block.startDelay} : {this.startDelay}");
            //Log.Info($"[maxMoveDuration {block.GetID()} VS {this.GetID()}] {block.maxMoveDuration} : {this.maxMoveDuration}");
            //Log.Info($"[functionX {block.GetID()} VS {this.GetID()}] {block.functionX} : {this.functionX}");
            //Log.Info($"[functionY {block.GetID()} VS {this.GetID()}] {block.functionY} : {this.functionY}");
            return block.flag.Flag == flag.Flag && 
                   block.Depth == Depth &&
                   block.startDelay.IsBetween(startDelay - e, startDelay + e) &&
                   block.maxMoveDuration.IsBetween(maxMoveDuration - e, maxMoveDuration + e) &&
                   block.functionX == functionX &&
                   block.functionY == functionY;
        }
        
        return false;
    }

    private Coroutine normalRoutine;
    private float overflowProtection = 1000000f;
    public IEnumerator NormalSequence()
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
        
        Dictionary<string, Func<Session, object, object>> frostContext = new()
        {
            { "t", (session, str) => elapsed },
            { "time", (session, str) => elapsed }
        };
        
        while (true)
        {
            Vc2 delta = Vc2.Zero;
            delta.X = functionX.Calculate(expressionType, getVariable:GetVariable, simpleCommands:frostContext);
            delta.Y = functionY.Calculate(expressionType, getVariable:GetVariable, simpleCommands:frostContext);
            
            MoveH(delta.X * Engine.DeltaTime);
            MoveV(delta.Y * Engine.DeltaTime);
            
            if (elapsed >= overflowProtection ||
                (Position - Nodes[0]).X.GetAbs() > overflowProtection || 
                (Position - Nodes[0]).Y.GetAbs() > overflowProtection)
            {
                break;
            }
            
            yield return null;
        }
    }

    public void RoutineUpdate()
    {
        elapsed += Engine.DeltaTime;
        normalRoutine.Update();
    }

    public float GetVariable(string variable)
    {
        if (variable == "e") { return (float)Math.E; }
        if (new string[]{ "pi", "PI", "Pi" }.Contains(variable)) { return (float)Math.PI; }
        if (variable.ToLower() == "time" || variable.ToLower() == "t")
        {
            return elapsed;
        }
        
        return MaP.level?.Session.GetSlider(variable) ?? 0f;
    }
}
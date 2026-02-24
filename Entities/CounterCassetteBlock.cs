using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.References;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/CounterCassetteBlock")]
[Credits("Kyfex for Dash Toggle Heelper")]
public class CounterCassetteBlock : CassetteBlock
{
    [ChroniaHelper.Cores.LoadHook]
    public static void Load()
    {
        On.Celeste.CassetteBlock.FindInGroup += FindInGroupOverride;
        On.Celeste.CassetteBlock.CheckForSame += CheckForSameOverride;
        On.Celeste.CassetteBlock.SetImage += SetImageOverride;
    }
    [ChroniaHelper.Cores.UnloadHook]
    public static void Unload()
    {
        On.Celeste.CassetteBlock.FindInGroup -= FindInGroupOverride;
        On.Celeste.CassetteBlock.CheckForSame -= CheckForSameOverride;
        On.Celeste.CassetteBlock.SetImage -= SetImageOverride;
    }

    public static void FindInGroupOverride(On.Celeste.CassetteBlock.orig_FindInGroup orig, CassetteBlock self,
        CassetteBlock block)
    {
        if (self is CounterCassetteBlock dtSelf)
        {
            dtSelf.FindInGroupOverride(block);
        }
        else
        {
            orig(self, block);
        }
    }

    public static bool CheckForSameOverride(On.Celeste.CassetteBlock.orig_CheckForSame orig, CassetteBlock self,
        float x, float y)
    {
        if (self is CounterCassetteBlock dtSelf)
        {
            return dtSelf.CheckForSameOverride(x, y);
        }

        return orig(self, x, y);
    }

    public static void SetImageOverride(On.Celeste.CassetteBlock.orig_SetImage orig, CassetteBlock self, float x,
        float y, int tx, int ty)
    {
        if (self is CounterCassetteBlock dtSelf)
        {
            dtSelf.SetImageOverride(x, y, tx, ty);
        }
        else
        {
            orig(self, x, y, tx, ty);
        }
    }

    public string prefix, counter;
    private bool initialized;
    private ChroniaColor baseColor, backColor;
    public CounterCassetteBlock(Vector2 position, EntityID id, EntityData data)
        : base(data.Position + position, id, data.Width, data.Height, data.Int("counterValue", 0), -1f)
    {
        color = (baseColor = data.GetChroniaColor("color", Color.White)).Parsed();
        this.prefix = data.Attr("directory", "objects/ChroniaHelper/counterCassetteBlock/").TrimEnd('/') + '/';
        this.counter = data.Attr("counter", "counterCassetteBlockCounter");

        backColor = data.GetChroniaColor("disabledColor", "667da5");
        backColor.alpha = baseColor.alpha;
    }

    public CounterCassetteBlock(EntityData data, Vector2 offset, EntityID id)
        : this(offset, id, data)
    { }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        ShiftSize(1);
        side.color = Color.Transparent;
    }

    public override void Render()
    {
        base.Render();

        if(blockHeight > 0)
        {
            Draw.Rect(Position.X, Position.Y + Height, 
                Width, blockHeight, 
                backColor.Parsed(1.2f));
        }
    }

    public override void Update()
    {
        base.Update();
        if (!initialized)
        {
            var entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                Activated = entity.Dashes == Index;
                initialized = true;
            }
        }

        if (Index == counter.GetCounter()) Activated = true;
        else Activated = false;
    }

    public void FindInGroupOverride(CassetteBlock block)
    {
        foreach (CounterCassetteBlock entity in Scene.Tracker.GetEntities<CounterCassetteBlock>())
        {
            if (entity != this && entity != block && entity.Index == Index &&
                (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2,
                    (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1,
                    (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
            {
                group.Add(entity);
                FindInGroupOverride(entity);
                entity.group = this.group;
            }
        }
    }

    public bool CheckForSameOverride(float x, float y)
    {
        foreach (Entity maybe in Scene.Tracker.GetEntities<CounterCassetteBlock>())
        {
            if (maybe is CassetteBlock entity)
            {
                if (entity.Index == Index && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
                    return true;
            }
        }

        return false;
    }

    public void SetImageOverride(float x, float y, int tx, int ty)
    {
        var atlasSubtextures = GFX.Game.GetAtlasSubtextures(prefix + "Inactive");
        pressed.Add(CreateImage(x, y, tx, ty, atlasSubtextures[Index % atlasSubtextures.Count]));
        solid.Add(CreateImage(x, y, tx, ty, GFX.Game[prefix + "Active"]));
    }
}

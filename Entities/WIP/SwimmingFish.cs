using Celeste.Mod.Entities;
using ChroniaHelper.Components.SwimmingFish;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities.WIP;

[Tracked]
[WorkingInProgress]
[CustomEntity("ChroniaHelper/SwimmingFish")]
public class SwimmingFish : BaseEntity
{
    public SwimmingFish(EntityData data, Vc2 offset) : base(data, offset)
    {
        Collider = new Circle(4f);

        sprite = new(GFX.Game, "ChroniaHelper/SwimmingFish/");
        sprite.Add("idle", "fish");
        Add(sprite);
        sprite.Justify = Vc2.One * 0.5f;
        sprite.Play("idle");

        motion = new(Position, new());
        Add(motion);
    }
    private FishMotion motion;
    private Sprite sprite;

    private int WaterGroup = -1;
    protected override void AwakeExecute(Scene scene)
    {
        base.AwakeExecute(scene);
        
        // Get all the waters
        var waters = MaP.level.Tracker.GetEntities<Water>();
        
        if (this.ID == 
            MaP.level.Tracker.GetEntities<SwimmingFish>()
                .GetMinItem((e) => e.GetID()).GetID()
            )
        {
            // Prepare for groupping
            Md.Session.GroupedWaters = waters.GroupByInteraction(
                (water) => water as Water,
                WaterCrossover,
                (water) => water.GetID(),
                (i, n) => n);
        }
        
        //Find out which water is this in
        foreach (Water water in waters)
        {
            if (water.CollideCheck(this))
            {
                int n = water.GetID();

                foreach (var reg in Md.Session.GroupedWaters)
                {
                    if (reg.Value.Contains(n))
                    {
                        WaterGroup = reg.Key;
                        break;
                    }
                }

                break;
            }
        }

        if (WaterGroup == -1)
        {
            return;
        }
        
        List<Rectangle> borders = new();
        int delta = 0;
        var slicedWaters = MaP.SlicedEntities(waters);
        foreach (var n in Md.Session.GroupedWaters[WaterGroup])
        {
            if (slicedWaters.TryGetValue(n, out Entity b))
            {
                Rectangle border = new((int)b.Position.X - delta, (int)b.Position.Y - delta, 
                    (int)b.Width + delta * 2, (int)b.Height + delta * 2);
                
                borders.Add(border);
            }
        }

        if (PUt.TryGetPlayer(out Player player))
        {
            motion.InterferePoints.Add(player.Center);
        }
    }

    //public override void Render()
    //{
    //    base.Render();

    //    Draw.Rect(motion.Position - Vc2.One * 2f, 4f, 4f, Color.Red);
    //}

    public override void Update()
    {
        base.Update();

        sprite.Scale.X = -Math.Sign(motion.Position.X - Position.X);

        Position = motion.Position;
    }
    
    public bool WaterCrossover(Water a, Water b)
    {
        int delta = 2;
        Rectangle m = new((int)a.Position.X - delta, (int)a.Position.Y - delta, 
            (int)a.Width + delta * 2, (int)a.Height + delta * 2);
        Rectangle n = new((int)b.Position.X - delta, (int)b.Position.Y - delta, 
            (int)b.Width + delta * 2, (int)b.Height + delta * 2);

        return m.Crossover(n);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
[CustomEntity("ChroniaHelper/EntityDuplicator")]
public class EntityDuplicator : BaseEntity
{
    public EntityDuplicator(EntityData data, Vc2 offset) : base(data, offset)
    {
        generateFlag = data.Attr("generateFlag", "generate");
        Add(listener = new FlagListener(generateFlag));
        
        Collider = new Hitbox(data.Width, data.Height);
    }
    private string generateFlag;
    private FlagListener listener;

    private List<Entity> colliding = new();
    protected override void AwakeExecute(Scene scene)
    {
        //this.CollideAll(Tags.FrozenUpdate).AddAllTo(ref colliding);
        //this.CollideAll(Tags.Global).AddAllTo(ref colliding);
        //this.CollideAll(Tags.HUD).AddAllTo(ref colliding);
        //this.CollideAll(Tags.PauseUpdate).AddAllTo(ref colliding);
        //this.CollideAll(Tags.Persistent).AddAllTo(ref colliding);
        //this.CollideAll(Tags.TransitionUpdate).AddAllTo(ref colliding);
        //colliding = colliding.Distinct().ToList();
        foreach(var entity in MaP.level.Entities)
        {
            if (this.CollideCheck(entity))
            {
                colliding.Add(entity);
            }
        }

        Log.Info(colliding.Count);
    }

    protected override void UpdateExecute()
    {
        listener.onEnable = () =>
        {
            foreach(var entity in colliding)
            {
                Entity duplicate = entity.CreateEntity(Position, MaP.level.Session.LevelData);
                
                MaP.level.Add(duplicate);
            }
        };
    }
}

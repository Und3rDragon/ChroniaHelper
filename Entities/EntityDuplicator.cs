using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using VivHelper;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
[Tracked]
[CustomEntity("ChroniaHelper/EntityDuplicator")]
public class EntityDuplicator : BaseEntity
{
    public EntityDuplicator(EntityData data, Vc2 offset) : base(data, offset)
    {
        generateFlag = data.Attr("generateFlag", "generate");
        Add(listener = new FlagListener(generateFlag));

        duplicatorTargetTag = data.Attr("duplicatorTag", "duplicator");
        
        Collider = new Hitbox(data.Width, data.Height);
    }
    private string generateFlag;
    private FlagListener listener;
    private string duplicatorTargetTag;

    private List<Entity> colliding = new();
    private List<Entity> duplicatedEntities = new();
    private List<EntityDuplicatorTarget> duplicateTarget = new();
    protected override void AwakeExecute(Scene scene)
    {
        foreach(var entity in MaP.level.Entities)
        {
            if (this.CollideCheck(entity))
            {
                colliding.Add(entity);
            }
        }

        foreach(EntityDuplicatorTarget target in MaP.level.Tracker.GetEntities<EntityDuplicatorTarget>())
        {
            if(target.duplicatorTag == duplicatorTargetTag)
            {
                duplicateTarget.Add(target);
            }
        }
        duplicateTarget.Sort((item1, item2) => item1.ID - item2.ID);

        //Log.Info(colliding.Count);
    }

    protected override void UpdateExecute()
    {
        listener.onEnable = () =>
        {
            foreach(var entity in colliding)
            {
                Vc2 targetPos = Position;

                if(duplicateTarget.Count > 0)
                {
                    targetPos = duplicateTarget.Last().Position;
                }

                Entity duplicate = entity.DuplicateEntity(targetPos, MaP.level.Session.LevelData);

                MaP.level.Add(duplicate);
                duplicatedEntities.Add(duplicate);

                duplicate.Added(Scene);
                duplicate.Awake(Scene);
            }
        };
    }

    protected override void RemovedExecute(Scene scene)
    {
        foreach(var entity in duplicatedEntities)
        {
            entity.Removed(scene);
            entity.RemoveSelf();
        }

        duplicatedEntities.Clear();
    }
}

[Tracked]
[CustomEntity("ChroniaHelper/EntityDuplicatorTarget")]
public class EntityDuplicatorTarget : BaseEntity
{
    public EntityDuplicatorTarget(EntityData data, Vc2 offset) : base(data, offset)
    {
        duplicatorTag = data.Attr("duplicatorTag", "duplicator");
    }
    public string duplicatorTag;
}

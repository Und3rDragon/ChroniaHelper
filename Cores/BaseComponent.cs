using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public class BaseComponent : Component
{
    public BaseComponent(bool active, bool visible) : base(active, visible)
    {
        coroutineManager = new();
    }
    
    public BaseComponent() : base(true, true)
    {
        coroutineManager = new();
    }

    public CoroutineManager coroutineManager;

    public override void Added(Entity entity)
    {
        BeforeAdded(entity);
        
        base.Added(entity);
        
        AfterAdded(entity);

        coroutineManager.Start(AddedRoutine(entity));
    }
    
    protected virtual void BeforeAdded(Entity entity) { }
    protected virtual void AfterAdded(Entity entity) { }
    protected virtual IEnumerator AddedRoutine(Entity entity)
    {
        yield return null;
    }

    public override void Removed(Entity entity)
    {
        BeforeRemoved(entity);

        coroutineManager.Start(RemovedRoutine(entity));
        
        base.Removed(entity);

        AfterRemoved(entity);
    }

    protected virtual void BeforeRemoved(Entity entity) { }
    protected virtual void AfterRemoved(Entity entity) { }
    protected virtual IEnumerator RemovedRoutine(Entity entity)
    {
        yield return null;
    }

    public override void EntityAdded(Scene scene)
    {
        BeforeEntityAdded(scene);
        
        base.EntityAdded(scene);

        AfterEntityAdded(scene);

        coroutineManager.Start(EntityAddedRoutine(scene));
    }
    
    protected virtual void BeforeEntityAdded(Scene scene) { }
    protected virtual void AfterEntityAdded(Scene scene) { }
    protected IEnumerator EntityAddedRoutine(Scene scene)
    {
        yield return null;
    }

    public override void EntityRemoved(Scene scene)
    {
        BeforeEntityRemoved(scene);

        coroutineManager.Start(EntityRemovedRoutine(scene));

        base.EntityRemoved(scene);

        AfterEntityRemoved(scene);
    }

    protected virtual void BeforeEntityRemoved(Scene scene) { }
    protected virtual void AfterEntityRemoved(Scene scene) { }
    protected virtual IEnumerator EntityRemovedRoutine(Scene scene)
    {
        yield return null;
    }

    public override void Update()
    {
        coroutineManager.Update();

        Updating();
    }

    protected virtual void Updating() { }

    public override void SceneEnd(Scene scene)
    {
        //coroutineManager.Dispose();
        //coroutineManager = null;
    }
}

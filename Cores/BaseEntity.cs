using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using IL.MonoMod;
using VivHelper;

namespace ChroniaHelper.Cores;

public class BaseEntity : Entity
{
    public BaseEntity(EntityData data, Vc2 offset) : base(data.Position + offset)
    {
        nodes = data.NodesWithPosition(offset);
    }
    /// <summary>
    /// If there are no nodes, there is only one element in the array, and it's the Position
    /// </summary>
    public Vc2[] nodes;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        session = level.Session;

        if(AddedAwait <= 0f && AddedFreeze <= 0f)
        {
            if (!string.IsNullOrEmpty(AddedSound))
            {
                Audio.Play(AddedSound);
            }
            AddedExecute(scene);
            Add(new Coroutine(AddedRoutine(scene), true));
        }
        else
        {
            Add(new Coroutine(AddedInterfere(scene), true));
        }
    }
    public Level level;
    public Session session;
    public float AddedAwait = -1f, AddedFreeze = -1f;
    public string AddedSound = string.Empty;
    
    protected virtual void AddedExecute(Scene scene) { }
    protected virtual IEnumerator AddedRoutine(Scene scene) { yield break; }
    private IEnumerator AddedInterfere(Scene scene)
    {
        if(AddedAwait > 0f)
        {
            yield return AddedAwait;
        }
        if(AddedFreeze > 0f)
        {
            Celeste.Celeste.Freeze(AddedFreeze);
            yield return null;
        }
        if (!string.IsNullOrEmpty(AddedSound))
        {
            Audio.Play(AddedSound);
        }
        AddedExecute(scene);
        yield return AddedRoutine(scene);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        level = SceneAs<Level>();
        session = level.Session;

        if(AwakeAwait <= 0f && AwakeFreeze <= 0f)
        {
            AwakeExecute(scene);
            Add(new Coroutine(AwakeRoutine(scene), true));
        }
        else
        {
            Add(new Coroutine(AwakeInterfere(scene), true));
        }
    }
    public float AwakeAwait = -1f, AwakeFreeze = -1f;
    public string AwakeSound = string.Empty;

    protected virtual void AwakeExecute(Scene scene) { }
    protected virtual IEnumerator AwakeRoutine(Scene scene) { yield break; }
    private IEnumerator AwakeInterfere(Scene scene)
    {
        if (AwakeAwait > 0f) 
        { 
            yield return AwakeAwait; 
        }
        if (AwakeFreeze > 0f) 
        { 
            Celeste.Celeste.Freeze(AwakeFreeze); 
            yield return null; 
        }
        if (AwakeSound.IsNotNullOrEmpty()) 
        { 
            Audio.Play(AwakeSound); 
        }
        AwakeExecute(scene);
        yield return AwakeRoutine(scene);
    }
    
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        level = SceneAs<Level>();
        session = level.Session;
        
        if(RemovedAwait <= 0f && RemovedFreeze <= 0f)
        {
            RemovedExecute(scene);
            Add(new Coroutine(RemovedRoutine(scene), true));
        }
        else
        {
            Add(new Coroutine(RemovedInterfere(scene), true));
        }
    }
    public float RemovedAwait = -1f, RemovedFreeze = -1f;
    public string RemovedSound = string.Empty;
    
    protected virtual void RemovedExecute(Scene scene) { }
    protected virtual IEnumerator RemovedRoutine(Scene scene) { yield break; }
    private IEnumerator RemovedInterfere(Scene scene)
    {
        if(RemovedAwait > 0f)
        {
            yield return RemovedAwait;
        }
        if(RemovedFreeze > 0f)
        {
            Celeste.Celeste.Freeze(RemovedAwait); 
            yield return null;
        }
        if (RemovedSound.IsNotNullOrEmpty())
        {
            Audio.Play(RemovedSound);
        }
        RemovedExecute(scene);
        yield return RemovedRoutine(scene);
    }

    public override void Render()
    {
        base.Render();

        if (!RenderArg) { return; }
        
        if(RenderDelay <= 0f)
        {
            RenderExecute();
            Add(new Coroutine(RenderInterfere(), true));
        }
        else
        {
            Add(new Coroutine(RenderInterfere(), true));
        }
    }
    public bool RenderArg = true;
    public float RenderDelay = -1f;
    
    protected virtual void RenderExecute() { }
    protected virtual IEnumerator RenderRoutine() { yield break; }
    private IEnumerator RenderInterfere()
    {
        if(RenderDelay > 0f)
        {
            yield return RenderDelay;
        }
        RenderExecute();
        yield return RenderRoutine();
    }
    
    public override void Update()
    {
        base.Update();

        if (!UpdateArg) { return; }
        
        if(UpdateDelay <= 0f)
        {
            UpdateExecute();
            Add(new Coroutine(UpdateRoutine(), true));
        }
        else
        {
            Add(new Coroutine(UpdateInterfere(), true));
        }
    }
    public bool UpdateArg = true;
    public float UpdateDelay = -1f;
    
    protected virtual void UpdateExecute() { }
    protected virtual IEnumerator UpdateRoutine() { yield break; }
    private IEnumerator UpdateInterfere()
    {
        if(UpdateDelay > 0f)
        {
            yield return UpdateDelay;
        }
        UpdateExecute();
        yield return UpdateRoutine();
    }

    public override void SceneBegin(Scene scene)
    {
        base.SceneBegin(scene);
        
        if(SceneBeginAwait <= 0f && SceneBeginFreeze <= 0f)
        {
            SceneBeginExecute(scene);
            Add(new Coroutine(SceneBeginRoutine(scene), true));
        }
        else
        {
            Add(new Coroutine(SceneBeginInterfere(scene), true));
        }
    }
    public float SceneBeginAwait = -1f, SceneBeginFreeze = -1f;

    protected virtual void SceneBeginExecute(Scene scene) { }
    protected virtual IEnumerator SceneBeginRoutine(Scene scene) { yield break; }
    private IEnumerator SceneBeginInterfere(Scene scene)
    {
        if(SceneBeginAwait > 0f)
        {
            yield return SceneBeginAwait;
        }
        if(SceneBeginFreeze > 0f)
        {
            Celeste.Celeste.Freeze(SceneBeginFreeze);
            yield return null;
        }
        SceneBeginExecute(scene);
        yield return SceneBeginRoutine(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        
        if(SceneEndAwait <= 0f && SceneEndFreeze <= 0f)
        {
            SceneEndExecute(scene);
            Add(new Coroutine(SceneEndRoutine(scene), true));
        }
        else
        {
            Add(new Coroutine(SceneEndInterfere(scene), true));
        }
    }
    public float SceneEndAwait = -1f, SceneEndFreeze = -1f;
    
    protected virtual void SceneEndExecute(Scene scene) { }
    protected virtual IEnumerator SceneEndRoutine(Scene scene) { yield break; }
    private IEnumerator SceneEndInterfere(Scene scene)
    {
        if(SceneEndAwait > 0f)
        {
            yield return SceneEndAwait;
        }
        if(SceneEndFreeze > 0f)
        {
            Celeste.Celeste.Freeze(SceneEndFreeze);
            yield return null;
        }
        SceneEndExecute(scene);
        yield return SceneEndRoutine(scene);
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        
        if(DebugRenderDelay <= 0f)
        {
            DebugRenderExecute(camera);
            Add(new Coroutine(DebugRenderRoutine(camera), true));
        }
        else
        {
            Add(new Coroutine(DebugRenderInterfere(camera), true));
        }
    }
    public float DebugRenderDelay = -1f;
    
    protected virtual void DebugRenderExecute(Camera camera) { }
    protected virtual IEnumerator DebugRenderRoutine(Camera camera) { yield break; }
    private IEnumerator DebugRenderInterfere(Camera camera)
    {
        if(DebugRenderDelay > 0f)
        {
            yield return DebugRenderDelay;
        }
        DebugRenderExecute (camera);
        yield return DebugRenderRoutine(camera);
    }
}

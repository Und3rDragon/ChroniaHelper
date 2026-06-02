using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public class BaseComponent : Component
{
    public BaseComponent(bool active, bool visible) : base(active, visible) { }
    
    public BaseComponent() : base(true, true) { }

    public void AddTo(Entity entity)
    {
        entity.Add(this);
        Entity = entity;
    }
}

public static class BaseComponentUtils
{
    public static void GetBaseComponents(this Entity entity, params BaseComponent[] comps)
    {
        foreach (var comp in comps)
        {
            comp.AddTo(entity);
        }
    }

    public static void GetComponents(this Entity entity, params Component[] comps)
    {
        foreach (var comp in comps)
        {
            entity.Add(comp);
            comp.Entity = entity;
        }
    }
}

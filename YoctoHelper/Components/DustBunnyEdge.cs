using System;
using System.Collections.Generic;
using ChroniaHelper;
using YoctoHelper.Entities;

namespace YoctoHelper.Components;

[Tracked(false)]
public class DustBunnyEdge : Component
{

    public Action RenderDustBunny { get; set; }

    public DustBunnyEdge(Action onRenderDustBunny) : base(active: false, visible: true)
    {
        this.RenderDustBunny = onRenderDustBunny;
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        Color edgeColor = (entity as CustomDustBunny).borderColor;
        if (ChroniaHelperModule.Session.EdgeColorDictionary.TryGetValue(edgeColor, out List<DustBunnyEdge> list))
        {
            list.Add(this);
        }
        else
        {
            ChroniaHelperModule.Session.EdgeColorDictionary[edgeColor] = new List<DustBunnyEdge>() { this };
        }
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        ChroniaHelperModule.Session.EdgeColorDictionary.Clear();
    }

}

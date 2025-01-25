using System.Diagnostics;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/OpenUrlTrigger")]
public sealed class OpenUrlTrigger : Trigger
{
    private readonly EntityID id;
    private readonly string url;
    private readonly bool once;
    private readonly bool loadOnce;

    public OpenUrlTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
    {
        this.id = id;
        url = data.Attr("url", "https://www.celestegame.com/");
        once = data.Bool("once", false);
        loadOnce = data.Bool("loadOnce", false);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        try
        {
            ProcessStartInfo info = new(url) { UseShellExecute = true };
            Process.Start(info);
        }
        catch (Exception e)
        {
            Logger.LogDetailed(e, "OpenUrlHelper");
        }
        if (once)
            RemoveSelf();
        if (loadOnce)
            SceneAs<Level>().Session.DoNotLoad.Add(id);
    }
}

using System.Diagnostics;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Uri;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/OpenUrlTrigger")]
public sealed class OpenUrlTrigger : Trigger
{
    private readonly EntityID id;
    private readonly string url;
    private readonly bool once;
    private readonly bool loadOnce;
    private bool safe;
    
    public OpenUrlTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
    {
        this.id = id;
        once = data.Bool("once", false);
        loadOnce = data.Bool("loadOnce", false);
        url = data.Attr("url", "https://www.celestegame.com/");

        // Ensure a URL is being opened and not arbitrary code
        Uri validURL;
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri validURL))
        {
            safe = (validURL.Scheme == Uri.UriSchemeHttp || validURL.Scheme == Uri.UriSchemeHttps);
        }
        else safe = false;
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (safe) { 
            try
            {
                ProcessStartInfo info = new(url) { UseShellExecute = true };
                Process.Start(info);
            }
            catch (Exception e)
            {
                Logger.LogDetailed(e, "OpenUrlHelper");
            }
        }
        else
        {
            Logger.LogError("OpenUrlHelper", "Invalid URL! It should match the \"https://www.example.com\" format.");
        }
        if (once)
            RemoveSelf();
        if (loadOnce)
            SceneAs<Level>().Session.DoNotLoad.Add(id);
    }
}

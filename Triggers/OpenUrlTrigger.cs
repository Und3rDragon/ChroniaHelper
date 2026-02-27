using System.Diagnostics;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using ChroniaHelper.Cores;

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

        LinkValidation();
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        if (safe)
        {
            GenerateUI();
        }
        else
        {
            Logger.Error("OpenUrlHelper", "Invalid URL! It should match the \"https://www.example.com\" format.");
        }

        if (once)
            RemoveSelf();
        if (loadOnce)
            SceneAs<Level>().Session.DoNotLoad.Add(id);
    }

    [Credits("Gamation")]
    public void LinkValidation()
    {
        // Ensure a URL is being opened and not arbitrary code
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri validURL))
        {
            safe = (validURL.Scheme == Uri.UriSchemeHttp || validURL.Scheme == Uri.UriSchemeHttps);
        }
        else safe = false;
    }

    public void GenerateUI()
    {
        MaP.level.Paused = true;
        
        TextMenu menu = new();
        menu.AutoScroll = false;
        menu.Position = new Vector2((float)Engine.Width / 2f, (float)Engine.Height / 2f - 100f);
        menu.Add(new TextMenu.Header("Link redirect?"));
        menu.Add(new TextMenu.SubHeader("Are you familiar with the link?", false));
        menu.Add(new TextMenu.SubHeader(url, false));
        menu.Add(new TextMenu.Button("Confirm").Pressed(delegate
        {
            ProcessStartInfo info = new(url) { UseShellExecute = true };
            Process.Start(info);
            menu.OnCancel();
        }));
        menu.Add(new TextMenu.Button("Cancel").Pressed(delegate
        {
            menu.OnCancel();
        }));
        menu.OnPause = (menu.OnESC = delegate
        {
            menu.RemoveSelf();

            MaP.level.Paused = false;
            MaP.level.unpauseTimer = 0.15f;
            Audio.Play("event:/ui/game/unpause");
        });
        menu.OnCancel = delegate
        {
            Audio.Play("event:/ui/main/button_back");
            menu.RemoveSelf();
            MaP.level.Paused = false;
        };

        MaP.level.Add(menu);
    }
}

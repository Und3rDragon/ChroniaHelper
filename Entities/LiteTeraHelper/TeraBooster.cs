using Celeste.Mod.Entities;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/teraBooster")]
public class TeraBooster : Booster
{
    private Image image;
    public TeraType tera { get; private set; }

    public TeraBooster(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("red"))
    {
        Ch9HubBooster = false;
        tera = data.Enum("tera", TeraType.Normal);
        Add(image = new Image(GFX.Game[TeraUtil.GetImagePath(tera)]));
        image.CenterOrigin();
    }
    public override void Render()
    {
        var sprite = Get<Sprite>();
        Vector2 position = sprite.Position;
        image.Position = position.Floor();
        image.Visible = sprite.CurrentAnimationID != "pop" && sprite.CurrentAnimationID != "" && sprite.Visible;
        base.Render();
        image.Position = position;
    }
    public static void OnLoad()
    {
        On.Celeste.Player.Render += PlayerRenderInTeraBooster;
    }
    public static void OnUnload()
    {
        On.Celeste.Player.Render -= PlayerRenderInTeraBooster;
    }
    private static void PlayerRenderInTeraBooster(On.Celeste.Player.orig_Render orig, Player self)
    {
        orig(self);
        if (self == null) return;
        var playerData = DynamicData.For(self);
        if (playerData.TryGet("teraSprite", out Sprite sprite))
        {
            if (sprite != null)
            {
                sprite.Visible = !self.InTeraBooster();
            }
        }
    }

}
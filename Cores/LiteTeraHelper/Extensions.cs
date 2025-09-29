using ChroniaHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores.LiteTeraHelper;

internal static class Extensions
{
    public static bool ChangeTera(this Player player, TeraType newTera)
    {
        if (player == null) return false;
        if (!Md.Session.ActiveTera) return false;
        var playerData = DynamicData.For(player);
        if (playerData.TryGet("tera", out TeraType oldTera))
        {
            if (oldTera == newTera)
                return false;
        }
        playerData.Set("tera", newTera);
        if (playerData.TryGet("teraSprite", out Sprite sprite))
        {
            if (sprite != null)
            {
                sprite.Play(newTera.ToString());
            }
        }
        return true;
    }
    public static void InitTera(this Player player)
    {
        
        if (player == null) return;
        var playerData = DynamicData.For(player);
        var tera = Md.Session.StartTera;
        if (tera == TeraType.Any)
            tera = TeraType.Normal;
        playerData.Set("tera", tera);
        var sprite = GFX.SpriteBank.Create("ChroniaHelper_teraPlayer");
        sprite.Position = new Vector2(0f, -18f);
        sprite.Play(tera.ToString());
        playerData.Set("teraSprite", sprite);
        player.Add(sprite);
    }
    public static void RemoveTera(this Player player)
    {
        
        if (player == null) return;
        var playerData = DynamicData.For(player);
        if (playerData.TryGet("teraSprite", out Sprite sprite))
        {
            if (sprite != null)
            {
                sprite.RemoveSelf();
                playerData.Set("teraSprite", null);
            }
        }
    }
    public static bool InTeraBooster(this Player player)
    {
        if (player == null) return false;
        if (player.CurrentBooster != null)
            return player.CurrentBooster is TeraBooster;
        if (player.LastBooster != null && player.LastBooster.BoostingPlayer)
            return player.LastBooster is TeraBooster;
        return false;
    }
    public static TeraType GetTera(this Player player, bool ignoreBooster = false)
    {
        if (player == null) return TeraType.Any;
        if (!Md.Session.ActiveTera) return TeraType.Any;
        if (!ignoreBooster && player.InTeraBooster())
        {
            if (player.LastBooster is TeraBooster teraBooster)
            {
                return teraBooster.tera;
            }
            return TeraType.Any;
        }
        var playerData = DynamicData.For(player);
        if (playerData.TryGet("tera", out TeraType tera))
        {
            return tera;
        }
        return TeraType.Any;
    }
}
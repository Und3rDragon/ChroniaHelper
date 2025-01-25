using Celeste.Mod.Entities;
using ChroniaHelper.Cores.LiteTeraHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;

namespace ChroniaHelper.Entities;

[TrackedAs(typeof(TouchSwitch))]
[CustomEntity("ChroniaHelper/teraTouchSwitch")]
public class TeraTouchSwitch : TouchSwitch
{
    private TeraType tera;
    private Color inactiveColor = Calc.HexToColor("5fcde4");
    public TeraTouchSwitch(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        tera = data.Enum("tera", TeraType.Normal);
        var switchData = DynamicData.For(this);
        var icon = switchData.Get<Sprite>("icon");
        Remove(icon);
        icon = new Sprite(GFX.Game, "ChroniaHelper/objects/tera/TouchSwitch/" + tera.ToString());
        icon.Add("idle", "", 0f, default(int));
        icon.Add("spin", "", 0.1f, new Chooser<string>("spin", 1f), 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
        icon.Play("spin");
        icon.Color = inactiveColor;
        icon.CenterOrigin();
        switchData.Set("icon", icon);
        Remove(Get<PlayerCollider>());
        Remove(Get<HoldableCollider>());
        Remove(Get<SeekerCollider>());
        Add(icon);
        Add(new PlayerCollider(OnPlayer, null, new Hitbox(30f, 30f, -15f, -15f)));
        Add(new HoldableCollider(OnHoldable, new Hitbox(20f, 20f, -10f, -10f)));
        Add(new SeekerCollider(OnSeeker, new Hitbox(24f, 24f, -12f, -12f)));
    }
    private void OnPlayer(Player player)
    {
        if (EffectAsDefender(player.GetTera()) == TeraEffect.Super)
            TurnOn();
    }

    private void OnHoldable(Holdable h)
    {
        if (h.Entity is TeraCrystal crystal)
        {
            if (EffectAsDefender(crystal.tera) == TeraEffect.Super)
                TurnOn();
        }
    }
    private void OnSeeker(Seeker s)
    {

    }
    public TeraEffect EffectAsDefender(TeraType atk)
    {
        return TeraUtil.GetEffect(atk, tera);
    }
}
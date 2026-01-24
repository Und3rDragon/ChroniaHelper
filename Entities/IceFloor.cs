using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/IceFloor")]
public class IceFloor : BaseEntity
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Player.RefillDash += WhenRefillDash;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Player.RefillDash -= WhenRefillDash;
    }

    public static bool WhenRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self)
    {
        if (self.CollideCheck<IceFloor>())
        {
            return false;
        }
        else
        {
            return orig(self);
        }
    }

    public IceFloor(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        base.Depth = data.Int("depth", -9999);
        base.Collider = new Hitbox((float)data.Width, 2f, 0f, 6f);
        xml = data.Attr("spriteXML", "iceFloor");
        onGroundSpriteOffset = data.Bool("onGroundSpriteOffset", false);
        this.tiles = this.BuildSprite();
        createStaticMover = data.Bool("createStaticMover", false);
        
        if (createStaticMover)
        {
            Add(new StaticMover());
        }
    }
    private string xml;
    private bool createStaticMover = false;
    private bool onGroundSpriteOffset = false;

    //public override void Added(Scene scene)
    //{
    //    base.Added(scene);
    //    this.tiles.ForEach(delegate (Sprite t)
    //    {
    //        t.Play("ice", false, false);
    //    });
    //}

    private List<Sprite> BuildSprite()
    {
        List<Sprite> list = new List<Sprite>();
        int num = 0;
        while ((float)num < base.Width)
        {
            bool flag = num == 0;
            string text;
            if (flag)
            {
                text = "top";
            }
            else
            {
                bool flag2 = (float)(num + 16) > base.Width;
                if (flag2)
                {
                    text = "bottom";
                }
                else
                {
                    text = "middle";
                }
            }
            Sprite sprite = GFX.SpriteBank.Create(xml);
            sprite.Position = new Vector2((float)num, onGroundSpriteOffset? 8f: 0f);
            sprite.Play(text);
            list.Add(sprite);
            Add(sprite);
            num += 8;
        }
        return list;
    }

    private readonly List<Sprite> tiles;
}

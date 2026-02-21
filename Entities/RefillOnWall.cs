using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/RefillOnWall")]
public class RefillOnWall : BaseEntity
{
    public RefillOnWall(EntityData data, Vc2 offset) : base(data, offset)
    {
        Depth = data.Int("depth", -10500);

        sides = (Sides)data.Int("sides", 0);

        ColliderList detectionRange = new();
        if(sides == Sides.Left || sides == Sides.Both)
        {
            detectionRange.Add(new Hitbox(2f, data.Height, -2f, 0f));
        }
        if(sides == Sides.Right || sides == Sides.Both)
        {
            detectionRange.Add(new Hitbox(2f, data.Height, data.Width, 0f));
        }
        pc = new PlayerCollider(OnPlayer, detectionRange);

        Add(pc);

        requireGrab = data.Bool("requireGrab", true);
        
        spriteDir = data.Attr("directory", "objects/ChroniaHelper/comfyWall/");
        spriteDir = spriteDir.TrimEnd('/') + "/";

        sprites = BuildSprite(data);

        if(data.Bool("createStaticMover", false))
        {
            Collider = new Hitbox(data.Width, data.Height);
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                OnDestroy = RemoveSelf
            });
        }
    }
    private PlayerCollider pc;
    public enum Sides { Left = 0, Right = 1, Both = 2 }
    private Sides sides;
    private string spriteDir;
    private bool requireGrab;

    public void OnPlayer(Player player)
    {
        if (requireGrab)
        {
            if (player.StateMachine.State == Player.StClimb)
            {
                player.RefillDash();
                player.RefillStamina();
            }
        }
        else
        {
            player.RefillDash();
            player.RefillStamina();
        }
    }

    private void OnShake(Vector2 pos)
    {
        foreach (Component component in Components)
        {
            if (component is Sprite sprite)
            {
                sprite.Position += pos;
            }
        }
    }

    private bool IsRiding(Solid solid)
    {
        return CollideCheck(solid);
    }

    private List<Sprite> sprites = new();
    private List<Sprite> BuildSprite(EntityData data)
    {
        List<Sprite> list = new List<Sprite>();
        int num = 0;
        while ((float)num < data.Height)
        {
            bool flag = num == 0;
            string text;
            if (flag)
            {
                text = "top";
            }
            else
            {
                bool flag2 = (float)(num + 16) > data.Height;
                if (flag2)
                {
                    text = "bottom";
                }
                else
                {
                    text = "mid";
                }
            }
            
            if(sides == Sides.Left || sides == Sides.Both)
            {
                Sprite sprite = new(GFX.Game, spriteDir);
                sprite.AddLoop(text, text, 0.1f);
                sprite.Play(text);
                sprite.Position = new Vector2(0f, (float)num);
                sprite.CurrentAnimationFrame = RandomUtils.RandomInt(sprite.CurrentAnimationTotalFrames);
                sprite.Animating = false;
                list.Add(sprite);
                Add(sprite);
            }

            if(sides == Sides.Right || sides == Sides.Both)
            {
                Sprite sprite = new(GFX.Game, spriteDir);
                sprite.AddLoop(text, text, 0.1f);
                sprite.Play(text);
                sprite.FlipX = true;
                sprite.Position = new Vector2(data.Width - 8f, (float)num);
                sprite.CurrentAnimationFrame = RandomUtils.RandomInt(sprite.CurrentAnimationTotalFrames);
                sprite.Animating = false;
                list.Add(sprite);
                Add(sprite);
            }

            num += 8;
        }

        return list;
    }
}

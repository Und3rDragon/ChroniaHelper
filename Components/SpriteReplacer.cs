using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Components;

[Note("Should be usable but the effect may not be as good as imagined",
    "Besides, there're no failure protection")]
public class SpriteReplacer : BaseComponent
{
    public SpriteReplacer(string from, string to)
    {
        From = from; To = to;
    }
    private string From, To;

    public override void EntityAwake()
    {
        foreach (var components in Entity.Components)
        {
            if (components is Sprite sprite)
            {
                foreach (var animation in sprite.Animations.Values)
                {
                    foreach (var texture in animation.Frames)
                    {
                        if (texture.AtlasPath.Contains(From))
                        {
                            texture.AtlasPath = texture.AtlasPath.Replace(From, To);
                        }
                    }
                }
            }

            if (components is Image image)
            {
                if (image.Texture.AtlasPath.Contains(From))
                {
                    image.Texture.AtlasPath = image.Texture.AtlasPath.Replace(From, To);
                }
            }
        }
    }
}

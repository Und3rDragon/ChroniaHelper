using YoctoHelper.Components;
using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

public class DustBunnyEyeballs : Entity
{

    public DustBunnyGraphic dustBunnyGraphic { get; set; }

    public Color color { get; set; }

    public DustBunnyEyeballs(DustBunnyGraphic dustBunnyGraphic)
    {
        this.dustBunnyGraphic = dustBunnyGraphic;
        base.Depth = this.dustBunnyGraphic.Entity.Depth - 1;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.color = this.dustBunnyGraphic.dustBunny.eyesColor;
    }

    public override void Update()
    {
        base.Update();
        if ((ObjectUtils.IsNull(this.dustBunnyGraphic.Entity)) || (ObjectUtils.IsNull(this.dustBunnyGraphic.Scene)))
        {
            base.RemoveSelf();
        }
    }

    public override void Render()
    {
        if (this.dustBunnyGraphic.Visible && this.dustBunnyGraphic.Entity.Visible)
        {
            Vector2 vector = new Vector2(0F - this.dustBunnyGraphic.eyeDirection.Y, this.dustBunnyGraphic.eyeDirection.X).SafeNormalize();
            if (this.dustBunnyGraphic.leftEyeVisible)
            {
                this.dustBunnyGraphic.eyeTexture.DrawCentered(this.dustBunnyGraphic.renderPosition + (this.dustBunnyGraphic.eyeDirection * 5F + vector * 3F) * this.dustBunnyGraphic.scale, this.color, this.dustBunnyGraphic.scale);
            }
            if (this.dustBunnyGraphic.rightEyeVisible)
            {
                this.dustBunnyGraphic.eyeTexture.DrawCentered(this.dustBunnyGraphic.renderPosition + (this.dustBunnyGraphic.eyeDirection * 5F - vector * 3F) * this.dustBunnyGraphic.scale, this.color, this.dustBunnyGraphic.scale);
            }
        }
    }

}

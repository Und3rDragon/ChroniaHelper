using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/LightingTrigger")]
public class LightingTrigger : BaseTrigger
{

    private Color lightingColor;

    private float lightingAlpha;

    private OldLighting oldLighting;

    public struct OldLighting
    {
        public Color lightingColor;

        public float lightingAlpha;

        public float lightingAlphaAdd;
    };

    public LightingTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.lightingColor = data.HexColor("lightingColor", Color.Black);
        this.lightingAlpha = data.Float("lightingAlpha", 0F);
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset)
        {
            this.oldLighting.lightingColor = base.level.Lighting.BaseColor;
            this.oldLighting.lightingAlpha = base.level.Lighting.Alpha;
            this.oldLighting.lightingAlphaAdd = base.session.LightingAlphaAdd;
        }
        base.level.Lighting.BaseColor = this.lightingColor;
        base.level.Lighting.Alpha = this.lightingAlpha;
        base.session.LightingAlphaAdd = this.lightingAlpha - base.level.BaseLightingAlpha;
    }

    protected override void LeaveReset(Player player)
    {
        base.level.Lighting.BaseColor = this.oldLighting.lightingColor;
        base.level.Lighting.Alpha = this.oldLighting.lightingAlpha;
        base.session.LightingAlphaAdd = this.oldLighting.lightingAlphaAdd;
    }

}

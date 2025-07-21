using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/LightingTrigger")]
public class LightingTrigger : BaseTrigger
{

    private Color lightingColor;

    private float lightingAlpha;

    private OldLighting oldLighting;

    private float timed, timer;

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
        timed = data.Float("timed", -1f);
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset || timed > 0f)
        {
            this.oldLighting.lightingColor = base.level.Lighting.BaseColor;
            this.oldLighting.lightingAlpha = base.level.Lighting.Alpha;
            this.oldLighting.lightingAlphaAdd = base.session.LightingAlphaAdd;
        }
        if(timed <= 0f)
        {
            base.level.Lighting.BaseColor = this.lightingColor;
            base.level.Lighting.Alpha = this.lightingAlpha;
            base.session.LightingAlphaAdd = this.lightingAlpha - base.level.BaseLightingAlpha;
        }
        else if(timed > 0f)
        {
            timer = timed;
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.level.Lighting.BaseColor = this.oldLighting.lightingColor;
        base.level.Lighting.Alpha = this.oldLighting.lightingAlpha;
        base.session.LightingAlphaAdd = this.oldLighting.lightingAlphaAdd;
    }

    public override void Update()
    {
        base.Update();

        if(timed > 0f)
        {
            if (timer >= 0f)
            {
                timer = Calc.Approach(timer, -1f, Engine.DeltaTime);
                level.Lighting.BaseColor = Color.Lerp(oldLighting.lightingColor, lightingColor,
                    FadeUtils.LerpValue(timer, timed, 0f, 0f, 1f));
                level.Lighting.Alpha = FadeUtils.LerpValue(timer, timed, 0f, oldLighting.lightingAlpha, lightingAlpha);

            }
        }
    }

}

using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using static ChroniaHelper.Triggers.LightingTrigger;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/LightingFadeTrigger")]
public class LightingFadeTrigger : BaseTrigger
{

    private Color lightingColorFrom;

    private Color lightingColorTo;

    private float lightingAlphaFrom;

    private float lightingAlphaTo;

    private PositionModes positionMode;

    private LightingTrigger.OldLighting oldLighting;

    private float timed, timer;

    private string lightingAlphaAddFrom, lightingAlphaAddTo, baseLightingAlphaFrom, baseLightingAlphaTo;

    public LightingFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.lightingColorFrom = data.HexColor("lightingColorFrom", Color.Black);
        this.lightingColorTo = data.HexColor("lightingColorTo", Color.Black);
        this.lightingAlphaFrom = data.Float("lightingAlphaFrom", 0F);
        this.lightingAlphaTo = data.Float("lightingAlphaTo", 0F);
        this.positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
        timed = data.Float("timed", -1f);

        lightingAlphaAddFrom = data.Attr("lightingAlphaAddFrom");
        lightingAlphaAddTo = data.Attr("lightingAlphaAddTo");
        baseLightingAlphaFrom = data.Attr("baseLightingAlphaFrom");
        baseLightingAlphaTo = data.Attr("baseLightingAlphaTo");
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset)
        {
            oldLighting = new OldLighting
            {
                lightingColor = base.level.Lighting.BaseColor,
                lightingAlpha = base.level.Lighting.Alpha,
                lightingAlphaAdd = base.session.LightingAlphaAdd,
                baseLightingAlpha = base.level.BaseLightingAlpha,
            };
        }
        if(timed > 0f)
        {
            timer = timed;
        }
    }

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        if (timed > 0f)
        {
            while (timer >= 0f)
            {
                timer = Calc.Approach(timer, -1f, Engine.DeltaTime);
                float progress = FadeUtils.LerpValue(timer, timed, 0f, 0f, 1f);
                level.Lighting.BaseColor = Color.Lerp(lightingColorFrom, lightingColorTo, progress);
                level.Lighting.Alpha = FadeUtils.LerpValue(timer, timed, 0f, lightingAlphaFrom, lightingAlphaTo);
                if(baseLightingAlphaFrom.IsNotNullOrEmpty() && baseLightingAlphaTo.IsNotNullOrEmpty())
                {
                    level.BaseLightingAlpha = FadeUtils.LerpValue(timer, timed, 0f, baseLightingAlphaFrom.ParseFloat(0f), baseLightingAlphaTo.ParseFloat(0f));
                }
                if(lightingAlphaAddFrom.IsNotNullOrEmpty() && lightingAlphaAddTo.IsNotNullOrEmpty())
                {
                    level.Session.LightingAlphaAdd = FadeUtils.LerpValue(timer, timed, 0f, lightingAlphaAddFrom.ParseFloat(0f), lightingAlphaAddTo.ParseFloat(0f));
                }

                yield return null;
            }
        }
    }

    protected override void OnStayExecute(Player player)
    {
        float lerp = base.GetPositionLerp(player, this.positionMode);
        float lightingAlpha = Calc.ClampedMap(lerp, 0f, 1f, this.lightingAlphaFrom, this.lightingAlphaTo);
        if(timed <= 0f)
        {
            base.level.Lighting.BaseColor = this.lightingColorTo;
            base.level.Lighting.Alpha = this.lightingAlphaTo;
            if (lightingAlphaAddTo.IsNotNullOrEmpty())
            {
                base.session.LightingAlphaAdd = this.lightingAlphaAddTo.ParseFloat(0f);
            }
            if (baseLightingAlphaTo.IsNotNullOrEmpty())
            {
                base.level.BaseLightingAlpha = this.baseLightingAlphaTo.ParseFloat(0f);
            }
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.level.Lighting.BaseColor = this.oldLighting.lightingColor;
        base.level.Lighting.Alpha = this.oldLighting.lightingAlpha;
        base.session.LightingAlphaAdd = this.oldLighting.lightingAlphaAdd;
        base.level.BaseLightingAlpha = this.oldLighting.baseLightingAlpha;
    }

}

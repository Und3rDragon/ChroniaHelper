using System.Collections;
using AsmResolver.DotNet.Cloning;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using static ChroniaHelper.Triggers.LightingTrigger;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/LightingFadeTrigger")]
public class LightingFadeTrigger : BaseTrigger
{

    private string lightingColorFrom;

    private string lightingColorTo;

    private string lightingAlphaFrom;

    private string lightingAlphaTo;

    private PositionModes positionMode;

    private LightingTrigger.OldLighting oldLighting;

    private float timed, timer;

    private string lightingAlphaAddFrom, lightingAlphaAddTo, baseLightingAlphaFrom, baseLightingAlphaTo;

    public LightingFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.lightingColorFrom = data.Attr("lightingColorFrom");
        this.lightingColorTo = data.Attr("lightingColorTo");
        this.lightingAlphaFrom = data.Attr("lightingAlphaFrom");
        this.lightingAlphaTo = data.Attr("lightingAlphaTo");
        this.positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
        timed = data.Float("timed", -1f);

        lightingAlphaAddFrom = data.Attr("lightingAlphaAddFrom");
        lightingAlphaAddTo = data.Attr("lightingAlphaAddTo");
        baseLightingAlphaFrom = data.Attr("baseLightingAlphaFrom");
        baseLightingAlphaTo = data.Attr("baseLightingAlphaTo");
    }

    protected override void OnEnterExecute(Player player)
    {
        oldLighting = new OldLighting
        {
            lightingColor = base.level.Lighting.BaseColor,
            lightingAlpha = base.level.Lighting.Alpha,
            lightingAlphaAdd = base.session.LightingAlphaAdd,
            baseLightingAlpha = base.level.BaseLightingAlpha,
        };

        if (timed > 0f)
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

                if (lightingColorTo.HasValidContent())
                {
                    Color _from = lightingColorFrom.HasValidContent() ?
                        Calc.HexToColor(lightingColorFrom) : oldLighting.lightingColor;
                    Color _to = Calc.HexToColor(lightingColorTo);
                    level.Lighting.BaseColor = Color.Lerp(_from, _to, progress);
                }
                
                if (lightingAlphaTo.HasValidContent())
                {
                    float _from = oldLighting.lightingAlpha;
                    if (lightingAlphaFrom.HasValidContent())
                    {
                        float.TryParse(lightingAlphaFrom, out _from);
                    }
                    float _to = 0f;
                    float.TryParse(lightingAlphaTo, out _to);

                    if (!baseLightingAlphaFrom.HasValidContent() && !baseLightingAlphaTo.HasValidContent())
                    {
                        level.BaseLightingAlpha = FadeUtils.LerpValue(timer, timed, 0f, _from, _to);
                    }

                    level.Lighting.Alpha = FadeUtils.LerpValue(timer, timed, 0f, _from, _to);
                }

                if(baseLightingAlphaTo.HasValidContent())
                {
                    float _from = oldLighting.baseLightingAlpha;
                    if (baseLightingAlphaFrom.HasValidContent())
                    {
                        float.TryParse(baseLightingAlphaFrom, out _from);
                    }
                    float _to = 0f;
                    float.TryParse(baseLightingAlphaTo, out _to);

                    level.BaseLightingAlpha = FadeUtils.LerpValue(timer, timed, 0f, _from, _to);
                }

                if(lightingAlphaAddTo.HasValidContent())
                {
                    float _from = oldLighting.lightingAlphaAdd;
                    if (lightingAlphaAddFrom.HasValidContent())
                    {
                        float.TryParse(lightingAlphaAddFrom, out _from);
                    }
                    float _to = 0f;
                    float.TryParse(lightingAlphaAddTo, out _to);

                    level.Session.LightingAlphaAdd = FadeUtils.LerpValue(timer, timed, 0f, _from, _to);
                }

                yield return null;
            }
        }
    }

    protected override void OnStayExecute(Player player)
    {
        float lerp = base.GetPositionLerp(player, this.positionMode);
        if(timed <= 0f)
        {
            if (lightingColorTo.HasValidContent())
            {
                Color _from = lightingColorFrom.HasValidContent() ?
                    Calc.HexToColor(lightingColorFrom) : oldLighting.lightingColor;
                Color _to = Calc.HexToColor(lightingColorTo);
                level.Lighting.BaseColor = Color.Lerp(_from, _to, lerp);
            }

            if (lightingAlphaTo.HasValidContent())
            {
                float _from = oldLighting.lightingAlpha;
                if (lightingAlphaFrom.HasValidContent())
                {
                    float.TryParse(lightingAlphaFrom, out _from);
                }
                float _to = 0f;
                float.TryParse(lightingAlphaTo, out _to);

                if (!baseLightingAlphaFrom.HasValidContent() && !baseLightingAlphaTo.HasValidContent())
                {
                    level.BaseLightingAlpha = FadeUtils.LerpValue(lerp, 0f, 1f, _from, _to);
                }

                level.Lighting.Alpha = FadeUtils.LerpValue(lerp, 0f, 1f, _from, _to);
            }

            if (baseLightingAlphaTo.HasValidContent())
            {
                float _from = oldLighting.baseLightingAlpha;
                if (baseLightingAlphaFrom.HasValidContent())
                {
                    float.TryParse(baseLightingAlphaFrom, out _from);
                }
                float _to = 0f;
                float.TryParse(baseLightingAlphaTo, out _to);

                level.BaseLightingAlpha = FadeUtils.LerpValue(lerp, 0f, 1f, _from, _to);
            }

            if (lightingAlphaAddTo.HasValidContent())
            {
                float _from = oldLighting.lightingAlphaAdd;
                if (lightingAlphaAddFrom.HasValidContent())
                {
                    float.TryParse(lightingAlphaAddFrom, out _from);
                }
                float _to = 0f;
                float.TryParse(lightingAlphaAddTo, out _to);

                level.Session.LightingAlphaAdd = FadeUtils.LerpValue(lerp, 0f, 1f, _from, _to);
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

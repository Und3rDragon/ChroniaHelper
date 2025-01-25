﻿using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

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

    public LightingFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.lightingColorFrom = data.HexColor("lightingColorFrom", Color.Black);
        this.lightingColorTo = data.HexColor("lightingColorTo", Color.Black);
        this.lightingAlphaFrom = data.Float("lightingAlphaFrom", 0F);
        this.lightingAlphaTo = data.Float("lightingAlphaTo", 0F);
        this.positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset)
        {
            this.oldLighting.lightingColor = base.level.Lighting.BaseColor;
            this.oldLighting.lightingAlpha = base.level.Lighting.Alpha;
            this.oldLighting.lightingAlphaAdd = base.session.LightingAlphaAdd;
        }
    }

    protected override void OnStayExecute(Player player)
    {
        float lerp = base.GetPositionLerp(player, this.positionMode);
        float lightingAlpha = Calc.ClampedMap(lerp, 0f, 1f, this.lightingAlphaFrom, this.lightingAlphaTo);
        base.level.Lighting.BaseColor = Color.Lerp(this.lightingColorFrom, this.lightingColorTo, lerp);
        base.level.Lighting.Alpha = lightingAlpha;
        base.session.LightingAlphaAdd = lightingAlpha - base.level.BaseLightingAlpha;
    }

    protected override void LeaveReset(Player player)
    {
        base.level.Lighting.BaseColor = this.oldLighting.lightingColor;
        base.level.Lighting.Alpha = this.oldLighting.lightingAlpha;
        base.session.LightingAlphaAdd = this.oldLighting.lightingAlphaAdd;
    }

}

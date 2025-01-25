using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/AmbienceFadeTrigger")]
public class AmbienceFadeTrigger : BaseTrigger
{

    private float ambienceVolumeFrom;

    private float ambienceVolumeTo;

    private float ambienceParameterValueFrom;

    private float ambienceParameterValueTo;

    private string ambienceParameter;

    private PositionModes positionMode;

    private List<MEP> oldAmbienceParameters;

    private float oldAmbienceVolume;

    public AmbienceFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.ambienceVolumeFrom = data.Float("ambienceVolumeFrom", 1F);
        this.ambienceVolumeTo = data.Float("ambienceVolumeTo", 1F);
        this.ambienceParameterValueFrom = data.Float("ambienceParameterValueFrom", 1F);
        this.ambienceParameterValueTo = data.Float("ambienceParameterValueTo", 1F);
        this.ambienceParameter = data.Attr("ambienceParameter", null);
        this.positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
        if (string.IsNullOrEmpty(this.ambienceParameter))
        {
            this.ambienceParameter = "fade";
        }
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset)
        {
            AudioState audioState = base.session.Audio;
            this.oldAmbienceVolume = audioState.AmbienceVolume ?? 0F;
            this.oldAmbienceParameters = ObjectUtils.DeepCopyList<MEP>(audioState.Ambience.Parameters);
        }
    }

    protected override void OnStayExecute(Player player)
    {
        float lerp = base.GetPositionLerp(player, this.positionMode);
        float ambienceVolume = Calc.ClampedMap(lerp, 0f, 1f, this.ambienceVolumeFrom, this.ambienceVolumeTo);
        float ambienceParameterValue = Calc.ClampedMap(lerp, 0f, 1f, this.ambienceParameterValueFrom, this.ambienceParameterValueTo);
        AudioState audioState = base.session.Audio;
        audioState.AmbienceVolume = ambienceVolume;
        audioState.Ambience.Param(this.ambienceParameter, ambienceParameterValue);
        audioState.Apply();
    }

    protected override void LeaveReset(Player player)
    {
        AudioState audioState = base.session.Audio;
        audioState.AmbienceVolume = this.oldAmbienceVolume;
        audioState.Ambience.Parameters = ObjectUtils.DeepCopyList<MEP>(this.oldAmbienceParameters);
        audioState.Apply();
    }

}

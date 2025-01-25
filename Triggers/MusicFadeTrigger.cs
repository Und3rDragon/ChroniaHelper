using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/MusicFadeTrigger")]
public class MusicFadeTrigger : BaseTrigger
{

    private float musicParameterValueFrom;

    private float musicParameterValueTo;

    private string musicParameter;

    private PositionModes positionMode;

    private List<MEP> oldMusicParameters;

    public MusicFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.musicParameterValueFrom = data.Float("musicParameterValueFrom", 1F);
        this.musicParameterValueTo = data.Float("musicParameterValueTo", 0F);
        this.musicParameter = data.Attr("musicParameter", null);
        this.positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
        if (string.IsNullOrEmpty(this.musicParameter))
        {
            this.musicParameter = "fade";
        }
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset)
        {
            this.oldMusicParameters = ObjectUtils.DeepCopyList<MEP>(base.session.Audio.Music.Parameters);
        }
    }

    protected override void OnStayExecute(Player player)
    {
        float lerp = base.GetPositionLerp(player, this.positionMode);
        float musicParameterValue = Calc.ClampedMap(lerp, 0f, 1f, this.musicParameterValueFrom, this.musicParameterValueTo);
        AudioState audioState = base.session.Audio;
        audioState.Music.Param(this.musicParameter, musicParameterValue);
        audioState.Apply();
    }

    protected override void LeaveReset(Player player)
    {
        AudioState audioState = base.session.Audio;
        audioState.Music.Parameters = ObjectUtils.DeepCopyList<MEP>(this.oldMusicParameters);
        audioState.Apply();
    }

}

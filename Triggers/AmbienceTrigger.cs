using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/AmbienceTrigger")]
public class AmbienceTrigger : BaseTrigger
{

    private string ambienceTrack;

    private int ambienceProgress;

    private string ambienceParameter;

    private float ambienceParameterValue;

    private float ambienceVolume;

    private int[] ambienceLayers;

    private OldAmbience oldAmbience;

    public struct OldAmbience
    {
        public string ambienceTrack;

        public int ambienceProgress;

        public List<MEP> ambienceParameters;

        public float ambienceVolume;
    };

    public AmbienceTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.ambienceTrack = data.Attr("ambienceTrack", null);
        this.ambienceProgress = data.Int("ambienceProgress", 0);
        this.ambienceParameter = data.Attr("ambienceParameter", null);
        this.ambienceParameterValue = data.Float("ambienceParameterValue", 1F);
        this.ambienceVolume = data.Float("ambienceVolume", 1F);
        this.ambienceLayers = StringUtils.SplitToIntArray(data.Attr("ambienceLayers", null));
        if (string.IsNullOrEmpty(this.ambienceParameter))
        {
            this.ambienceParameter = "fade";
        }
    }

    protected override void OnEnterExecute(Player player)
    {
        AudioState audioState = base.session.Audio;
        if (base.leaveReset)
        {
            this.oldAmbience.ambienceTrack = audioState.Ambience.Event;
            this.oldAmbience.ambienceProgress = audioState.Ambience.Progress;
            this.oldAmbience.ambienceParameters = ObjectUtils.DeepCopyList<MEP>(audioState.Ambience.Parameters);
            this.oldAmbience.ambienceVolume = audioState.AmbienceVolume ?? 0F;
        }
        audioState.Ambience.Event = SFX.EventnameByHandle(this.ambienceTrack);
        audioState.Ambience.Progress = this.ambienceProgress;
        audioState.Ambience.Param(this.ambienceParameter, this.ambienceParameterValue);
        audioState.AmbienceVolume = this.ambienceVolume;
        foreach (int layer in this.ambienceLayers)
        {
            audioState.Ambience.Layer(layer, true);
        }
        audioState.Apply();
    }

    protected override void LeaveReset(Player player)
    {
        AudioState audioState = base.session.Audio;
        audioState.Ambience.Event = SFX.EventnameByHandle(this.oldAmbience.ambienceTrack);
        audioState.Ambience.Progress = this.oldAmbience.ambienceProgress;
        audioState.Ambience.Parameters = ObjectUtils.DeepCopyList<MEP>(this.oldAmbience.ambienceParameters);
        audioState.AmbienceVolume = this.oldAmbience.ambienceVolume;
        audioState.Apply();
    }

}

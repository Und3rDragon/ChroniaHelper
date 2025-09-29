using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/MusicTrigger")]
public class MusicTrigger : BaseTrigger
{

    private string musicTrack;

    private int musicProgress;

    private string musicParameter;

    private float musicParameterValue;

    private int[] musicLayers;

    public MusicTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.musicTrack = data.Attr("musicTrack", null);
        this.musicProgress = data.Int("musicProgress", 0);
        this.musicParameter = data.Attr("musicParameter", null);
        this.musicParameterValue = data.Float("musicParameterValue", 1F);
        this.musicLayers = StringUtils.SplitToIntArray(data.Attr("musicLayers", null));

        deathReset = data.Bool("resetOnDeath", false);
    }
    private bool deathReset;

    protected override void OnEnterExecute(Player player)
    {
        Md.Session.musicReset = deathReset;
        AudioState audioState = base.session.Audio;

        Md.Session.oldMusic.musicTrack = audioState.Music.Event;
        Md.Session.oldMusic.musicProgress = audioState.Music.Progress;
        Md.Session.oldMusic.musicParameters = ObjectUtils.DeepCopyList<MEP>(audioState.Music.Parameters);
        Md.Session.musicStored = true;

        if(this.musicTrack != "current")
        {
            audioState.Music.Event = SFX.EventnameByHandle(this.musicTrack);
        }
        audioState.Music.Progress = this.musicProgress;
        if (!musicParameter.IsNullOrEmpty())
        {
            audioState.Music.Param(this.musicParameter, this.musicParameterValue);
        }
        foreach (int layer in this.musicLayers)
        {
            audioState.Music.Layer(layer, true);
        }
        audioState.Apply();
    }

    protected override void LeaveReset(Player player)
    {
        AudioState audioState = base.session.Audio;
        audioState.Music.Event = SFX.EventnameByHandle(Md.Session.oldMusic.musicTrack);
        audioState.Music.Progress = Md.Session.oldMusic.musicProgress;
        audioState.Music.Parameters = ObjectUtils.DeepCopyList<MEP>(Md.Session.oldMusic.musicParameters);
        audioState.Apply();
    }

    protected override void AddedExecute(Scene scene)
    {
        AudioState audioState = base.session.Audio;
        if (!Md.Session.musicStored || !Md.Session.musicReset)
        {
            return;
        }
        audioState.Music.Event = SFX.EventnameByHandle(Md.Session.oldMusic.musicTrack);
        audioState.Music.Progress = Md.Session.oldMusic.musicProgress;
        audioState.Music.Parameters = ObjectUtils.DeepCopyList<MEP>(Md.Session.oldMusic.musicParameters);
        audioState.Apply();
    }

}

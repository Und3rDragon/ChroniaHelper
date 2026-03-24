using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities.RandomSeries;

[Tracked]
[CustomEntity("ChroniaHelper/RandomMusicController")]
public class RandomMusicController : BaseEntity
{
    public RandomMusicController(EntityData d, Vc2 o) : base(d, o)
    {
        musics = d.StringArray("musicNames", ';');
        interval = d.Float("interval", 1f).GetAbs().ClampMin(Engine.DeltaTime / 2f);
        mode = (Modes)d.Int("mode", 0);

        startDelay = d.Float("startDelay", -1f);
        if (mode == Modes.OnAdded)
        {
            AddedAwait = startDelay;
            active = false;
        }

        if (d.Bool("global", false))
        {
            Tag = Tags.Global;
        }
    }
    public string[] musics;
    public float interval;
    public enum Modes { OnAdded = 0 }
    public Modes mode;
    public float startDelay;

    public float timer;
    public bool active = false;
    protected override void AddedExecute(Scene scene)
    {
        if ((Tag | Tags.Global) != 0 && $"ChroniaHelper_TimedRandomController_{SourceData.ID}".GetFlag())
        {
            RemoveSelf();
            return;
        }
        $"ChroniaHelper_TimedRandomController_{SourceData.ID}".SetFlag(true);
        timer = 0f;
        active = true;
    }

    protected override void UpdateExecute()
    {
        if (!active)
        {
            return;
        }

        if (timer <= 0f)
        {
            SetRandomMusic();
            timer = interval;
        }

        timer -= Engine.DeltaTime;
    }

    public void SetRandomMusic()
    {
        MaP.level.Session.Audio.Music.Event = SFX.EventnameByHandle(musics[RandomUtils.RandomInt(0, musics.Length)]);
        MaP.level.Session.Audio.Apply();
    }
}

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
        musics = d.Attr("musics").ParseSquaredString();
        mode = (Modes)d.Int("mode", 0);
        startDelay = d.Float("startDelay", -1f);
        if (mode == Modes.OnAdded)
        {
            AddedAwait = startDelay;
            active = false;
        }
        allowRepeat = d.Bool("allowRepeat", true);

        if (d.Bool("global", false))
        {
            Tag = Tags.Global;
        }
    }
    public string[][] musics;
    public enum Modes { OnAdded = 0 }
    public Modes mode;
    public float startDelay;
    public bool allowRepeat;

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
        }

        timer -= Engine.DeltaTime;
    }

    private string lastPlayed = MaP.level?.Session?.Audio.Music.Event ?? "";
    public void SetRandomMusic()
    {
        int count = musics.GetLength(0);
        int index = RandomUtils.RandomInt(0, count);
        string[] choose = musics[index];
        float interval = 60f;
        if(choose.Length >= 2) 
        { 
            float.TryParse(choose[1], out interval); 
        }
        if (choose[0] == lastPlayed)
        {
            index++;
            choose = musics.ClampLoop(index);

            if (choose.Length >= 2)
            {
                float.TryParse(choose[1], out interval);
            }

            timer = interval;
            //Log.Info($"Equal, extend [{choose[0]}] by {interval}");
            return;
        }

        timer = interval;

        MaP.level.Session.Audio.Music.Event = SFX.EventnameByHandle(choose[0]);
        MaP.level.Session.Audio.Apply();
        lastPlayed = choose[0];
        //Log.Info($"Different, play [{choose[0]}] for {interval}");
    }
}

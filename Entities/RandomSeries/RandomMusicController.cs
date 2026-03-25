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

        if (global = d.Bool("globalEntity", false))
        {
            Tag = Tags.Global;
        }
    }
    public string[][] musics;
    public enum Modes { OnAdded = 0 }
    public Modes mode;
    public float startDelay;
    public bool allowRepeat;
    public bool global;

    public float timer;
    public bool active = false;
    protected override void AddedExecute(Scene scene)
    {
        if (global)
        {
            if (Md.Session.GlobalEntitiesRegistry.Contains(SourceId))
            {
                RemoveSelf();
                return;
            }
            Md.Session.GlobalEntitiesRegistry.Add(SourceId);
        }
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
            if (!allowRepeat)
            {
                index++;
                choose = musics.ClampLoop(index);
                interval = 60f;
                if (choose.Length >= 2)
                {
                    float.TryParse(choose[1], out interval);
                }

                ApplyMusic(choose[0]);
                timer = interval;
                return;
            }

            timer = interval;
            //Log.Info($"Equal, extend [{choose[0]}] by {interval}");
            return;
        }

        timer = interval;

        ApplyMusic(choose[0]);
        //Log.Info($"Different, play [{choose[0]}] for {interval}");
    }

    public void ApplyMusic(string name)
    {
        MaP.level.Session.Audio.Music.Event = SFX.EventnameByHandle(name);
        MaP.level.Session.Audio.Apply();
        lastPlayed = name;
    }
}

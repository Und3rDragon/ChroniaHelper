using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagCarouselTrigger")]
public class FlagCarouselTrigger : FlagManageTrigger
{

    private string carouselId;

    private string[][] flagList;

    private int index;

    private float interval;
    private string[] intervals;

    private bool reverse;

    private bool leavePause;

    private bool loopStartup;

    public bool onPause
    {
        get;
        private set;
    }

    private string[] addFlag;

    private HashSet<int> countSet;

    public FlagCarouselTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;
        regID = $"AutoCarouselID-{ID}";
        this.carouselId = data.Attr("carouselId", null);
        this.flagList = FlagUtils.ParseList(data.Attr("flagList", null));
        this.index = data.Int("index", 0);
        this.intervals = data.Attr("interval", "1").Split(',',StringSplitOptions.TrimEntries);
        this.reverse = data.Bool("reverse", false);
        this.leavePause = data.Bool("leavePause", false);
        if (!string.IsNullOrEmpty(this.carouselId))
        {
            regID = this.carouselId;
        }
        FlagCarouselManageTrigger.CarouselDictionary[regID] = this;
        base.AddTag(Tags.Global);
        if (base.onlyOnce)
        {
            this.countSet = new HashSet<int>();
        }
    }
    private int ID;
    private string regID;

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        
        if (this.loopStartup)
        {
            yield break;
        }

        if (ChroniaHelperModule.Session.CarouselState.ContainsKey(regID))
        {
            if (ChroniaHelperModule.Session.CarouselState[regID])
            {
                yield break;
            }
        }

        this.Resume();
        while (true)
        {
            if (this.onPause)
            {
                this.loopStartup = false;
                ChroniaHelperModule.Session.CarouselState[regID] = false;
                break;
            }
            foreach (string[] flag in this.flagList)
            {
                base.Remove(flag);
            }
            base.Add(this.addFlag = (this.flagList[index]));

            // process index
            if (intervals.Length <= 0)
            {
                interval = 1f;
            }
            else if (index < 0)
            {
                int i = index;
                while (i < intervals.Length)
                {
                    i += intervals.Length;
                    if (i >= 0 && i < intervals.Length)
                    {
                        break;
                    }
                }
                if (i >= intervals.Length)
                {
                    i = intervals.Length - 1;
                }
                if (!float.TryParse(intervals[i], out interval))
                {
                    interval = 1f;
                }
            }
            else if (index >= 0 && index < intervals.Length)
            {
                if (!float.TryParse(intervals[index], out interval))
                {
                    interval = 1f;
                }
            }
            else if (index >= 0 && index >= intervals.Length)
            {
                int i = index;
                while (i >= 0)
                {
                    i -= intervals.Length;
                    if (i >= 0 && i < intervals.Length)
                    {
                        break;
                    }
                }
                if (i < 0)
                {
                    i = intervals.Length - 1;
                }
                if (!float.TryParse(intervals[i], out interval))
                {
                    interval = 1f;
                }
            }

            if (base.onlyOnce)
            {
                if (this.countSet.Count >= this.flagList.Length - 1)
                {
                    base.RemoveSelf();
                }
                this.countSet.Add(index);
            }
            if (this.flagList.Length > 1)
            {
                base.Remove(this.flagList[NumberUtils.Mutation(this.index, 0, this.flagList.Length - 1, !this.reverse)]);
                this.index = NumberUtils.Mutation(this.index, 0, this.flagList.Length - 1, this.reverse);
            }
            
            yield return this.interval;
        }
        yield break;
    }

    protected override void OnLeaveExecute(Player player)
    {
        if (this.leavePause)
        {
            this.Pause();
        }
        if (this.leaveReset)
        {
            base.Remove(this.addFlag);
            this.Pause();
        }
    }

    protected override bool OnlyOnceExpression(Player player) => false;

    public void Pause()
    {
        this.onPause = true;
        ChroniaHelperModule.Session.CarouselState[regID] = false;
    }

    public void Resume()
    {
        this.onPause = false;
        this.loopStartup = true;
        ChroniaHelperModule.Session.CarouselState[regID] = true;
    }

    public void Cancel()
    {
        this.Pause();
        this.index = 0;
        foreach (string[] flag in this.flagList)
        {
            base.Remove(flag);
        }
        ChroniaHelperModule.Session.CarouselState.Remove(regID);
    }

    public void Remove()
    {
        this.Cancel();
        base.RemoveSelf();
    }

}

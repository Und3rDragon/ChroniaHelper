using Celeste.Mod.Entities;
using Celeste.Pico8;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/BPMCounter")]
public class BPMCounter : BaseEntity
{
    public BPMCounter(EntityData data, Vc2 offset) : base(data, offset)
    {
        Tag = Tags.FrozenUpdate | Tags.PauseUpdate | Tags.TransitionUpdate;
        
        BPM = data.Int("bpm" , 144).ClampMin(1);
        Loop = data.Int("beatsPerLoop", 8).ClampMin(1);
        name = data.Attr("counter", "bpmCounter");
        flag = data.Attr("flag");
        mode = (Mode)data.Int("mode", 0);
        double.TryParse(data.Attr("offsetSeconds"), out double _beatOffset);
        beatOffset = new TimeSpan(0, 0,0,0, (int)(_beatOffset * 1000));
    }
    public string name, flag;
    public int BPM = 144;
    public int Loop = 8;
    public double Beat => 60000.0 / BPM;
    public int Index = 0;
    public enum Mode {RawLevelTime, RelativeTime}
    public Mode mode;
    public DateTime registerTime = DateTime.Now;
    public TimeSpan t = TimeSpan.Zero;
    public TimeSpan beatOffset;

    public override void Update()
    {
        base.Update();
        
        if (flag.HasValidContent())
        {
            if (!flag.GetGeneralInvertedFlag())
            {
                registerTime = DateTime.Now;
                return;
            }
        }
        
        if (mode == Mode.RelativeTime)
        {
            t = DateTime.Now - registerTime + beatOffset;
        }
        else
        {
            t = DateTime.Now - Md.Session.LevelStartTime + beatOffset;
        }
        
        Index = (int)NumberUtils.Mod(t.TotalMilliseconds / Beat, Loop);
        name.SetCounter(Index);
    }
}
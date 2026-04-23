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
        BPM = data.Int("bpm" , 144).ClampMin(1);
        Loop = data.Int("beatsPerLoop", 8).ClampMin(1);
        name = data.Attr("counter", "bpmCounter");
        flag = data.Attr("flag");
        mode = (Mode)data.Int("mode", 0);
    }
    public string name, flag;
    public int BPM = 144;
    public int Loop = 8;
    public double Beat => 60.0 / BPM;
    public int Index = 0;
    public enum Mode {RawLevelTime, RelativeTime}
    public Mode mode;
    public double registerTime = 0, t = 0;

    public override void Update()
    {
        base.Update();
        
        if (flag.HasValidContent())
        {
            if (!flag.GetGeneralInvertedFlag())
            {
                registerTime = MaP.level?.RawTimeActive ?? 0;
                return;
            }
        }
        
        if (mode == Mode.RelativeTime)
        {
            t = (MaP.level?.RawTimeActive ?? 0) - registerTime;
        }
        else
        {
            t = MaP.level?.RawTimeActive ?? 0;
        }
        
        Index = (int)NumberUtils.Mod(t / Beat, Loop);
        name.SetCounter(Index);
    }
}
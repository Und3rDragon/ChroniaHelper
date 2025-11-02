using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.StopwatchSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/StopclockFlagController")]
public class StopclockFlagController : Entity
{
    public StopclockFlagController(EntityData d, Vc2 o) : base(d.Position + o)
    {
        tags = d.Attr("stopclockTags").Split(',',StringSplitOptions.TrimEntries);
        global = d.Bool("global", false);
        
        Initialize();
    }
    private string[] tags;
    private string prefix = "ChroniaHelper_Stopclock_";
    private bool global;
    private Dictionary<string, int> maxDigitals = new();
    
    private void Initialize()
    {
        
    }

    public override void Update()
    {
        base.Update();

        foreach(string tag in tags)
        {
            if (!tag.GetStopclock(out Stopclock clock)) { continue; }

            clock.GetTimeData(out int[] digitals);

            int maxDigital = 0;
            for(int i = 0; i < digitals.Length; i++)
            {
                if (digitals[digitals.Length - 1 - i] != 0) 
                { 
                    maxDigital = digitals.Length - i; 
                    break; 
                }
            }
            
            if (maxDigitals.ContainsKey(tag))
            {
                maxDigitals[tag] = maxDigital;
            }
            else
            {
                maxDigitals.Enter(tag, digitals.Length);
            }

            for (int i = 0; i < maxDigitals[tag]; i++)
            {
                for (int j = 0; j <= 9; j++)
                {
                    $"{prefix}{tag}_{i}_{j}".SetFlag(false, global);
                }
            }

            for (int i = 0; i < digitals.Length; i++)
            {
                $"{prefix}{tag}_{i}_{digitals[i]}".SetFlag(true, global);
            }
        }
    }
}

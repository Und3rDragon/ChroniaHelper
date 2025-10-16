using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.StopwatchSystem;

namespace ChroniaHelper.Entities;

public class StopclockFlagController : Entity
{
    public StopclockFlagController(EntityData d, Vc2 o) : base(d.Position + o)
    {
        tags = d.Attr("stopclockTags").Split(',',StringSplitOptions.TrimEntries);

        Initialize();
    }
    private string[] tags;
    private string prefix = "ChroniaHelper_Stopclock_";
    private bool global;
    
    private void Initialize()
    {
        
    }

    public override void Update()
    {
        base.Update();

        foreach(string tag in tags)
        {
            if (!tag.GetStopclock(out Stopclock clock)) { continue; }

            clock.GetDigitals(out int[] digitals, true);

            for (int i = 0; i < digitals.Length; i++)
            {
                for (int j = 0; j <= 9; j++)
                {
                    $"{prefix}{tag}_{i}_{j}".SetFlag(false);
                }
                $"{prefix}{tag}_{i}_{digitals[i]}".SetFlag(true);
            }
        }
    }
}

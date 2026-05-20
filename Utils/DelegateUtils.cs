using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

[Note("An assistive class that process certain delegates that's suggested to use a reference function instead of EmitDelegate () => {} or such")]
public static class DelegateUtils
{
    [Credits("zuccanium for delegate instrctions")]
    public static float ModifyBoosterRedDashSpeed(float fallback)
    {
        if (PUt.TryGetPlayer(out var p))
        {
            if (p.CurrentBooster is PatientBooster b)
            {
                return b.redSpeed.Value;
            }
            if (p.CurrentBooster is CustomBooster c)
            {
                if (c.redBoostMovingSpeed.Expression.ToLower() == "playerspeed")
                {
                    return c.recordedSpeed.Length();
                }
                return c.redBoostMovingSpeed.Value;
            }
        }
        return fallback;
    }

    [Credits("zuccanium for delegate instrctions")]
    public static float ModifyBoosterGreenDashSpeed(float fallback)
    {
        if (PUt.TryGetPlayer(out var p))
        {
            if (p.CurrentBooster is PatientBooster b)
            {
                return b.greenSpeed.Value;
            }
            if (p.CurrentBooster is CustomBooster c)
            {
                if (c.greenBoostMovingSpeed.Expression.ToLower() == "playerspeed")
                {
                    return c.recordedSpeed.Length();
                }
                return c.greenBoostMovingSpeed.Value;
            }
        }
        return fallback;
    }
}

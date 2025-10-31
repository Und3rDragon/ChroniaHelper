using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

public class TimedCameraOffsetTrigger : BaseTrigger
{
    public TimedCameraOffsetTrigger(EntityData data, Vc2 offset) : base(data, offset)
    {
        nodes = data.NodesWithPosition(offset);

        activateDelay = data.Float("activateDelay", 0.5f);
        activateProgress = data.Float("activateDuration", 0.5f);
        duration = data.Float("duration", 1f);
        recoverDuration = data.Float("recoverDuration", 0.5f);
        
        this.offset = new Vc2(data.Float("offsetX", 0f), data.Float("offsetY", 0f));
    }
    private Vc2 offset = Vc2.Zero;
    private float activateDelay = 0.5f, activateProgress = 0.5f, duration = 1f, recoverDuration = 0.5f;
    
    protected override IEnumerator OnEnterRoutine(Player player)
    {
        yield return activateDelay;

        float coeff = 0f;

        Vc2 orig_offset = MaP.level.CameraOffset;

        while (coeff < 1f)
        {
            coeff = Calc.Approach(coeff, 1f, Engine.DeltaTime * activateProgress);
            MaP.level.CameraOffset = coeff.LerpValue(0f, 1f, orig_offset, offset);
        }

        yield return duration;

        coeff = 1f;

        while (coeff > 0f)
        {
            coeff = Calc.Approach(coeff, 1f, Engine.DeltaTime * activateProgress);
            MaP.level.CameraOffset = coeff.LerpValue(0f, 1f, orig_offset, offset);
        }

        MaP.level.CameraOffset = orig_offset;
    }
}

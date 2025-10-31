using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Triggers;

public class TimedCameraTargetTrigger : BaseTrigger
{
    public TimedCameraTargetTrigger(EntityData data, Vc2 offset) : base(data, offset)
    {
        nodes = data.NodesWithPosition(offset);
        
        activateDelay = data.Float("activateDelay", 0.5f);
        activateProgress = data.Float("activateDuration", 0.5f);
        duration = data.Float("duration", 1f);
        recoverDuration = data.Float("recoverDuration", 0.5f);
        lerpStrength = data.Float("lerpStrength", 1f);

        ignoreX = data.Bool("ignoreX", false);
        ignoreY = data.Bool("ignoreY", false);
    }
    private float activateDelay = 0.5f, activateProgress = 0.5f, duration = 1f, recoverDuration = 0.5f, lerpStrength = 1f;
    private bool ignoreX = false, ignoreY = false;
    
    protected override IEnumerator OnEnterRoutine(Player player)
    {
        yield return activateDelay;

        float coeff = 0f;
        player.CameraAnchor = nodes[1] - new Vc2(160f, 90f);
        player.CameraAnchorLerp = Vc2.Zero;
        player.CameraAnchorIgnoreX = ignoreX;
        player.CameraAnchorIgnoreY = ignoreY;
        
        while(coeff < 1f)
        {
            coeff = Calc.Approach(coeff, 1f, Engine.DeltaTime * activateProgress);
            player.CameraAnchorLerp = Vc2.One * lerpStrength * coeff;
        }

        yield return duration;

        coeff = 1f;
        
        while(coeff > 0f)
        {
            coeff = Calc.Approach(coeff, 0f, Engine.DeltaTime * recoverDuration);
            player.CameraAnchorLerp = Vc2.One * lerpStrength * coeff;
        }

        player.CameraAnchorLerp = Vc2.Zero;
    }
}

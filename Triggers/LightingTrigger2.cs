using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/LightingTrigger2")]
public class LightingTrigger2 : BaseTrigger
{
    public LightingTrigger2(EntityData d, Vc2 o) : base(d, o)
    {
        this.lightingColorFrom = d.Attr("lightingColorFrom");
        this.lightingColorTo = d.Attr("lightingColorTo");
        this.lightingAlphaFrom = d.Attr("lightingAlphaFrom");
        this.lightingAlphaTo = d.Attr("lightingAlphaTo");
        this.positionMode = d.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
        timed = d.Float("timed", -1f);
    }
    private string lightingColorFrom;
    private string lightingColorTo;
    private string lightingAlphaFrom;
    private string lightingAlphaTo;
    private PositionModes positionMode;
    private float timed, timer;

    private MaP.LevelEnvironmentData oldData;
    private float lerp;

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        oldData = MaP.FetchLevelEnvironment();

        timer = 0f;
        main = new(ChangeLighting());
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);

        lerp = GetPositionLerp(player, positionMode);
    }

    public override void Update()
    {
        base.Update();

        main?.Update();
    }

    private Coroutine main = null;
    public IEnumerator ChangeLighting()
    {
        if(timed > 0f)
        {
            float progress = 0f;

            while(timer < timed)
            {
                timer = Calc.Approach(timer, timed, Engine.DeltaTime);
                progress = timer / timed;

                if (lightingColorTo.HasValidContent())
                {
                    Color c1 = oldData.LightingColor;
                    if (lightingColorFrom.HasValidContent())
                    {
                        c1 = Calc.HexToColor(lightingColorFrom);
                    }

                    Color c2 = Calc.HexToColor(lightingColorTo);

                    MaP.SetLightingColor(Color.Lerp(c1, c2, progress));
                }

                if (lightingAlphaTo.HasValidContent())
                {
                    float a1 = oldData.LightingAlpha;
                    if (lightingAlphaFrom.HasValidContent())
                    {
                        float.TryParse(lightingAlphaFrom, out a1);
                    }

                    float.TryParse(lightingAlphaTo, out float a2);

                    MaP.SetLightingAlpha(progress.LerpValue(0f, 1f, a1, a2));
                }

                yield return null;
            }
        }
        else
        {
            while (true)
            {
                if (lightingColorTo.HasValidContent())
                {
                    Color c1 = oldData.LightingColor;
                    if (lightingColorFrom.HasValidContent())
                    {
                        c1 = Calc.HexToColor(lightingColorFrom);
                    }

                    Color c2 = Calc.HexToColor(lightingColorTo);

                    MaP.SetLightingColor(Color.Lerp(c1, c2, lerp));
                }

                if (lightingAlphaTo.HasValidContent())
                {
                    float a1 = oldData.LightingAlpha;
                    if (lightingAlphaFrom.HasValidContent())
                    {
                        float.TryParse(lightingAlphaFrom, out a1);
                    }

                    float.TryParse(lightingAlphaTo, out float a2);

                    MaP.SetLightingAlpha(lerp.LerpValue(0f, 1f, a1, a2));
                }

                yield return null;
            }
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.LeaveReset(player);

        MaP.SetLightingColor(oldData.LightingColor, clear: true);
        MaP.SetLightingAlpha(oldData.LightingAlpha);
    }
}

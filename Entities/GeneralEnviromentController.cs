using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using MonoMod.Logs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using YoctoHelper.Hooks;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/GeneralEnviromentController")]
public class GeneralEnviromentController : GeneralSetupController
{
    public GeneralEnviromentController(EntityData data, Vc2 offset) : base(data, offset)
    {
        fadeTime = data.Float("fadeTime", -1f);
        bloomBaseTo = data.Attr("bloomBaseTo");
        bloomColorTo = data.Attr("bloomColorTo");
        bloomStrengthTo = data.Attr("bloomStrengthTo");
        lightingTo = data.Attr("lightingTo");
        lightingColorTo = data.Attr("lightingColorTo");

        Tag = Tags.TransitionUpdate;
    }
    private float fadeTime = -1f;
    private bool fade => fadeTime > 0f;
    private string bloomBaseTo, bloomStrengthTo, bloomColorTo,
        lightingTo, lightingColorTo;

    private float fadeTimer = 0f;

    private MaP.LevelEnvironmentData oldData;
    public void TryRegisterOldParams(Scene scene)
    {
        Level level = MaP.level;

        oldData = MaP.FetchLevelEnvironment(level);
    }

    public override void SceneBegin(Scene scene)
    {
        TryRegisterOldParams(scene);

        base.SceneBegin(scene);
    }

    public override void Added(Scene scene)
    {
        TryRegisterOldParams(scene);

        base.Added(scene);
    }

    public override void ExecuteByUpdateState(bool current, bool previous)
    {
        base.ExecuteByUpdateState(current, previous);

        if (current != previous && current)
        {
            Add(new Coroutine(Changing()));
        }
    }

    private IEnumerator Changing()
    {
        Level level = MaP.level;

        TryRegisterOldParams(null);

        if (!fade)
        {
            ChangeEnvironmentImmediately();
            yield break;
        }

        float progress = 0f;

        while(fadeTimer != fadeTime)
        {
            fadeTimer = Calc.Approach(fadeTimer, fadeTime, Engine.DeltaTime);

            progress = fadeTimer / fadeTime;

            if (bloomBaseTo.HasValidContent() && 
                float.TryParse(bloomBaseTo, out float b1))
            {
                MaP.SetBloomBase(progress.LerpValue(0f, 1f, oldData.BloomBase, b1));
            }
            
            if (bloomColorTo.HasValidContent())
            {
                MaP.SetBloomColor(Color.Lerp(oldData.BloomColor, Calc.HexToColor(bloomColorTo), progress));
            }
            
            if (bloomStrengthTo.HasValidContent() && 
                float.TryParse(bloomStrengthTo, out float b2))
            {
                MaP.SetBloomStrength(progress.LerpValue(0f, 1f, oldData.BloomStrength, b2));
            }
            
            if (lightingTo.HasValidContent() && 
                float.TryParse(lightingTo, out float l1))
            {
                MaP.SetLightingAlpha(progress.LerpValue(0f, 1f, oldData.LightingAlpha, l1));
            }
            
            if (lightingColorTo.HasValidContent())
            {
                MaP.SetLightingColor(Color.Lerp(oldData.LightingColor, Calc.HexToColor(lightingColorTo), progress));
            }

            yield return null;
        }

        yield return null;
    }

    private void ChangeEnvironmentImmediately()
    {
        Level level = MaP.level;

        if (bloomBaseTo.HasValidContent() && float.TryParse(bloomBaseTo, out float b1))
        {
            MaP.SetBloomBase(b1);
        }
        
        if (bloomColorTo.HasValidContent())
        {
            MaP.SetBloomColor(Calc.HexToColor(bloomColorTo));
        }
        
        if(bloomStrengthTo.HasValidContent() && float.TryParse(bloomStrengthTo, out float b2))
        {
            MaP.SetBloomStrength(b2);
        }
        
        if(lightingTo.HasValidContent() && float.TryParse(lightingTo, out float l1))
        {
            MaP.SetLightingAlpha(l1);
        }
        
        if (lightingColorTo.HasValidContent())
        {
            MaP.SetLightingColor(Calc.HexToColor(lightingColorTo));
        }
    }
}

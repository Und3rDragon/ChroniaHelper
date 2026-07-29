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
        lightingBaseTo = data.Attr("lightingBaseTo");
        lightingAddTo = data.Attr("lightingAddTo");
        lightingColorTo = data.Attr("lightingColorTo");

        Tag = Tags.TransitionUpdate;
    }
    private float fadeTime = -1f;
    private bool fade => fadeTime > 0f;
    private string bloomBaseTo, bloomStrengthTo, bloomColorTo,
        lightingBaseTo, lightingAddTo, lightingColorTo;

    private float fadeTimer = 0f;

    private Color? oldBloomColor = null;
    private float? oldBloomBase = null, oldBloomStrength = null;
    private Color? oldLightingColor = null;
    private float? oldLightingBase = null, oldLightingAdd = null;
    public void TryRegisterOldParams(Scene scene)
    {
        Level level = MaP.level;

        oldBloomBase = level.Bloom.Base;
        oldBloomStrength = level.Bloom.Strength;
        oldBloomColor = GetBloomColor();

        oldLightingAdd = level.Session.LightingAlphaAdd;
        oldLightingBase = level.BaseLightingAlpha;
        oldLightingColor = level.Lighting.BaseColor;
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

    public override void Execute()
    {
        Add(new Coroutine(Changing()));
    }

    private IEnumerator Changing()
    {
        if (!fade)
        {
            ChangeEnvironmentImmediately();
            yield break;
        }

        Level level = MaP.level;

        float progress = 0f;

        while(fadeTimer != fadeTime)
        {
            fadeTimer = Calc.Approach(fadeTimer, fadeTime, Engine.DeltaTime);

            progress = fadeTimer / fadeTime;

            if (oldBloomBase != null && 
                bloomBaseTo.HasValidContent() && 
                float.TryParse(bloomBaseTo, out float b1))
            {
                level.Bloom.Base = progress.LerpValue(0f, 1f, (float)oldBloomBase, b1);
            }
            if (oldBloomColor != null && bloomColorTo.HasValidContent())
            {
                SetBloomColor(Color.Lerp((Color)oldBloomColor, Calc.HexToColor(bloomColorTo), progress));
            }
            if (oldBloomStrength != null && 
                bloomStrengthTo.HasValidContent() && 
                float.TryParse(bloomStrengthTo, out float b2))
            {
                level.Bloom.Strength = progress.LerpValue(0f, 1f, (float)oldBloomStrength, b2);
            }
            if (oldLightingBase != null &&
                lightingBaseTo.HasValidContent() && 
                float.TryParse(lightingBaseTo, out float l1))
            {
                level.BaseLightingAlpha = progress.LerpValue(0f, 1f, (float)oldLightingBase, l1);
            }
            if (oldLightingAdd != null &&
                lightingAddTo.HasValidContent() && 
                float.TryParse(lightingAddTo, out float l2))
            {
                level.Session.LightingAlphaAdd = progress.LerpValue(0f, 1f, (float)oldLightingAdd, l2);
            }
            level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
            if (oldLightingColor != null &&
                lightingColorTo.HasValidContent())
            {
                level.Lighting.BaseColor = Color.Lerp((Color)oldLightingColor, Calc.HexToColor(lightingColorTo), progress);
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
            level.Bloom.Base = b1;
        }
        if (bloomColorTo.HasValidContent())
        {
            SetBloomColor(Calc.HexToColor(bloomColorTo));
        }
        if(bloomStrengthTo.HasValidContent() && float.TryParse(bloomStrengthTo, out float b2))
        {
            level.Bloom.Strength = b2;
        }
        if(lightingBaseTo.HasValidContent() && float.TryParse(lightingBaseTo, out float l1))
        {
            level.BaseLightingAlpha = l1;
        }
        if(lightingAddTo.HasValidContent() && float.TryParse(lightingAddTo, out float l2))
        {
            level.Session.LightingAlphaAdd = l2;
        }
        if (lightingColorTo.HasValidContent())
        {
            level.Lighting.BaseColor = Calc.HexToColor(lightingColorTo);
        }
    }

    private Color GetBloomColor()
    {
        return ChroniaHelperModule.Instance.HookManager.GetHookDataValue<Color>(HookId.BloomColor);
    }

    private void SetBloomColor(Color value)
    {
        ChroniaHelperModule.Instance.HookManager.SetHookDataValue<Color>(HookId.BloomColor, value, false);
    }
}

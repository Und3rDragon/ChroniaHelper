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
            
            if (oldLightingBase != null && oldLightingAdd != null &&
                lightingTo.HasValidContent() && 
                float.TryParse(lightingTo, out float l1))
            {
                level.Session.LightingAlphaAdd = progress.LerpValue(0f, 1f, (float)oldLightingAdd, l1 - (float)oldLightingBase);
                level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
            }
            
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
        
        if(lightingTo.HasValidContent() && float.TryParse(lightingTo, out float l1))
        {
            level.Session.LightingAlphaAdd = l1 - level.BaseLightingAlpha;
            level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
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

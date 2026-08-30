using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Triggers;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Triggers;

[Tracked]
[CustomEntity("ChroniaHelper/GeneralEnviromentTrigger")]
public class GeneralEnviromentTrigger : BaseTrigger
{
    public GeneralEnviromentTrigger(EntityData data, Vc2 offset) : base(data, offset)
    {
        fadeTime = data.Float("fadeTime", -1f);

        bloomBaseTo = data.Attr("bloomBaseTo");
        bloomColorTo = data.Attr("bloomColorTo");
        bloomStrengthTo = data.Attr("bloomStrengthTo");
        lightingTo = data.Attr("lightingTo");
        lightingColorTo = data.Attr("lightingColorTo");
        bloomBaseFrom = data.Attr("bloomBaseFrom");
        bloomColorFrom = data.Attr("bloomColorFrom");
        bloomStrengthFrom = data.Attr("bloomStrengthFrom");
        lightingFrom = data.Attr("lightingFrom");
        lightingColorFrom = data.Attr("lightingColorFrom");

        positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);

        Tag = Tags.TransitionUpdate;
    }
    private float fadeTime = -1f;
    private bool timeFade => fadeTime > 0f;
    private string bloomBaseTo, bloomStrengthTo, bloomColorTo,
        lightingTo, lightingColorTo;
    private string bloomBaseFrom, bloomStrengthFrom, bloomColorFrom,
        lightingFrom, lightingColorFrom;

    private float fadeTimer = 0f;
    private PositionModes positionMode;

    private MaP.LevelEnvironmentData oldData;
    public void TryRegisterOldParams(Scene scene)
    {
        Level level = scene is Level ? scene as Level : MaP.level;

        oldData = MaP.FetchLevelEnvironment(level);
    }

    public void TryRecoverOldParams(Scene scene)
    {
        Level level = scene is Level ? scene as Level : MaP.level;

        MaP.SetBloomBase(oldData.BloomBase);
        MaP.SetBloomColor(oldData.BloomColor);
        MaP.SetBloomStrength(oldData.BloomStrength, clear: true);
        MaP.SetLightingAlpha(oldData.LightingAlpha);
        MaP.SetLightingColor(oldData.LightingColor, clear: true);
    }

    protected override void LeaveReset(Player player)
    {
        base.LeaveReset(player);

        TryRecoverOldParams(SceneAs<Level>());
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

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        Level level = SceneAs<Level>();

        TryRegisterOldParams(level);

        if (!timeFade)
        {
            yield break;
        }

        float progress = 0f;

        while (fadeTimer != fadeTime)
        {
            fadeTimer = Calc.Approach(fadeTimer, fadeTime, Engine.DeltaTime);

            progress = fadeTimer / fadeTime;

            if (bloomBaseTo.HasValidContent() &&
                float.TryParse(bloomBaseTo, out float b1))
            {
                float from = oldData.BloomBase;
                if (bloomBaseFrom.HasValidContent())
                {
                    float.TryParse(bloomBaseFrom, out from);
                }

                MaP.SetBloomBase(progress.LerpValue(0f, 1f, from, b1));
            }

            if (bloomColorTo.HasValidContent())
            {
                Color from = oldData.BloomColor;
                Color to = Calc.HexToColor(bloomColorTo);
                if (bloomColorFrom.HasValidContent())
                {
                    from = Calc.HexToColor(bloomColorFrom);
                }

                MaP.SetBloomColor(Color.Lerp(from, to, progress));
            }

            if (bloomStrengthTo.HasValidContent() &&
                float.TryParse(bloomStrengthTo, out float b2))
            {
                float from = oldData.BloomStrength;
                if (bloomStrengthFrom.HasValidContent())
                {
                    float.TryParse (bloomStrengthFrom, out from);
                }

                MaP.SetBloomStrength(progress.LerpValue(0f, 1f, from, b2));
            }

            if (lightingTo.HasValidContent() &&
                float.TryParse(lightingTo, out float l1))
            {
                float from = oldData.LightingAlpha;
                if (lightingFrom.HasValidContent())
                {
                    float.TryParse(lightingFrom, out from);
                }

                MaP.SetLightingAlpha(progress.LerpValue(0f, 1f, from, l1));
            }

            if (lightingColorTo.HasValidContent())
            {
                Color from = oldData.LightingColor;
                if (lightingColorFrom.HasValidContent())
                {
                    from = Calc.HexToColor(lightingColorFrom);
                }
                Color to = Calc.HexToColor(lightingColorTo);

                MaP.SetLightingColor(Color.Lerp(from, to, progress));
            }

            yield return null;
        }

        yield return null;
    }

    protected override void OnStayExecute(Player player)
    {
        base.OnStayExecute(player);

        if (timeFade) { return; }

        float progress = GetPositionLerp(player, positionMode);
        
        if (bloomBaseTo.HasValidContent() &&
                float.TryParse(bloomBaseTo, out float b1))
        {
            float from = oldData.BloomBase;
            if (bloomBaseFrom.HasValidContent())
            {
                float.TryParse(bloomBaseFrom, out from);
            }

            MaP.SetBloomBase(progress.LerpValue(0f, 1f, from, b1));
        }

        if (bloomColorTo.HasValidContent())
        {
            Color from = oldData.BloomColor;
            Color to = Calc.HexToColor(bloomColorTo);
            if (bloomColorFrom.HasValidContent())
            {
                from = Calc.HexToColor(bloomColorFrom);
            }

            MaP.SetBloomColor(Color.Lerp(from, to, progress));
        }

        if (bloomStrengthTo.HasValidContent() &&
            float.TryParse(bloomStrengthTo, out float b2))
        {
            float from = oldData.BloomStrength;
            if (bloomStrengthFrom.HasValidContent())
            {
                float.TryParse(bloomStrengthFrom, out from);
            }

            MaP.SetBloomStrength(progress.LerpValue(0f, 1f, from, b2));
        }

        if (lightingTo.HasValidContent() &&
            float.TryParse(lightingTo, out float l1))
        {
            float from = oldData.LightingAlpha;
            if (lightingFrom.HasValidContent())
            {
                float.TryParse(lightingFrom, out from);
            }

            MaP.SetLightingAlpha(progress.LerpValue(0f, 1f, from, l1));
        }

        if (lightingColorTo.HasValidContent())
        {
            Color from = oldData.LightingColor;
            if (lightingColorFrom.HasValidContent())
            {
                from = Calc.HexToColor(lightingColorFrom);
            }
            Color to = Calc.HexToColor(lightingColorTo);

            MaP.SetLightingColor(Color.Lerp(from, to, progress));
        }
    }
}


using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using YoctoHelper.Hooks;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/BloomFadeTrigger")]
public class BloomFadeTrigger : BaseTrigger
{

    private string bloomBaseFrom;

    private string bloomBaseTo;

    private string bloomStrengthFrom;

    private string bloomStrengthTo;

    private string bloomColorFrom;

    private string bloomColorTo;

    private PositionModes positionMode;

    private float timer, t;

    private bool timedFade;

    private MaP.LevelEnvironmentData oldData;

    public BloomFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.bloomBaseFrom = data.Attr("bloomBaseFrom");
        this.bloomBaseTo = data.Attr("bloomBaseTo");
        this.bloomStrengthFrom = data.Attr("bloomStrengthFrom");
        this.bloomStrengthTo = data.Attr("bloomStrengthTo");
        this.bloomColorFrom = data.Attr("bloomColorFrom");
        this.bloomColorTo = data.Attr("bloomColorTo");
        this.positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
        this.timer = data.Float("timedFade", -1f);
        timedFade = timer > 0;
    }

    protected override void OnEnterExecute(Player player)
    {
        if(timedFade)
        {
            t = timer;
        }

        oldData = MaP.FetchLevelEnvironment();
    }

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        if (timedFade)
        {
            while (t >= 0f)
            {
                t = Calc.Approach(t, -1f, Engine.DeltaTime);
                float progress = ((timer - t) / timer).Clamp(0f, 1f);

                bool _from = float.TryParse(bloomBaseFrom, out float from);
                bool _to = float.TryParse(bloomBaseTo, out float to);

                if (_to)
                {
                    float bloomBase = Calc.ClampedMap(progress, 0f, 1f, _from ? from : oldData.BloomBase, to);
                    MaP.SetBloomBase(bloomBase);
                }

                bool _sfrom = float.TryParse(bloomStrengthFrom, out float sfrom);
                bool _sto = float.TryParse(bloomStrengthTo, out float sto);

                if (_sto)
                {
                    MaP.SetBloomStrength(Calc.ClampedMap(progress, 0f, 1f, _sfrom ? sfrom : oldData.BloomStrength, sto));
                }

                Color cfrom = Calc.HexToColor(bloomColorFrom);
                Color cto = Calc.HexToColor(bloomColorTo);

                if (bloomColorTo.HasValidContent())
                {
                    MaP.SetBloomColor(Color.Lerp(cfrom, cto, progress));
                }

                yield return null;
            }
        }
    }

    protected override void OnStayExecute(Player player)
    {
        if (!timedFade)
        {
            float progress = base.GetPositionLerp(player, this.positionMode);

            bool _from = float.TryParse(bloomBaseFrom, out float from);
            bool _to = float.TryParse(bloomBaseTo, out float to);

            if (_to)
            {
                float bloomBase = Calc.ClampedMap(progress, 0f, 1f, _from ? from : oldData.BloomBase, to);
                MaP.SetBloomBase(bloomBase);
            }

            bool _sfrom = float.TryParse(bloomStrengthFrom, out float sfrom);
            bool _sto = float.TryParse(bloomStrengthTo, out float sto);

            if (_sto)
            {
                MaP.SetBloomStrength(Calc.ClampedMap(progress, 0f, 1f, _sfrom ? sfrom : oldData.BloomStrength, sto));
            }

            Color cfrom = Calc.HexToColor(bloomColorFrom);
            Color cto = Calc.HexToColor(bloomColorTo);

            if (bloomColorTo.HasValidContent())
            {
                MaP.SetBloomColor(Color.Lerp(cfrom, cto, progress));
            }
        }
    }

    protected override void LeaveReset(Player player)
    {
        MaP.SetBloomBase(oldData.BloomBase);
        MaP.SetBloomStrength(oldData.BloomStrength, clear: true);
        MaP.SetBloomColor(oldData.BloomColor);
    }

}

using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using YoctoHelper.Hooks;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/BloomTrigger")]
public class BloomTrigger : BaseTrigger
{

    private float bloomBase;

    private float bloomStrength;

    private Color bloomColor;

    private MaP.LevelEnvironmentData oldData;

    private float timer, t;
    private bool timedFade;

    public BloomTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.bloomBase = data.Float("bloomBase", 0F);
        this.bloomStrength = data.Float("bloomStrength", 1F);
        this.bloomColor = data.HexColor("bloomColor", Color.White);
        this.timer = data.Float("timedFade", -1);
        timedFade = timer > 0f;
    }

    protected override void OnEnterExecute(Player player)
    {
        t = timer;

        oldData = MaP.FetchLevelEnvironment();

        if (!timedFade)
        {
            MaP.SetBloomBase(bloomBase);
            MaP.SetBloomStrength(bloomStrength);
            MaP.SetBloomColor(this.bloomColor);
        }
    }

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        if (timedFade)
        {
            while (t >= 0f)
            {
                t = Calc.Approach(t, -1f, Engine.DeltaTime);
                float progress = ((timer - t) / timer).Clamp(0f, 1f);
                float bloomBase = Calc.ClampedMap(progress, 0f, 1f, this.oldData.BloomBase, this.bloomBase);
                MaP.SetBloomBase(bloomBase);
                MaP.SetBloomStrength(Calc.ClampedMap(progress, 0f, 1f, this.oldData.BloomStrength, this.bloomStrength));
                MaP.SetBloomColor(Color.Lerp(this.oldData.BloomColor, this.bloomColor, progress));

                yield return null;
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

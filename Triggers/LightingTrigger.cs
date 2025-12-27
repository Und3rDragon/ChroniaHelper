using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/LightingTrigger")]
public class LightingTrigger : BaseTrigger
{

    private Color lightingColor;

    private float lightingAlpha;

    private OldLighting oldLighting;

    private float timed, timer;

    public struct OldLighting
    {
        public Color lightingColor;

        public float lightingAlpha;

        public float lightingAlphaAdd;
        
        public float baseLightingAlpha;
    };

    private float lightingAlphaAdd;
    private float baseLightingAlpha;

    private bool _lcolor, _lalpha, _lalphaadd, _lbase;
    public LightingTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.lightingColor = data.HexColor("lightingColor", Color.Black);
        this.lightingAlpha = data.Float("lightingAlpha", 0F);
        this.lightingAlphaAdd = data.Float("lightingAlphaAdd", 0f);
        this.baseLightingAlpha = data.Float("baseLightingAlpha", 0f);

        _lcolor = data.Bool("changeLightingColor", true);
        _lalpha = data.Bool("changeLightingAlpha", true);
        _lalphaadd = data.Bool("changeLightingAlphaAdd", false);
        _lbase = data.Bool("changeBaseLightingAlpha", false);

        timed = data.Float("timed", -1f);
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset || timed > 0f)
        {
            oldLighting = new OldLighting
            {
                lightingColor = base.level.Lighting.BaseColor,
                lightingAlpha = base.level.Lighting.Alpha,
                lightingAlphaAdd = base.session.LightingAlphaAdd,
                baseLightingAlpha = base.level.BaseLightingAlpha,
            };
        }
        if(timed <= 0f)
        {
            if (_lcolor)
            {
                base.level.Lighting.BaseColor = this.lightingColor;
            }
            if (_lalpha)
            {
                base.level.Lighting.Alpha = this.lightingAlpha;
            }
            if (_lalphaadd)
            {
                base.session.LightingAlphaAdd = this.lightingAlphaAdd;
            }
            if (_lbase)
            {
                base.level.BaseLightingAlpha = this.baseLightingAlpha;
            }
        }
        else if(timed > 0f)
        {
            timer = timed;
        }
    }

    protected override IEnumerator OnEnterRoutine(Player player)
    {
        if (timed > 0f)
        {
            while (timer >= 0f)
            {
                timer = Calc.Approach(timer, -1f, Engine.DeltaTime);
                if (_lcolor)
                {
                    level.Lighting.BaseColor = Color.Lerp(oldLighting.lightingColor, lightingColor,
                        FadeUtils.LerpValue(timer, timed, 0f, 0f, 1f));
                }
                if (_lalpha)
                {
                    level.Lighting.Alpha = FadeUtils.LerpValue(timer, timed, 0f, oldLighting.lightingAlpha, lightingAlpha);
                }
                if (_lalphaadd)
                {
                    level.Session.LightingAlphaAdd = FadeUtils.LerpValue(timer, timed, 0f, oldLighting.lightingAlphaAdd, lightingAlphaAdd);
                }
                if (_lbase)
                {
                    level.BaseLightingAlpha = FadeUtils.LerpValue(timer, timed, 0f, oldLighting.baseLightingAlpha, baseLightingAlpha);
                }

                yield return null;
            }
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.level.Lighting.BaseColor = this.oldLighting.lightingColor;
        base.level.Lighting.Alpha = this.oldLighting.lightingAlpha;
        base.session.LightingAlphaAdd = this.oldLighting.lightingAlphaAdd;
        base.level.BaseLightingAlpha = this.oldLighting.baseLightingAlpha;
    }


}

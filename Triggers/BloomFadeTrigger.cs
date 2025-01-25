using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using YoctoHelper.Hooks;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/BloomFadeTrigger")]
public class BloomFadeTrigger : BaseTrigger
{

    private float bloomBaseFrom;

    private float bloomBaseTo;

    private float bloomStrengthFrom;

    private float bloomStrengthTo;

    private Color bloomColorFrom;

    private Color bloomColorTo;

    private PositionModes positionMode;

    private BloomTrigger.OldBloom oldBloom;

    public BloomFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.bloomBaseFrom = data.Float("bloomBaseFrom", 0F);
        this.bloomBaseTo = data.Float("bloomBaseTo", 0F);
        this.bloomStrengthFrom = data.Float("bloomStrengthFrom", 1F);
        this.bloomStrengthTo = data.Float("bloomStrengthTo", 1F);
        this.bloomColorFrom = data.HexColor("bloomColorFrom", Color.White);
        this.bloomColorTo = data.HexColor("bloomColorTo", Color.White);
        this.positionMode = data.Enum<PositionModes>("positionMode", PositionModes.NoEffect);
    }

    protected override void OnEnterExecute(Player player)
    {
        if (base.leaveReset)
        {
            this.oldBloom.bloomBase = base.level.Bloom.Base;
            this.oldBloom.bloomBaseAdd = base.session.BloomBaseAdd;
            this.oldBloom.bloomStrength = base.level.Bloom.Strength;
            this.oldBloom.bloomColor = this.GetBloomColor();
        }
    }

    protected override void OnStayExecute(Player player)
    {
        float lerp = base.GetPositionLerp(player, this.positionMode);
        float bloomBase = Calc.ClampedMap(lerp, 0f, 1f, this.bloomBaseFrom, this.bloomBaseTo);
        base.level.Bloom.Base = bloomBase;
        base.session.BloomBaseAdd = bloomBase - AreaData.Get(base.level).BloomBase;
        base.level.Bloom.Strength = Calc.ClampedMap(lerp, 0f, 1f, this.bloomStrengthFrom, this.bloomStrengthTo);
        this.SetBloomColor(Color.Lerp(this.bloomColorFrom, this.bloomColorTo, lerp));
    }

    protected override void LeaveReset(Player player)
    {
        base.level.Bloom.Base = this.oldBloom.bloomBase;
        base.session.BloomBaseAdd = this.oldBloom.bloomBaseAdd;
        base.level.Bloom.Strength = this.oldBloom.bloomStrength;
        this.SetBloomColor(this.oldBloom.bloomColor);
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

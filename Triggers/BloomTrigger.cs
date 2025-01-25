using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using YoctoHelper.Hooks;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/BloomTrigger")]
public class BloomTrigger : BaseTrigger
{

    private float bloomBase;

    private float bloomStrength;

    private Color bloomColor;

    private OldBloom oldBloom;

    public struct OldBloom
    {
        public float bloomBase;

        public float bloomBaseAdd;

        public float bloomStrength;

        public Color bloomColor;
    };

    public BloomTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.bloomBase = data.Float("bloomBase", 0F);
        this.bloomStrength = data.Float("bloomStrength", 1F);
        this.bloomColor = data.HexColor("bloomColor", Color.White);
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
        base.level.Bloom.Base = this.bloomBase;
        base.session.BloomBaseAdd = this.bloomBase - AreaData.Get(base.level).BloomBase;
        base.level.Bloom.Strength = this.bloomStrength;
        this.SetBloomColor(this.bloomColor);
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

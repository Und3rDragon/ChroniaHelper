using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/ScreenshakeSettingTrigger")]
public class ScreenshakeSettingTrigger : BaseTrigger
{
    public ScreenshakeSettingTrigger(EntityData data, Vc2 offset) : base(data, offset)
    {
        screenshake = (ScreenshakeAmount)data.Int("value", 0);
    }
    public ScreenshakeAmount screenshake = 0;

    protected override void OnEnterExecute(Player player)
    {
        Celeste.Settings.Instance.ScreenShake = screenshake;
        Md.Session.CurrentScreenshake = screenshake;
    }
}

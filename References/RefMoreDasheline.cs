using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.MaxHelpingHand.Module;
using MoreDasheline;

namespace ChroniaHelper.References;

public static class RefMoreDasheline
{
    public static MoreDashelineModule GetModule()
    {
        return MoreDashelineModule.Instance;
    }

    public static MoreDashelineSession GetSession()
    {
        return MoreDashelineModule.Session;
    }

    public static MoreDashModuleSettings GetSettings()
    {
        return MoreDashelineModule.Settings;
    }

    public static Color GetDashColor(int dashes, bool badeline)
    {
        return MoreDashelineModule.GetDashColor(dashes, badeline);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.CommunalHelper;

namespace ChroniaHelper.References;

public static class RefCommunalHelper
{
    public static CommunalHelperModule GetModule()
    {
        return CommunalHelperModule.Instance;
    }

    public static CommunalHelperSaveData GetSaveData()
    {
        return CommunalHelperModule.SaveData;
    }

    public static CommunalHelperSession GetSession()
    {
        return CommunalHelperModule.Session;
    }

    public static CommunalHelperSettings GetSettings()
    {
        return CommunalHelperModule.Settings;
    }

    public static ButtonBinding ActivateFlagController => GetSettings().ActivateFlagController;
}

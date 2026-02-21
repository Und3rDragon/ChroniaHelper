using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.XaphanHelper;

namespace ChroniaHelper.References;

public static class RefXaphanHelper
{
    public static XaphanModule GetModule()
    {
        return XaphanModule.Instance;
    }

    public static XaphanModuleSaveData GetSaveData()
    {
        return XaphanModule.ModSaveData;
    }

    public static XaphanModuleSession GetSession()
    {
        return XaphanModule.ModSession;
    }

    public static XaphanModuleSettings GetSettings()
    {
        return XaphanModule.ModSettings;
    }

    public static HashSet<string> GlobalFlags => GetSaveData().GlobalFlags;
}

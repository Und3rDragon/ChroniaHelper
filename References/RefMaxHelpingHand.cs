using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.MaxHelpingHand.Module;

namespace ChroniaHelper.References;

public static class RefMaxHelpingHand
{
    public static MaxHelpingHandModule GetModule()
    {
        return MaxHelpingHandModule.Instance;
    }

    public static MaxHelpingHandSaveData GetSaveData()
    {
        return MaxHelpingHandModule.Instance.SaveData;
    }

    public static MaxHelpingHandSession GetSession()
    {
        return MaxHelpingHandModule.Instance.Session;
    }

    public static MaxHelpingHandSettings GetSettings()
    {
        return MaxHelpingHandModule.Instance.Settings;
    }

    public static int CameraWidth => MaxHelpingHandModule.CameraWidth;
    public static int CameraHeight => MaxHelpingHandModule.CameraHeight;
}

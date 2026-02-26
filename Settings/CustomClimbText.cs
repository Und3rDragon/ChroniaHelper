using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;
using MonoMod.Utils;

namespace ChroniaHelper.Settings;

public static class CustomClimbText
{
    [Cores.LoadHook]
    public static void Load()
    {
        On.Celeste.OuiChapterPanel.Reset += OnReset;
    }
    [Cores.UnloadHook]
    public static void Unload()
    {
        On.Celeste.OuiChapterPanel.Reset -= OnReset;
    }

    public static void OnReset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self)
    {
        orig(self);

        string chapterID = ("chroniahelper_climbtext_" + self.Area.GetSID()).DialogKeyify();
        
        List<OuiChapterPanel.Option> options = (List<OuiChapterPanel.Option>)new DynData<OuiChapterPanel>(self)["modes"];

        foreach(var entry in options)
        {
            if (entry.ID == "A")
            {
                if (Dialog.Has(chapterID))
                {
                    entry.Label = Dialog.Clean(chapterID);
                }
            }
            else
            {
                string alterID = chapterID + $"_{entry.ID}";

                if (Dialog.Has(alterID))
                {
                    entry.Label = Dialog.Clean(alterID);
                }
            }
        }
    }
}

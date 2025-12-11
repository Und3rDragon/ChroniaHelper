using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Settings;

public class FlagAssistMode
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Update += OnLevelUpdate;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Update -= OnLevelUpdate;
    }
    
    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        if (self.Session.GetFlag("chronia_assistmode"))
        {
            self.Session.SetFlag("chronia_assistmode", false);
            MaP.currentSaveData?.Item2.AssistMode = true;
        }

        if (self.Session.GetFlag("chronia_cheatmode"))
        {
            self.Session.SetFlag("chronia_cheatmode", false);
            MaP.currentSaveData?.Item2.CheatMode = true;
        }

        if (self.Session.GetFlag("chronia_variantmode"))
        {
            self.Session.SetFlag("chronia_variantmode", false);
            MaP.currentSaveData?.Item2.VariantMode = true;
        }

        if (self.Session.GetFlag("chronia_fullcheat"))
        {
            self.Session.SetFlag("chronia_fullcheat", false);
            MaP.currentSaveData?.Item2.CheatMode = true;
            MaP.currentSaveData?.Item2.VariantMode = true;
        }
    }
}

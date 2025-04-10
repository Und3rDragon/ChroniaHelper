using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Entities;
using ChroniaHelper.Modules;
using ChroniaHelper.Triggers;
using MonoMod.RuntimeDetour;
using ChroniaHelper.Utils;
using ChroniaHelper.Effects;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static Celeste.ClutterBlock;
using System.Collections;

namespace ChroniaHelper.Cores;

public static class MapProcessor
{
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += OnLevelLoadLevel;
        On.Celeste.MapData.Load += OnMapDataLoad;
        On.Celeste.LevelLoader.LoadingThread += OnLevelReload;
        On.Celeste.SaveData.LoadModSaveData += OnLoadSaveData;
        On.Celeste.Level.Update += OnLevelUpdate;
        On.Celeste.Level.Reload += LevelReload;
    }

    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= OnLevelLoadLevel;
        On.Celeste.MapData.Load -= OnMapDataLoad;
        On.Celeste.LevelLoader.LoadingThread -= OnLevelReload;
        On.Celeste.SaveData.LoadModSaveData -= OnLoadSaveData;
        On.Celeste.Level.Update -= OnLevelUpdate;
        On.Celeste.Level.Reload -= LevelReload;
    }

    public static AreaKey areakey;
    public static MapData mapdata;
    public static int saveSlotIndex;
    public static Level level;
    public static Session session;
    public static Dictionary<Type, List<Entity>> entities;

    public static Entity globalEntityDummy = new Entity();

    public static bool isRespawning = false;
    private static void OnLevelLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level level, Player.IntroTypes intro, bool isFromLoader)
    {
        MapProcessor.level = level;
        session = level.Session;
        entities = level.Tracker.Entities;
        mapdata = session.MapData;
        areakey = session.MapData.Area;

        // Dummy Entity setup
        level.Add(globalEntityDummy);

        // Apply global flags
        foreach (var item in ChroniaHelperModule.SaveData.globalflags)
        {
            level.Session.SetFlag(item.Key, item.Value);
        }
        
        // Check all the switches and save the flags
        var levels = level.Session.MapData.Levels;
        string flagName;
        foreach (var lv in levels)
        {
            foreach (var item in lv.Entities)
            {
                if (item.Name == "ChroniaHelper/RealFlagSwitch" || item.Name == "ChroniaHelper/RealFlagSwitchAlt")
                {
                    // Can get the Entity ID here
                    flagName = item.Values["flag"].ToString().Trim();
                    SwitchFlagSlot($"ChroniaButtonFlag-{flagName}-ButtonID-{item.ID}", false);
                    if (!ChroniaHelperModule.Session.flagNames.Contains(flagName))
                    {
                        ChroniaHelperModule.Session.flagNames.Add(flagName);
                    }
                }
            }
        }

        isRespawning = (intro != Player.IntroTypes.Transition);
        orig.Invoke(level, intro, isFromLoader);
        isRespawning = false;
    }
    
    private static void OnMapDataLoad(On.Celeste.MapData.orig_Load orig, MapData map)
    {
        mapdata = map;

        orig.Invoke(map);
    }

    private static void OnLevelReload(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        orig(self);

        // Flag Carousel Trigger State reset
        foreach (var item in ChroniaHelperModule.Session.CarouselState.Keys)
        {
            ChroniaHelperModule.Session.CarouselState[item] = false;
        }
    }

    private static void LevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        // Only once after respawn, not when into the room
        orig(self);

        // Reset Temporary Flags
        foreach (var item in ChroniaHelperSession.TemporaryFlags.Keys)
        {
            string ID = ChroniaHelperSession.TemporaryFlags[item].flagID;
            bool state = ChroniaHelperSession.TemporaryFlags[item].flagState;
            bool global = ChroniaHelperSession.TemporaryFlags[item].isGlobal;
            Utils.FlagUtils.SetFlag(ID, state, global);
        }
        ChroniaHelperSession.TemporaryFlags.Clear();
    }

    public static void OnLoadSaveData(On.Celeste.SaveData.orig_LoadModSaveData orig, int index)
    {
        saveSlotIndex = index;
        orig(index);
    }

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        // Flag Button Flag setup
        if (ChroniaHelperModule.Session.flagNames != null)
        {
            foreach (var item in ChroniaHelperModule.Session.flagNames)
            {
                if (IsSwitchFlagCompleted(item))
                {
                    level.Session.SetFlag(item, true);
                    SwitchFlagSave(item, true);
                }
            }
        }

        orig(self);
    }

    // Check whether the group of touch switches is completed
    public static bool IsSwitchFlagCompleted(string flagIndex)
    {
        bool b = true;
        int count = 0;
        foreach (string key in ChroniaHelperModule.Session.switchFlag.Keys)
        {
            if (key.StartsWith($"ChroniaButtonFlag-{flagIndex}-ButtonID-"))
            {
                b = b ? ChroniaHelperModule.Session.switchFlag[key] : false;
                count++;
            }
        }
        if (count == 0) { return false; }
        return b;
    }

    // Creating slots for the flags
    public static void SwitchFlagSlot(string key, bool defaultValue)
    {
        if (!ChroniaHelperModule.Session.switchFlag.ContainsKey(key))
        {
            ChroniaHelperModule.Session.switchFlag.Add(key, defaultValue);
        }
    }

    // Save or overwrite the existing values
    public static void SwitchFlagSave(string key, bool overwrite)
    {
        if (ChroniaHelperModule.Session.switchFlag.ContainsKey(key))
        {
            ChroniaHelperModule.Session.switchFlag.Remove(key);
        }
        ChroniaHelperModule.Session.switchFlag.Add(key, overwrite);
    }

}

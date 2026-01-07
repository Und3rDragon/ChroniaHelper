using System;
using System.Diagnostics;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using MonoMod.Utils;
using YamlDotNet.Core;
using YoctoHelper.Cores;

namespace ChroniaHelper.Cores;

public static class MapProcessor
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.SaveData.Start += OnSaveDataStart;
        On.Celeste.Level.LoadLevel += OnLevelLoadLevel;
        On.Celeste.MapData.Load += OnMapDataLoad;
        On.Celeste.LevelLoader.LoadingThread += OnLevelReload;
        On.Celeste.SaveData.LoadModSaveData += OnLoadModSaveData;
        On.Celeste.Level.Update += OnLevelUpdate;
        On.Celeste.Level.Reload += LevelReload;
        On.Monocle.Scene.Update += GlobalUpdate;
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.SaveData.Start -= OnSaveDataStart;
        On.Celeste.Level.LoadLevel -= OnLevelLoadLevel;
        On.Celeste.MapData.Load -= OnMapDataLoad;
        On.Celeste.LevelLoader.LoadingThread -= OnLevelReload;
        On.Celeste.SaveData.LoadModSaveData -= OnLoadModSaveData;
        On.Celeste.Level.Update -= OnLevelUpdate;
        On.Celeste.Level.Reload -= LevelReload;
        On.Monocle.Scene.Update -= GlobalUpdate;
    }

    public static AreaKey areakey;
    public static MapData mapdata;
    public static int saveSlotIndex;
    public static Level level;
    public static EntityList entities;

    public static Entity globalEntityDummy = new Monocle.Entity();

    public static bool isRespawning = false;
    public static Vector2 camOffset = Vector2.Zero;

    public static Dictionary<string, Session.Slider> sliders = new();

    //BG Switch references
    public static Grid bgSolidTilesGrid (Level level) => CreateBgtileGrid(level);
    public static Solid bgModeSolidTiles(Level level) => new Solid(new Vector2((float)level.Bounds.Left, (float)level.Bounds.Top), 1f, 1f, true)
    {
        AllowStaticMovers = false,
        Collidable = bgMode,
        Collider = ((bgMode || BgEntityInLevel(level)) ? bgSolidTilesGrid(level) : null),
        EnableAssistModeChecks = false,
    };
    public static Entity bgSolidTiles(Level level) => new Entity(new Vector2((float)level.Bounds.Left, (float)level.Bounds.Top))
    {
        Collidable = true,
        Collider = bgSolidTilesGrid(level),
    };
    public static bool bgMode = false;

    public static Tuple<int, SaveData> currentSaveData = null;
    private static void OnSaveDataStart(On.Celeste.SaveData.orig_Start orig, SaveData self, int index)
    {
        orig(self, index);

        currentSaveData = new (index, self);
    }
    private static void OnLevelLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level level, Player.IntroTypes intro, bool isFromLoader)
    {
        MaP.level = level;
        entities = level.Entities;
        camOffset = level.CameraOffset;
        mapdata = level.Session.MapData;
        areakey = level.Session.MapData.Area;

        object _slider = new DynamicData(level.Session).Get("_Sliders");
        sliders = (Dictionary<string, Session.Slider>)_slider;

        // Dummy Entity setup
        level.Add(globalEntityDummy);

        // Apply Flag Timer Trigger flags
        foreach (var flag in Md.SaveData.FlagTimerS.Keys)
        {
            if (Md.SaveData.FlagTimerS[flag] > 0)
            {
                flag.SetFlag(true);
            }
        }

        // Check all the switches and save the flags
        Md.Session.flagNames = new();
        Md.Session.switchFlag = new();
        var levels = level.Session.MapData.Levels;
        string flagName;
        HashSet<string> switches = new()
        { 
            "ChroniaHelper/RealFlagSwitch", 
            "ChroniaHelper/RealFlagSwitchAlt", 
            "ChroniaHelper/RealFlagSwitch2" 
        };
        foreach (var lv in levels)
        {
            foreach (var item in lv.Entities)
            {
                if (switches.Contains(item.Name))
                {
                    // Can get the Entity ID here
                    flagName = item.Values["flag"].ToString().Trim();
                    RegisterSwitchFlags(flagName, item.ID);
                    
                    Md.Session.flagNames.Enter(flagName);
                }
            }
        }

        isRespawning = (intro != Player.IntroTypes.Transition);
        orig.Invoke(level, intro, isFromLoader);
        isRespawning = false;
        
        // level grid and bg switch
        level.Session.SetFlag("bg_mode", bgMode);
        level.SolidTiles.Collidable = !bgMode;
    }
    
    public static Grid CreateBgtileGrid(this Level level)
    {
        Rectangle rectangle = new Rectangle(level.Bounds.Left / 8, level.Bounds.Y / 8, level.Bounds.Width / 8, level.Bounds.Height / 8);
        Rectangle tileBounds = level.Session.MapData.TileBounds;
        bool[,] array = new bool[rectangle.Width, rectangle.Height];
        for (int i = 0; i < rectangle.Width; i++)
        {
            for (int j = 0; j < rectangle.Height; j++)
            {
                array[i, j] = (level.BgData[i + rectangle.Left - tileBounds.Left, j + rectangle.Top - tileBounds.Top] != '0');
            }
        }
        return new Grid(8f, 8f, array);
    }

    private static bool BgEntityInLevel(Level level)
    {
        //return level.Entities.Any((Entity e) => e is BGModeToggle || e is BGModeTrigger);
        return false;
    }
    
    private static void OnMapDataLoad(On.Celeste.MapData.orig_Load orig, MapData map)
    {
        mapdata = map;

        orig.Invoke(map);
    }

    private static void OnLevelReload(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        level = self.Level;
        
        orig(self);

        // Flag Carousel Trigger State reset
        foreach (var item in Md.Session.CarouselState.Keys)
        {
            Md.Session.CarouselState[item] = false;
        }
    }

    private static void LevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        level = self;

        // Only once after respawn, not when into the room
        orig(self);
    }

    public static void OnLoadModSaveData(On.Celeste.SaveData.orig_LoadModSaveData orig, int index)
    {
        saveSlotIndex = index;
        orig(index);
    }

    public static Vc2 cameraPos => level?.Camera.Position ?? Vc2.Zero;
    public static Vc2 cameraCenter => cameraPos + new Vc2(160f, 90f);
    public static Vc2 levelPos => new Vc2(level?.Bounds.Left ?? 0, level?.Bounds.Top ?? 0);
    public static Vc2 InParallax(this Vc2 position, Vc2 parallax)
    {
        return cameraCenter + (position - cameraCenter) * parallax;
    }
    public static Vc2 InGlobalParallax(this Vc2 position, Vc2 parallax)
    {
        return position.InParallax(parallax) + levelPos;
    }
    public static Vc2 InParallax(this Vc2 position, Vc2 parallax, Vc2 staticScreen)
    {
        Vc2 inparallax = position.InParallax(parallax);
        float X = parallax.X == 0 ? cameraPos.X + staticScreen.X : inparallax.X;
        float Y = parallax.Y == 0 ? cameraPos.Y + staticScreen.Y : inparallax.Y;
        return new Vc2(X, Y);
    }
    public static Vc2 InGlobalParallax(this Vc2 position, Vc2 parallax, Vc2 staticScreen)
    {
        Vc2 inglobalparallax = position.InGlobalParallax(parallax);
        float X = parallax.X == 0 ? cameraPos.X + staticScreen.X : inglobalparallax.X;
        float Y = parallax.Y == 0 ? cameraPos.Y + staticScreen.Y : inglobalparallax.Y;
        return new Vc2(X, Y);
    }
    
    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        
        level = self;
        entities = level.Entities;

        string languageFlag = $"ChroniaHelper_Language_{Dialog.Language?.Id ?? "english"}";
        self.Session.Flags.RemoveWhere((flag) =>
            flag.IsNotNull() && flag.StartsWith("ChroniaHelper_Language_") && flag != languageFlag
        );
        languageFlag.SetFlag(true);
        
        // Flag Button Flag setup
        if (Md.Session.flagNames != null)
        {
            foreach (var item in Md.Session.flagNames)
            {
                if (IsSwitchFlagCompleted(item))
                {
                    level.Session.SetFlag(item, true);
                }
            }
        }

        // Flag Timer Trigger
        foreach (var timer in Md.Session.FlagTimer.Keys)
        {
            Md.Session.FlagTimer[timer] = Monocle.Calc.Approach(Md.Session.FlagTimer[timer], 0f, Monocle.Engine.DeltaTime);
            if (Md.Session.FlagTimer[timer] == 0f) {
                timer.SetFlag(false);
            }
        }
    }

    public static Scene scene;
    public static Level sLevel => scene as Level;
    public static void GlobalUpdate(On.Monocle.Scene.orig_Update orig, Monocle.Scene self)
    {
        scene = self;
        orig(self);

        if (Md.SaveData.IsNotNull())
        {
            // Flag Timer Trigger
            foreach (var timer in Md.SaveData.FlagTimerS.Keys)
            {
                Md.SaveData.FlagTimerS[timer] = Monocle.Calc.Approach(Md.SaveData.FlagTimerS[timer], 0f, Monocle.Engine.DeltaTime);
                if (Md.SaveData.FlagTimerS[timer] == 0f) {
                    timer.SetFlag(false);
                }
            }
        }
    }

    public static Vc2 CameraPos(this Level level) => level?.Camera.Position ?? Vc2.Zero;
    public static Vc2 CameraCenter(this Level level) => CameraPos(level) + new Vc2(160f, 90f);
    public static Vc2 LevelPos(this Level level) => new Vc2(level?.Bounds.Left ?? 0, level?.Bounds.Top ?? 0);
    public static Vc2 CameraPos(this Scene scene) => (scene as Level)?.Camera.Position ?? Vc2.Zero;
    public static Vc2 CameraCenter(this Scene scene) => CameraPos(scene as Level) + new Vc2(160f, 90f);
    public static Vc2 LevelPos(this Scene scene) => new Vc2((scene as Level)?.Bounds.Left ?? 0, (scene as Level)?.Bounds.Top ?? 0);

    // Check whether the group of touch switches is completed
    public static bool IsSwitchFlagCompleted(string flagIndex)
    {
        bool b = true;
        foreach (string key in Md.Session.switchFlag)
        {
            if (key.StartsWith($"ChroniaButtonFlag-{flagIndex}-ButtonID-"))
            {
                b.TryNegative(key.GetFlag());
            }
        }
        
        return b;
    }

    // Creating slots for the flags
    public static void RegisterSwitchFlags(string name, int ID)
    {
        Md.Session.switchFlag.Add($"ChroniaButtonFlag-{name}-ButtonID-{ID}");
    }
    
}

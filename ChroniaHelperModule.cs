using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Celeste.Mod.ChroniaHelperIndicatorZone;
using Celeste.Mod.Helpers;
using Celeste.Mod.Meta;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;
using FMOD.Studio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.ModInterop;
using YoctoHelper.Hooks;
using Extensions = Celeste.Mod.Extensions;

namespace ChroniaHelper;

public class ChroniaHelperModule : EverestModule
{

    public const string Name = "ChroniaHelper";

    public static ChroniaHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(ChroniaHelperSettings);

    public static ChroniaHelperSettings Settings => (ChroniaHelperSettings)ChroniaHelperModule.Instance._Settings;

    public override Type SessionType => typeof(ChroniaHelperSession);

    public static ChroniaHelperSession Session => (ChroniaHelperSession)ChroniaHelperModule.Instance._Session;

    public override Type SaveDataType => typeof(ChroniaHelperSaveData);

    public static ChroniaHelperSaveData SaveData => (ChroniaHelperSaveData)ChroniaHelperModule.Instance._SaveData;

    public static ChroniaHelperGlobalSaveData GlobalData { get; private set; }
    
    public ChroniaHelperHandle ChroniaHelperHandle { get; private set; }

    public HookManager HookManager { get; private set; }
    
    public enum Languages
    {
        English = 0,
        Brazilian = 1,
        French = 2,
        German = 3,
        Italian = 4,
        Japanese = 5,
        Korean =6,
        Russian = 7,
        SimplifiedChinese = 8,
        Spanish = 9,
    }

    public static Dictionary<Languages, string> LanguageID = new()
    {
        { Languages.English, "english" },
        { Languages.Brazilian, "brazilian" },
        { Languages.French, "french" },
        { Languages.German, "german" },
        { Languages.Italian, "italian" },
        { Languages.Japanese, "japanese" },
        { Languages.Korean, "korean" },
        { Languages.Russian, "russian" },
        { Languages.SimplifiedChinese, "schinese" },
        { Languages.Spanish, "spanish" },
    };

    public string ModDirectory
    {
        get => Path.Combine(Path.GetDirectoryName(FakeAssembly.GetFakeEntryAssembly().Location), $"Mods\\{ChroniaHelperModule.Name}");
    }

    public static bool teraMode = false;

    public ChroniaHelperModule()
    {
        Instance = this;
    }

    public static bool InstanceReady => Session != null && SaveData != null;

    public static bool FrostHelperLoaded => CheckDependency("FrostHelper", "1.70.2");
    public static bool CommunalHelperLoaded => CheckDependency("CommunalHelper", "1.23.0");
    public static bool MaddieLoaded => CheckDependency("MaxHelpingHand", "1.38.0");
    public static bool XaphanHelperLoaded => CheckDependency("XaphanHelper", "1.0.78");

    public static bool CheckDependency(string modName, string minimumVersion)
    {
        EverestModuleMetadata meta = new()
        {
            Name = modName,
            Version = new Version(minimumVersion)
        };

        return Everest.Loader.DependencyLoaded(meta);
    }

    public static bool CheckDependency(string modName, string minimumVersion,
        out EverestModule module)
    {
        EverestModuleMetadata meta = new()
        {
            Name = modName,
            Version = new Version(minimumVersion)
        };

        return Everest.Loader.TryGetDependency(meta, out module);
    }

    public override void Load()
    {
        Log.Info("Welcome to use ChroniaHelper!");
        ChroniaHelperModule.Instance = this;
        this.ChroniaHelperHandle = new ChroniaHelperHandle();
        this.HookManager = new HookManager();
        ChroniaHelperModule.Instance.ChroniaHelperHandle.LoadHandle();

        GlobalData = new ChroniaHelperGlobalSaveData();
        GlobalData.LoadAll(); // 自动从多个 XML 文件加载
        
        On.Celeste.Celeste.OnExiting += OnGameExiting;
        
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        LoadingManager.Load();
        
        Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;

        // migrated from VivHelper
        On.Celeste.GameLoader.Begin += LateInitialize;

        // API Imports
        typeof(APIFrostHelper).ModInterop();
        typeof(APICameraDynamics).ModInterop();

        // Map Hider?
        IL.Celeste.AreaData.Load += HookAreaDataLoad;
    }

    public override void Unload()
    {
        ChroniaHelperModule.Instance.ChroniaHelperHandle.UnloadHandle();
        
        LoadingManager.Unload();

        On.Celeste.Celeste.OnExiting -= OnGameExiting;

        Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        
        // migrated from VivHelper
        On.Celeste.GameLoader.Begin -= LateInitialize;

        IL.Celeste.AreaData.Load -= HookAreaDataLoad;
    }

    private static bool IsFromHelpers(ModAsset asset)
    {
        if (GlobalData.IsNull()) { return false; }
        
        foreach(var item in GlobalData.HelperMapsToHide)
        {
            if (item.ToLower() == asset?.Source?.Name.ToLower()) { return true; }
        }
        
        return false;
    }

    private static void HookAreaDataLoad(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        int i = 0;
        if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdloc(out i),
                instr => instr.MatchLdfld<ModAsset>("PathVirtual"))
            && cursor.TryGotoPrev(MoveType.After, instr => instr.MatchStloc(i)))
        {
            cursor.Emit(OpCodes.Ldloc, i);
            cursor.EmitDelegate(IsFromHelpers);
            ILLabel target = cursor.DefineLabel();
            cursor.Emit(OpCodes.Brtrue, target);
            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld<AreaData>("OnLevelBegin")).MarkLabel(target);
        }
    }

    private void OnGameExiting(On.Celeste.Celeste.orig_OnExiting orig, Celeste.Celeste self, object sender, EventArgs args)
    {
        GlobalData.SaveAll();
        
        orig(self, sender, args);
    }

    // 这个需要参数（UnhandledExceptionEventArgs）
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", "Emergency save due to crash...");
            GlobalData?.SaveAll();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "ChroniaHelper", $"Emergency save failed: {ex}");
        }
    }

    public static void LateInitialize(On.Celeste.GameLoader.orig_Begin orig, GameLoader self)
    {
        orig(self);
        // Temporary.
        SolidModifierComponent.player_WallJumpCheck_getNum = (player, dir) =>
        {
            int num = 3;
            bool flag = player.DashAttacking && player.DashDir.X == 0f && player.DashDir.Y == -1f;
            if (flag)
            {
                Spikes.Directions directions = ((dir <= 0) ? Spikes.Directions.Right : Spikes.Directions.Left);
                foreach (Spikes entity in player.Scene.Tracker.GetEntities<Spikes>())
                {
                    if (entity.Direction == directions && player.CollideCheck(entity, player.Position + Vc2.UnitX * dir * 5f))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
            {
                num = 5;
            }
            return num;
        };
    }

    private static void LevelLoader_OnLoadingThread(Level level)
    {
        level.Add(new PlayerIndicatorZone.IconRenderer());
    }

    public override void PrepareMapDataProcessors(MapDataFixup context)
    {
        base.PrepareMapDataProcessors(context);

        context.Add<Modules.MaxHelpingHandMapDataProcessor>();
    }

    // Create Custom ChroniaHelper Menu
    public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot)
    {
        base.CreateModMenuSection(menu, inGame, snapshot);

        TextMenu.Item wiki_mapalong = new TextMenu.Button(Extensions.DialogClean("wiki_mapalong"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://www.youtube.com/watch?v=gzHQOnYHaO0") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_iamdadbod = new TextMenu.Button(Extensions.DialogClean("wiki_iamdadbod"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://www.youtube.com/watch?v=TqoQdNZ_CRA&list=PLBP5_qAilzbjr7DGxatTQbPfftY3LiVA4") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_fandom = new TextMenu.Button(Extensions.DialogClean("wiki_fandom"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://celestegame.fandom.com/wiki/Celeste") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_ink = new TextMenu.Button(Extensions.DialogClean("wiki_ink"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://celeste.ink/wiki/Main_Page") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_everest = new TextMenu.Button(Extensions.DialogClean("wiki_everest"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://saplonily.top/celeste_wiki/general/wiki/") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_bilibili = new TextMenu.Button(Extensions.DialogClean("wiki_bilibili"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://wiki.biligame.com/celeste/%E9%A6%96%E9%A1%B5") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_all = new TextMenu.Button(Extensions.DialogClean("wiki_all"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://saplonily.top/celeste_wiki/general/wiki/") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_UnderDragon = new TextMenu.Button(Extensions.DialogClean("wiki_UnderDragon"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://www.bilibili.com/video/BV1Eu4y1L78Y/?spm_id_from=333.337.search-card.all.click") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_psyZ = new TextMenu.Button(Extensions.DialogClean("wiki_psyZ"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://www.bilibili.com/video/BV1tR4y1X7wu/?spm_id_from=333.337.search-card.all.click") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_miao = new TextMenu.Button(Extensions.DialogClean("wiki_miao"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://celestenyaserver.github.io/CelesteMiaoServer.Wiki/#/zh-cn/Celeste/README") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_maddie = new TextMenu.Button(Extensions.DialogClean("wiki_maddie"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://maddie480.ovh/") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_prismatic = new TextMenu.Button(Extensions.DialogClean("wiki_prismatic"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://github.com/l-Luna/PrismaticHelper/blob/master/DOCUMENTATION.md") { UseShellExecute = true };
                Process.Start(info);
            }
        },

        wiki_adam = new TextMenu.Button(Extensions.DialogClean("wiki_adam"))
        {
            OnPressed = delegate ()
            {
                ProcessStartInfo info = new("https://gist.github.com/AdamKorinek/7e27d288701db5a0df095f756f0f8e9a") { UseShellExecute = true };
                Process.Start(info);
            }
        };

        TextMenu.Item[] menuOrder = new[]
        {
            wiki_all,
            wiki_everest,
            wiki_fandom,
            wiki_ink,
            wiki_bilibili,
            wiki_miao,
            wiki_iamdadbod,
            wiki_mapalong,
            wiki_psyZ,
            wiki_UnderDragon,
            wiki_maddie,
            wiki_adam,
            wiki_prismatic,
        };

        TextMenuExt.SubMenu submenu = new(Extensions.DialogClean("ChroniaHelper_wikiSet"), false);

        foreach (var item in menuOrder)
        {
            submenu.Add(item);
        }
        menu.Add(submenu);
        
    }
    
    
}

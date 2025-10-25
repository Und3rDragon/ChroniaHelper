using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.ChroniaHelperIndicatorZone;
using Celeste.Mod.Helpers;
using ChroniaHelper.Cores;
using ChroniaHelper.Effects;
using ChroniaHelper.Entities;
using ChroniaHelper.Entities.MigratedNeonHelper;
using ChroniaHelper.Imports;
using ChroniaHelper.Modules;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using FMOD.Studio;
using MonoMod.ModInterop;
using YoctoHelper.Hooks;
using FASF2025Helper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Components;
using ChroniaHelper.Utils.StopwatchSystem;
using ChroniaHelper.Settings;
using Microsoft.Xna.Framework.Graphics;

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

    public ChroniaHelperHandle ChroniaHelperHandle { get; private set; }

    public HookManager HookManager { get; private set; }

    public string ModDirectory
    {
        get => Path.Combine(Path.GetDirectoryName(FakeAssembly.GetFakeEntryAssembly().Location), $"Mods\\{ChroniaHelperModule.Name}");
    }

    public static bool teraMode = false;

    public ChroniaHelperModule()
    {
        Instance = this;
    }

    public static bool FrostHelperLoaded;
    public static bool CommunalHelperLoaded { get; private set; }
    public static bool VivHelperLoaded;
    public static bool MaddieLoaded;
    public static Assembly CommunalHelperAssembly { get; private set; }
    public static Assembly VivHelperAssembly { get; private set; }

    public override void Load()
    {
        Log.Info("Welcome to use ChroniaHelper!");
        ChroniaHelperModule.Instance = this;
        this.ChroniaHelperHandle = new ChroniaHelperHandle();
        this.HookManager = new HookManager();
        ChroniaHelperModule.Instance.ChroniaHelperHandle.LoadHandle();
        
        LoadingManager.Load();
        
        Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;

        // migrated from VivHelper
        On.Celeste.GameLoader.Begin += LateInitialize;

        // API Imports
        typeof(FrostHelperImports).ModInterop();
        typeof(CameraDynamicsImports).ModInterop();

        // FrostHelper load judgement
        EverestModuleMetadata frostHelperMetadata = new()
        {
            Name = "FrostHelper",
            Version = new Version(1, 70, 2)
        };
        FrostHelperLoaded = Everest.Loader.DependencyLoaded(frostHelperMetadata);

        // Communal Helper load judgement
        EverestModuleMetadata communalHelperMetadata = new()
        {
            Name = "CommunalHelper",
            Version = new Version("1.23.0"),
        };
        CommunalHelperLoaded = Everest.Loader.DependencyLoaded(communalHelperMetadata);

        if (Everest.Loader.TryGetDependency(communalHelperMetadata, out var communalModule))
        {
            CommunalHelperAssembly = communalModule.GetType().Assembly;
        }

        // Viv Helper load judgement
        EverestModuleMetadata vivHelperMetadata = new()
        {
            Name = "VivHelper",
            Version = new Version("1.14.10"),
        };
        VivHelperLoaded = Everest.Loader.DependencyLoaded(vivHelperMetadata);

        if (Everest.Loader.TryGetDependency(vivHelperMetadata, out var vivModule))
        {
            VivHelperAssembly = vivModule.GetType().Assembly;
        }

        // Max Helping Hand judgement
        EverestModuleMetadata maddieMetadata = new()
        {
            Name = "MaxHelpingHand",
            Version = new Version("1.38.0"),
        };
        MaddieLoaded = Everest.Loader.DependencyLoaded(maddieMetadata);

        PolygonCollider.Load();
    }

    public override void Unload()
    {
        ChroniaHelperModule.Instance.ChroniaHelperHandle.UnloadHandle();
        
        LoadingManager.Unload();
        
        Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        
        // migrated from VivHelper
        On.Celeste.GameLoader.Begin -= LateInitialize;
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
                    if (entity.Direction == directions && player.CollideCheck(entity, player.Position + Vector2.UnitX * dir * 5f))
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

        context.Add<MaxHelpingHandMapDataProcessor>();
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

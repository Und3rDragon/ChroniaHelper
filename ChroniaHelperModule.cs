using System;
using System.IO;
using Celeste.Mod.ChroniaHelperIndicatorZone;
using Celeste.Mod.Helpers;
using ChroniaHelper.Cores;
using ChroniaHelper.Entities;
using ChroniaHelper.Modules;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using YoctoHelper.Hooks;
using Celeste;
using Celeste.Mod;
using ChroniaHelper.Effects;
using ChroniaHelper.Imports;
using MonoMod.ModInterop;
using FMOD.Studio;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

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

    public static SpriteBank spriteBank;

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

    public override void Load()
    {
        Log.Info("Welcome to use ChroniaHelper!");
        ChroniaHelperModule.Instance = this;
        this.ChroniaHelperHandle = new ChroniaHelperHandle();
        this.HookManager = new HookManager();
        ChroniaHelperModule.Instance.ChroniaHelperHandle.LoadHandle();

        ActiveTera.OnLoad();
        TeraBlock.OnLoad();
        TeraZipMover.OnLoad();
        TeraFallingBlock.OnLoad();
        TeraBooster.OnLoad();
        TeraDashBlock.OnLoad();
        TeraDreamBlock.OnLoad();
        TeraMoveBlock.OnLoad();
        TeraBarrier.OnLoad();
        TeraSwapBlock.OnLoad();
        TeraCrushBlock.OnLoad();
        TeraBounceBlock.OnLoad();
        TeraCrystal.OnLoad();
        BubblePushField.Load();
        CustomTimer.Load();
        Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        CustomBooster.Load();
        OmniZipWater.Load();
        EntityChangingInterfaces.Load();
        PatientBooster.Load();
        BoosterZipper.Load();
        SpriteEntity.Load();
        PlatformLineController.Load();

        MapProcessor.Load();
        ChroniaFlag.Onload();

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
    }

    public override void Unload()
    {
        ChroniaHelperModule.Instance.ChroniaHelperHandle.UnloadHandle();

        ActiveTera.OnUnload();
        TeraBlock.OnUnload();
        TeraZipMover.OnUnload();
        TeraFallingBlock.OnUnload();
        TeraBooster.OnUnload();
        TeraDashBlock.OnUnload();
        TeraDreamBlock.OnUnload();
        TeraMoveBlock.OnUnload();
        TeraBarrier.OnUnload();
        TeraSwapBlock.OnUnload();
        TeraCrushBlock.OnUnload();
        TeraBounceBlock.OnUnload();
        TeraCrystal.OnUnload();
        CustomTimer.UnLoad();
        Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        CustomBooster.Unload();
        OmniZipWater.Unload();
        EntityChangingInterfaces.Unload();
        PatientBooster.Unload();
        BoosterZipper.Unload();
        SpriteEntity.Unload();
        PlatformLineController.Unload();

        MapProcessor.Unload();
        ChroniaFlag.Unload();
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

    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);

        //spriteBank = new SpriteBank(GFX.Game, "Graphics/ChroniaHelper/Sprites.xml");

    }

    // Create Custom ChroniaHelper Menu
    protected override void CreateModMenuSectionHeader(TextMenu menu, bool inGame, EventInstance snapshot)
    {
        base.CreateModMenuSectionHeader(menu, inGame, snapshot);

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

        foreach(var item in menuOrder)
        {
            submenu.Add(item);
        }
        menu.Add(submenu);
    }
}

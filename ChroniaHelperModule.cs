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
        MapProcessor.Load();
        EntityChangingInterfaces.Load();
        PatientBooster.Load();
        BoosterZipper.Load();
        Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
        SpriteEntity.Load();

        // API Imports
        typeof(FrostHelperImports).ModInterop();
        typeof(CameraDynamicsImports).ModInterop();
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
        MapProcessor.Unload();
        EntityChangingInterfaces.Unload();
        PatientBooster.Unload();
        BoosterZipper.Unload();
        Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
        SpriteEntity.Unload();
    }

    private static void LevelLoader_OnLoadingThread(Level level)
    {
        level.Add(new PlayerIndicatorZone.IconRenderer());
    }

    private Backdrop Level_OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
    {
        if (child.Name.Equals("ChroniaHelper/WindRainFG", StringComparison.OrdinalIgnoreCase))
        {
            if (child.HasAttr("colors") && !string.IsNullOrWhiteSpace(child.Attr("colors")))
                return new WindRainFG(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), child.Attr("colors"), child.AttrFloat("windStrength"), child.AttrInt("Amount", 240), child.AttrFloat("alpha", 1f));
            else
                return new WindRainFG(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), child.Attr("Colors"), child.AttrFloat("windStrength"), child.AttrInt("Amount", 240), child.AttrFloat("alpha", 1f));
        }
        else if (child.Name.Equals("ChroniaHelper/CustomRain", StringComparison.OrdinalIgnoreCase))
        {
            if (child.HasAttr("colors") && !string.IsNullOrWhiteSpace(child.Attr("colors")))
                return new CustomRain(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), child.AttrFloat("angle", 270f), child.AttrFloat("angleDiff", 3f), child.AttrFloat("speedMult", 1f), child.AttrInt("Amount", 240), child.Attr("colors", "161933"), child.AttrFloat("alpha"));
            else
                return new CustomRain(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), child.AttrFloat("angle", 270f), child.AttrFloat("angleDiff", 3f), child.AttrFloat("speedMult", 1f), child.AttrInt("Amount", 240), child.Attr("Colors", "161933"), child.AttrFloat("alpha"));
        }
        return null;
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
}

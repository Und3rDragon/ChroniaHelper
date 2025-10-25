using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Cores.LiteTeraHelper;
using ChroniaHelper.Utils;
using FASF2025Helper.Utils;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/activeTera")]
public class ActiveTera : Entity
{
    private bool active ,once;
    private TeraType tera;
    private EntityID ID;

    public ActiveTera(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        ID = id;
        active = data.Bool("active");
        tera = data.Enum("tera", TeraType.Normal);
        once = data.Bool("onlyOnce");
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        var session = ChroniaHelperModule.Session;
        var level = SceneAs<Level>();

        ChroniaHelperModule.teraMode = active;

        if (session.ActiveTera != active)
        {
            var player = level.Tracker.GetEntity<Player>();
            session.ActiveTera = active;
            if (!active)
            {
                player.RemoveTera();
            }
            else
            {
                if (session.StartTera == TeraType.Any)
                {
                    session.StartTera = tera;
                }
                player.InitTera();
            }
        }
        if (once)
        {
            RemoveSelf();
            level.Session.DoNotLoad.Add(ID);
        }
        
    }
    [LoadHook]
    public static void OnLoad()
    {
        On.Celeste.Player.ctor += CreatePlayerTera;
        On.Celeste.Session.UpdateLevelStartDashes += UpdateLevelTera;
    }
    [UnloadHook]
    public static void OnUnload()
    {
        On.Celeste.Player.ctor -= CreatePlayerTera;
        On.Celeste.Session.UpdateLevelStartDashes -= UpdateLevelTera;
    }
    private static void CreatePlayerTera(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
    {
        if (orig != null)
        {
            orig(self, position, spriteMode);
        }
        if (Md.Session.ActiveTera)
        {
            self.InitTera();
        }
    }
    private static void UpdateLevelTera(On.Celeste.Session.orig_UpdateLevelStartDashes orig, Session self)
    {
        orig(self);
        if (Engine.Scene is not Level level)
            return;
        if (Md.Session.ActiveTera)
        {
            var player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                Md.Session.StartTera = player.GetTera(true);
            }
        }
    }
}
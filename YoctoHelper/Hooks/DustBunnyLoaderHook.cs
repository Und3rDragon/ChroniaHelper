using System.Linq;
using MonoMod.Utils;
using YoctoHelper.Cores;
using ChroniaHelper.Utils;

namespace YoctoHelper.Hooks;

[HookRegister(id: HookId.DustBunnyLoader, useData: false)]
public class DustBunnyLoaderHook
{

    private bool hasDustBunny { get; set; }

    [Load]
    private void Load()
    {
        On.Celeste.LevelLoader.LoadingThread += this.DustBunnyLoader;
        On.Celeste.Level.Update += this.AddDustStaticSpinner;
    }

    [Unload]
    private void Unload()
    {
        On.Celeste.LevelLoader.LoadingThread -= this.DustBunnyLoader;
        On.Celeste.Level.Update -= this.AddDustStaticSpinner;
    }

    private void DustBunnyLoader(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        orig(self);
        DynData<LevelLoader> dynData = new DynData<LevelLoader>(self);
        Session session = dynData.Get<Session>("session");
        if ((session?.MapData?.Levels.Any<LevelData>((level) => (level.Entities?.Any((entity) => (entity.Name == "ChroniaHelper/CustomDustBunny")) ?? false))) ?? false)
        {
            self.Level.Add(new DustBunnyEdges());
            this.hasDustBunny = true;
        }
    }

    private void AddDustStaticSpinner(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        if ((this.hasDustBunny) && (ObjectUtils.IsNull(self.Tracker.GetComponent<DustEdge>())))
        {
            self.Add(new DustStaticSpinner(new Vector2(self.Bounds.X - 320, self.Bounds.Y - 180), false, false));
        }
    }

}

using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.References;
using ChroniaHelper.Utils;
using System.Collections;

namespace ChroniaHelper.Entities;

/// <summary>
/// 自定义出生点。放置在原版 Player 出生点的位置，行为与原版出生点一致：
/// 参与所在房间的出生点列表，进房、死亡重生与切屏后的最近出生点判定都会算上它。
/// </summary>
[CustomEntity("ChroniaHelper/CustomSpawnPoint")]
public class CustomSpawnPoint : Entity
{
    public CustomSpawnPoint(EntityData data, Vc2 offset)
        : base(data.Position + offset)
    {
        RegisterSpawns((Engine.Scene as Level ?? (Engine.Scene as LevelLoader)?.Level)?.Session?.LevelData);
        GlobalCoroutines.Register($"ChroniaHelper_SpawnPoint_{data.ID}", Processor(data));
        
        if(zoneE = data.Bool("autoChangeRespawn", true))
        {
            zoneW = data.Float("changeRespawnZoneWidth", 32);
            zoneH = data.Float("changeRespawnZoneHeight", 48);
            Collider c = new Hitbox(zoneW, zoneH, X - zoneW / 2, Y - zoneH);
            changeRespawn = new((p) => { }, c);
            playerCollide = new(null, false);
        }

        camCatchup = data.Float("cameraCatchupSpeed", 1f);
    }
    //private PlayerCollider changeRespawn;
    private float zoneW, zoneH;
    private bool zoneE;
    PlayerCollider changeRespawn;
    DataPack<bool> playerCollide = new(null, false);
    private float camCatchup;

    public override void Update()
    {
        base.Update();

        Player player = SceneAs<Level>().Tracker.GetEntity<Player>();

        playerCollide.SetValue(player?.Collider.Collide(changeRespawn.Collider) ?? false);

        if(playerCollide.Value && !playerCollide._Value)
        {
            ChangeRespawn(player);
        }
    }

    private void ChangeRespawn(Player player)
    {
        Vc2 spawn = SceneAs<Level>().GetSpawnPoint(Position);

        Session session = (base.Scene as Level).Session;
        if (SolidCheck(spawn) && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != spawn))
        {
            session.HitCheckpoint = true;
            session.RespawnPoint = spawn;
            session.UpdateLevelStartDashes();
        }
    }

    private bool SolidCheck(Vc2 spawn)
    {
        Vector2 point = spawn + Vector2.UnitY * -4f;
        if (base.Scene.CollideCheck<Solid>(point))
        {
            return base.Scene.CollideCheck<FloatySpaceBlock>(point);
        }

        return true;
    }

    private IEnumerator Processor(EntityData data)
    {
        while(Engine.Scene is not Level)
        {
            yield return null;
        }

        Level level = Engine.Scene as Level;

        while (level.Transitioning)
        {
            yield return null;
        }

        if (level.Session.RespawnPoint != Position)
        {
            yield break;
        }

        level.CameraOffset = data.Vector2("cameraOffsetX", "cameraOffsetY", Vc2.Zero);

        if (data.Attr("setFlag").HasValidContent())
        {
            var s = data.StringArray("setFlag");

            foreach(var i in s)
            {
                string name = i;
                if (i.StartsWith('!'))
                {
                    name = i.TrimStart('!');
                    level.Session.SetFlag(name, false);
                }
                else
                {
                    level.Session.SetFlag(name, true);
                }
            }
        }

        Player player = level.Tracker.GetEntity<Player>();

        while(player.Scene == null)
        {
            player = level.Tracker.GetEntity<Player>();
            yield return null;
        }

        int facing = data.Int("playerFacing", 0);
        if (facing != 0)
        {
            player.Facing = facing > 0 ? Facings.Right : Facings.Left;
        }

        float catchup = data.Float("cameraCatchupSpeed", -1f);
        if (Md.MaddieLoaded && catchup > 0)
        {
            RefMaxHelpingHand.GetSession().CameraCatchupSpeed = catchup;
        }
    }

    /// <summary>
    /// 把某个房间数据里的自定义出生点补进出生点列表，使 Debug Map 等直接读 LevelData 的场景也能识别它们。
    /// </summary>
    private static void RegisterSpawns(LevelData levelData)
    {
        if (levelData == null)
        {
            return;
        }

        Vc2 offset = new(levelData.Bounds.Left, levelData.Bounds.Top);
        bool found = false;

        foreach (EntityData entity in levelData.Entities)
        {
            if (entity.Name != "ChroniaHelper/CustomSpawnPoint")
            {
                continue;
            }

            found = true;
            Vc2 position = entity.Position + offset;

            if (!levelData.Spawns.Contains(position))
            {
                levelData.Spawns.Add(position);
            }

            if (entity.Bool("isDefaultSpawn", false))
            {
                levelData.DefaultSpawn = position;
            }
        }

        if (found)
        {
            levelData.Dummy = false;
        }
    }

    [ExA.LoadHook]
    private static void Load()
    {
        On.Celeste.MapData.Reload += MapData_Reload;
    }

    [ExA.UnloadHook]
    private static void Unload()
    {
        On.Celeste.MapData.Reload -= MapData_Reload;
    }

    private static void MapData_Reload(On.Celeste.MapData.orig_Reload orig, MapData self)
    {
        orig(self);

        if (self?.Levels == null)
        {
            return;
        }

        foreach (LevelData level in self.Levels)
        {
            RegisterSpawns(level);
        }
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);

        Draw.Rect(-2f, -2f, 4f, 4f, Color.Red);
    }
}

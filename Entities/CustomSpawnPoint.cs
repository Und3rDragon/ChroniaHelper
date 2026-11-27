using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.References;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/CustomSpawnPoint")]
[Tracked]
public class CustomSpawnPoint : BaseEntity
{
    public CustomSpawnPoint(EntityData data, Vc2 offset)
        : base(data, offset)
    {
        playerFacing = data.Int("playerFacing", 0);

        setupOffset = data.Attr("cameraOffsetX").HasValidContent() && data.Attr("cameraOffsetY").HasValidContent() ?
            data.Vector2("cameraOffsetX", "cameraOffsetY", Vc2.Zero) : null;
        setFlag = data.Attr("setFlag");
        autoRespawn = data.Bool("autoChangeRespawn", true);
        catchup = data.Float("cameraCatchupSpeed", -1f);
        clear = data.Bool("clearFlagsBeforeSet", false);

        if (nodes.Length >= 3)
        {
            Collider = new Hitbox((nodes[2] - nodes[1]).X.GetAbs(), (nodes[2] - nodes[1]).Y.GetAbs(),
                float.Min(nodes[1].X, nodes[2].X) - Position.X, float.Min(nodes[1].Y, nodes[2].Y) - Position.Y);
        }
    }
    private int playerFacing;

    private Vc2? setupOffset;
    private string setFlag;
    private bool autoRespawn;
    private float catchup;
    private bool clear;

    private DataPack<bool> colliding = new(null, false);

    public override void Update()
    {
        base.Update();

        if (Collider == null)
        {
            return;
        }

        Player player = SceneAs<Level>().Tracker.GetEntity<Player>();

        colliding.SetValue(player?.Scene != null && player.CollideCheck(this));

        if (colliding.Value && !colliding._Value)
        {
            ApplyEffects();

            if (autoRespawn)
            {
                SetRespawnPoint(level.Session);
            }
        }
    }

    public void ApplyPlayerFacing(Player player)
    {
        if (playerFacing == 0)
        {
            return;
        }

        player.Facing = playerFacing > 0 ? Facings.Right : Facings.Left;
    }

    private void ApplyEffects()
    {
        Level level = SceneAs<Level>();

        if (setupOffset != null)
        {
            SetCameraOffset(level);
        }

        if (clear)
        {
            level.Session.Flags.Clear();
        }

        if (setFlag.HasValidContent())
        {
            string[] flags = setFlag.Split(',', StringSplitOptions.TrimEntries);

            foreach (string flag in flags)
            {
                if (flag.StartsWith('!'))
                {
                    level.Session.SetFlag(flag.TrimStart('!'), false);
                }
                else
                {
                    level.Session.SetFlag(flag, true);
                }
            }
        }

        if (Md.MaddieLoaded && catchup > 0)
        {
            RefMaxHelpingHand.SetCameraCatchupSpeed(catchup);
        }
    }

    private void SetCameraOffset(Level level)
    {
        level.CameraOffset = new Vector2(Level.CameraOffsetXInterval, Level.CameraOffsetYInterval) * (Vc2)setupOffset;
    }

    private void SetRespawnPoint(Session session)
    {
        if (session.RespawnPoint.HasValue && session.RespawnPoint.Value == Position)
        {
            return;
        }

        session.HitCheckpoint = true;
        session.RespawnPoint = Position;
        session.UpdateLevelStartDashes();
    }

    private static readonly HashSet<LevelData> registeredLevels = new();

    /// <summary>
    /// 把整张地图里所有房间的自定义出生点一次补全。
    /// </summary>
    private static void RegisterMapSpawns(MapData mapData)
    {
        if (mapData?.Levels == null)
        {
            return;
        }

        foreach (LevelData levelData in mapData.Levels)
        {
            RegisterLevelSpawns(levelData);
        }
    }

    /// <summary>
    /// 把某个房间数据里的自定义出生点补进出生点列表。同一个位置只补一次。
    /// </summary>
    private static void RegisterLevelSpawns(LevelData levelData)
    {
        if (levelData?.Entities == null || !registeredLevels.Add(levelData))
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

            if (!levelData.DefaultSpawn.HasValue && entity.Bool("isDefaultSpawn", false))
            {
                levelData.DefaultSpawn = position;
            }
        }

        if (found)
        {
            levelData.Dummy = false;
        }
    }

    /// <summary>
    /// 玩家确实在这个出生点上出生时，应用出生点朝向与效果区域设置，并立即把镜头对齐到目标位置。
    /// </summary>
    private static void AfterSpawning(Level level)
    {
        Session session = level.Session;

        if (session?.RespawnPoint == null || level.Transitioning)
        {
            return;
        }

        Player player = level.Tracker.GetEntity<Player>();

        if (player == null || player.Position != session.RespawnPoint.Value)
        {
            return;
        }

        bool applied = false;

        foreach (CustomSpawnPoint spawn in level.Tracker.GetEntities<CustomSpawnPoint>())
        {
            if (spawn.Position != session.RespawnPoint.Value)
            {
                continue;
            }

            spawn.ApplyPlayerFacing(player);

            spawn.ApplyEffects();

            spawn.colliding.SetValue(true);

            applied = true;
        }

        if (applied)
        {
            level.Camera.Position = level.GetFullCameraTargetAt(player, player.Position);
        }
    }

    [ExA.LoadHook]
    private static void Load()
    {
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
        On.Celeste.MapData.Reload += MapData_Reload;
    }

    [ExA.UnloadHook]
    private static void Unload()
    {
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
        On.Celeste.MapData.Reload -= MapData_Reload;
    }

    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        RegisterMapSpawns(self.Session?.MapData);

        orig(self, playerIntro, isFromLoader);

        AfterSpawning(self);
    }

    private static void MapData_Reload(On.Celeste.MapData.orig_Reload orig, MapData self)
    {
        orig(self);

        RegisterMapSpawns(self);
    }
}

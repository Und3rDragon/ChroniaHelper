using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.References;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

/// <summary>
/// 自定义出生点。放在原版 Player 出生点的位置时与原版出生点等价：参与所在房间的出生点列表，
/// 进房、死亡重生、切屏与调试地图的出生点判定都会算上它。勾选 isDefaultSpawn 时成为该房间的
/// 默认出生点，playerFacing 决定玩家在这个点出生时的朝向。
/// 拉出两个节点会构成一块效果区域，玩家进入时触发一次相机偏移、相机跟随速度、flag 与重生点切换。
/// </summary>
[CustomEntity("ChroniaHelper/CustomSpawnPoint")]
[Tracked]
public class CustomSpawnPoint : BaseEntity
{
    public CustomSpawnPoint(EntityData data, Vc2 offset)
        : base(data, offset)
    {
        playerFacing = data.Int("playerFacing", 0);

        camOffset = data.Vector2("cameraOffsetX", "cameraOffsetY", Vc2.Zero);
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

    private Vc2 camOffset;
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
        }
    }

    /// <summary>
    /// 让玩家面朝设置好的方向。0 表示不改变朝向。
    /// </summary>
    public void ApplyPlayerFacing(Player player)
    {
        if (playerFacing == 0)
        {
            return;
        }

        player.Facing = playerFacing > 0 ? Facings.Right : Facings.Left;
    }

    /// <summary>
    /// 玩家进入效果区域时应用一次全部设置。
    /// </summary>
    private void ApplyEffects()
    {
        Level level = SceneAs<Level>();

        SetCameraOffset(level);

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
            RefMaxHelpingHand.GetSession().CameraCatchupSpeed = catchup;
        }

        if (autoRespawn)
        {
            SetRespawnPoint(level.Session);
        }
    }

    /// <summary>
    /// 相机偏移按原版房间的 cameraOffsetX/Y 换算，数值 1 等于 48x32 像素。
    /// </summary>
    private void SetCameraOffset(Level level)
    {
        level.CameraOffset = new Vector2(Level.CameraOffsetXInterval, Level.CameraOffsetYInterval) * camOffset;
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

    /// <summary>
    /// 把某个房间数据里的自定义出生点补进出生点列表。同一个位置只补一次。
    /// </summary>
    private static void RegisterLevelSpawns(LevelData levelData)
    {
        if (levelData?.Entities == null)
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
    /// 玩家确实在这个出生点上出生时，按出生点的设置应用朝向。
    /// </summary>
    private static void ApplyPlayerFacing(Level level)
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

        foreach (CustomSpawnPoint spawn in level.Tracker.GetEntities<CustomSpawnPoint>())
        {
            if (spawn.Position != session.RespawnPoint.Value)
            {
                continue;
            }

            spawn.ApplyPlayerFacing(player);
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
        RegisterLevelSpawns(self.Session?.LevelData);

        orig(self, playerIntro, isFromLoader);

        ApplyPlayerFacing(self);
    }

    private static void MapData_Reload(On.Celeste.MapData.orig_Reload orig, MapData self)
    {
        orig(self);

        if (self?.Levels == null)
        {
            return;
        }

        foreach (LevelData levelData in self.Levels)
        {
            RegisterLevelSpawns(levelData);
        }
    }
}

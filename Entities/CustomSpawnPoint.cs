using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
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
        Register(data);
        GlobalCoroutines.Register($"ChroniaHelper_SpawnPoint_{data.ID}", Processor(data));
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

        player.Facing = data.Bool("playerFacingRight", true) ? Facings.Right : Facings.Left;
    }

    private void Register(EntityData data)
    {
        Level level = Engine.Scene as Level ?? (Engine.Scene as LevelLoader)?.Level;
        LevelData levelData = level?.Session?.LevelData;

        if (levelData == null)
        {
            return;
        }

        if (!levelData.Spawns.Contains(Position))
        {
            levelData.Spawns.Add(Position);
        }

        if (data.Bool("isDefaultSpawn", false))
        {
            levelData.DefaultSpawn = Position;
        }
    }
}

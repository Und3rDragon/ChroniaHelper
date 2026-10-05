using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/LevelDataToSessionController")]
public class LevelDataToSessionController : BaseEntity
{
    public LevelDataToSessionController()
    {
        Tag = Tags.FrozenUpdate | Tags.PauseUpdate | Tags.TransitionUpdate;
    }

    public override void Update()
    {
        base.Update();

        Level level = SceneAs<Level>();
        Player player = level.Tracker.GetEntity<Player>();

        "ChroniaHelper_LevelData_CameraPositionX".SetSlider(level.Camera?.Position.X ?? 0);
        "ChroniaHelper_LevelData_CameraPositionY".SetSlider(level.Camera?.Position.Y ?? 0);
        "ChroniaHelper_LevelData_PlayerPositionX".SetSlider(player?.Position.X ?? 0);
        "ChroniaHelper_LevelData_PlayerPositionY".SetSlider(player?.Position.Y ?? 0);
        "ChroniaHelper_LevelData_LevelTop".SetCounter(level.Bounds.Top);
        "ChroniaHelper_LevelData_LevelLeft".SetCounter(level.Bounds.Left);
        "ChroniaHelper_LevelData_LevelBottom".SetCounter(level.Bounds.Bottom);
        "ChroniaHelper_LevelData_LevelRight".SetCounter(level.Bounds.Right);
    }
}

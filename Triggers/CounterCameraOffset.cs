using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using IL.Celeste.Mod.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Triggers;

[CustomEntity("ChroniaHelper/CounterCameraOffset")]
public class CounterCameraOffset : BaseTrigger
{
    public CounterCameraOffset(EntityData data, Vc2 offset) : base(data, offset)
    {
        counterName = data.Attr("counter", "cameraIndex");
        param1 = data.Counter("param1");
        param2 = data.Counter("param2");
        mode = (Mode)data.Int("requirement", 0);
        offsetXFrom = data.Slider("offsetXFrom");
        offsetXTo = data.Slider("offsetXTo");
        offsetYFrom = data.Slider("offsetYFrom");
        offsetYTo = data.Slider("offsetYTo");
        positionMode = (PositionModes)data.Int("positionMode", 0);
        operateMode = (CameraMode)data.Int("cameraMode", 0);

        leaveReset = data.Bool("leaveReset", false);
        onlyOnce = data.Bool("onlyOnce", false);
    }
    private string counterName;
    private SelectiveCounter param1, param2;
    private enum Mode { Disabled, MatchParam1, NotMatchParam1, BetweenParams, NotBetweenParams }
    private Mode mode;
    private SelectiveSlider offsetXFrom, offsetXTo, offsetYFrom, offsetYTo;
    private PositionModes positionMode;
    private enum CameraMode { Both, X, Y }
    private CameraMode operateMode;

    private Vc2 currentCamera, setCamera;
    protected override void OnEnterExecute(Player player)
    {
        currentCamera = MaP.level.CameraOffset;
        offsetXFrom.Fallback = currentCamera.X;
        offsetYFrom.Fallback = currentCamera.Y;

        setCamera = new Vc2(offsetXTo.Value, offsetYTo.Value);

        if (positionMode == PositionModes.NoEffect && ActivateArg())
        {
            if(operateMode != CameraMode.Y)
            {
                MaP.level.CameraOffset.X = setCamera.X;
            }
            if (operateMode != CameraMode.X)
            {
                MaP.level.CameraOffset.Y = setCamera.Y;
            }
        }
    }

    protected override void OnStayExecute(Player player)
    {
        if(positionMode != PositionModes.NoEffect && ActivateArg())
        {
            if (operateMode != CameraMode.Y)
            {
                MaP.level.CameraOffset.X = GetPositionLerp(player, positionMode)
                    .LerpValue(0f, 1f, offsetXFrom.Value, setCamera.X);
            }
            if (operateMode != CameraMode.X)
            {
                MaP.level.CameraOffset.Y = GetPositionLerp(player, positionMode)
                    .LerpValue(0f, 1f, offsetYFrom.Value, setCamera.Y);
            }
        }
    }

    private bool ActivateArg()
    {
        if (mode == Mode.Disabled)
        {
            return true;
        }
        else if (mode == Mode.MatchParam1 || mode == Mode.NotMatchParam1)
        {
            return (mode == Mode.MatchParam1 && param1.Value == counterName.GetCounter()) ||
                (mode == Mode.NotMatchParam1 && param1.Value != counterName.GetCounter());
        }
        else if (mode == Mode.BetweenParams || mode == Mode.NotBetweenParams)
        {
            return (mode == Mode.BetweenParams && counterName.GetCounter().IsBetween(param1.Value, param2.Value)) ||
                (mode == Mode.NotBetweenParams && !counterName.GetCounter().IsBetween(param1.Value, param2.Value));
        }
        else { return true; }
    }

    protected override void LeaveReset(Player player)
    {
        MaP.level.CameraOffset = currentCamera;
    }
}

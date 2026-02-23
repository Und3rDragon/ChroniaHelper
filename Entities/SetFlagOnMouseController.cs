using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/SetFlagOnMouseController")]
public class SetFlagOnMouseController : BaseEntity
{
    public SetFlagOnMouseController(EntityData data, Vc2 offset) : base(data, offset)
    {
        int mode = data.Int("mouseMode", 0);

        mouseConfig = new()
        {
            leftClick = mode == 0,
            rightClick = mode == 1,
            middleClick = mode == 2,
            leftHold = mode == 3,
            rightHold = mode == 4,
            middleHold = mode == 5,
            leftEmpty = mode == 3,
            rightEmpty = mode == 4,
            middleEmpty = mode == 5,
        };

        flags = data.StringArray("flags");
        flagMode = (FlagMode)data.Int("flagMode", 0);
    }
    private string[] flags;
    private enum FlagMode { On, Off, Switch }
    private FlagMode flagMode;

    protected override void OnMouseLeftClick()
    {
        OperateFlags();
    }
    protected override void OnMouseRightClick()
    {
        OperateFlags();
    }
    protected override void OnMouseMiddleClick()
    {
        OperateFlags();
    }
    protected override void OnMouseLeftHold()
    {
        OperateFlags();
    }
    protected override void OnMouseRightHold()
    {
        OperateFlags();
    }
    protected override void OnMouseMiddleHold()
    {
        OperateFlags();
    }
    protected override void OnMouseLeftEmpty()
    {
        DisableFlags();
    }
    protected override void OnMouseRightEmpty()
    {
        DisableFlags();
    }
    protected override void OnMouseMiddleEmpty()
    {
        DisableFlags();
    }

    public void OperateFlags()
    {
        if (flagMode == FlagMode.On)
        {
            foreach (var flag in flags)
            {
                flag.SetFlag(true);
            }
        }
        else if (flagMode == FlagMode.Off)
        {
            foreach (var flag in flags)
            {
                flag.SetFlag(false);
            }
        }
        else
        {
            foreach (var flag in flags)
            {
                flag.SetFlag(!flag.GetFlag());
            }
        }
    }

    public void DisableFlags()
    {
        if (flagMode == FlagMode.On)
        {
            foreach (var flag in flags)
            {
                flag.SetFlag(false);
            }
        }
        else if (flagMode == FlagMode.Off)
        {
            foreach (var flag in flags)
            {
                flag.SetFlag(true);
            }
        }
        else
        {
            foreach (var flag in flags)
            {
                flag.SetFlag(false);
            }
        }
    }
}

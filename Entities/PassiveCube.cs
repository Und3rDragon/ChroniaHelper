using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
public class PassiveCube : BaseEntity
{
    public PassiveCube(EntityData d, Vc2 o) : base(d,o)
    {
        rotateUpFlag = new FlagListener(d.Attr("rotateUpFlag"));
        rotateDownFlag = new FlagListener(d.Attr("rotateDownFlag"));
        rotateInFlag = new FlagListener(d.Attr("rotateInFlag"));
        rotateOutFlag = new FlagListener(d.Attr("rotateOutFlag"));
        clockwiseFlag = new FlagListener(d.Attr("clockwiseFlag"));
        counterclockwiseFlag = new FlagListener(d.Attr("counterclockwiseFlag"));
        Add(rotateUpFlag, rotateDownFlag, rotateInFlag, rotateOutFlag, clockwiseFlag, counterclockwiseFlag);

        coefficient = d.Float("thicknessCoefficient", 8f);
        listener = new Passive3D(Position + new Vc2(Width, Height) / 2f, Width, Height, 0.5f);
        listener.Recalculate(coefficient);
        Add(listener);
    }
    private FlagListener rotateUpFlag, rotateDownFlag, rotateInFlag, rotateOutFlag, clockwiseFlag, counterclockwiseFlag;
    private float coefficient;
    private Passive3D listener;

    private void RefreshMembers()
    {

    }

    public override void Update()
    {
        base.Update();

        rotateUpFlag.onEnable = () =>
        {
            RefreshMembers();
            listener.RotateUp();
        };

        rotateDownFlag.onEnable = () =>
        {
            RefreshMembers();
            listener.RotateDown();
        };

        rotateInFlag.onEnable = () =>
        {
            RefreshMembers();
            listener.RotateIn();
        };

        rotateOutFlag.onEnable = () =>
        {
            RefreshMembers();
            listener.RotateOut();
        };

        clockwiseFlag.onEnable = () =>
        {
            RefreshMembers();
            listener.RotateClockwise();
        };

        counterclockwiseFlag.onEnable = () =>
        {
            RefreshMembers();
            listener.RotateCounterclockwise();
        };
    }
}

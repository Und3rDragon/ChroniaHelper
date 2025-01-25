using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SmoothToOffsetCamera")]
public class SmoothToOffsetCamera : Trigger
{
    private Vector2 offsetFrom;

    private Vector2 offsetTo;

    private PositionModes positionMode;

    private bool onlyOnce;

    private bool xOnly;

    private bool yOnly;

    private Level level;

    private Vector2 currentOffset;

    private float scaleX, scaleY;

    private string units, modes, flag;

    private enum flagControl { disabled, required, inverted};
    private flagControl flagcontrol;


    public SmoothToOffsetCamera(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        
        positionMode = data.Enum("positionMode", PositionModes.NoEffect);
        onlyOnce = data.Bool("onlyOnce");
        xOnly = data.Bool("xOnly");
        yOnly = data.Bool("yOnly");
        this.units = data.Attr("unit");
        if(this.units == "offset")
        {
            this.scaleX = 48f; this.scaleY = 32f;
        }
        else if(this.units == "offsetSquared")
        {
            this.scaleX = 48f; this.scaleY = 48f;
        }
        else if (this.units == "tiles")
        {
            this.scaleX = 8f; this.scaleY = 8f;
        }
        else if(this.units == "pixels")
        {
            this.scaleX = 1f; this.scaleY = 1f;
        }
        else { this.scaleX = 48f; this.scaleY = 32f; }

        offsetFrom = new Vector2(currentOffset.X * this.scaleX, currentOffset.Y * this.scaleY);
        offsetTo = new Vector2(data.Float("offsetXTo") * this.scaleX, data.Float("offsetYTo") * this.scaleY);

        this.modes = data.Attr("modes");

        this.flag = data.Attr("flag");

        if(data.Attr("flagControl") == "disabled") { flagcontrol = flagControl.disabled; }
        else if(data.Attr("flagControl") == "flagRequired") { flagcontrol = flagControl.required; }
        else if(data.Attr("flagControl") == "flagInverted") { flagcontrol = flagControl.inverted; }
        else { flagcontrol = flagControl.disabled; }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        this.currentOffset = SceneAs<Level>().CameraOffset;
    }
    public override void OnStay(Player player)
    {
        base.OnStay(player);
        bool operate;
        if(flagcontrol == flagControl.disabled) { operate = true; }
        else if(flagcontrol == flagControl.required && level.Session.GetFlag(this.flag)) { operate = true; }
        else if (flagcontrol == flagControl.inverted && !level.Session.GetFlag(this.flag)) { operate = true; }
        else { operate = false; }

        if (operate)
        {
            if (this.modes == "normal" || this.modes == "xOnly")
            {
                SceneAs<Level>().CameraOffset.X = MathHelper.Lerp(currentOffset.X, offsetTo.X, GetPositionLerp(player, positionMode));
            }

            if (this.modes == "normal" || this.modes == "yOnly")
            {
                SceneAs<Level>().CameraOffset.Y = MathHelper.Lerp(currentOffset.Y, offsetTo.Y, GetPositionLerp(player, positionMode));
            }
        }
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        if (onlyOnce)
        {
            RemoveSelf();
        }
    }
}

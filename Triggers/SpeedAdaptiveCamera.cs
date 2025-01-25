using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SpeedAdaptiveCamera")]

public class SpeedAdaptiveCamera : Trigger
{
    public Vector2 CameraOffset, currentOffset;

    public enum camMode { both, x, y };
    public enum tgMode { toggle, inZone };
    public camMode cameraMode;
    public tgMode triggerMode;
    public Level level;
    public string flag;
    public enum flagMode { normal, required, inverted }
    public flagMode flagControl;

    public float mx, my, setX, setY, scaleX = 48f, scaleY = 32f;

    public bool onlyonce;
    public SpeedAdaptiveCamera(EntityData data, Vector2 offset) : base(data, offset)
    {
        string cam = data.Attr("cameraMode");
        if (cam == "Y Only") { this.cameraMode = camMode.y; }
        else if (cam == "X Only") { this.cameraMode = camMode.x; }
        else { this.cameraMode = camMode.both; }

        string tg = data.Attr("triggerMode");
        if (tg == "toggle") { this.triggerMode = tgMode.toggle; }
        else if (tg == "inZone") { this.triggerMode = tgMode.inZone; }



        string fM = data.Attr("flagControl");
        if (fM == "disabled")
        {
            this.flagControl = flagMode.normal;
        }
        else if (fM == "flagNeeded")
        {
            this.flagControl = flagMode.required;
        }
        else if (fM == "flagInverted")
        {
            this.flagControl = flagMode.inverted;
        }

        this.flag = data.Attr("flag");
        if (this.flag == null) { this.flag = "flag"; }

        this.onlyonce = data.Bool("onlyOnce");
        this.mx = data.Float("multiplierX");
        this.my = data.Float("multiplierY");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
    }

    public void setCam(Player player)
    {
        this.setX = player.Speed.X / 120f * this.mx;
        this.setY = player.Speed.Y / 105f * this.my;

        if (this.cameraMode == camMode.both)
        {
            CameraOffset = new Vector2(this.setX * this.scaleX, this.setY * this.scaleY);
        }
        else if (this.cameraMode == camMode.x)
        {
            CameraOffset = new Vector2(this.setX * this.scaleX, this.currentOffset.Y);
        }
        else if (this.cameraMode == camMode.y)
        {
            CameraOffset = new Vector2(this.currentOffset.X, this.setY * this.scaleY);
        }
    }

    public void setOffset()
    {
        if (this.flagControl == flagMode.required)
        {
            if (level.Session.GetFlag(this.flag))
            {
                SceneAs<Level>().CameraOffset = CameraOffset;
            }
        }
        else if (this.flagControl == flagMode.inverted)
        {
            if (!level.Session.GetFlag(this.flag))
            {
                SceneAs<Level>().CameraOffset = CameraOffset;
            }
        }
        else
        {
            SceneAs<Level>().CameraOffset = CameraOffset;
        }
    }

    public override void OnEnter(Player player)
    {
        this.currentOffset.X = level.CameraOffset.X;
        this.currentOffset.Y = level.CameraOffset.Y;

        setCam(player);
        setOffset();

    }

    public override void OnStay(Player player)
    {
        setCam(player);
        setOffset();
    }

    public override void OnLeave(Player player)
    {
        if (this.triggerMode == tgMode.inZone)
        {
            SceneAs<Level>().CameraOffset = currentOffset;
        }

        if (this.onlyonce)
        {
            RemoveSelf();
        }
    }

}

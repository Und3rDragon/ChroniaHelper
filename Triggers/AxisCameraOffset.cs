using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using static ChroniaHelper.Triggers.AxisCameraOffset;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/AxisCameraOffset")]

public class AxisCameraOffset : Trigger
{
    public Vector2 CameraOffset, currentOffset;

    public enum camMode {both, x, y};
    public enum tgMode {toggle, inZone};
    public camMode cameraMode;
    public tgMode triggerMode;
    public Level level;
    public string flag;
    public enum flagMode { normal, required, inverted}
    public flagMode flagControl;

    public float setX, setY, scaleX, scaleY;

    public bool onlyonce;
    public AxisCameraOffset(EntityData data, Vector2 offset) : base(data, offset)
    {
        string cam = data.Attr("cameraMode");
        if (cam == "Y Only") { this.cameraMode = camMode.y; }
        else if (cam == "X Only") { this.cameraMode = camMode.x; }
        else{ this.cameraMode = camMode.both; }

        string tg = data.Attr("triggerMode");
        if (tg == "toggle") { this.triggerMode = tgMode.toggle; }
        else if (tg == "inZone") { this.triggerMode = tgMode.inZone; }

        this.setX = data.Float("cameraX");
        this.setY = data.Float("cameraY");
        

        string fM = data.Attr("flagControl");
        if(fM == "disabled")
        {
            this.flagControl = flagMode.normal;
        }
        else if(fM == "flagNeeded")
        {
            this.flagControl = flagMode.required;
        }
        else if(fM == "flagInverted")
        {
            this.flagControl = flagMode.inverted;
        }

        this.flag = data.Attr("flag");
        if(this.flag == null) { this.flag = "flag"; }

        //摄像机长度换算
        string unit = data.Attr("units");
        if (unit == "offset")
        {
            scaleX = 48f;
            scaleY = 32f;
        }
        else if(unit == "offsetSquared")
        {
            scaleX = 48f;
            scaleY = 48f;
        }
        else if(unit == "tiles")
        {
            scaleX = 8f;
            scaleY = 8f;
        }
        else if(unit == "pixels")
        {
            scaleX = 1f;
            scaleY = 1f;
        }

        this.onlyonce = data.Bool("onlyOnce");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
    }

    public void setCam()
    {
        if (cameraMode == camMode.both)
        {
            CameraOffset = new Vector2(this.setX * this.scaleX, this.setY * this.scaleY);
        }
        else if (cameraMode == camMode.x)
        {
            CameraOffset = new Vector2(this.setX * this.scaleX, this.currentOffset.Y);
        }
        else if (cameraMode == camMode.y)
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

        setCam();
        setOffset();

    }

    public override void OnStay(Player player)
    {
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
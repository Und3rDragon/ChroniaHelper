using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/PlatformLineController")]
public class PlatformLineController : Entity
{
    public static void Load()
    {
        On.Celeste.MovingPlatformLine.Added += MP_Added_Modify;
        On.Celeste.MovingPlatformLine.Render += MP_Render_Modify;
        On.Celeste.SinkingPlatformLine.Render += SP_Render_Modify;
    }

    public static void Unload()
    {
        On.Celeste.MovingPlatformLine.Added -= MP_Added_Modify;
        On.Celeste.MovingPlatformLine.Render -= MP_Render_Modify;
        On.Celeste.SinkingPlatformLine.Render -= SP_Render_Modify;
    }

    // Vanilla constants
    public Color sinkLineEdgeColor = Calc.HexToColor("2a1923");
    public Color sinkLineInnerColor = Calc.HexToColor("160b12");
    public Color specialMoveLineEdgeColor = Calc.HexToColor("a4464a");
    public Color specialMoveLineInnerColor = Calc.HexToColor("86354e");
    public Color moveLineEdgeColor = Calc.HexToColor("2a1923");
    public Color moveLineInnerColor = Calc.HexToColor("160b12");

    public PlatformLineController(EntityData data) : this(data, data.Position)
    {

    }
    public PlatformLineController(EntityData data, Vector2 position) : base(position)
    {
        ChroniaHelperModule.Session.platformLine_edgeColor = Calc.HexToColor(data.Attr("edgeColor", "a4464a"));
        ChroniaHelperModule.Session.platformLine_centerColor = Calc.HexToColor(data.Attr("centerColor", "86354e"));

        switch(data.Int("renderMode", 0))
        {
            case 1:
                // moving platform
                ChroniaHelperModule.Session.modifyMovingPlatformLine = true;
                ChroniaHelperModule.Session.modifySinkingPlatformLine = false;
                break;
            case 2:
                // sinking platform
                ChroniaHelperModule.Session.modifyMovingPlatformLine = false;
                ChroniaHelperModule.Session.modifySinkingPlatformLine = true;
                break;
            case 3:
                // all
                ChroniaHelperModule.Session.modifyMovingPlatformLine = true;
                ChroniaHelperModule.Session.modifySinkingPlatformLine = true;
                break;
            default:
                ChroniaHelperModule.Session.modifyMovingPlatformLine = false;
                ChroniaHelperModule.Session.modifySinkingPlatformLine = false;
                break;
        }

        base.Tag = Tags.TransitionUpdate;
    }

    private int ID;

    public override void Added(Scene scene)
    {
        int maxID = 0;
        foreach(var item in MapProcessor.level.Entities)
        {
            if(item is PlatformLineController controller)
            {
                maxID = Math.Max(controller.ID, ID);
            }
        }
        if(ID < maxID)
        {
            RemoveSelf();
        }

        base.Added(scene);
    }
    public static void MP_Added_Modify(On.Celeste.MovingPlatformLine.orig_Added orig, MovingPlatformLine self, Scene scene)
    {
        orig(self, scene);

        if (ChroniaHelperModule.Session.modifyMovingPlatformLine)
        {
            if ((scene as Level).Session.Area.ID == 4)
            {
                self.lineEdgeColor = Calc.HexToColor("a4464a");
                self.lineInnerColor = Calc.HexToColor("86354e");
            }
            else
            {
                self.lineEdgeColor = Calc.HexToColor("2a1923");
                self.lineInnerColor = Calc.HexToColor("160b12");
            }
        }
    }

    public static void MP_Render_Modify(On.Celeste.MovingPlatformLine.orig_Render orig, MovingPlatformLine self)
    {
        orig(self);

        if (ChroniaHelperModule.Session.modifyMovingPlatformLine)
        {
            Vector2 vector = (self.end - self.Position).SafeNormalize();
            Vector2 vector2 = new Vector2(0f - vector.Y, vector.X);
            Draw.Line(self.Position - vector - vector2, self.end + vector - vector2, ChroniaHelperModule.Session.platformLine_edgeColor);
            Draw.Line(self.Position - vector, self.end + vector, ChroniaHelperModule.Session.platformLine_edgeColor);
            Draw.Line(self.Position - vector + vector2, self.end + vector + vector2, ChroniaHelperModule.Session.platformLine_edgeColor);
            Draw.Line(self.Position, self.end, ChroniaHelperModule.Session.platformLine_centerColor);
        }
    }

    public static void SP_Render_Modify(On.Celeste.SinkingPlatformLine.orig_Render orig, SinkingPlatformLine self)
    {
        orig(self);

        if (ChroniaHelperModule.Session.modifySinkingPlatformLine)
        {
            Draw.Rect(self.X - 1f, self.Y, 3f, self.height, ChroniaHelperModule.Session.platformLine_edgeColor);
            Draw.Rect(self.X, self.Y + 1f, 1f, self.height, ChroniaHelperModule.Session.platformLine_centerColor);
        }
    }
}

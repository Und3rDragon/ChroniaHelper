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
        On.Celeste.SinkingPlatformLine.Added += SP_Added_Modify;
        On.Celeste.SinkingPlatformLine.Render += SP_Render_Modify;
    }

    public static void Unload()
    {
        On.Celeste.MovingPlatformLine.Added -= MP_Added_Modify;
        On.Celeste.MovingPlatformLine.Render -= MP_Render_Modify;
        On.Celeste.SinkingPlatformLine.Added -= SP_Added_Modify;
        On.Celeste.SinkingPlatformLine.Render -= SP_Render_Modify;
    }

    public PlatformLineController(EntityData data) : this(data, data.Position)
    {

    }
    public PlatformLineController(EntityData data, Vector2 position) : base(position)
    {
        Color edgeColor = Calc.HexToColor(data.Attr("edgeColor", "a4464a"));
        Color centerColor = Calc.HexToColor(data.Attr("centerColor", "86354e"));

        switch(mode = data.Int("renderMode", 0))
        {
            case 1:
                // moving platform
                Md.Session.modifyMovingPlatformLine = true;

                Md.Session.platformLine_MP_centerColor = centerColor;
                Md.Session.platformLine_MP_edgeColor = edgeColor;

                Md.Session.platformLine_MP_depth = data.Int("depth", 9001);
                break;
            case 2:
                // sinking platform
                Md.Session.modifySinkingPlatformLine = true;

                Md.Session.platformLine_SP_centerColor = centerColor;
                Md.Session.platformLine_SP_edgeColor = edgeColor;

                Md.Session.platformLine_SP_depth = data.Int("depth", 9001);
                break;
            case 3:
                // all
                Md.Session.modifyMovingPlatformLine = true;
                Md.Session.modifySinkingPlatformLine = true;

                Md.Session.platformLine_MP_centerColor = centerColor;
                Md.Session.platformLine_MP_edgeColor = edgeColor;

                Md.Session.platformLine_SP_centerColor = centerColor;
                Md.Session.platformLine_SP_edgeColor = edgeColor;

                Md.Session.platformLine_SP_depth = data.Int("depth", 9001);
                Md.Session.platformLine_MP_depth = data.Int("depth", 9001);
                break;
            default:
                Md.Session.modifyMovingPlatformLine = false;
                Md.Session.modifySinkingPlatformLine = false;
                break;
        }

        base.Tag = Tags.TransitionUpdate;
    }

    private int ID, mode;

    public override void Added(Scene scene)
    {
        // comparator
        int maxModeID = 3;

        int[] maxID = new int[maxModeID + 1];
        bool[] modeExist = new bool[maxModeID + 1];

        for(int i = 0; i <= maxModeID; i ++)
        {
            maxID[i] = 0;
            modeExist[i] = false;
        }

        foreach(var item in MapProcessor.level.Entities)
        {
            if(item is PlatformLineController controller)
            {
                maxID[controller.mode] = Math.Max(controller.ID, ID);
                modeExist[controller.mode] = true;
            }
        }

        if(mode == 0)
        {
            for(int i = 1; i <= maxModeID; i++)
            {
                if (modeExist[i]) { RemoveSelf(); }
            }
        }
        else if(mode == 3)
        {
            if (modeExist[1] || modeExist[2]) { RemoveSelf(); }
        }

        if(ID < maxID[mode])
        {
            RemoveSelf();
        }

        base.Added(scene);
    }
    public static void MP_Added_Modify(On.Celeste.MovingPlatformLine.orig_Added orig, MovingPlatformLine self, Scene scene)
    {
        orig(self, scene);

        if (Md.Session.modifyMovingPlatformLine)
        {
            self.Depth = Md.Session.platformLine_MP_depth;

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

        if (Md.Session.modifyMovingPlatformLine)
        {
            Vector2 vector = (self.end - self.Position).SafeNormalize();
            Vector2 vector2 = new Vector2(0f - vector.Y, vector.X);
            Draw.Line(self.Position - vector - vector2, self.end + vector - vector2, Md.Session.platformLine_MP_edgeColor);
            Draw.Line(self.Position - vector, self.end + vector, Md.Session.platformLine_MP_edgeColor);
            Draw.Line(self.Position - vector + vector2, self.end + vector + vector2, Md.Session.platformLine_MP_edgeColor);
            Draw.Line(self.Position, self.end, Md.Session.platformLine_MP_centerColor);
        }
    }

    public static void SP_Added_Modify(On.Celeste.SinkingPlatformLine.orig_Added orig, SinkingPlatformLine self, Scene scene)
    {
        orig(self, scene);

        if (Md.Session.modifySinkingPlatformLine)
        {
            self.Depth = Md.Session.platformLine_SP_depth;
        }
    }
    public static void SP_Render_Modify(On.Celeste.SinkingPlatformLine.orig_Render orig, SinkingPlatformLine self)
    {
        orig(self);

        if (Md.Session.modifySinkingPlatformLine)
        {
            Draw.Rect(self.X - 1f, self.Y, 3f, self.height, Md.Session.platformLine_SP_edgeColor);
            Draw.Rect(self.X, self.Y + 1f, 1f, self.height, Md.Session.platformLine_SP_centerColor);
        }
    }
}

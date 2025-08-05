using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/ZaggingLine")]
public class ZaggingLine : Entity
{
    public ZaggingLine(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        nodes = data.NodesWithPosition(offset);

        // bg line
        hasBgLine = data.Bool("showBgLine", false);
        bgLineColor = Calc.HexToColorWithAlpha(data.Attr("bgLineColor", "ffffffff"));

        // node sprite
        showNodes = data.Bool("showNodes", true);
        nodeSprite = new(GFX.Game, "ChroniaHelper/ZaggingLine/node");
        nodeSprite.AddLoop("idle","", 0.1f);
        Add(nodeSprite);
        nodeSprite.JustifyOrigin(0.5f, 0.5f);
        nodeSprite.Play("idle");

        string[] _nodeColor = data.Attr("nodeColors", "ffffffff").Split(",",StringSplitOptions.TrimEntries);
        nodeColors = new Color[_nodeColor.Length]; 
        for(int i = 0; i < _nodeColor.Length; i++)
        {
            nodeColors[i] = Calc.HexToColorWithAlpha(_nodeColor[i]);
        }
        

        // main line
        string[] _lineColors = data.Attr("lineColors", "ffffffff").Split(",",StringSplitOptions.TrimEntries);
        fgLineColors = new Color[_lineColors.Length];
        for(int i = 0; i < _lineColors.Length; i++)
        {
            fgLineColors[i] = Calc.HexToColorWithAlpha(_lineColors[i]);
        }
        timer = new float[fgLineColors.Length];

        string[] _intervals = data.Attr("intervals", "1").Split(",",StringSplitOptions.TrimEntries);
        intervals = new float[_intervals.Length];
        for(int i = 0; i < _intervals.Length; i++)
        {
            float t = _intervals[i].ParseFloat(1f);
            if (t < Engine.DeltaTime) { t = Engine.DeltaTime; }
            intervals[i] = t;
        }
        
        string[] _durations = data.Attr("durations", "3").Split(",",StringSplitOptions.TrimEntries);
        durations = new float[_durations.Length];
        for(int i = 0; i < _durations.Length; i++)
        {
            float t = _durations[i].ParseFloat(3f);
            if(t < Engine.DeltaTime) { t = Engine.DeltaTime; }
            durations[i] = t;
        }

        string[] _easers = data.Attr("ease", "sinein").Split(",", StringSplitOptions.TrimEntries);
        easers = new EaseMode[_easers.Length];
        for(int i = 0; i < _easers.Length; i++)
        {
            easers[i] = EaseUtils.StringToEaseMode(_easers[i]);
        }

        Depth = data.Int("depth", 9500);
    }
    private Vector2[] nodes;
    private bool hasBgLine, showNodes;
    private Sprite nodeSprite;
    private Color bgLineColor;
    private Color[] fgLineColors, nodeColors;
    private float[] intervals, durations;
    private EaseMode[] easers;

    private float[] timer;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        timer = 0f.CreateArray(fgLineColors.Length);
    }

    public override void Update()
    {
        base.Update();

        for(int i = 0; i < timer.Length; i++)
        {
            timer[i] += Engine.DeltaTime;
        }

        for(int i = 0; i < fgLineColors.Length; i++)
        {
            if(timer.SafeGet(i) >= intervals.SafeGet(i) + durations.SafeGet(i))
            {
                CollectiveUtils.SafeSet(ref timer, i, 0f);
            }
        }
    }

    public override void Render()
    {
        base.Render();
        
        for(int i = 0; i < nodes.MaxIndex(); i++)
        {
            Vector2 from = nodes[i], to = nodes[i + 1];

            for(int j = 0; j < fgLineColors.Length; j++)
            {
                float loopLength = intervals.SafeGet(j) + durations.SafeGet(j);

                Vector2 p1 = FadeUtils.LerpValue(timer.SafeGet(j), 0f, durations.SafeGet(j), from, to, easers.SafeGet(j)),
                    p2 = FadeUtils.LerpValue(timer.SafeGet(j), intervals.SafeGet(j), loopLength, from, to, easers.SafeGet(j));

                Draw.Line(p1, p2, fgLineColors.SafeGet(j));
            }
        }
    }

}

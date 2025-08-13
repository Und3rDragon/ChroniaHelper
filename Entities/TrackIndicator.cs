using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using Line = ChroniaHelper.Utils.GeometryUtils.Line;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TrackIndicator")]
public class TrackIndicator : Entity
{
    public TrackIndicator(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        nodes = data.NodesWithPosition(offset);
        lines = new Line[nodes.Length];
        for(int i = 0; i < nodes.MaxIndex(); i++)
        {
            lines[i] = new(nodes[i], nodes[i + 1]);
        }
    }
    private Vector2[] nodes;
    private Line[] lines;

    private int player_node = -1;
    private float player_progress = 0f;
    private void GetPlayerProgress()
    {
        if (!MaP.playerAlive) { return; }

        float minDistance = -1f;
        int index = -1;
        for(int i = 0; i < lines.Length; i++)
        {
            if(i == 0)
            {
                minDistance = lines[i].GetDistance(MaP.player.Position);
                index = 0;
            }
            else
            {
                if (lines[i].GetDistance(MaP.player.Position) < minDistance)
                {
                    minDistance = lines[i].GetDistance(MaP.player.Position);
                    index = i;
                }
                else if(lines[i].GetDistance(MaP.player.Position) == minDistance)
                {
                    if (i < index) { index = i; }
                }
            }
        }

        player_node = index;
        player_progress = MaP.player.Position.ClampProgress(lines[index].A, lines[index].B);
    }

    public override void Update()
    {
        base.Update();

        GetPlayerProgress();
    }
}

[Tracked(true)]
[CustomEntity("ChroniaHelper/TrackProgressBar")]
public class TrackProgressBar : Entity
{
    public TrackProgressBar(EntityData data, Vector2 offset) : base(data.Position + offset)
    {

    }

}
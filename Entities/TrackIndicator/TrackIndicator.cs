using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using Line = ChroniaHelper.Utils.GeometryUtils.Line;

namespace ChroniaHelper.Entities.TrackIndicator;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TrackIndicator")]
public class TrackIndicator : Entity
{
    public TrackIndicator(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        ID = data.ID;
        nodes = data.NodesWithPosition(offset);
        lines = new Line[nodes.Length];
        for(int i = 0; i < nodes.MaxIndex(); i++)
        {
            lines[i] = new(nodes[i], nodes[i + 1]);
        }
    }
    private Vector2[] nodes;
    private Line[] lines;
    private int ID;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        tracking = new Coroutine(PlayerTracking());
        Add(tracking);
    }
    private Coroutine tracking;

    private IEnumerator PlayerTracking()
    {
        playerIndex = 0;
        playerProgress = 0f;
        int maxIndex = lines.Length - 1;
        while (true)
        {
            if (!PUt.getPlayer) { yield return null; continue; }

            Line line = lines[playerIndex];
            playerProgress = PUt.player.Position.ClampProgress(line.A, line.B);
            if(playerProgress == 1f && playerIndex < maxIndex)
            {
                playerIndex++;
                playerProgress = 0f;
            }
            
            yield return null;
        }
    }
    private int playerIndex = 0;
    private float playerProgress = 0f;

    public override void Update()
    {
        base.Update();
    }
}
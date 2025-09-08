using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Celeste;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers.PolygonSeries
{
    [CustomEntity("ChroniaHelper/PolygonTrigger")]
    public class PolygonTrigger : Trigger
    {
        public PolygonCollider polygonCollider => (PolygonCollider)Collider;
        public VertexPositionColor[] shanpe;
        public Color RenderColor { get; set; } = Color.Transparent;
        internal Vector2 prevPos; internal Color prevColor;

        public PolygonTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Collider = new PolygonCollider(data.NodesWithPosition(offset), this, true);

            string[] _ids = data.Attr("triggerIDs").Split(',', StringSplitOptions.TrimEntries);
            foreach(var id in _ids)
            {
                if (id.IsInt()) { ids.Enter(id.ParseInt()); }
            }

            once = data.Bool("oneUse", false);

            Visible = true;
            Collidable = true;
        }
        private HashSet<int> ids = new();
        private HashSet<Trigger> targets = new();
        private bool once;

        public override void Awake(Scene scene)
        {
            foreach(var i in MaP.level.Tracker.GetEntities<Trigger>())
            {
                if (ids.Contains(i.SourceData.ID))
                {
                    targets.Enter(i as Trigger);
                }
            }

            base.Awake(scene);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            foreach(var trigger in targets)
            {
                trigger.OnEnter(player);
            }
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);

            foreach (var trigger in targets)
            {
                trigger.OnStay(player);
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);

            foreach (var trigger in targets)
            {
                trigger.OnLeave(player);
            }

            if (once) { RemoveSelf(); }
        }

        public override void Update()
        {
            base.Update();
            if (Visible && (prevPos != Position || RenderColor != prevColor) || shanpe == null)
            {
                if (shanpe == null)
                    shanpe = new VertexPositionColor[polygonCollider.Indices.Length];
                for (int i = 0; i < polygonCollider.Indices.Length; i += 3)
                {
                    shanpe[i].Position = new Vector3(Position + polygonCollider.TriangulatedPoints[polygonCollider.Indices[i]], 0f);
                    shanpe[i].Color = RenderColor;
                    shanpe[i + 1].Position = new Vector3(Position + polygonCollider.TriangulatedPoints[polygonCollider.Indices[i + 1]], 0f);
                    shanpe[i + 1].Color = RenderColor;
                    shanpe[i + 2].Position = new Vector3(Position + polygonCollider.TriangulatedPoints[polygonCollider.Indices[i + 2]], 0f);
                    shanpe[i + 2].Color = RenderColor;
                }
            }
            prevPos = Position;
            prevColor = RenderColor;
        }
        public override void Render()
        {
            if (RenderColor != Color.Transparent)
                GFX.DrawVertices((Scene as Level)!.Camera.Matrix, shanpe, polygonCollider.Indices.Length);
        }

    }
}

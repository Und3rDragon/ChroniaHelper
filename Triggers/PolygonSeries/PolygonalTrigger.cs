using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;

namespace ChroniaHelper.Triggers.PolygonSeries
{
    [CustomEntity(new string[]
    {
        "ChroniaHelper/PolygonTrigger"
    })]
    public class PolygonalTriggerTrigger : AbstractPolygonTrigger
    {
        public PolygonalTriggerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            this.triggerPoint = this.Position;
            this.onlyOnce = data.Bool("oneUse", false);
            base.Collider = new PolygonCollider(data.NodesWithPosition(offset), this, true);
            string typeSet = data.Attr("Types", "");
            this.assignableTypes = new List<Type>();
            this.Types = new List<Type>();
            VivHelper.VivHelper.AppendTypesToList(typeSet, ref this.Types, ref this.assignableTypes, typeof(Trigger));
            this.Collidable = false;
            this.Associators = new List<Trigger>(this.Types.Count + this.assignableTypes.Count);
            this.Visible = true;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Entity entity in Enumerable.Where<Entity>(scene.Entities, (Entity f) => Collide.CheckPoint(f, this.triggerPoint)))
            {
                bool flag = VivHelper.VivHelper.MatchTypeFromTypeSet(entity.GetType(), this.Types, this.assignableTypes) && entity.Collider.GetType() != typeof(PolygonCollider);
                if (flag)
                {
                    this.Associators.Add(entity as Trigger);
                    entity.Collidable = false;
                    break;
                }
            }
            this.Collidable = true;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            foreach (Trigger trigger in this.Associators)
            {
                Log.Info($"associator {trigger.SourceData.ID}");

                bool flag = trigger == null || trigger.Scene == null || (this.flagToggle != null && !(base.Scene as Level).Session.GetFlag(this.flagToggle));

                Log.Info(trigger == null, trigger.Scene == null, this.flagToggle != null, (base.Scene as Level).Session.GetFlag(this.flagToggle));
                Log.Info(flag);

                if (!flag)
                {
                    Vector2 position = player.Position;
                    Vector2 value = base.GetPercentageOfBoundingBox_Safe(player.Center).Value;
                    player.Position = trigger.TopLeft + new Vector2(trigger.Width * value.X, trigger.Height * value.Y);
                    trigger.Triggered = true;
                    trigger.OnEnter(player);

                    Log.Info(trigger.SourceData.ID, "triggered");

                    player.Position = position;
                }
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            foreach (Trigger trigger in this.Associators)
            {
                bool flag = trigger == null || trigger.Scene == null || (this.flagToggle != null && !(base.Scene as Level).Session.GetFlag(this.flagToggle));
                if (!flag)
                {
                    Vector2 position = player.Position;
                    Vector2 value = base.GetPercentageOfBoundingBox_Safe(player.Center).Value;
                    player.Position = trigger.TopLeft + new Vector2(trigger.Width * value.X, trigger.Height * value.Y);
                    trigger.OnStay(player);
                    player.Position = position;
                }
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnStay(player);
            foreach (Trigger trigger in this.Associators)
            {
                bool flag = trigger == null || trigger.Scene == null || (this.flagToggle != null && !(base.Scene as Level).Session.GetFlag(this.flagToggle));
                if (!flag)
                {
                    Vector2 position = player.Position;
                    Vector2 value = base.GetPercentageOfBoundingBox_Safe(player.Center).Value;
                    player.Position = trigger.TopLeft + new Vector2(trigger.Width * value.X, trigger.Height * value.Y);
                    trigger.OnLeave(player);
                    trigger.Triggered = false;
                    player.Position = position;
                }
            }
        }

        public string flagToggle;

        public new bool onlyOnce;

        public List<Trigger> Associators;

        public List<Type> Types;

        public List<Type> assignableTypes;

        private Vector2 triggerPoint;

        internal Vector2 percentageBoundingBox;
    }
}

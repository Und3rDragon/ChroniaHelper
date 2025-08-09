using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

// The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
// The original author is ricky06, code modified by UnderDragon

namespace ChroniaHelper.Entities.MigratedNeonHelper
{
	[Tracked]
	[CustomEntity("ChroniaHelper/ShiftingSwitch")]

	public class ShiftingSwitch : Solid
	{
		[Pooled]
		private class RingEffect : Entity
		{
			private MTexture ring;
			private Vector2 directionalOffset;
			private float rotation;
			private Color color;

			public RingEffect()
			{
				ring = GFX.Game["objects/ChroniaHelper/shiftingSwitch/ring"];
				Depth = -12000;
			}

			public RingEffect Init(Vector2 position, Sides side, Color color)
			{
				Position = position;
				this.color = color;
				switch (side)
				{
					case Sides.Left:
						directionalOffset = new Vector2(-0.5f, 0);
						rotation = 0f;
						break;
					case Sides.Right:
						directionalOffset = new Vector2(0.5f, 0);
						rotation = 0f;
						break;
					case Sides.Up:
						directionalOffset = new Vector2(0f, -0.5f);
						rotation = (float)Math.PI / 2f;
						break;
					case Sides.Down:
						directionalOffset = new Vector2(0f, 0.5f);
						rotation = (float)Math.PI / 2f;
						break;
				}
				return this;
			}

			public override void Update()
			{
				base.Update();
				if (Scene.OnInterval(0.03f))
				{
					Position += directionalOffset;
				}
				color = Color.Lerp(color, color * 0f, Engine.DeltaTime);
				if (color.A <= 0f)
				{
					RemoveSelf();
				}
			}

			public override void Render()
			{
				base.Render();
				ring.DrawCentered(Position, color, 1f, rotation);
			}
		}
		public enum Sides
		{
			Left,
			Right,
			Up,
			Down
		};

		private Sprite sprite;
		private MTexture connector;
		private HashSet<Sides> activeSides;
		private bool spikesLeft;
		private bool spikesRight;
		private bool spikesUp;
		private bool spikesDown;
		private bool noConnector;
		private bool oneConnector;
		private bool silent;

		private List<ShiftingBlock> targets;
		private List<Vector2> nodes;

		private float speed, distance;

		private float sineTimer = 0f;

		private Color connectorColor = Calc.HexToColor("aefaff");
		private Color effectsColor = Calc.HexToColor("aefaff");

		private DashCollisionResults normal, rebound;

		public ShiftingSwitch(Vector2 position, bool left, bool right, bool top, bool down, float speed, float distance, List<Vector2> nodes, bool oneConnector, bool silent, EntityData data)
			: base(position, 24f, 24f, safe: true)
		{
			Depth = -13000;
			SurfaceSoundIndex = 11;
			Add(sprite = GFX.SpriteBank.Create(data.Attr("sprite", "shiftingSwitch")));
			connector = GFX.Game[data.Attr("connectorSprite", "objects/ChroniaHelper/shiftingSwitch/connector")];
			sprite.Position = new Vector2(Width, Height) / 2f;

			// custom rebounding
			normal = (DashCollisionResults)data.Int("onNormal", 1);
            rebound = (DashCollisionResults)data.Int("onRebound", 0);

            OnDashCollide = Dashed;

			this.speed = speed;
			this.distance = distance;
			this.nodes = nodes;
			this.oneConnector = oneConnector;
			this.silent = silent;

			targets = new List<ShiftingBlock>();
			activeSides = new HashSet<Sides>();

			if (left)
			{
				activeSides.Add(Sides.Left);
			}
			if (right)
			{
				activeSides.Add(Sides.Right);
			}
			if (top)
			{
				activeSides.Add(Sides.Up);
			}
			if (down)
			{
				activeSides.Add(Sides.Down);
			}
		}

		public ShiftingSwitch(EntityData e, Vector2 levelOffset)
			: this(e.Position + levelOffset, e.Bool("left"), e.Bool("right"), e.Bool("top"), e.Bool("down"), e.Float("speed"), e.Float("distance"), new List<Vector2>(e.NodesOffset(levelOffset)), e.Bool("oneConnector"), e.Bool("silent"),
				  e)
		{

		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			spikesUp = CollideCheck<Spikes>(Position - Vector2.UnitY);
			spikesDown = CollideCheck<Spikes>(Position + Vector2.UnitY);
			spikesLeft = CollideCheck<Spikes>(Position - Vector2.UnitX);
			spikesRight = CollideCheck<Spikes>(Position + Vector2.UnitX);

			populateTargetsList();
		}

		private ShiftingBlock findNearestBlock(Vector2 node)
		{
			ShiftingBlock nearest = null;
			float nearestDist = float.MaxValue;
			foreach (ShiftingBlock sb in Scene.Tracker.GetEntities<ShiftingBlock>())
			{
				float dist = Vector2.Distance(node, sb.Position);
				if (dist < nearestDist)
				{
					nearest = sb;
					nearestDist = dist;
				}
			}
			return nearest;
		}

		private void populateTargetsList()
		{
			targets.Clear();
			if (nodes.Count == 0)
			{
				noConnector = true;
				foreach (ShiftingBlock sb in Scene.Tracker.GetEntities<ShiftingBlock>())
				{
					targets.Add(sb);
				}
				return;
			}
			foreach (Vector2 n in nodes)
			{
				targets.Add(findNearestBlock(n));
			}
		}

		public DashCollisionResults Dashed(Player player, Vector2 dir)
		{
			Sides dashOn = Sides.Down;


			if (dir == Vector2.UnitX)
			{
				dashOn = Sides.Left;
			}
			if (dir == -Vector2.UnitX)
			{
				dashOn = Sides.Right;
			}
			if (dir == Vector2.UnitY)
			{
				dashOn = Sides.Up;
			}
			if (dir == -Vector2.UnitY)
			{
				dashOn = Sides.Down;
			}

			switch (dashOn)
			{
				case Sides.Left:
					if (activeSides.Contains(Sides.Left))
					{
						return handleRebound(dir, Sides.Left);
					}
					return normal;

				case Sides.Right:
					if (activeSides.Contains(Sides.Right))
					{
						return handleRebound(dir, Sides.Right);
					}
					return normal;

				case Sides.Up:
					if (activeSides.Contains(Sides.Up))
					{
						return handleRebound(dir, Sides.Up);
					}
					return normal;

				case Sides.Down:
					if (activeSides.Contains(Sides.Down))
					{
						return handleRebound(dir, Sides.Down);
					}
					return normal;
				default:
					return normal;
			}
		}

		public void ActivateAll()
		{
			if (activeSides.Contains(Sides.Left))
			{
				moveTargets(Sides.Left);
			}
			if (activeSides.Contains(Sides.Right))
			{
				moveTargets(Sides.Right);
			}
			if (activeSides.Contains(Sides.Up))
			{
				moveTargets(Sides.Up);
			}
			if (activeSides.Contains(Sides.Down))
			{
				moveTargets(Sides.Down);
			}
			activeSides.Clear();
			Deactivate();
		}

		private void Deactivate()
		{
			Depth = 100;
			DestroyStaticMovers();
			sprite.Play("burst");
			if (!silent)
			{
				Audio.Play("event:/ricky06/NeonCity/switchBreak", Position);
			}
			SceneAs<Level>().Displacement.AddBurst(Center, 0.4f, 0f, 64f, alpha:0.5f);
			Collidable = false;
		}

		private DashCollisionResults handleRebound(Vector2 dir, Sides side)
		{
            // TODO: temporary sound
            Audio.Play("event:/game/05_mirror_temple/button_activate", Position);

            sprite.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
			moveTargets(side);
			Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.RefillDash();
				Audio.Play("event:/game/general/diamond_touch", Position);
			}
			activeSides.Remove(side);
			if (activeSides.Count <= 0)
			{
				Deactivate();
			}
			else
			{
				sprite.Play("flash");
			}

			return rebound;
		}

		public override void Removed(Scene scene)
		{
			DestroyStaticMovers();
			base.Removed(scene);
		}

		private void moveTargets(Sides side)
		{
			if (distance == 0) return;
			if (nodes.Count == 0)
			{
				foreach (ShiftingBlock sb in targets)
				{
					bool blockSilent = true;
					if (findNearestBlock(Position) == sb)
					{
						blockSilent = false;
					}
					switch (side)
					{
						case Sides.Left:
							sb.MoveBlocks(new Vector2(-distance, 0), speed, blockSilent);
							break;
						case Sides.Right:
							sb.MoveBlocks(new Vector2(distance, 0), speed, blockSilent);
							break;
						case Sides.Up:
							sb.MoveBlocks(new Vector2(0, -distance), speed, blockSilent);
							break;
						case Sides.Down:
							sb.MoveBlocks(new Vector2(0, distance), speed, blockSilent);
							break;
					}
				}
			}
			else
            {
				bool blockSilent = false;
				foreach (ShiftingBlock sb in targets)
				{
					switch (side)
					{
						case Sides.Left:
							sb.MoveBlocks(new Vector2(-distance, 0), speed, blockSilent);
							break;
						case Sides.Right:
							sb.MoveBlocks(new Vector2(distance, 0), speed, blockSilent);
							break;
						case Sides.Up:
							sb.MoveBlocks(new Vector2(0, -distance), speed, blockSilent);
							break;
						case Sides.Down:
							sb.MoveBlocks(new Vector2(0, distance), speed, blockSilent);
							break;
					}
					blockSilent = true;
				}
			}
		}

		public override void Update()
		{
			base.Update();
			sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
			sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
			LiftSpeed = Vector2.Zero;

			sineTimer += Engine.DeltaTime * 3f;

			if (Scene.OnInterval(1f))
			{
				connectorColor = Color.LightGoldenrodYellow;
			}

			if (connectorColor != Color.SkyBlue)
			{
				connectorColor = Color.Lerp(connectorColor, Calc.HexToColor("aefaff"), Engine.DeltaTime * 2f);
			}

			if (Scene.OnInterval(0.3f))
			{
				foreach (Sides side in activeSides)
				{
					switch (side)
					{
						case Sides.Left:
							AddRing(CenterLeft, side);
							break;
						case Sides.Right:
							AddRing(CenterRight, side);
							break;
						case Sides.Up:
							AddRing(TopCenter, side);
							break;
						case Sides.Down:
							AddRing(BottomCenter, side);
							break;
					}
				}
			}
		}

		private void AddRing(Vector2 position, Sides side)
		{
			SceneAs<Level>().Add(Engine.Pooler.Create<RingEffect>().Init(position, side, effectsColor));
		}

		public override void Render()
		{
			base.Render();

			// draw connectors
			if(oneConnector)
            {
				ShiftingBlock sb = targets.First();
				Draw.SineTextureH(connector, Center, Vector2.Zero, new Vector2(Vector2.Distance(Center, sb.Center) / 128f, 1.5f), Calc.Angle(Center, sb.Center), connectorColor * 0.5f, SpriteEffects.None, sineTimer, 2f, 1, 0.05f);
			}
			else if (!noConnector)
			{
				foreach (ShiftingBlock sb in targets)
				{
					if(sb.noConnector)
                    {
						continue;
                    }
					Draw.SineTextureH(connector, Center, Vector2.Zero, new Vector2(Vector2.Distance(Center, sb.Center) / 128f, 1.5f), Calc.Angle(Center, sb.Center), connectorColor * 0.5f, SpriteEffects.None, sineTimer, 2f, 1, 0.05f);
				}
			}
			if (sprite.Scale.X != 1f || sprite.Scale.Y != 1f)
			{
				return;
			}
			foreach (Sides side in activeSides)
			{
				switch (side)
				{
					case Sides.Left:
						Draw.Line(TopLeft, BottomLeft, effectsColor);
						break;
					case Sides.Right:
						Draw.Line(TopRight + Vector2.UnitX, BottomRight + Vector2.UnitX, effectsColor);
						break;
					case Sides.Up:
						Draw.Line(TopLeft - Vector2.UnitY, TopRight - Vector2.UnitY, effectsColor);
						break;
					case Sides.Down:
						Draw.Line(BottomLeft, BottomRight, effectsColor);
						break;
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using MonoMod.Utils;
using Celeste;
using ChroniaHelper.Utils;

// The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
// The original author is ricky06, code modified by UnderDragon

namespace ChroniaHelper.Entities.MigratedNeonHelper
{
	[CustomEntity("ChroniaHelper/PufferBomb")]
	[TrackedAs(typeof(Puffer))]
	public class PufferBomb : Actor
	{
		private enum States
		{
			Idle,
			Hit,
			Gone
		}

		private const float RespawnTime = 2.5f;

		private const float RespawnMoveTime = 0.5f;

		private const float BounceSpeed = 200f;

		private const float ExplodeRadius = 40f;

		private const float DetectRadius = 32f;

		private const float StunnedAccel = 320f;

		private const float AlertedRadius = 60f;

		private const float CantExplodeTime = 0.5f;

		private Sprite sprite;

		private States state;

		private Vector2 startPosition;

		private Vector2 anchorPosition;

		private Vector2 lastSpeedPosition;

		private Vector2 lastSinePosition;

		private Hitbox pushRadius;

		private Circle breakWallsRadius;

		private Hitbox detectRadius;

		private Vector2 hitSpeed;

		private float goneTimer;

		private float cannotHitTimer;

		private Collision onCollideV;

		private Collision onCollideH;

		private float alertTimer;

		private Wiggler bounceWiggler;

		private Wiggler inflateWiggler;

		private Vector2 scale;

		private SimpleCurve returnCurve;

		private float cantExplodeTimer;

		private Vector2 lastPlayerPos;

		private float playerAliveFade;

		private Vector2 facing = Vector2.One;

		private float eyeSpin;

		private Sprite explosionRange;

		private bool oneUse;

		private bool moreFreezeFrames;

		private bool alwaysBoost;

		private bool exploding;

		private bool longRange, ignoreSolids;

		private float rotation = 0f;

		public static ParticleType P_Explosion = new ParticleType
		{
			Color = Calc.HexToColor("ff7d7d"),
			Color2 = Calc.HexToColor("ffa938"),
			ColorMode = ParticleType.ColorModes.Blink,
			FadeMode = ParticleType.FadeModes.Late,
			Size = 1f,
			LifeMin = 0.4f,
			LifeMax = 1.2f,
			SpeedMin = 20f,
			SpeedMax = 100f,
			SpeedMultiplier = 0.4f,
			DirectionRange = (float)Math.PI / 3f
		};

		public PufferBomb(Vector2 position, bool oneUse, bool moreFreezeFrames, bool alwaysBoost, bool longRange, bool ignoreSolids, EntityData data)
				: base(position)
		{
			this.oneUse = true;
			this.moreFreezeFrames = moreFreezeFrames;
			this.alwaysBoost = alwaysBoost;
			this.longRange = longRange;
			this.ignoreSolids = ignoreSolids;

			// colliders set
			bool cl_basic_done, cl_player_done;
            ColliderList cl_basic = Utils.ColliderUtils.ParseColliderList(data.Attr("basicColliders", "r,12,10,-6,-5"), out cl_basic_done), 
				cl_player = Utils.ColliderUtils.ParseColliderList(data.Attr("playerColliders", "r,14,12,-7,-7"), out cl_player_done);


            Collider = cl_basic_done? cl_basic : new Hitbox(12f, 10f, -6f, -5f);
			Add(explosionRange = GFX.SpriteBank.Create(data.Attr("rangeIndicator", "bombRange")));
			Add(new PlayerCollider(OnPlayer, cl_player_done? cl_player : new Hitbox(14f, 12f, -7f, -7f)));
			Add(sprite = GFX.SpriteBank.Create(data.Attr("sprite", "pufferBomb")));
			sprite.Play("idle");
			anchorPosition = Position;
			state = States.Idle;
			startPosition = lastSinePosition = lastSpeedPosition = Position;
			pushRadius = new Hitbox(longRange ? 120f : 60f, 60f, longRange ? -60f : -30f, -30f);
			// modified detectRadius
            detectRadius = data.Attr("detectCollider", "60,30,-30,0").ParseRectangleCollider(new Hitbox(60f, 30f, -30f, 0f));
			// modified WallBreak
			breakWallsRadius = data.Attr("wallbreakCollider", "16,0,0").ParseCircle(new Circle(16f));

			onCollideV = OnCollideV;
			onCollideH = OnCollideH;
			scale = Vector2.One;
			bounceWiggler = Wiggler.Create(0.6f, 2.5f, delegate (float v)
			{
				sprite.Rotation = v * 20f * ((float)Math.PI / 180f);
			});
			Add(bounceWiggler);
			inflateWiggler = Wiggler.Create(0.6f, 2f);
			Add(inflateWiggler);
		}

		public PufferBomb(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("oneUse"), data.Bool("moreFreezeFrames"), 
				  data.Bool("alwaysBoost", true), data.Bool("longRange"), data.Bool("ignoreSolids"), data)
		{

		}

		private static MethodInfo springBounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.Instance | BindingFlags.NonPublic);

		public static void Load()
		{
			On.Celeste.Spring.ctor_Vector2_Orientations_bool += springOrientHook;
		}

		public static void Unload()
		{
			On.Celeste.Spring.ctor_Vector2_Orientations_bool -= springOrientHook;
		}

		private static void springOrientHook(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
		{
			orig(self, position, orientation, playerCanUse);
			var collider = new PufferBombCollider((p) => {
				if (p.HitSpring(self))
				{
					springBounceAnimate.Invoke(self, new object[] { });
				}
			});

			switch (self.Orientation)
			{
				case Spring.Orientations.Floor:
					collider.Collider = new Hitbox(16f, 10f, -8f, -10f); break;
				case Spring.Orientations.WallLeft:
					collider.Collider = new Hitbox(12f, 16f, 0f, -8f); break;
				case Spring.Orientations.WallRight:
					collider.Collider = new Hitbox(12f, 16f, -12f, -8f); break;
			}

			self.Add(collider);
		}

		public override bool IsRiding(JumpThru jumpThru)
		{
			return false;
		}

		public override bool IsRiding(Solid solid)
		{
			return false;
		}

		public override void OnSquish(CollisionData data)
		{
			if (!exploding)
			{
				Explode();
				GotoGone();
				exploding = true;
			}
		}

		private void OnCollideH(CollisionData data)
		{
			hitSpeed.X *= -0.8f;
		}

		private void OnCollideV(CollisionData data)
		{
			if (!(data.Direction.Y > 0f))
			{
				return;
			}
			for (int i = -1; i <= 1; i += 2)
			{
				for (int j = 1; j <= 2; j++)
				{
					Vector2 vector = Position + Vector2.UnitX * j * i;
					if (!CollideCheck<Solid>(vector) && !OnGround(vector))
					{
						Position = vector;
						return;
					}
				}
			}
			hitSpeed.Y *= -0.2f;
		}

		private void GotoIdle()
		{
			if (state == States.Gone)
			{
				Position = startPosition;
				cantExplodeTimer = 0.5f;
				//sprite.Play("recover");
				explosionRange.Play("on");
				Audio.Play("event:/new_content/game/10_farewell/puffer_reform", Position);
				exploding = false;
			}
			lastSinePosition = lastSpeedPosition = anchorPosition = Position;
			hitSpeed = Vector2.Zero;
			state = States.Idle;
		}

		private void GotoHit(Vector2 from)
		{
			scale = new Vector2(1.2f, 0.8f);
			hitSpeed = Vector2.UnitY * 200f;
			state = States.Hit;
			bounceWiggler.Start();
			Audio.Play("event:/new_content/game/10_farewell/puffer_boop", Position);
		}

		private void GotoHitSpeed(Vector2 speed)
		{
			hitSpeed = speed;
			state = States.Hit;
		}

		private void GotoGone()
		{
			Vector2 control = Position + (startPosition - Position) * 0.5f;
			if ((startPosition - Position).LengthSquared() > 100f)
			{
				if (Math.Abs(Position.Y - startPosition.Y) > Math.Abs(Position.X - startPosition.X))
				{
					if (Position.X > startPosition.X)
					{
						control += Vector2.UnitX * -24f;
					}
					else
					{
						control += Vector2.UnitX * 24f;
					}
				}
				else if (Position.Y > startPosition.Y)
				{
					control += Vector2.UnitY * -24f;
				}
				else
				{
					control += Vector2.UnitY * 24f;
				}
			}
			returnCurve = new SimpleCurve(Position, startPosition, control);
			Collidable = false;
			goneTimer = 2.5f;
			state = States.Gone;
		}

		private void Explode()
		{
			Collider collider = Collider;
            Collider = pushRadius;
			Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
			sprite.Play("explode");
			explosionRange.Play("off");
			Player player = CollideFirst<Player>();
			if (player != null && (ignoreSolids || !Scene.CollideCheck<Solid>(Position, player.Center)))
			{
				if (player.Speed.Y < 0f && Position.X + Collider.Left < player.X && player.X < Position.X + Collider.Right && !longRange)
				{
					//if (moreFreezeFrames)
					//{
					//	Celeste.Freeze(0.1f);
					//}
					player.ExplodeLaunch(new Vector2(player.X, Center.Y), true, false);
				}
				else
				{
					//if (moreFreezeFrames)
					//{
					//	Celeste.Freeze(0.1f);
					//}
					player.ExplodeLaunch(new Vector2(Center.X, player.Y), true, false);
					if (alwaysBoost && Math.Abs(player.Speed.X) < 300f)
					{
						// force fish boost
						player.Speed.X *= 1.2f;
					}
				}
			}
			TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
			if (theoCrystal != null && !Scene.CollideCheck<Solid>(Position, theoCrystal.Center))
			{
				theoCrystal.ExplodeLaunch(Position);
			}
			foreach (TempleCrackedBlock entity in Scene.Tracker.GetEntities<TempleCrackedBlock>())
			{
				if (CollideCheck(entity))
				{
					entity.Break(Position);
				}
			}
			foreach (TouchSwitch entity2 in Scene.Tracker.GetEntities<TouchSwitch>())
			{
				if (CollideCheck(entity2))
				{
					entity2.TurnOn();
				}
			}
			foreach (FloatingDebris entity3 in Scene.Tracker.GetEntities<FloatingDebris>())
			{
				if (CollideCheck(entity3))
				{
					entity3.OnExplode(Position);
				}
			}
			foreach (ShiftingSwitch entity4 in Scene.Tracker.GetEntities<ShiftingSwitch>())
			{
				if (CollideCheck(entity4))
				{
					entity4.ActivateAll();
				}
			}
            Collider = collider;
			Level level = SceneAs<Level>();
			level.Shake();
			level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
			level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
			level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
			for (float num = 0f; num < (float)Math.PI * 2f; num += 0.17453292f)
			{
				Vector2 position = Center + Calc.AngleToVector(num + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 18));
				level.Particles.Emit(P_Explosion, position, num);
			}
			if (player != null)
			{
				DynData<Player> playerData = new DynData<Player>(player);
				playerData.Set("dashCooldownTimer", 0f);
			}
		}

		public override void Render()
		{
			sprite.Scale = scale * (1f + inflateWiggler.Value * 0.4f);
			sprite.Scale *= facing;
			bool flag = false;
			if (sprite.CurrentAnimationID != "hidden" && sprite.CurrentAnimationID != "explode" && sprite.CurrentAnimationID != "recover")
			{
				flag = true;
			}
			else if (sprite.CurrentAnimationID == "explode" && sprite.CurrentAnimationFrame <= 1)
			{
				flag = true;
			}
			else if (sprite.CurrentAnimationID == "recover" && sprite.CurrentAnimationFrame >= 4)
			{
				flag = true;
			}
			if (flag)
			{
				sprite.DrawSimpleOutline();
			}
			//float num = playerAliveFade * Calc.ClampedMap((Position - lastPlayerPos).Length(), 128f, 96f);
			//if (num > 0f && state != States.Gone)
			//{
			//	bool flag2 = false;
			//	Vector2 vector = lastPlayerPos;
			//	if (vector.Y < base.Y)
			//	{
			//		vector.Y = base.Y - (vector.Y - base.Y) * 0.5f;
			//		vector.X += vector.X - base.X;
			//		flag2 = true;
			//	}
			//	float radiansB = (vector - Position).Angle();
			//	for (int i = 0; i < 28; i++)
			//	{
			//		float num2 = (float)Math.Sin(base.Scene.TimeActive * 0.5f) * 0.02f;
			//		float num3 = Calc.Map((float)i / 28f + num2, 0f, 1f, -(float)Math.PI / 30f, 3.24631262f);
			//		num3 += bounceWiggler.Value * 20f * ((float)Math.PI / 180f);
			//		Vector2 vector2 = Calc.AngleToVector(num3, 1f);
			//		Vector2 vector3 = Position + vector2 * 32f;
			//		float t = Calc.ClampedMap(Calc.AbsAngleDiff(num3, radiansB), (float)Math.PI / 2f, 0.17453292f);
			//		t = Ease.CubeOut(t) * 0.8f * num;
			//		if (!(t > 0f))
			//		{
			//			continue;
			//		}
			//		if (i == 0 || i == 27)
			//		{
			//			Draw.Line(vector3, vector3 - vector2 * 10f, Color.White * t);
			//			continue;
			//		}
			//		Vector2 vector4 = vector2 * (float)Math.Sin(base.Scene.TimeActive * 2f + (float)i * 0.6f);
			//		if (i % 2 == 0)
			//		{
			//			vector4 *= -1f;
			//		}
			//		vector3 += vector4;
			//		if (!flag2 && Calc.AbsAngleDiff(num3, radiansB)F <= 0.17453292f)
			//		{
			//			Draw.Line(vector3, vector3 - vector2 * 3f, Color.White * t);
			//		}
			//		else
			//		{
			//			Draw.Point(vector3, Color.White * t);
			//		}
			//	}
			//}
			base.Render();
			if (sprite.CurrentAnimationID == "alerted")
			{
				Vector2 vector5 = Position + new Vector2(3f, facing.X < 0f ? -5 : -4) * sprite.Scale;
				Vector2 to = lastPlayerPos + new Vector2(0f, -4f);
				Vector2 vector6 = Calc.AngleToVector(Calc.Angle(vector5, to) + eyeSpin * ((float)Math.PI * 2f) * 2f, 1f);
				Vector2 vector7 = vector5 + new Vector2((float)Math.Round(vector6.X), (float)Math.Round(Calc.ClampedMap(vector6.Y, -1f, 1f, -1f, 2f)));
				Draw.Rect(vector7.X, vector7.Y, 1f, 1f, Color.Black);
			}
			sprite.Scale /= facing;
		}


		public override void Update()
		{
			base.Update();
			eyeSpin = Calc.Approach(eyeSpin, 0f, Engine.DeltaTime * 1.5f);
			scale = Calc.Approach(scale, Vector2.One, 1f * Engine.DeltaTime);
			//rotation += 3*Engine.DeltaTime;
			//rotation %= (float)Math.PI * 2f;
			//sprite.Rotation = rotation;
			if (cannotHitTimer > 0f)
			{
				cannotHitTimer -= Engine.DeltaTime;
			}
			if (state != States.Gone && cantExplodeTimer > 0f)
			{
				cantExplodeTimer -= Engine.DeltaTime;
			}
			if (alertTimer > 0f)
			{
				alertTimer -= Engine.DeltaTime;
			}
			Player entity = Scene.Tracker.GetEntity<Player>();
			if (entity == null)
			{
				playerAliveFade = Calc.Approach(playerAliveFade, 0f, 1f * Engine.DeltaTime);
			}
			else
			{
				playerAliveFade = Calc.Approach(playerAliveFade, 1f, 1f * Engine.DeltaTime);
				lastPlayerPos = entity.Center;
			}
			switch (state)
			{
				case States.Idle:
					{
						if (Position != lastSinePosition)
						{
							anchorPosition += Position - lastSinePosition;
						}
						Vector2 vector = anchorPosition;
						MoveToX(vector.X);
						MoveToY(vector.Y);
						lastSinePosition = Position;
						if (ProximityExplodeCheck())
						{
							Explode();
							GotoGone();
							break;
						}
						{
							foreach (PufferBombCollider component in Scene.Tracker.GetComponents<PufferBombCollider>())
							{
								component.Check(this);
							}
							break;
						}
					}
				case States.Hit:
					lastSpeedPosition = Position;
					MoveH(hitSpeed.X * Engine.DeltaTime, onCollideH);
					MoveV(hitSpeed.Y * Engine.DeltaTime, OnCollideV);
					anchorPosition = Position;
					hitSpeed.X = Calc.Approach(hitSpeed.X, 0f, 150f * Engine.DeltaTime);
					hitSpeed = Calc.Approach(hitSpeed, Vector2.Zero, 320f * Engine.DeltaTime);
					if (ProximityExplodeCheck())
					{
						Explode();
						GotoGone();
						break;
					}
					if (Top >= SceneAs<Level>().Bounds.Bottom + 5)
					{
						//sprite.Play("hidden");
						GotoGone();
						break;
					}
					foreach (PufferBombCollider component2 in Scene.Tracker.GetComponents<PufferBombCollider>())
					{
						component2.Check(this);
					}
					if (hitSpeed == Vector2.Zero)
					{
						ZeroRemainderX();
						ZeroRemainderY();
						GotoIdle();
					}
					break;
				case States.Gone:
					{
						float num = goneTimer;
						goneTimer -= Engine.DeltaTime;
						if (goneTimer <= 0.5f)
						{
							if (oneUse)
							{
								RemoveSelf();
								break;
							}
							if (num > 0.5f && returnCurve.GetLengthParametric(8) > 8f)
							{
								Audio.Play("event:/new_content/game/10_farewell/puffer_return", Position);
							}
							Position = returnCurve.GetPoint(Ease.CubeInOut(Calc.ClampedMap(goneTimer, 0.5f, 0f)));
						}
						if (goneTimer <= 0f)
						{
							Visible = Collidable = true;
							GotoIdle();
						}
						break;
					}
			}
		}

		public bool HitSpring(Spring spring)
		{
			if (spring is PufferBombSpring customSpring && customSpring.Orientation == PufferBombSpring.CustomOrientations.Ceiling)
			{
				if (hitSpeed.Y <= 0f)
				{
					GotoHitSpeed(224f * Vector2.UnitY);
					MoveTowardsX(spring.CenterX, 4f, null);
					bounceWiggler.Start();
					return true;
				}
				return false;
			}
			else
			{
				switch (spring.Orientation)
				{
					case Spring.Orientations.Floor:
						if (hitSpeed.Y >= 0f)
						{
							GotoHitSpeed(224f * -Vector2.UnitY);
							MoveTowardsX(spring.CenterX, 4f);
							bounceWiggler.Start();
							return true;
						}
						return false;
					case Spring.Orientations.WallLeft:
						if (hitSpeed.X <= 60f)
						{
							facing.X = 1f;
							GotoHitSpeed(280f * Vector2.UnitX);
							MoveTowardsY(spring.CenterY, 4f);
							bounceWiggler.Start();
							return true;
						}
						return false;
					case Spring.Orientations.WallRight:
						if (hitSpeed.X >= -60f)
						{
							facing.X = -1f;
							GotoHitSpeed(280f * -Vector2.UnitX);
							MoveTowardsY(spring.CenterY, 4f);
							bounceWiggler.Start();
							return true;
						}
						return false;
					default:
						if (hitSpeed.Y <= 0f)
						{
							GotoHitSpeed(224f * Vector2.UnitY);
							MoveTowardsX(spring.CenterX, 4f);
							bounceWiggler.Start();
							return true;
						}
						return false;
				}
			}
		}

		private bool ProximityExplodeCheck()
		{
			if (cantExplodeTimer > 0f)
			{
				return false;
			}
			bool result = false;
			Collider collider = Collider;
            Collider = detectRadius;
			Player player;
			if ((player = CollideFirst<Player>()) != null && player.CenterY >= Y + collider.Bottom - 4f && (ignoreSolids || !Scene.CollideCheck<Solid>(Position, player.Center)))
			{
				result = true;
			}
            Collider = collider;
			return result;
		}

		private void OnPlayer(Player player)
		{
			if (state == States.Gone || !(cantExplodeTimer <= 0f))
			{
				return;
			}
			if (cannotHitTimer <= 0f)
			{
				if (player.Bottom > lastSpeedPosition.Y + 3f)
				{
					Explode();
					GotoGone();
				}
				else
				{
					player.Bounce(Top);
					GotoHit(player.Center);
					MoveToX(anchorPosition.X);
					anchorPosition = lastSinePosition = Position;
					eyeSpin = 1f;
				}
			}
			cannotHitTimer = 0.1f;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (Depth == 0 && ((AreaKey)(object)(scene as Level).Session.Area).LevelSet != "Celeste")
			{
                Depth = -1;
			}
		}
	}
}

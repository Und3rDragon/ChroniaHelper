using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Monocle;
using FMOD.Studio;

// The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
// The original author is ricky06, code modified by UnderDragon

namespace ChroniaHelper.Entities.MigratedNeonHelper
{
	[Tracked]
	[CustomEntity("ChroniaHelper/ShiftingBlock")]
	public class ShiftingBlock : Solid
	{
		public bool noConnector;

		private class Movement
		{
			public Vector2 Offset { get; private set; }
			public float Speed { get; private set; }

			public bool Silent { get; private set; }

			public Movement(Vector2 offset, float speed, bool silent=false)
			{
				Offset = offset;
				Speed = speed;
				Silent = silent;
			}
		}
		private char tileType;
		private float width;
		private float height;
		public BloomPoint bloomPoint;
		public TileGrid tileGrid;
		private Queue<Movement> movementQueue;

		private Coroutine moveRoutine;
		private float shakeTime;
		private bool isMoving;
		private bool linear, isElevator, easeInOnly;
		private Color centerColor;

		private Image centerNode;

		private SoundSource moveSfx;
		private float mult;

		public ShiftingBlock(Vector2 position, char tiletype, float shakeTime, float width, float height, bool linear, bool isElevator, bool noConnector, bool easeInOnly)
			: base(position, width, height, safe: false)
		{
			Depth = -12999;

			this.width = width;
			this.height = height;
			this.shakeTime = shakeTime;
			this.linear = linear;
			this.isElevator = isElevator;
			this.noConnector = noConnector;
			this.easeInOnly = easeInOnly;

			tileType = tiletype;
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
			movementQueue = new Queue<Movement>();
			Add(moveSfx = new SoundSource());

			if (shakeTime > 0)
			{
				mult = Calc.Clamp(1f / shakeTime, 20f, 3f);
			}
			else
			{
				mult = 20f;
			}
		}

		public ShiftingBlock(EntityData data, Vector2 offset, EntityID id)
			: this(data.Position + offset, data.Char("tiletype", '3'), data.Float("shakeTime", 0.4f), data.Width, data.Height, data.Bool("linear"), data.Bool("isElevator"), data.Bool("noConnector"), data.Bool("easeInOnly"))
		{
		}

		public void MoveBlocks(Vector2 offset, float speed, bool silent)
		{
			if (isMoving)
			{
				movementQueue.Enqueue(new Movement(offset, speed, silent));
			}
			else
			{
				Add(moveRoutine = new Coroutine(move(offset, speed, silent)));
			}
		}

		public override void OnShake(Vector2 amount)
		{
			base.OnShake(amount);
			tileGrid.Position += amount;
		}

		private IEnumerator move(Vector2 offset, float moveTime, bool silent)
		{
			isMoving = true;
			Vector2 begin = Position;
			StartShaking();
			if (!silent)
			{
				Audio.Play("event:/game/general/fallblock_shake");
				moveSfx.Play("event:/ricky06/NeonCity/humLoop2D");
			}
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			yield return shakeTime;
			StopShaking();
			float at = 0f;
			//moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return"); 
			while (true)
			{
				Vector2 newPos = Vector2.Lerp(begin, begin + offset, Ease.QuadInOut(at));
				if (linear)
				{
					newPos = Vector2.Lerp(begin, begin + offset, Ease.Linear(at));
				}
				if(isElevator)
                {
					newPos = Vector2.Lerp(begin, begin + offset, Ease.SineOut(at));
				}
				if(easeInOnly)
                {
					newPos = Vector2.Lerp(begin, begin + offset, Ease.QuadIn(at));
				}
				MoveToX(newPos.X);
				MoveToY(newPos.Y);

				if (at >= 1f)
				{
					if(isElevator)
                    {
						StartShaking();

						Audio.Play("event:/game/general/fallblock_shake", Center);
						
						Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						yield return shakeTime;
						StopShaking();
					}
					break;
				}
				yield return null;
				at = MathHelper.Clamp(at + Engine.DeltaTime / moveTime, 0f, 1f);
			}
			//Audio.SetParameter(moveSfx, "end", 1f);
			//Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", base.Center);
			moveSfx.Stop();
			if (movementQueue.Count > 0)
			{
				Movement m = movementQueue.Dequeue();
				Add(moveRoutine = new Coroutine(move(m.Offset, m.Speed, m.Silent)));
			}
			else
			{
				isMoving = false;
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);

			tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)width / 8, (int)height / 8).TileGrid;
			tileGrid.Color = Calc.HexToColor("7fa9ad");
			//Add(new LightOcclude());
			Add(bloomPoint = new BloomPoint(new Vector2(width/2f, height/2f), 0f, Math.Min(width, height)));
			bloomPoint.Alpha = 0f;

			Add(tileGrid);
			Add(new TileInterceptor(tileGrid, highPriority: true));
			if (!noConnector)
			{
				Add(centerNode = new Image(GFX.Game["objects/NeonCity/shiftingSwitch/center"]));
				centerNode.CenterOrigin();
				centerNode.Position = new Vector2(width / 2, height / 2);
			}
			centerColor = Calc.HexToColor("5b7e82");
			if (centerNode != null)
			{
				centerNode.SetColor(centerColor);
			}

			foreach (StaticMover component in scene.Tracker.GetComponents<StaticMover>())
			{
				if (component.IsRiding(this))
				{
					component.Entity.Depth = Depth + 1;
				}
			}
		}

        public override void Update()
        {
            base.Update();
			if(isMoving)
            {
				tileGrid.Color = Color.Lerp(tileGrid.Color, Color.White, Engine.DeltaTime * mult);
				bloomPoint.Alpha = Calc.LerpClamp(bloomPoint.Alpha, 1f, Engine.DeltaTime * mult);
				centerColor = Color.Lerp(centerColor, Color.White, Engine.DeltaTime * mult);
            }
			else
            {
				tileGrid.Color = Color.Lerp(tileGrid.Color, Calc.HexToColor("7fa9ad"), Engine.DeltaTime * mult);
				centerColor = Color.Lerp(centerColor, Calc.HexToColor("5b7e82"), Engine.DeltaTime * mult);
				bloomPoint.Alpha = Calc.LerpClamp(bloomPoint.Alpha, 0f, Engine.DeltaTime * mult);
			}

			if(centerNode != null)
            {
				centerNode.SetColor(centerColor);
            }
		}

        //      public override void Removed(Scene scene)
        //      {
        //          base.Removed(scene);
        //	Audio.SetParameter(moveSfx, "end", 1f);
        //}
    }

}

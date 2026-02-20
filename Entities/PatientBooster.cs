using System;
using System.Collections;
using System.Data.Common;
using System.Reflection;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/PatientBooster")]
public class PatientBooster : Booster
{
	public static Booster TempCurrentBooster = null;

	private float respawnDelay;

	private Vector2? lastSpritePos;

	public Sprite Sprite => sprite;

	public PatientBooster(EntityData data, Vector2 offset)
		: base(data.Position + offset, data.Bool("red"))
	{
		// parsing hitbox data
		string[] hitboxData = data.Attr("customHitbox").Split(';',StringSplitOptions.TrimEntries);
        //对于每组数据
        ColliderList CL = new ColliderList();
		bool emptyCollider = true;
        for (int i = 0; i < hitboxData.Length; i++)
        {
            //首先分割并去空
            string[] hb = hitboxData[i].Split(",", StringSplitOptions.TrimEntries);
            //淘汰length为0
            if (hb.Length == 0) { break; }
            //length !=0 , 检查第一位
            if (hb[0] == "" || (hb[0] != "c" && hb[0] != "r")) { break; }
            //第一位稳定，开始记录
            float p1 = 0f, p2 = 0f, p3 = 0f, p4 = 0f;
            if (hb.Length >= 2)
            {
                p1 = hb[1].ParseFloat(0f);
            }
            if (hb.Length >= 3)
            {
                p2 = hb[2].ParseFloat(0f);
            }
            if (hb.Length >= 4)
            {
                p3 = hb[3].ParseFloat(0f);
            }
            if (hb.Length >= 5)
            {
                p4 = hb[4].ParseFloat(0f);
            }
            if (hb[0] == "r")
            {
                if (p1 <= 0) { break; }
                if (p2 <= 0) { break; }
                CL.Add(new Hitbox(p1, p2, p3, p4));
				emptyCollider = false;
            }
            else
            {
                if (p1 <= 0) { break; }
                CL.Add(new Circle(p1, p2, p3));
				emptyCollider = false;
            }
        }
		if (!emptyCollider)
		{
            base.Collider = CL;
        }
        
		respawnDelay = data.Float("respawnDelay", 1f);

		// New dashes and stamina setups
		dashes = data.Int("dashes", 1);
		stamina = data.Int("stamina", 110);
		staminaMode = (StaminaRefill)data.Int("staminaMode", 0);
        dashesMode = (DashRefill)data.Int("dashesMode", 0);
		// Old data
        string old_dashes = data.Attr("refillDashes"),
			old_stamina = data.Attr("refillStamina");
		if (!string.IsNullOrEmpty(old_dashes))
		{
			int d = 1;
            int.TryParse(old_dashes, out d);
			dashes = d;
        }
		if (!string.IsNullOrEmpty(old_stamina))
		{
			if (data.Bool("refillStamina", true)) { staminaMode = 0; }
		}

		var spriteName = data.Attr("sprite", "");
		var red = data.Bool("red");

		killTimer = data.Float("killIfStayed", -1f);
        
        freeMoveSpeed = data.Float("freeMoveSpeed", -1f);

		// out speed and boost speed from Custom Booster
		outSpeed = data.Float("outSpeedMultiplier", 1f);
		greenSpeed = data.Float("greenBoostMovingSpeed", 240f);
        redSpeed = data.Float("redBoostMovingSpeed", 240f);

        customBurstParticleType = new ParticleType(red ? P_BurstRed : P_Burst);
		if (data.Attr("burstParticleColor").IsNotNullOrEmpty())
		{
            customBurstParticleType.Color = data.GetChroniaColor("burstParticleColor", Color.White).Parsed();
        }
        customAppearParticleType = new ParticleType(red ? P_RedAppear : P_Appear);
		if (data.Attr("appearParticleColor").IsNotNullOrEmpty())
		{
            customAppearParticleType.Color = data.GetChroniaColor("appearParticleColor", Color.White).Parsed();
        }

        Remove(sprite);
		Add(sprite = GFX.SpriteBank.Create(!string.IsNullOrEmpty(spriteName) ? spriteName : (killTimer > 0f ? "Preset_yellow" : (red ? "Preset_red" : "Preset_green"))));
	}
	private int dashes, stamina;
	private enum DashRefill { refill, set, delta };
	private DashRefill dashesMode;
	private enum StaminaRefill { refill, set, delta };
	private StaminaRefill staminaMode;
	private float killTimer = -1f, timer_killTimer = -1f;
	private bool timerRunning = false;

	private float freeMoveSpeed = -1f;
    private Vc2 freeMoveOffset = Vc2.Zero;

	private float outSpeed, redSpeed, greenSpeed;
    private ParticleType customBurstParticleType, customAppearParticleType;

    public override void Added(Scene scene)
    {
        base.Added(scene);
		timer_killTimer = killTimer;
    }
	public override void Update()
	{
		base.Update();
		var player = Scene.Tracker.GetEntity<Player>();
        
		if ((player?.CollideCheck(this) ?? false) && respawnTimer <= 0f)
		{
			timerRunning = true;
		}
		if (timerRunning)
		{
            if (timer_killTimer > 0f)
            {
                timer_killTimer = Calc.Approach(timer_killTimer, 0f, Engine.DeltaTime);
                Sprite.Color = timer_killTimer.LerpValue(killTimer, 0f, Color.White, Color.Red);
            }
            if (timer_killTimer == 0f)
            {
                player?.Die(player?.Speed.SafeNormalize() ?? Vc2.Zero);
            }
        }
		if (player != null && player.CurrentBooster == this)
		{
			BoostingPlayer = true;
			player.boostTarget = Center;
			var targetPos = Center - player.Collider.Center + (Input.Aim.Value * 3f);
            if(freeMoveSpeed > 0f)
			{
				freeMoveOffset += freeMoveSpeed * Engine.DeltaTime * Input.Aim.Value;
            }
			targetPos += freeMoveOffset;
			this.sprite.Position = freeMoveOffset;
			player.MoveToX(targetPos.X);
			player.MoveToY(targetPos.Y);
		}
		var sprite = Sprite;
		if (sprite.CurrentAnimationID == "pop")
		{
			if (lastSpritePos == null)
			{
				lastSpritePos = sprite.RenderPosition;
			}

			sprite.RenderPosition = lastSpritePos.Value;
		}
		else
		{
			lastSpritePos = null;
		}
	}

	private static ILHook origBoostBeginHook;
    private static ILHook redDashCoroutineHook, greenDashCoroutineHook;

    [LoadHook]
    public static void Load()
	{
		On.Celeste.Player.Boost += Player_Boost;
		On.Celeste.Player.RedBoost += Player_RedBoost;
		On.Celeste.Player.BoostCoroutine += Player_BoostCoroutine;
		On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
		IL.Celeste.Player.BoostBegin += Player_BoostBegin;

		origBoostBeginHook = new ILHook(typeof(Player).GetMethod("orig_BoostBegin", BindingFlags.NonPublic | BindingFlags.Instance), Player_BoostBegin);

		On.Celeste.Booster.BoostRoutine += Booster_BoostRoutine;
        redDashCoroutineHook = new ILHook(typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), Player_RedDashHook);
        greenDashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), Player_GreenDashHook);

        On.Celeste.Booster.AppearParticles += Booster_AppearParticles;
    }
	[UnloadHook]
    public static void Unload()
	{
		On.Celeste.Player.Boost -= Player_Boost;
		On.Celeste.Player.RedBoost -= Player_RedBoost;
		On.Celeste.Player.BoostCoroutine -= Player_BoostCoroutine;
		On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
		IL.Celeste.Player.BoostBegin -= Player_BoostBegin;

		origBoostBeginHook?.Dispose();
		origBoostBeginHook = null;

        On.Celeste.Booster.BoostRoutine -= Booster_BoostRoutine;

        redDashCoroutineHook.Dispose();
        greenDashCoroutineHook.Dispose();

        On.Celeste.Booster.AppearParticles -= Booster_AppearParticles;
    }

	private static void Player_Boost(On.Celeste.Player.orig_Boost orig, Player self, Booster booster)
	{
		TempCurrentBooster = booster;
		orig(self, booster);
		TempCurrentBooster = null;
	}

	private static void Player_RedBoost(On.Celeste.Player.orig_RedBoost orig, Player self, Booster booster)
	{
		TempCurrentBooster = booster;
		orig(self, booster);
		TempCurrentBooster = null;
	}

	private static IEnumerator Player_BoostCoroutine(On.Celeste.Player.orig_BoostCoroutine orig, Player self)
	{
		if (self.CurrentBooster is PatientBooster)
		{
			yield break;
		}

		var orig_enum = orig(self);
		while (orig_enum.MoveNext())
		{
			yield return orig_enum.Current;
		}
	}

	private static void Booster_PlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
	{
		orig(self);

		if (self is PatientBooster patientBooster)
		{
			patientBooster.respawnTimer = patientBooster.respawnDelay;
			patientBooster.timer_killTimer = patientBooster.killTimer;
            
            patientBooster.freeMoveOffset = Vc2.Zero;
            patientBooster.timerRunning = false;
            patientBooster.timer_killTimer = patientBooster.killTimer;
            patientBooster.Sprite.Color = Color.White;
        }
	}

	private static void Player_BoostBegin(ILContext il)
	{
		var cursor = new ILCursor(il);

		if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("RefillStamina")))
		{
			Log.Error($"Failed IL Hook for Booster.BoostBegin (RefillStamina)");
			return;
		}

		var afterRefillsLabel = cursor.MarkLabel();

		if (!cursor.TryGotoPrev(MoveType.AfterLabel, instr => instr.MatchCallvirt<Player>("RefillDash")))
		{
			Logger.Log(LogLevel.Error, "ChroniaHelper", $"Failed IL Hook for Booster.BoostBegin (RefillDash)");
			return;
		}

		var continueLabel = cursor.DefineLabel();

		cursor.Emit(OpCodes.Ldarg_0);
		cursor.EmitDelegate<Func<Player, bool>>(player =>
		{
			if (TempCurrentBooster is PatientBooster booster)
			{
				// Insert Stamina and Dashes logic here
				if(booster.staminaMode == PatientBooster.StaminaRefill.delta)
				{
					player.Stamina += booster.stamina;
					player.Stamina = player.Stamina.ClampMin(0f);
				}
				else if (booster.staminaMode == PatientBooster.StaminaRefill.set)
				{
					player.Stamina = booster.stamina;
				}
				else
				{
                    if (player.Stamina < booster.stamina)
                    {
                        player.Stamina = booster.stamina;
                    }
                }

				if (booster.dashesMode == PatientBooster.DashRefill.delta)
				{
					player.Dashes += booster.dashes;
					player.Dashes = player.Dashes.ClampMin(0);
				}
				else if (booster.dashesMode == PatientBooster.DashRefill.set)
				{
					player.Dashes = booster.dashes;
				}
				else
				{
                    if (player.Dashes < booster.dashes)
                    {
                        player.Dashes = booster.dashes;
                    }
                }
				
				return true;
			}
			return false;
		});
		cursor.Emit(OpCodes.Brfalse, continueLabel);
		cursor.Emit(OpCodes.Pop);
		cursor.Emit(OpCodes.Br, afterRefillsLabel);
		cursor.MarkLabel(continueLabel);
	}

    private static IEnumerator Booster_BoostRoutine(On.Celeste.Booster.orig_BoostRoutine orig, Booster self, Player player, Vector2 dir)
    {
        if (self is PatientBooster myBooster)
        {
            float angle = (-dir).Angle();
            // angle calculation, left is 0, right is PI, topright +, bottomright -
            while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && myBooster.BoostingPlayer)
            {
                myBooster.sprite.RenderPosition = player.Center + playerOffset;
                myBooster.loopingSfx.Position = myBooster.sprite.Position;
                if (myBooster.Scene.OnInterval(0.02f))
                {
                    ParticleType particleType = self is PatientBooster booster ? booster.customBurstParticleType : myBooster.particleType;
                    (myBooster.Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
                }

                yield return null;
            }

            myBooster.PlayerReleased();

            player.Speed *= myBooster.outSpeed;

            if (player.StateMachine.State == 4)
            {
                myBooster.sprite.Visible = false;
            }

            while (myBooster.SceneAs<Level>().Transitioning)
            {
                yield return null;
            }

            myBooster.Tag = 0;
        }
        else
        {
            yield return new SwapImmediately(orig(self, player, dir));
        }
    }

    private static void Player_RedDashHook(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        if (c.TryGotoNext(ins => ins.MatchLdcR4(240f)))
        {
            c.Index += 1;

            c.EmitDelegate<Func<float, float>>(fallback =>
            {
                if (PUt.TryGetPlayer(out var p))
                {
                    if (p.CurrentBooster is PatientBooster b)
                    {
                        return b.redSpeed;
                    }
                }
                return fallback;
            });
        }
    }

    private static void Player_GreenDashHook(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        if (c.TryGotoNext(ins => ins.MatchLdcR4(240f)))
        {
            c.Index += 1;

            c.EmitDelegate<Func<float, float>>(fallback =>
            {
                if (PUt.TryGetPlayer(out var p))
                {
                    if (p.CurrentBooster is PatientBooster b)
                    {
                        return b.greenSpeed;
                    }
                }
                return fallback;
            });
        }
    }

    public static void Booster_AppearParticles(On.Celeste.Booster.orig_AppearParticles orig, Booster self)
    {
        if (self is PatientBooster booster)
        {
            ParticleSystem particlesBG = MapProcessor.level.ParticlesBG;
            for (int i = 0; i < 360; i += 30)
            {
                particlesBG.Emit(booster.customAppearParticleType, 1, booster.Center, Vector2.One * 2f, (float)i * (MathF.PI / 180f));
            }
        }
        else
        {
            orig(self);
        }
    }
}

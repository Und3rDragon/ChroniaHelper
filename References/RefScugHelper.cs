global using SV = ChroniaHelper.References.RefScugHelper.SessionVariables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.References;

public static class RefScugHelper
{
    /// <summary>
    /// Session Variable names defined by ScugHelper
    /// </summary>
    public static class SessionVariables
    {
        public static class Flags
        {
            public const string PlayerDead = "ScugHelper.PlayerDead";
            public const string HasGolden = "ScugHelper.HasGolden";
            public const string RestartedFromGolden = "ScugHelper.RestartedFromGolden";
            public const string StartedFromBeginning = "ScugHelper.StartedFromBeginning";
            public const string PlayerOnGround = "ScugHelper.PlayerOnGround";
            public const string PlayerOnSafeGround = "ScugHelper.PlayerOnSafeGround";
            public const string PlayerDashAttacking = "ScugHelper.PlayerDashAttacking";
            public const string IsPlayerSeeker = "ScugHelper.IsPlayerSeeker";
            public const string DreamBlocksEnabled = "ScugHelper.DreamBlocksEnabled";
            public const string HasMidair = "ScugHelper.HasMidair";
            public const string HasOvercharge = "ScugHelper.HasOvercharge";
            public const string InLimbo = "ScugHelper.InLimbo";
            public const string SaveQuitDisabled = "ScugHelper.SaveQuitDisabled";
            public const string PlayerHolding = "ScugHelper.PlayerHolding";
            public const string PlayerDucking = "ScugHelper.PlayerDucking";
            public const string InBooster = "ScugHelper.InBooster";

            public static class FrostHelper
            {
                public const string Enabled = "ScugHelper.FrostHelper.Enabled";
            }

            public static class GravityHelper
            {
                public const string Enabled = "ScugHelper.GravityHelper.Enabled";
                public const string PlayerInverted = "ScugHelper.GravityHelper.PlayerInverted";
            }

            public static class ExtendedVariants
            {
                public const string Enabled = "ScugHelper.ExtendedVariantMode.Enabled";

                public const string DashAssist = "ScugHelper.ExtendedVariantMode.DashAssist";
                public const string Hiccups = "ScugHelper.ExtendedVariantMode.Hiccups";
                public const string InfiniteStamina = "ScugHelper.ExtendedVariantMode.InfiniteStamina";
                public const string Invincible = "ScugHelper.ExtendedVariantMode.Invincible";
                public const string InvisibleMotion = "ScugHelper.ExtendedVariantMode.InvisibleMotion";
                public const string LowFriction = "ScugHelper.ExtendedVariantMode.LowFriction";
                public const string MirrorMode = "ScugHelper.ExtendedVariantMode.MirrorMode";
                public const string NoGrabbing = "ScugHelper.ExtendedVariantMode.NoGrabbing";
                public const string PlayAsBadeline = "ScugHelper.ExtendedVariantMode.PlayAsBadeline";
                public const string SuperDashing = "ScugHelper.ExtendedVariantMode.SuperDashing";
                public const string ThreeSixtyDashing = "ScugHelper.ExtendedVariantMode.ThreeSixtyDashing";
                public const string AffectExistingChasers = "ScugHelper.ExtendedVariantMode.AffectExistingChasers";
                public const string AllStrawberriesAreGoldens = "ScugHelper.ExtendedVariantMode.AllStrawberriesAreGoldens";
                public const string AllowLeavingTheoBehind = "ScugHelper.ExtendedVariantMode.AllowLeavingTheoBehind";
                public const string AllowThrowingTheoOffscreen = "ScugHelper.ExtendedVariantMode.AllowThrowingTheoOffscreen";
                public const string AlternativeBuffering = "ScugHelper.ExtendedVariantMode.AlternativeBuffering";
                public const string AlwaysFeather = "ScugHelper.ExtendedVariantMode.AlwaysFeather";
                public const string AlwaysInvisible = "ScugHelper.ExtendedVariantMode.AlwaysInvisible";
                public const string AutoDash = "ScugHelper.ExtendedVariantMode.AutoDash";
                public const string AutoJump = "ScugHelper.ExtendedVariantMode.AutoJump";
                public const string BadelineBossesEverywhere = "ScugHelper.ExtendedVariantMode.BadelineBossesEverywhere";
                public const string BadelineChasersEverywhere = "ScugHelper.ExtendedVariantMode.BadelineChasersEverywhere";
                public const string BounceEverywhere = "ScugHelper.ExtendedVariantMode.BounceEverywhere";
                public const string BufferableGrab = "ScugHelper.ExtendedVariantMode.BufferableGrab";
                public const string ChangePatternsOfExistingBosses = "ScugHelper.ExtendedVariantMode.ChangePatternsOfExistingBosses";
                public const string ConsistentThrowing = "ScugHelper.ExtendedVariantMode.ConsistentThrowing";
                public const string CornerboostProtection = "ScugHelper.ExtendedVariantMode.CornerboostProtection";
                public const string CorrectedMirrorMode = "ScugHelper.ExtendedVariantMode.CorrectedMirrorMode";
                public const string CrouchDashFix = "ScugHelper.ExtendedVariantMode.CrouchDashFix";
                public const string DashBeforePickup = "ScugHelper.ExtendedVariantMode.DashBeforePickup";
                public const string DashTrailAllTheTime = "ScugHelper.ExtendedVariantMode.DashTrailAllTheTime";
                public const string DisableClimbJumping = "ScugHelper.ExtendedVariantMode.DisableClimbJumping";
                public const string DisableDashCooldown = "ScugHelper.ExtendedVariantMode.DisableDashCooldown";
                public const string DisableAutoJumpGravityLowering = "ScugHelper.ExtendedVariantMode.DisableAutoJumpGravityLowering";
                public const string DisableJumpGravityLowering = "ScugHelper.ExtendedVariantMode.DisableJumpGravityLowering";
                public const string DisableJumpingOutOfWater = "ScugHelper.ExtendedVariantMode.DisableJumpingOutOfWater";
                public const string DisableKeysSpotlight = "ScugHelper.ExtendedVariantMode.DisableKeysSpotlight";
                public const string DisableMadelineSpotlight = "ScugHelper.ExtendedVariantMode.DisableMadelineSpotlight";
                public const string DisableNeutralJumping = "ScugHelper.ExtendedVariantMode.DisableNeutralJumping";
                public const string DisableOshiroSlowdown = "ScugHelper.ExtendedVariantMode.DisableOshiroSlowdown";
                public const string DisableRefillsOnScreenTransition = "ScugHelper.ExtendedVariantMode.DisableRefillsOnScreenTransition";
                public const string DisableSeekerSlowdown = "ScugHelper.ExtendedVariantMode.DisableSeekerSlowdown";
                public const string DisableSuperBoosts = "ScugHelper.ExtendedVariantMode.DisableSuperBoosts";
                public const string DisableWallJumping = "ScugHelper.ExtendedVariantMode.DisableWallJumping";
                public const string DisplayDashCount = "ScugHelper.ExtendedVariantMode.DisplayDashCount";
                public const string DontRefillStaminaOnGround = "ScugHelper.ExtendedVariantMode.DontRefillStaminaOnGround";
                public const string EveryJumpIsUltra = "ScugHelper.ExtendedVariantMode.EveryJumpIsUltra";
                public const string EverythingIsUnderwater = "ScugHelper.ExtendedVariantMode.EverythingIsUnderwater";
                public const string FirstBadelineSpawnRandom = "ScugHelper.ExtendedVariantMode.FirstBadelineSpawnRandom";
                public const string ForceDuckOnGround = "ScugHelper.ExtendedVariantMode.ForceDuckOnGround";
                public const string FriendlyBadelineFollower = "ScugHelper.ExtendedVariantMode.FriendlyBadelineFollower";
                public const string HeldDash = "ScugHelper.ExtendedVariantMode.HeldDash";
                public const string InvertDashes = "ScugHelper.ExtendedVariantMode.InvertDashes";
                public const string InvertGrab = "ScugHelper.ExtendedVariantMode.InvertGrab";
                public const string InvertHorizontalControls = "ScugHelper.ExtendedVariantMode.InvertHorizontalControls";
                public const string InvertVerticalControls = "ScugHelper.ExtendedVariantMode.InvertVerticalControls";
                public const string LegacyDashSpeedBehavior = "ScugHelper.ExtendedVariantMode.LegacyDashSpeedBehavior";
                public const string LiftboostProtection = "ScugHelper.ExtendedVariantMode.LiftboostProtection";
                public const string MidairTech = "ScugHelper.ExtendedVariantMode.MidairTech";
                public const string NoFreezeFrames = "ScugHelper.ExtendedVariantMode.NoFreezeFrames";
                public const string NoFreezeFramesAdvanceCassetteBlocks = "ScugHelper.ExtendedVariantMode.NoFreezeFramesAdvanceCassetteBlocks";
                public const string OshiroEverywhere = "ScugHelper.ExtendedVariantMode.OshiroEverywhere";
                public const string PermanentBinoStorage = "ScugHelper.ExtendedVariantMode.PermanentBinoStorage";
                public const string PermanentDashAttack = "ScugHelper.ExtendedVariantMode.PermanentDashAttack";
                public const string PreserveExtraDashesUnderwater = "ScugHelper.ExtendedVariantMode.PreserveExtraDashesUnderwater";
                public const string PreserveWallbounceSpeed = "ScugHelper.ExtendedVariantMode.PreserveWallbounceSpeed";
                public const string RefillJumpsOnDashRefill = "ScugHelper.ExtendedVariantMode.RefillJumpsOnDashRefill";
                public const string ResetJumpCountOnGround = "ScugHelper.ExtendedVariantMode.ResetJumpCountOnGround";
                public const string RestoreDashesOnRespawn = "ScugHelper.ExtendedVariantMode.RestoreDashesOnRespawn";
                public const string RisingLavaEverywhere = "ScugHelper.ExtendedVariantMode.RisingLavaEverywhere";
                public const string SaferDiagonalSmuggle = "ScugHelper.ExtendedVariantMode.SaferDiagonalSmuggle";
                public const string SnowballsEverywhere = "ScugHelper.ExtendedVariantMode.SnowballsEverywhere";
                public const string StretchUpDashes = "ScugHelper.ExtendedVariantMode.StretchUpDashes";
                public const string TheoCrystalsEverywhere = "ScugHelper.ExtendedVariantMode.TheoCrystalsEverywhere";
                public const string ThrowIgnoresForcedMove = "ScugHelper.ExtendedVariantMode.ThrowIgnoresForcedMove";
                public const string TrueNoGrabbing = "ScugHelper.ExtendedVariantMode.TrueNoGrabbing";
                public const string UltraProtection = "ScugHelper.ExtendedVariantMode.UltraProtection";
                public const string UpsideDown = "ScugHelper.ExtendedVariantMode.UpsideDown";
                public const string WalllessWallbounce = "ScugHelper.ExtendedVariantMode.WalllessWallbounce";
                public const string WindCrouchMove = "ScugHelper.ExtendedVariantMode.WindCrouchMove";
            }
        }

        public static class Counters
        {
            public const string DeathCount = "ScugHelper.DeathCount";
            public const string DeathHereCount = "ScugHelper.DeathHereCount";
            public const string PlayerDashes = "ScugHelper.PlayerDashes";
            public const string PlayerMaxDashes = "ScugHelper.PlayerMaxDashes";
            public const string PlayerTotalDashes = "ScugHelper.PlayerTotalDashes";
            public const string LevelX = "ScugHelper.LevelX";
            public const string LevelY = "ScugHelper.LevelY";
            public const string LevelWidth = "ScugHelper.LevelWidth";
            public const string LevelHeight = "ScugHelper.LevelHeight";
            public const string FPS = "ScugHelper.FPS";
            public const string CassetteBlockIndex = "ScugHelper.CassetteBlockIndex";
            public const string CoreMode = "ScugHelper.CoreMode";
            public const string EpochTime = "ScugHelper.EpochTime";

            public static class ExtendedVariants
            {
                public const string JumpCount = "ScugHelper.ExtendedVariantMode.JumpCount";
                public const string AddSeekers = "ScugHelper.ExtendedVariantMode.AddSeekers";
                public const string BadelineBossCount = "ScugHelper.ExtendedVariantMode.BadelineBossCount";
                public const string BadelineBossNodeCount = "ScugHelper.ExtendedVariantMode.BadelineBossNodeCount";
                public const string ChaserCount = "ScugHelper.ExtendedVariantMode.ChaserCount";
                public const string CornerCorrection = "ScugHelper.ExtendedVariantMode.CornerCorrection";
                public const string DashCount = "ScugHelper.ExtendedVariantMode.DashCount";
                public const string JellyfishEverywhere = "ScugHelper.ExtendedVariantMode.JellyfishEverywhere";
                public const string MultiBuffering = "ScugHelper.ExtendedVariantMode.MultiBuffering";
                public const string OshiroCount = "ScugHelper.ExtendedVariantMode.OshiroCount";
                public const string ReverseOshiroCount = "ScugHelper.ExtendedVariantMode.ReverseOshiroCount";
                public const string ScreenTransitionDashCount = "ScugHelper.ExtendedVariantMode.ScreenTransitionDashCount";
                public const string SpawnDashCount = "ScugHelper.ExtendedVariantMode.SpawnDashCount";
                public const string Stamina = "ScugHelper.ExtendedVariantMode.Stamina";
                public const string WallBounceDistance = "ScugHelper.ExtendedVariantMode.WallBounceDistance";
                public const string WallJumpDistance = "ScugHelper.ExtendedVariantMode.WallJumpDistance";
            }
        }

        public static class Sliders
        {
            public const string TimeRate = "ScugHelper.TimeRate";
            public const string PlayerX = "ScugHelper.PlayerX";
            public const string PlayerY = "ScugHelper.PlayerY";
            public const string PlayerSpeedX = "ScugHelper.PlayerSpeedX";
            public const string PlayerSpeedY = "ScugHelper.PlayerSpeedY";
            public const string PlayerSubpixelX = "ScugHelper.PlayerSubpixelX";
            public const string PlayerSubpixelY = "ScugHelper.PlayerSubpixelY";
            public const string PlayerStamina = "ScugHelper.PlayerStamina";
            public const string SessionTime = "ScugHelper.SessionTime";

            public static class ExtendedVariants
            {
                public const string VanillaGameSpeed = "ScugHelper.ExtendedVariantMode.VanillaGameSpeed";
                public const string AirFriction = "ScugHelper.ExtendedVariantMode.AirFriction";
                public const string AnxietyEffect = "ScugHelper.ExtendedVariantMode.AnxietyEffect";
                public const string BackgroundBlurLevel = "ScugHelper.ExtendedVariantMode.BackgroundBlurLevel";
                public const string BackgroundBrightness = "ScugHelper.ExtendedVariantMode.BackgroundBrightness";
                public const string BadelineLag = "ScugHelper.ExtendedVariantMode.BadelineLag";
                public const string BlurLevel = "ScugHelper.ExtendedVariantMode.BlurLevel";
                public const string BoostMultiplier = "ScugHelper.ExtendedVariantMode.BoostMultiplier";
                public const string ClimbDownSpeed = "ScugHelper.ExtendedVariantMode.ClimbDownSpeed";
                public const string ClimbHoldStaminaDrainRate = "ScugHelper.ExtendedVariantMode.ClimbHoldStaminaDrainRate";
                public const string ClimbJumpStaminaCost = "ScugHelper.ExtendedVariantMode.ClimbJumpStaminaCost";
                public const string ClimbUpSpeed = "ScugHelper.ExtendedVariantMode.ClimbUpSpeed";
                public const string ClimbUpStaminaDrainRate = "ScugHelper.ExtendedVariantMode.ClimbUpStaminaDrainRate";
                public const string CoyoteTime = "ScugHelper.ExtendedVariantMode.CoyoteTime";
                public const string DashLength = "ScugHelper.ExtendedVariantMode.DashLength";
                public const string DashSpeed = "ScugHelper.ExtendedVariantMode.DashSpeed";
                public const string DashTimerMultiplier = "ScugHelper.ExtendedVariantMode.DashTimerMultiplier";
                public const string DelayBeforeRegrabbing = "ScugHelper.ExtendedVariantMode.DelayBeforeRegrabbing";
                public const string DelayBetweenBadelines = "ScugHelper.ExtendedVariantMode.DelayBetweenBadelines";
                public const string ExplodeLaunchSpeed = "ScugHelper.ExtendedVariantMode.ExplodeLaunchSpeed";
                public const string FallSpeed = "ScugHelper.ExtendedVariantMode.FallSpeed";
                public const string FastFallAcceleration = "ScugHelper.ExtendedVariantMode.FastFallAcceleration";
                public const string ForegroundEffectOpacity = "ScugHelper.ExtendedVariantMode.ForegroundEffectOpacity";
                public const string Friction = "ScugHelper.ExtendedVariantMode.Friction";
                public const string GameSpeed = "ScugHelper.ExtendedVariantMode.GameSpeed";
                public const string GlitchEffect = "ScugHelper.ExtendedVariantMode.GlitchEffect";
                public const string Gravity = "ScugHelper.ExtendedVariantMode.Gravity";
                public const string HiccupStrength = "ScugHelper.ExtendedVariantMode.HiccupStrength";
                public const string HorizontalSpringBounceDuration = "ScugHelper.ExtendedVariantMode.HorizontalSpringBounceDuration";
                public const string HorizontalWallJumpDuration = "ScugHelper.ExtendedVariantMode.HorizontalWallJumpDuration";
                public const string HyperdashSpeed = "ScugHelper.ExtendedVariantMode.HyperdashSpeed";
                public const string JumpBoost = "ScugHelper.ExtendedVariantMode.JumpBoost";
                public const string JumpCooldown = "ScugHelper.ExtendedVariantMode.JumpCooldown";
                public const string JumpDuration = "ScugHelper.ExtendedVariantMode.JumpDuration";
                public const string JumpHeight = "ScugHelper.ExtendedVariantMode.JumpHeight";
                public const string LiftboostCapDown = "ScugHelper.ExtendedVariantMode.LiftboostCapDown";
                public const string LiftboostCapUp = "ScugHelper.ExtendedVariantMode.LiftboostCapUp";
                public const string LiftboostCapX = "ScugHelper.ExtendedVariantMode.LiftboostCapX";
                public const string MinimumDelayBeforeThrowing = "ScugHelper.ExtendedVariantMode.MinimumDelayBeforeThrowing";
                public const string PickupDuration = "ScugHelper.ExtendedVariantMode.PickupDuration";
                public const string RegularHiccups = "ScugHelper.ExtendedVariantMode.RegularHiccups";
                public const string RisingLavaSpeed = "ScugHelper.ExtendedVariantMode.RisingLavaSpeed";
                public const string RoomLighting = "ScugHelper.ExtendedVariantMode.RoomLighting";
                public const string RoomBloom = "ScugHelper.ExtendedVariantMode.RoomBloom";
                public const string ScreenShakeIntensity = "ScugHelper.ExtendedVariantMode.ScreenShakeIntensity";
                public const string SlowfallGravityMultiplier = "ScugHelper.ExtendedVariantMode.SlowfallGravityMultiplier";
                public const string SlowfallSpeedThreshold = "ScugHelper.ExtendedVariantMode.SlowfallSpeedThreshold";
                public const string SnowballDelay = "ScugHelper.ExtendedVariantMode.SnowballDelay";
                public const string SpeedX = "ScugHelper.ExtendedVariantMode.SpeedX";
                public const string SuperdashSteeringSpeed = "ScugHelper.ExtendedVariantMode.SuperdashSteeringSpeed";
                public const string UltraSpeedMultiplier = "ScugHelper.ExtendedVariantMode.UltraSpeedMultiplier";
                public const string UnderwaterSpeedX = "ScugHelper.ExtendedVariantMode.UnderwaterSpeedX";
                public const string UnderwaterSpeedY = "ScugHelper.ExtendedVariantMode.UnderwaterSpeedY";
                public const string WallBouncingSpeed = "ScugHelper.ExtendedVariantMode.WallBouncingSpeed";
                public const string WallSlidingSpeed = "ScugHelper.ExtendedVariantMode.WallSlidingSpeed";
                public const string WaterSurfaceSpeedX = "ScugHelper.ExtendedVariantMode.WaterSurfaceSpeedX";
                public const string WaterSurfaceSpeedY = "ScugHelper.ExtendedVariantMode.WaterSurfaceSpeedY";
                public const string ZoomLevel = "ScugHelper.ExtendedVariantMode.ZoomLevel";
            }
        }
    }
}

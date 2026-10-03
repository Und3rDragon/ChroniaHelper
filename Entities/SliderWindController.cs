using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System.Collections;

namespace ChroniaHelper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/SliderWindController")]
[ExA.Credits("CrsyallineHelper for open-sourced codes")]
public class SliderWindController : GeneralSetupController
{
    [ExA.SelectiveLoadHook]
    public static void Load()
    {
        On.Celeste.WindController.Update += WindController_Update;
        On.Celeste.Refill.OnPlayer += Refill_OnPlayer;
        On.Celeste.Glider.OnPickup += Glider_OnPickup;
        On.Celeste.TheoCrystal.OnPickup += TheoCrystal_OnPickup;
    }
    [ExA.SelectiveUnloadHook]
    public static void Unload()
    {
        On.Celeste.WindController.Update -= WindController_Update;
        On.Celeste.Refill.OnPlayer -= Refill_OnPlayer;
        On.Celeste.Glider.OnPickup -= Glider_OnPickup;
        On.Celeste.TheoCrystal.OnPickup -= TheoCrystal_OnPickup;
    }

    public SliderWindController(EntityData data, Vc2 offset) : base(data, offset)
    {
        active = false;
        Tag = Tags.TransitionUpdate;

        this.speedX = data.Slider("speedX");
        this.speedY = data.Slider("speedY");
        this.otherActivators = data.Int("otherActivators", Activators.Default);

        Ldm.LoadHookWithMethod<SliderWindController>(Load);
    }

    public override void ExecuteByUpdateState(bool current, bool last)
    {
        base.ExecuteByUpdateState(current, last);

        CheckActivation();

        Level level = SceneAs<Level>();

        level.Wind = active && current ? speed : Vc2.Zero;

        if (level.Wind != Vector2.Zero && !level.Transitioning)
        {
            foreach (Component component in Scene.Tracker.GetComponents<WindMover>())
            {
                WindMover windMover = component as WindMover;
                windMover.Move(level.Wind * 0.1f * Engine.DeltaTime);
            }
        }
    }

    private struct Activators
    {
        public const int Default = -1;
        public const int Seeds = 0;
        public const int Strawberry = 1;
        public const int LockedDoor = 2;
        public const int Refill = 2;
        public const int Jellyfish = 2;
        public const int Theo = 2;
        public const int CoreModeHot = 3;
        public const int CoreModeCold = 4;
        public const int Death = 5;
    }
    private void CheckActivation()
    {
        switch (otherActivators)
        {
            case Activators.Seeds:
                bool activate1 = false;
                foreach (StrawberrySeed seed in Scene.Tracker.GetEntities<StrawberrySeed>())
                {
                    if (seed.Collected)
                    {
                        activate1 = true;
                        break;
                    }
                }
                if (activate1)
                {
                    ActivateWind();
                }
                else
                {
                    DeactivateWind();
                }
                break;
            case Activators.Strawberry:
                Player player = Scene.Tracker.GetEntity<Player>();
                bool activate2 = false;
                foreach (Follower follower in player.Leader.Followers)
                {
                    if (follower.Entity is Strawberry)
                    {
                        Strawberry berry = follower.Entity as Strawberry;
                        if (!berry.Golden)
                        {
                            activate2 = true;
                            break;
                        }
                    }
                }
                if (activate2)
                {
                    ActivateWind();
                }
                else
                {
                    DeactivateWind();
                }
                break;
            case Activators.LockedDoor 
                or Activators.Refill 
                or Activators.Jellyfish 
                or Activators.Theo:
                //handled inside hooks
                break;
            case Activators.CoreModeHot:
                if (SceneAs<Level>().CoreMode == Session.CoreModes.Hot)
                {
                    ActivateWind();
                }
                else
                {
                    DeactivateWind();
                }
                break;
            case Activators.CoreModeCold:
                if (SceneAs<Level>().CoreMode == Session.CoreModes.Cold)
                {
                    ActivateWind();
                }
                else
                {
                    DeactivateWind();
                }
                break;
            case Activators.Death:
                Player player3 = Scene.Tracker.GetEntity<Player>();
                if (player3 == null || player3.Dead)
                {
                    ActivateWind();
                }
                break;
            default:
                ActivateWind();
                break;
        }
    }

    public static void Refill_OnPlayer(On.Celeste.Refill.orig_OnPlayer orig, Celeste.Refill self, Player player)
    {
        orig(self, player);
        if (!self.Collidable)
        {
            SliderWindController wind = self.Scene.Tracker.GetEntity<SliderWindController>();
            if (wind != null && wind.otherActivators == Activators.Refill)
            {
                wind.ActivateWind();
            }
        }
    }

    public static void Glider_OnPickup(On.Celeste.Glider.orig_OnPickup orig, Glider self)
    {
        orig(self);
        SliderWindController wind = self.Scene.Tracker.GetEntity<SliderWindController>();
        if (wind != null && wind.otherActivators == Activators.Jellyfish)
        {
            wind.ActivateWind();
        }
    }

    public static void TheoCrystal_OnPickup(On.Celeste.TheoCrystal.orig_OnPickup orig, TheoCrystal self)
    {
        orig(self);
        SliderWindController wind = self.Scene.Tracker.GetEntity<SliderWindController>();
        if (wind != null && wind.otherActivators == Activators.Theo)
        {
            wind.ActivateWind();
        }
    }

    public static void WindController_Update(On.Celeste.WindController.orig_Update orig, WindController self)
    {
        if (self.Scene.Tracker.GetEntities<SliderWindController>().Count == 0)
        {
            orig(self);
        }
    }

    public void ActivateWind()
    {
        if (active) return;
        active = true;

        SetAmbience();
    }

    public void DeactivateWind()
    {
        if (!active) return;
        active = false;
        
        SetAmbience();
    }

    private void SetAmbience()
    {
        if (speed != Vector2.Zero)
        {
            float angle = (float)Math.Atan2(speed.Y, speed.X);
            angle += (float)Math.PI / 4;
            angle %= (float)Math.PI * 2;
            if (angle < Math.PI)
            {
                Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", 1f);
            }
            else
            {
                Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", -1f);
            }
        }
        else
        {
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", 0f);
        }
        Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "strong_wind", speed.LengthSquared() > 640000f ? 1f : 0f);
    }

    private bool active;

    private SelectiveSlider speedX;

    private SelectiveSlider speedY;

    private Vc2 speed => new Vc2(speedX?.Value ?? 0, speedY?.Value ?? 0);

    public int otherActivators;
}

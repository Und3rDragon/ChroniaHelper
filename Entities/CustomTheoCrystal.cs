using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Media;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CustomTheoCrystal")]
public class CustomTheoCrystal : TheoCrystal
{
    public CustomTheoCrystal(EntityData data, Vc2 offset) : base(data, offset)
    {
        Depth = data.Int("depth", 100);
        
        tutorialTitleText = data.Attr("birdTutorialTitle", "tutorial_hold");
        tutorialMain = data.Attr("birdTutorialText", "Grab");
        tutorialOffset = data.Vector2("tutorialIconOffsetX", "tutorialIconOffsetY", new(0f, -24f));

        hitSeekerSound = data.Attr("hitSeekerSound", "event:/game/05_mirror_temple/crystaltheo_hit_side");
        hitSideSound = data.Attr("hitSideSound", "event:/game/05_mirror_temple/crystaltheo_hit_side");
        hitGroundSound = data.Attr("hitGroundSound", "event:/game/05_mirror_temple/crystaltheo_hit_ground");
        
        displayTutorial = data.Bool("displayTutorial", false);
        
        // Redefine Sprites
        Remove(sprite);
        sprite = GFX.SpriteBank.Create(data.Attr("textureXML", "theo_crystal"));
        sprite.Scale.X = -1f;
        Add(sprite);
        
        // Redefine Holdable attribute
        Remove(Hold);
        float holdableCooldown = data.Float("holdableCooldown", 0.1f).ClampMin(0.01f);
        float holdableWidth = data.Float("holdableWidth", 16f).GetAbs();
        float holdableHeight = data.Float("holdableHeight", 22f).GetAbs();
        float holdableX = data.Float("holdableX", -8f);
        float holdableY = data.Float("holdableY", -16f);
        this.Add((Component) (this.Hold = new Holdable(holdableCooldown)));
        this.Hold.PickupCollider = (Collider) new Hitbox(holdableWidth, holdableHeight, holdableX, holdableY);
        this.Hold.SlowFall = data.Bool("slowFall", false);
        this.Hold.SlowRun = data.Bool("slowRun", true);
        this.Hold.OnPickup = new Action(this.OnPickup);
        this.Hold.OnRelease = new Action<Vector2>(this.OnRelease);
        this.Hold.DangerousCheck = new Func<HoldableCollider, bool>(this.Dangerous);
        this.Hold.OnHitSeeker = new Action<Seeker>(this.HitSeeker);
        this.Hold.OnSwat = new Action<HoldableCollider, int>(this.Swat);
        this.Hold.OnHitSpring = new Func<Spring, bool>(this.HitSpring);
        this.Hold.OnHitSpinner = new Action<Entity>(this.HitSpinner);
        this.Hold.SpeedGetter = (Func<Vector2>) (() => this.Speed);
        this.onCollideH = new Collision(this.OnCollideH);
        this.onCollideV = new Collision(this.OnCollideV);
        
        this.LiftSpeedGraceTime = data.Float("liftSpeedGraceTime", 0.1f).ClampMin(Engine.DeltaTime);
        
        // Redefine Components
        List<Component> toRemove = new();
        foreach (var item in Components)
        {
            if (item is VertexLight || item is MirrorReflection)
            {
                toRemove.Add(item);
            }
        }
        foreach (var item in toRemove)
        {
            Remove(item);
        }

        vertexLightOffset = data.Vector2("vertexLightOffsetX", "vertexLightOffsetY", Vc2.Zero);
        CColor hexLightColor = data.GetChroniaColor("hexLightColor", Color.White);
        this.Add((Component) new VertexLight(this.Collider.Center + vertexLightOffset, 
            hexLightColor.color, hexLightColor.alpha, 
            data.Int("vertexLightStartFade", 32),
            data.Int("vertexLightEndFade", 64)));
        //this.Tag = (int) Tags.TransitionUpdate;
        if (data.Bool("mirrorReflection", true))
        {
            this.Add((Component) new MirrorReflection());
        }
    }
    private string tutorialTitleText, tutorialMain;
    private Vc2 tutorialOffset;
    private string hitSeekerSound, hitSideSound, hitGroundSound;
    private Vc2 vertexLightOffset;
    private bool displayTutorial = false;

    // [LoadHook]
    // public static void Load()
    // {
    //     On.Celeste.TheoCrystal.Added += OnTheoAdded;
    //     // On.Celeste.TheoCrystal.Update += OnTheoUpdate;
    // }
    // [UnloadHook]
    // public static void Unload()
    // {
    //     On.Celeste.TheoCrystal.Added -= OnTheoAdded;
    //     // On.Celeste.TheoCrystal.Update -= OnTheoUpdate;
    // }

    // public static void OnTheoAdded(On.Celeste.TheoCrystal.orig_Added orig, TheoCrystal self, Scene scene)
    // {
    //     if (self is CustomTheoCrystal cus)
    //     {
    //         foreach (CustomTheoCrystal entity in MaP.level.Tracker.GetEntities<CustomTheoCrystal>())
    //         {
    //             if (entity != cus && entity.Hold.IsHeld)
    //                 cus.RemoveSelf();
    //         }
    //         if (!(MaP.level.Session.Level == "e-00"))
    //             return;
    //         cus.tutorialGui = cus.CreateBirdGUI(cus.tutorialOffset.X, cus.tutorialOffset.Y, 
    //             cus.tutorialTitleText, cus.tutorialMain);
    //         cus.tutorialGui.Open = false;
    //         cus.Scene.Add((Entity)cus.tutorialGui);
    //     }
    //     else
    //     {
    //         orig(self, scene);
    //     }
    // }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        this.Scene.Remove(tutorialGui);
        this.tutorialGui = this.CreateBirdGUI(tutorialOffset.X, tutorialOffset.Y, 
            tutorialTitleText, tutorialMain);
        this.tutorialGui.Open = displayTutorial;
        this.Scene.Add((Entity) this.tutorialGui);
    }

    public void HitSeeker(Seeker seeker)
    {
        if (!this.Hold.IsHeld)
            this.Speed = (this.Center - seeker.Center).SafeNormalize(120f);
        Audio.Play(hitSeekerSound, this.Position);
    }
    
    public void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            int num = (int) (data.Hit as DashSwitch).OnDashCollide((Player) null, Vector2.UnitX * (float) Math.Sign(this.Speed.X));
        }
        Audio.Play(hitSideSound, this.Position);
        if ((double) Math.Abs(this.Speed.X) > 100.0)
            this.ImpactParticles(data.Direction);
        this.Speed.X *= -0.4f;
    }

    public void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            int num = (int) (data.Hit as DashSwitch).OnDashCollide((Player) null, Vector2.UnitY * (float) Math.Sign(this.Speed.Y));
        }
        if ((double) this.Speed.Y > 0.0)
        {
            if ((double) this.hardVerticalHitSoundCooldown <= 0.0)
            {
                Audio.Play(hitGroundSound, this.Position, "crystal_velocity", Calc.ClampedMap(this.Speed.Y, 0.0f, 200f));
                this.hardVerticalHitSoundCooldown = 0.5f;
            }
            else
                Audio.Play(hitGroundSound, this.Position, "crystal_velocity", 0.0f);
        }
        if ((double) this.Speed.Y > 160.0)
            this.ImpactParticles(data.Direction);
        if ((double) this.Speed.Y > 140.0 && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            this.Speed.Y *= -0.6f;
        else
            this.Speed.Y = 0.0f;
    }
}
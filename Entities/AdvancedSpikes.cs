using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using static ChroniaHelper.Entities.SeamlessSpinner;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity([
    "ChroniaHelper/UpAdvancedSpikes = LoadUp",
    "ChroniaHelper/DownAdvancedSpikes = LoadDown",
    "ChroniaHelper/LeftAdvancedSpikes = LoadLeft",
    "ChroniaHelper/RightAdvancedSpikes = LoadRight",
    "ChroniaHelper/UpSpikes = LoadUp",
    "ChroniaHelper/DownSpikes = LoadDown",
    "ChroniaHelper/LeftSpikes = LoadLeft",
    "ChroniaHelper/RightSpikes = LoadRight"
])]
public class AdvancedSpikes : Entity
{
    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AdvancedSpikes(entityData, offset, DirectionMode.Up);
    }

    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AdvancedSpikes(entityData, offset, DirectionMode.Down);
    }

    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AdvancedSpikes(entityData, offset, DirectionMode.Left);
    }

    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AdvancedSpikes(entityData, offset, DirectionMode.Right);
    }

    private struct SpikeInfo
    {
        public AdvancedSpikes parent;

        public Vector2 position;

        public int spikeIndex;

        public int textureIndex;

        public bool triggered;

        public float triggerDelayTimer;

        public float retractDelayTimer;

        public float lerp;

        public Color color;


        public void Update()
        {
            if (this.triggered)
            {
                this.TriggerEvent();
                if (FlagUtils.IsCorrectFlag(this.parent.level, this.parent.retractFlag))
                {
                    this.RetractEvent();
                }
            }
            else
            {
                this.lerp = Calc.Approach(this.lerp, 0F, this.parent.retractLerpMove * Engine.DeltaTime);
                if (this.lerp <= 0F)
                {
                    this.triggered = false;
                }
            }

            if (this.parent.rainbow)
            {
                CrystalStaticSpinner spinner = parent.Spinner;
                if (spinner == null)
                {
                    spinner = parent.Spinner = new CrystalStaticSpinner(Vector2.Zero, false, Celeste.CrystalColor.Rainbow);
                    parent.Spinner.Scene = parent.Scene;
                }

                this.color = spinner.GetHue(this.position);
            }
        }

        private void TriggerEvent()
        {
            if (this.triggerDelayTimer > 0F)
            {
                this.triggerDelayTimer -= Engine.DeltaTime;
                if (this.triggerDelayTimer <= 0F)
                {
                    if (!this.parent.grouped && this.parent.PlayerCheck(this.spikeIndex))
                    {
                        this.triggerDelayTimer = this.parent.touchDelay;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.parent.triggerSound))
                        {
                            Audio.Play(this.parent.triggerSound, this.parent.Position + this.position);
                        }
                    }

                    this.retractDelayTimer = this.parent.retractDelay;
                }
            }
            else
            {
                this.lerp = Calc.Approach(this.lerp, this.parent.lerpMoveTime, this.parent.triggerLerpMove * Engine.DeltaTime);
            }
        }

        private void RetractEvent()
        {
            if (this.retractDelayTimer > 0F)
            {
                this.retractDelayTimer -= Engine.DeltaTime;
                if (this.retractDelayTimer <= 0F)
                {
                    if (!string.IsNullOrEmpty(this.parent.retractSound))
                    {
                        Audio.Play(this.parent.retractSound, this.parent.Position + this.position);
                    }

                    this.triggered = false;
                }
            }
        }

        public bool OnPlayer(Player player, Vector2 outwards)
        {
            if (player is null)
            {
                return false;
            }

            if (!this.triggered)
            {
                if (!FlagUtils.IsCorrectFlag(this.parent.level, this.parent.triggerFlag))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(this.parent.touchSound))
                {
                    Audio.Play(this.parent.touchSound, this.parent.Position + this.position);
                }

                this.triggerDelayTimer = this.parent.triggerDelay;
                this.triggered = true;
                return false;
            }

            if (this.lerp >= this.parent.lerpMoveTime)
            {
                if (parent.childModeParent.IsNotNull())
                {
                    if (parent.childModeTriggered) { return true; }
                    parent.childModeParent.PlayerActivated(player);
                    parent.childModeTriggered = true;
                }
                else
                {
                    player.Die(outwards);
                }
                
                return true;
            }

            return false;
        }
    }

    public static ILHook PlayerOrigUpdateHook;

    [LoadHook]
    public static void OnLoad()
    {
        PlayerOrigUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Instance | BindingFlags.Public), PlayerOnOrigUpdate);
    }
    [UnloadHook]
    public static void OnUnload()
    {
        PlayerOrigUpdateHook?.Dispose();
        PlayerOrigUpdateHook = null;
    }

    private static void PlayerOnOrigUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(
                ins=>ins.MatchCall("Monocle.Entity", "System.Boolean CollideCheck<Celeste.Spikes>(Microsoft.Xna.Framework.Vector2)")
                ))
        {
            cursor.Index += 1;
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Func<bool, Player, bool>>(( origResult,  player)=>
            {
                    if (player.CollideFirst<AdvancedSpikes>() is {} spike1)
                    {
                        if (!spike1.CanRefillDashOnTouch || !spike1.trigger)
                            return true;
                    }
                    if (player.CollideFirst<AnimatedSpikes>() is {} spike2)
                    {
                        if (!spike2.CanRefillDashOnTouch || !spike2.trigger)
                            return true;
                    }
                return origResult;
            });
        }
    }


    public CrystalStaticSpinner Spinner;
    public bool CanRefillDashOnTouch;
    private int size;

    private DirectionMode direction;

    private SpikeType spikeType;

    private string sprite;

    private Color spriteColor;

    private float spriteAlpha;

    private float spriteRotation;

    private float spriteScaleX;

    private float spriteScaleY;

    private int spriteOrigin;

    private int textureIndex;

    private int singleSize;

    private float lerpMoveTime;

    private float touchDelay;

    private string touchSound;

    private string[] triggerFlag;

    private float triggerDelay;

    private float triggerLerpMove;

    private string triggerSound;

    private string[] retractFlag;

    private float retractDelay;

    private float retractLerpMove;

    private string retractSound;

    private bool trigger;

    private bool waitPlayerLeave;

    private bool attached;

    private bool grouped;

    private bool rainbow;

    private bool randomTexture;

    private Level level;

    private Vector2 outwards;

    private SpikeInfo[] spikes;

    private Vector2 shakeOffset;

    private List<MTexture> spikeTextures;

    private Vector2 spriteScale;

    private bool enterGrouped;

    private bool enterFlag;

    public static string DefaultTouchSound;

    public static string DefaultTriggerSound;

    public static string DefaultRetractSound;

    static AdvancedSpikes()
    {
        AdvancedSpikes.DefaultTouchSound = "event:/game/03_resort/fluff_tendril_touch";
        AdvancedSpikes.DefaultTriggerSound = "event:/game/03_resort/fluff_tendril_emerge";
        AdvancedSpikes.DefaultRetractSound = "";
    }

    private enum SpikeType
    {
        Default,
        Outline,
        Cliffside,
        Reflection,
        Custom
    }

    public void Old(EntityData data)
    {
        this.spriteOrigin = data.Int("spriteOrigin"); //-4
        this.singleSize = data.Int("singleSize"); //8
        this.lerpMoveTime = data.Float("lerpMoveTime"); //1F
        this.triggerFlag = FlagUtils.Parse(data.Attr("triggerFlag")); //null
        this.triggerLerpMove = data.Float("triggerLerpMove"); //8F
        this.retractFlag = FlagUtils.Parse(data.Attr("retractFlag")); //null
        this.retractLerpMove = data.Float("retractLerpMove"); //8F
        this.textureIndex = data.Int("textureIndex"); //0
    }

    public AdvancedSpikes(Vector2 position, EntityData data, DirectionMode direction) : base(position)
    {
        Old(data);
        this.direction = direction;
        this.spikeType = data.Enum<SpikeType>("spikeType", SpikeType.Default);
        this.sprite = data.Attr("sprite", null);
        this.spriteColor = data.HexColor("spriteColor", Color.White);
        this.spriteAlpha = data.Float("spriteAlpha", 1F);
        this.spriteRotation = data.Float("spriteRotation", 0F);
        this.spriteScaleX = data.Float("spriteScaleX", 1F);
        this.spriteScaleY = data.Float("spriteScaleY", 1F);
        this.spriteScale = new Vector2(this.spriteScaleX, this.spriteScaleY);
        this.spriteOrigin = data.Int("spriteDisplacement", -4);
        this.textureIndex = data.Int("spriteFileIndex", 0);
        this.singleSize = data.Int("spriteSpacing", 8);
        this.lerpMoveTime = data.Float("animMoveMultiplier", 1F);
        this.touchDelay = data.Float("touchDelay", 0.05F);
        this.touchSound = data.Attr("touchSound", AdvancedSpikes.DefaultTouchSound);
        this.triggerFlag = FlagUtils.Parse(data.Attr("flagNeedToTrigger", null));
        this.triggerDelay = data.Float("triggerDelay", 0.4F);
        this.triggerLerpMove = data.Float("triggerAnimSpeed", 8F);
        this.triggerSound = data.Attr("triggerSound", AdvancedSpikes.DefaultTriggerSound);
        this.retractFlag = FlagUtils.Parse(data.Attr("flagNeedToRetract", null));
        this.retractDelay = data.Float("retractDelay", -1F);
        this.retractLerpMove = data.Float("retractAnimSpeed", 8F);
        this.retractSound = data.Attr("retractSound", AdvancedSpikes.DefaultRetractSound);
        this.trigger = data.Bool("trigger", false);
        this.waitPlayerLeave = data.Bool("waitPlayerLeave", true);
        this.attached = data.Bool("attached", true);
        this.grouped = data.Bool("grouped", false);
        this.rainbow = data.Bool("rainbow", false);
        this.randomTexture = data.Bool("randomTexture", true);
        if (this.triggerDelay <= 0F)
        {
            this.triggerDelay = Engine.DeltaTime;
        }

        if (this.retractDelay == 0F)
        {
            this.retractDelay = Engine.DeltaTime;
        }

        switch (this.direction)
        {
            case DirectionMode.Up:
                this.size = data.Width;
                this.outwards = new Vector2(0F, -1F);
                base.Collider = new Hitbox(this.size, 3F, 0F, -3F);
                base.Add(new LedgeBlocker(this.UpSafeBlockCheck));
                break;
            case DirectionMode.Down:
                this.size = data.Width;
                this.outwards = new Vector2(0F, 1F);
                base.Collider = new Hitbox(this.size, 3F);
                base.Add(new LedgeBlocker(this.UpSafeBlockCheck));
                break;
            case DirectionMode.Left:
                this.size = data.Height;
                this.outwards = new Vector2(-1F, 0F);
                base.Collider = new Hitbox(3F, this.size, -3F);
                base.Add(new LedgeBlocker(this.SideSafeBlockCheck));
                break;
            case DirectionMode.Right:
                this.size = data.Height;
                this.outwards = new Vector2(1F, 0F);
                base.Collider = new Hitbox(3F, this.size);
                base.Add(new LedgeBlocker(this.SideSafeBlockCheck));
                break;
        }

        base.Add(new SafeGroundBlocker());
        base.Add(new PlayerCollider(this.OnCollide));
        if (this.attached)
        {
            base.Add(new StaticMover
            {
                OnShake = this.OnShake,
                SolidChecker = this.IsRiding,
                JumpThruChecker = this.IsRiding
            });
        }

        base.Depth = data.Int("depth", -50);
        CanRefillDashOnTouch = data.Bool("canRefillDashOnTouch", true);

        childMode = data.Attr("childMode");
    }

    public string childMode;
    public DangerBubbler childModeParent = null;
    public bool childModeTriggered = false;

    public AdvancedSpikes(EntityData data, Vector2 offset, DirectionMode direction) : this(data.Position + offset, data, direction)
    {
    }

    private bool UpSafeBlockCheck(Player player)
    {
        bool triggered = true;
        for (int i = 0; i < spikes.Length; i++)
        {
            if (!spikes[i].triggered)
            {
                triggered = false;
                break;
            }
        }

        if (trigger && !triggered)
        {
            return false;
        }
        else
        {
            return true;
        }
        // Overwritten Original Logic
        // The codes below is ignored

        int dir = 8 * (int)player.Facing;
        int left = (int)((player.Left + (float)dir - base.Left) / 4F);
        int right = (int)((player.Right + (float)dir - base.Left) / 4F);
        if (right < 0 || left >= this.spikes.Length)
        {
            return false;
        }

        left = Math.Max(left, 0);
        right = Math.Min(right, this.spikes.Length - 1);
        for (int i = left; i <= right; i++)
        {
            if (this.spikes[i].lerp >= 1F)
            {
                return true;
            }
        }

        return false;
    }

    private bool SideSafeBlockCheck(Player player)
    {
        int top = (int)((player.Top - base.Top) / 4F);
        int bottom = (int)((player.Bottom - base.Top) / 4F);
        if (bottom < 0 || top >= this.spikes.Length)
        {
            return false;
        }

        top = Math.Max(top, 0);
        bottom = Math.Min(bottom, this.spikes.Length - 1);
        for (int i = top; i <= bottom; i++)
        {
            if (this.spikes[i].lerp >= 1F)
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollide(Player player)
    {
        this.GetPlayerCollideIndex(player, out var minIndex, out var maxIndex);
        if (maxIndex < 0 || minIndex >= this.spikes.Length)
        {
            return;
        }

        if (this.grouped && !this.enterFlag)
        {
            this.enterGrouped = true;
        }

        minIndex = Math.Max(minIndex, 0);
        maxIndex = Math.Min(maxIndex, this.spikes.Length - 1);
        for (int i = minIndex; i <= maxIndex; i++)
        {
            if (this.spikes[i].OnPlayer(player, this.outwards))
            {
                return;
            }
        }
    }

    private void GetPlayerCollideIndex(Player player, out int minIndex, out int maxIndex)
    {
        minIndex = (maxIndex = -1);
        switch (this.direction)
        {
            case DirectionMode.Up:
                if (player.Speed.Y >= 0F)
                {
                    minIndex = (int)((player.Left - base.Left) / this.singleSize);
                    maxIndex = (int)((player.Right - base.Left) / this.singleSize);
                }

                break;
            case DirectionMode.Down:
                if (player.Speed.Y <= 0F)
                {
                    minIndex = (int)((player.Left - base.Left) / this.singleSize);
                    maxIndex = (int)((player.Right - base.Left) / this.singleSize);
                }

                break;
            case DirectionMode.Left:
                if (player.Speed.X >= 0F)
                {
                    minIndex = (int)((player.Top - base.Top) / this.singleSize);
                    maxIndex = (int)((player.Bottom - base.Top) / this.singleSize);
                }

                break;
            case DirectionMode.Right:
                if (player.Speed.X <= 0F)
                {
                    minIndex = (int)((player.Top - base.Top) / this.singleSize);
                    maxIndex = (int)((player.Bottom - base.Top) / this.singleSize);
                }

                break;
        }
    }

    private void OnShake(Vector2 amount)
    {
        this.shakeOffset += amount;
    }

    private bool IsRiding(Solid solid) => (this.direction) switch
    {
        DirectionMode.Up => base.CollideCheckOutside(solid, base.Position + Vector2.UnitY),
        DirectionMode.Down => base.CollideCheckOutside(solid, base.Position - Vector2.UnitY),
        DirectionMode.Left => base.CollideCheckOutside(solid, base.Position + Vector2.UnitX),
        DirectionMode.Right => base.CollideCheckOutside(solid, base.Position - Vector2.UnitX),
        _ => false
    };

    private bool IsRiding(JumpThru jumpThru) => (this.direction == DirectionMode.Up) && (base.CollideCheck(jumpThru, base.Position + Vector2.UnitY));

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();

        childModeTriggered = false;
        if (childMode.IsNotNullOrEmpty())
        {
            level.Tracker.GetEntities<DangerBubbler>().ForEach(bubbler =>
            {
                if (bubbler.SourceData.ID.ToString() == childMode)
                {
                    childModeParent = bubbler as DangerBubbler;
                }
            });
        }
        
        this.spikes = new SpikeInfo[this.size / this.singleSize];
        string direction = this.direction.ToString().ToLower();
        this.spikeTextures = GFX.Game.GetAtlasSubtextures(this.GetSpikeSpritePath(direction));
        this.textureIndex = NumberUtils.SafeRangeInteger(this.textureIndex, 0, this.spikeTextures.Count - 1);
        for (int i = 0; i < this.spikes.Length; i++)
        {
            this.spikes[i].parent = this;
            this.spikes[i].spikeIndex = i;
            this.spikes[i].textureIndex = this.randomTexture ? Calc.Random.Next(this.spikeTextures.Count) : this.textureIndex;
            this.spikes[i].triggered = !this.trigger;
            this.spikes[i].lerp = (this.trigger ? 0F : this.lerpMoveTime);
            this.spikes[i].color = this.spriteColor;
            this.spikes[i].position = (this.direction) switch
            {
                DirectionMode.Up => Vector2.UnitX * (i + 0.5F) * 8F + Vector2.UnitY,
                DirectionMode.Down => Vector2.UnitX * (i + 0.5F) * 8F - Vector2.UnitY,
                DirectionMode.Left => Vector2.UnitY * (i + 0.5F) * 8F + Vector2.UnitX,
                DirectionMode.Right => Vector2.UnitY * (i + 0.5F) * 8F - Vector2.UnitX,
                _ => Vector2.Zero
            };
        }
    }

    private string GetSpikeSpritePath(string direction) => (this.spikeType) switch
    {
        SpikeType.Default => "danger/spikes/default_" + direction,
        SpikeType.Outline => "danger/spikes/outline_" + direction,
        SpikeType.Cliffside => "danger/spikes/cliffside_" + direction,
        SpikeType.Reflection => "danger/spikes/reflection_" + direction,
        _ => this.sprite + "_" + direction
    };

    public override void Update()
    {
        base.Update();
        if (this.enterGrouped)
        {
            for (int i = 0; i < this.spikes.Length; i++)
            {
                this.spikes[i].OnPlayer(this.level.Tracker.GetEntity<Player>(), this.outwards);
                if (!this.waitPlayerLeave)
                {
                    this.spikes[i].Update();
                }
            }

            if (!base.CollideCheck<Player>())
            {
                this.enterGrouped = false;
            }

            this.enterFlag = true;
        }
        else if (this.enterFlag)
        {
            for (int i = 0; i < this.spikes.Length; i++)
            {
                this.spikes[i].Update();
            }

            for (int i = 0; i < this.spikes.Length; i++)
            {
                if (this.spikes[i].lerp != this.lerpMoveTime)
                {
                    break;
                }

                this.enterFlag = false;
            }
        }
        else
        {
            for (int i = 0; i < this.spikes.Length; i++)
            {
                this.spikes[i].Update();
            }
        }

        if (!CollideCheck<Player>())
        {
            childModeTriggered = false;
        }
    }

    public override void Render()
    {
        base.Render();

        Vector2 pos = Position;
        Position += shakeOffset;

        Vector2 vector = new Vector2(Math.Abs(this.outwards.Y), Math.Abs(this.outwards.X));
        Vector2 justify = Vector2.Zero;
        switch (this.direction)
        {
            case DirectionMode.Up:
                justify = new Vector2(0.5F, 0.9F);
                break;
            case DirectionMode.Down:
                justify = new Vector2(0.5F, 0.1F);
                break;
            case DirectionMode.Left:
                justify = new Vector2(0.9F, 0.5F);
                break;
            case DirectionMode.Right:
                justify = new Vector2(0.1F, 0.5F);
                break;
        }

        for (int i = 0; i < this.spikes.Length; i++)
        {
            MTexture mTexture = this.spikeTextures[this.spikes[i].textureIndex];
            Vector2 position = base.Position + vector * (4 + i * this.singleSize) + this.outwards * (this.spriteOrigin + this.spikes[i].lerp * 4F);
            mTexture.DrawJustified(position, justify, this.spikes[i].color * this.spriteAlpha, this.spriteScale, this.spriteRotation);
        }

        Position = pos;
    }

    private bool PlayerCheck(int spikeIndex)
    {
        Player player = base.CollideFirst<Player>();
        if ((player is null) || (!this.waitPlayerLeave))
        {
            return false;
        }

        this.GetPlayerCollideIndex(player, out var minIndex, out var maxIndex);
        return (minIndex <= spikeIndex + 1) && (maxIndex >= spikeIndex - 1);
    }

    //private readonly Action<Player> origOnCollide;

    //private void OnPlayer(Player player)
    //{
    //    if (((player.StateMachine.State == Player.StDash
    //          || player.DashAttacking && player.StateMachine.State != Player.StRedDash
    //          || player.StateMachine.State == Player.StDreamDash
    //          || player.StateMachine.State == CmI.GetDreamTunnelDashState
    //          || CmI.IsSeekerDashAttacking
    //          || player.StateMachine.State == Player.StRedDash && player.CurrentBooster.red)
    //         && (player.Speed.Equals(Vector2.Zero))
    //         && CheckDir(player.DashDir)))
    //    {
    //        return;
    //    }
    //    origOnCollide(player);
    //}

    //private bool directionUp, directionDown, along, into, diag;
    //private bool CheckDir(Vector2 dashDir)
    //{
    //    if (directionUp || directionDown)
    //    {
    //        return dashDir.X != 0 && dashDir.Y == 0 && along
    //               || dashDir.X == 0 && dashDir.Y != 0 && into
    //               || dashDir.X != 0 && dashDir.Y != 0 && diag
    //               || dashDir.Equals(Vector2.Zero);
    //    }
    //    return dashDir.X != 0 && dashDir.Y == 0 && into
    //           || dashDir.X == 0 && dashDir.Y != 0 && along
    //           || dashDir.X != 0 && dashDir.Y != 0 && diag
    //           || dashDir.Equals(Vector2.Zero);
    //}
}
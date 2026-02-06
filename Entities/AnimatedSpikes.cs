using System.Collections.Generic;
using System;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity([
    "ChroniaHelper/UpAnimatedSpikes = LoadUp",
    "ChroniaHelper/DownAnimatedSpikes = LoadDown",
    "ChroniaHelper/LeftAnimatedSpikes = LoadLeft",
    "ChroniaHelper/RightAnimatedSpikes = LoadRight"
])]
public class AnimatedSpikes : Entity
{

    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AnimatedSpikes(entityData, offset, DirectionMode.Up);
    }

    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AnimatedSpikes(entityData, offset, DirectionMode.Down);
    }

    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AnimatedSpikes(entityData, offset, DirectionMode.Left);
    }

    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new AnimatedSpikes(entityData, offset, DirectionMode.Right);
    }

    private struct SpikeInfo
    {

        public AnimatedSpikes parent;

        public Vector2 position;

        public int spikeIndex;

        public bool triggered;

        public float triggerDelayTimer;

        public float retractDelayTimer;

        public float lerp;

        public Color disabledColor;

        public float disabledFrame;

        public Color enabledColor;

        public float enabledFrame;

        public float enabledRotation;

        public int enabledExterior;

        public void Update()
        {
            if (this.triggered)
            {
                this.TriggerEvent();
                if (FlagUtils.IsCorrectFlag(this.parent.level, this.parent.retractFlag))
                {
                    this.RetractEvent();
                }
                if (this.parent.spritingMode == SpritingMode.MoveDisabledSprite)
                {
                    this.disabledFrame += Engine.DeltaTime * this.parent.disabledFrame;
                }
                else
                {
                    this.enabledFrame += Engine.DeltaTime * this.parent.enabledFrame;
                    this.enabledRotation += Engine.DeltaTime * this.parent.enabledRotationSpeed;
                }
            }
            else
            {
                this.lerp = Calc.Approach(this.lerp, 0F, this.parent.retractLerpMove * Engine.DeltaTime);
                this.disabledFrame += Engine.DeltaTime * this.parent.disabledFrame;
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

                this.disabledColor = spinner.GetHue(this.position);
                this.enabledColor = spinner.GetHue(this.position);
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

    public bool CanRefillDashOnTouch;
    public CrystalStaticSpinner Spinner;
    private int size;

    private DirectionMode direction;

    private SpikeType spikeType;

    private SpritingMode spritingMode;

    private string disabledSprite;

    private Color disabledColor;

    private float disabledAlpha;

    private float disabledScaleX;

    private float disabledScaleY;

    private float disabledRotation;

    private int disabledOrigin;

    private float disabledFrame;

    private string enabledSprite;

    private Color enabledColor;

    private float enabledAlpha;

    private float enabledScaleX;

    private float enabledScaleY;

    private float enabledRotationSpeed;

    private float enabledFrame;

    private int[] enabledExteriors;

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

    public bool trigger;

    private bool waitPlayerLeave;

    private bool attached;

    private bool grouped;

    private bool rainbow;

    private Level level;

    private Vector2 outwards;

    private SpikeInfo[] spikes;

    private Vector2 shakeOffset;

    private Vector2 disabledScale;

    private Vector2 enabledScale;

    private List<MTexture> disableds;

    private List<MTexture> enableds;

    private Color[] disabledColors;

    private Vector2 disabledOffset;

    private bool enterGrouped;

    private bool enterFlag;

    public static string DefaultTouchSound;

    public static string DefaultTriggerSound;

    public static string DefaultRetractSound;

    static AnimatedSpikes()
    {
        AnimatedSpikes.DefaultTouchSound = "event:/game/03_resort/fluff_tendril_touch";
        AnimatedSpikes.DefaultTriggerSound = "event:/game/03_resort/fluff_tendril_emerge";
        AnimatedSpikes.DefaultRetractSound = "";
    }

    private enum SpikeType
    {
        Dust,
        Tentacles,
        Custom
    }

    private enum SpritingMode
    {
        MoveDisabledSprite,
        ChangeSprite
    }

    public void Old(EntityData data)
    {
        return;

        //this.spritingMode = data.Enum<SpritingMode>("spritingMode", SpritingMode.MoveDisabledSprite);
        string oldMode = data.Attr("tentacleMode");
        if (oldMode == "OnlyTentacle") { this.spritingMode = SpritingMode.MoveDisabledSprite; }
        else if (oldMode == "TentacleAndTexture") { this.spritingMode = SpritingMode.ChangeSprite; }
        else { this.spritingMode = SpritingMode.MoveDisabledSprite; }

        this.disabledSprite = data.Attr("tentacleSprite"); //  null);
        this.disabledColor = data.HexColor("tentacleColor"); //  Color.White);
        this.disabledAlpha = data.Float("tentacleAlpha"); //  1F);
        this.disabledScaleX = data.Float("tentacleScaleX"); //  1F);
        this.disabledScaleY = data.Float("tentacleScaleY"); //  1F);
        this.disabledScale = new Vector2(this.disabledScaleX, this.disabledScaleY);
        this.disabledRotation = data.Float("tentacleRotation"); //  0F);
        this.disabledOrigin = data.Int("tentacleDisplacement"); //  -4);
        this.disabledFrame = data.Float("tentacleFrame"); //  1F);
        this.enabledSprite = data.Attr("textureSprite"); //  null);
        this.enabledColor = data.HexColor("textureColor", Color.White);
        this.enabledAlpha = data.Float("textureAlpha"); //  1F);
        this.enabledScaleX = data.Float("textureScaleX"); //  1F);
        this.enabledScaleY = data.Float("textureScaleY"); //  1F);
        this.enabledScale = new Vector2(this.enabledScaleX, this.enabledScaleY);
        this.enabledRotationSpeed = data.Float("textureRotationSpeed"); //  0F);
        this.enabledFrame = data.Float("textureFrame"); //  1F);
        string enabledExteriors = data.Attr("textureExteriors"); //  null);
        if (string.IsNullOrWhiteSpace(enabledExteriors))
        {
            enabledExteriors = "0";
        }
        this.enabledExteriors = StringUtils.SplitToIntArray(enabledExteriors);
        this.singleSize = data.Int("singleSize", 8); //  8);
        this.lerpMoveTime = data.Float("lerpMoveTime"); //  1F);
        this.triggerFlag = FlagUtils.Parse(data.Attr("triggerFlag")); //  null));
        this.triggerLerpMove = data.Float("triggerLerpMove"); //  8F);
        this.retractFlag = FlagUtils.Parse(data.Attr("retractFlag")); //  null));
        this.retractLerpMove = data.Float("retractLerpMove"); //  8F);

    }

    public AnimatedSpikes(Vector2 position, EntityData data, DirectionMode direction) : base(position)
    {
        this.direction = direction;
        this.spikeType = data.Enum<SpikeType>("spikeType", SpikeType.Dust);
        this.spritingMode = data.Enum<SpritingMode>("spritingMode", SpritingMode.MoveDisabledSprite);
        this.disabledSprite = data.Attr("disabledSprite", null);
        this.disabledColor = data.HexColor("disabledColor", Color.White);
        this.disabledAlpha = data.Float("disabledAlpha", 1F);
        this.disabledScaleX = data.Float("disabledScaleX", 1F);
        this.disabledScaleY = data.Float("disabledScaleY", 1F);
        this.disabledScale = new Vector2(this.disabledScaleX, this.disabledScaleY);
        this.disabledRotation = data.Float("disabledRotation", 0F);
        this.disabledOrigin = data.Int("disabledDisplacement", -4);
        this.disabledFrame = data.Float("disabledFrame", 1F);
        this.enabledSprite = data.Attr("enabledSprite", null);
        this.enabledColor = data.HexColor("enabledColor", Color.White);
        this.enabledAlpha = data.Float("enabledAlpha", 1F);
        this.enabledScaleX = data.Float("enabledScaleX", 1F);
        this.enabledScaleY = data.Float("enabledScaleY", 1F);
        this.enabledScale = new Vector2(this.enabledScaleX, this.enabledScaleY);
        this.enabledRotationSpeed = data.Float("enabledRotationSpeed", 0F);
        this.enabledFrame = data.Float("enabledFrame", 1F);
        string enabledExteriors = data.Attr("enabledDisplacements", null);
        if (string.IsNullOrWhiteSpace(enabledExteriors))
        {
            enabledExteriors = "0";
        }
        this.enabledExteriors = StringUtils.SplitToIntArray(enabledExteriors);
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

        Old(data); // Disabled?

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
        if (this.trigger)
        {
            base.Add(new SafeGroundBlocker());
        }
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
        defragmentFrameOffset = data.Bool("defragmentFrameOffset", false);

        Tag |= Tags.TransitionUpdate;
    }

    public string childMode;
    public DangerBubbler childModeParent = null;
    public bool childModeTriggered = false;
    private bool defragmentFrameOffset;

    public AnimatedSpikes(EntityData data, Vector2 offset, DirectionMode direction) : this(data.Position + offset, data, direction)
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
        // --> Migrated from Advanced Spikes

        int dir = 8 * (int)player.Facing;
        int left = (int)((player.Left + dir - base.Left) / 4F);
        int right = (int)((player.Right + dir - base.Left) / 4F);
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
        
        string direction = this.direction.ToString().ToLower();
        this.disableds = GFX.Game.GetAtlasSubtextures(this.GetTentaclesPath(direction));
        this.enableds = GFX.Game.GetAtlasSubtextures(this.GetTexturesPath(direction));
        this.disabledColors = [this.disabledColor];
        this.SpikeTypeSetup(scene);
        this.spikes = new SpikeInfo[this.size / this.singleSize];
        for (int i = 0; i < this.spikes.Length; i++)
        {
            this.spikes[i].parent = this;
            this.spikes[i].spikeIndex = i;
            this.spikes[i].triggered = !this.trigger;
            this.spikes[i].lerp = (this.trigger ? 0F : this.lerpMoveTime);
            this.spikes[i].disabledColor = this.disabledColors[Calc.Random.Next(this.disabledColors.Length)];
            this.spikes[i].disabledFrame = Calc.Random.NextFloat(this.disableds.Count);
            if (defragmentFrameOffset)
            {
                this.spikes[i].disabledFrame = float.Round(this.spikes[i].disabledFrame);
            }
            this.spikes[i].enabledFrame = Calc.Random.NextFloat(this.enableds.Count);
            if (defragmentFrameOffset)
            {
                this.spikes[i].enabledFrame = float.Round(this.spikes[i].enabledFrame);
            }
            this.spikes[i].enabledColor = this.enabledColor;
            this.spikes[i].enabledExterior = Calc.Random.Choose(this.enabledExteriors);
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

    private void SpikeTypeSetup(Scene scene)
    {
        if (this.spikeType == SpikeType.Dust)
        {
            this.spritingMode = SpritingMode.ChangeSprite;
            this.disabledScale = new Vector2(1F, -1F);
            this.disabledOrigin = 3;
            this.disabledFrame = 12F;
            this.disabledOffset = new Vector2(-2F, 0F);
            this.enabledScale = new Vector2(0.5F, 0.5F);
            this.enabledRotationSpeed = 1.2F;
            this.enabledFrame = 0F;
            this.enabledExteriors = [-4, -3, -1];
            this.singleSize = 4;
            switch (this.direction)
            {
                case DirectionMode.Down:
                    this.disabledScale = new Vector2(1F, 1F);
                    this.disabledOrigin = 0;
                    this.enabledExteriors = [-1, 0, 2];
                    break;
                case DirectionMode.Left:
                    this.disabledScale = new Vector2(-1F, 1F);
                    this.disabledOffset = new Vector2(0F, -2F);
                    this.enabledExteriors = [-4, -3, -1];
                    break;
                case DirectionMode.Right:
                    this.disabledOrigin = 0;
                    this.disabledOffset = new Vector2(0F, -3F);
                    this.enabledExteriors = [-1, 0, 2];
                    break;
            }
            Vector3[] edgeColors = DustStyles.Get(scene).EdgeColors;
            this.disabledColors = new Color[edgeColors.Length];
            for (int i = 0; i < this.disabledColors.Length; i++)
            {
                this.disabledColors[i] = Color.Lerp(new Color(edgeColors[i]), Color.DarkSlateBlue, 0.4F);
            }
            base.Add(new DustEdge(this.RenderTextureSpikes));
        }
        else if (this.spikeType == SpikeType.Tentacles)
        {
            this.spritingMode = SpritingMode.MoveDisabledSprite;
            this.disabledScale = new Vector2(-1F, 1F);
            this.disabledOrigin = 0;
            this.disabledFrame = 12F;
            this.enabledFrame = 0F;
            this.singleSize = 16;
            this.disabledOffset = new Vector2(4F, 18F);
            switch (this.direction)
            {
                case DirectionMode.Down:
                    this.disabledScale = new Vector2(-1F, -1F);
                    this.disabledOffset = new Vector2(4F, 8F);
                    break;
                case DirectionMode.Left:
                    this.disabledRotation = 4.8F;
                    this.disabledOffset = new Vector2(5F, 10F);
                    break;
                case DirectionMode.Right:
                    this.disabledRotation = 1.6F;
                    this.disabledOffset = new Vector2(-5F, 10F);
                    break;
            }
        }
    }

    private string GetTentaclesPath(string direction)
    {
        if (this.spikeType == SpikeType.Dust)
        {
            if (this.direction == DirectionMode.Up || this.direction == DirectionMode.Down)
            {
                return "danger/triggertentacle/wiggle_v";
            }
            else if (this.direction == DirectionMode.Left || this.direction == DirectionMode.Right)
            {
                return "danger/triggertentacle/wiggle_h";
            }
        }
        if (this.spikeType == SpikeType.Tentacles)
        {
            return "danger/tentacles";
        }
        return this.disabledSprite + "_" + direction;
    }

    private string GetTexturesPath(string direction) => (this.spikeType) switch
    {
        SpikeType.Dust => "danger/dustcreature/base",
        _ => this.enabledSprite + "_" + direction
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
            if ((this.spikes[i].triggered) && (this.spritingMode == SpritingMode.ChangeSprite))
            {
                continue;
            }
            MTexture mTexture = this.disableds[(int)(this.spikes[i].disabledFrame % this.disableds.Count)];
            Vector2 position = base.Position + vector * (4 + i * this.singleSize) + this.outwards * (this.disabledOrigin + this.spikes[i].lerp * 4F) + this.disabledOffset;
            mTexture.DrawJustified(position, justify, this.spikes[i].disabledColor * this.disabledAlpha, this.disabledScale, this.disabledRotation);
            if (this.spikeType == SpikeType.Dust)
            {
                mTexture.DrawJustified(position + vector, justify, Color.Black, this.disabledScale, this.disabledRotation);
            }
        }
        if (this.spritingMode == SpritingMode.ChangeSprite)
        {
            this.RenderTextureSpikes();
        }

        Position = pos;
    }

    private void RenderTextureSpikes()
    {
        Vector2 vector = new Vector2(Math.Abs(this.outwards.Y), Math.Abs(this.outwards.X));
        for (int i = 0; i < this.spikes.Length; i++)
        {
            if (!this.spikes[i].triggered)
            {
                continue;
            }
            MTexture mTexture = this.enableds[(int)(this.spikes[i].enabledFrame % this.enableds.Count)];
            Vector2 position = base.Position + vector * (2 + i * this.singleSize) + this.outwards * (this.disabledOrigin + this.spikes[i].lerp * this.spikes[i].enabledExterior);
            mTexture.DrawCentered(position, this.spikes[i].enabledColor * this.enabledAlpha, this.enabledScale * this.spikes[i].lerp, this.spikes[i].enabledRotation);
        }
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

}

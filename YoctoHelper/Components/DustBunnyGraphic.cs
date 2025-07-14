using System;
using System.Collections;
using System.Collections.Generic;
using YoctoHelper.Cores;
using YoctoHelper.Entities;

namespace YoctoHelper.Components;

[Tracked(false)]
public class DustBunnyGraphic : Component
{

    public CustomDustBunny dustBunny { get; private set; }

    private MTexture center { get; set; }

    private float rotationTimer { get; set; }

    public Vector2 scale { get; private set; }

    public Vector2 position;

    private float shakeTimer { get; set; }

    private Vector2 shakeValue { get; set; }

    public Vector2 renderPosition
    {
        get => base.Entity.Position + this.position + this.shakeValue;
    }

    private List<DustBunnyNode> nodes { get; set; }

    private DustBunnyEyeballs eyes { get; set; }

    public MTexture eyeTexture { get; set; }

    private int eyeTextureIndex { get; set; }

    public bool leftEyeVisible { get; set; }

    public bool rightEyeVisible { get; set; }

    public Vector2 eyeTargetDirection { get; set; }

    public Vector2 eyeDirection { get; set; }

    private Vector2 eyeLookRange { get; set; }

    private bool eyesMoveByRotation { get; set; }

    private bool eyesFollowPlayer { get; set; }

    private Coroutine blink { get; set; }

    public bool established { get; private set; }

    public Action OnEstablish { get; set; }

    private bool inView
    {
        get
        {
            if ((ObjectUtils.IsNull(base.Scene)) || (ObjectUtils.IsNull(base.Entity)))
            {
                return false;
            }
            Camera camera = (base.Scene as Level).Camera;
            Vector2 position = base.Entity.Position;
            return ((position.Y + 16F) >= camera.Top) && ((position.Y - 16F) <= camera.Bottom) && ((position.X + 16F) >= camera.Left) && ((position.X - 16F) <= camera.Right);
        }
    }

    private string centerTexture, overlayTexture, baseTexture;
    public DustBunnyGraphic(CustomDustBunny customDustBunny) : base(active: true, visible: true)
    {
        this.dustBunny = customDustBunny;
        centerTexture = customDustBunny.centerTexture;
        overlayTexture = customDustBunny.overlayTexture;
        baseTexture = customDustBunny.baseTexture;
        this.center = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(centerTexture));
        this.rotationTimer = Calc.Random.NextFloat();
        this.scale = Vector2.One;
        this.nodes = new List<DustBunnyNode>();
        if (this.dustBunny.hasEyes)
        {
            this.eyeTextureIndex = 1;
            this.leftEyeVisible = true;
            this.rightEyeVisible = true;
            this.eyeTargetDirection = (this.eyeDirection = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2F), 1F));
            this.eyesFollowPlayer = true;
        }
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        entity.Add(new TransitionListener()
        {
            OnIn = delegate
            {
                this.AddDustBunnyNodes();
            }
        });
        entity.Add(new DustBunnyEdge(this.Render));
    }

    public override void Update()
    {
        this.rotationTimer += (Engine.RawDeltaTime * 0.6F);
        if ((ObjectUtils.IsNotNull(base.Entity.Scene)) && (this.nodes.Count <= 0) && (!this.established))
        {
            this.AddDustBunnyNodes();
            return;
        }
        foreach (DustBunnyNode node in this.nodes)
        {
            node.rotation += (Engine.RawDeltaTime * 0.5F);
        }
        if (this.shakeTimer > 0F)
        {
            this.shakeTimer -= Engine.RawDeltaTime;
            if (this.shakeTimer <= 0F)
            {
                this.shakeValue = Vector2.Zero;
            }
            else if (base.Scene.OnInterval(0.05F))
            {
                this.shakeValue = Calc.Random.ShakeVector();
            }
        }
        if (this.dustBunny.hasEyes)
        {
            if ((this.eyeDirection != this.eyeTargetDirection) && this.inView)
            {
                if (!this.eyesMoveByRotation)
                {
                    this.eyeDirection = Calc.Approach(this.eyeDirection, this.eyeTargetDirection, 12F * Engine.RawDeltaTime);
                }
                else
                {
                    float eyeTargetDirectionAngle = this.eyeTargetDirection.Angle();
                    float eyeDirectionAngle = Calc.AngleApproach(this.eyeDirection.Angle(), eyeTargetDirectionAngle, 8F * Engine.RawDeltaTime);
                    this.eyeDirection = (eyeDirectionAngle == eyeTargetDirectionAngle) ? this.eyeTargetDirection : Calc.AngleToVector(eyeDirectionAngle, 1F);
                }
            }
            if ((this.eyesFollowPlayer) && (this.inView))
            {
                Player player = base.Entity.Scene.Tracker.GetEntity<Player>();
                if (ObjectUtils.IsNotNull(player))
                {
                    Vector2 position = (player.Position - base.Entity.Position).SafeNormalize();
                    this.eyeTargetDirection = (this.eyesMoveByRotation) ? Calc.AngleToVector(Calc.AngleApproach(this.eyeLookRange.Angle(), position.Angle(), (float)Math.PI / 4F), 1F) : position;
                }
            }
            if (ObjectUtils.IsNotNull(this.blink))
            {
                this.blink.Update();
            }
        }
    }

    private void AddDustBunnyNodes()
    {
        if ((this.nodes.Count > 0) || (!this.inView) || (DustBunnyEdges.DustBunnyGraphicEstabledCounter > 25) || (this.established))
        {
            return;
        }
        int entityX = (int)base.Entity.X;
        int entityY = (int)base.Entity.Y;
        Vector2 vector = Vector2.One.SafeNormalize();
        this.AddDustBunnyNode(new Vector2(0F - vector.X, 0F - vector.Y), !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(entityX - 8, entityY - 8, 8, 8)));
        this.AddDustBunnyNode(new Vector2(vector.X, 0F - vector.Y), !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(entityX, entityY - 8, 8, 8)));
        this.AddDustBunnyNode(new Vector2(0F - vector.X, vector.Y), !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(entityX - 8, entityY, 8, 8)));
        this.AddDustBunnyNode(new Vector2(vector.X, vector.Y), !base.Entity.Scene.CollideCheck<Solid>(new Rectangle(entityX, entityY, 8, 8)));
        if ((nodes[0].enabled) || (nodes[2].enabled))
        {
            this.position.X -= 1F;
        }
        if ((nodes[1].enabled) || (nodes[3].enabled))
        {
            this.position.X += 1F;
        }
        if ((nodes[0].enabled) || (nodes[1].enabled))
        {
            this.position.Y -= 1F;
        }
        if ((nodes[2].enabled) || (nodes[3].enabled))
        {
            this.position.Y += 1F;
        }
        if (this.dustBunny.hasEyes)
        {
            List<MTexture> eyeTextures = GFX.Game.GetAtlasSubtextures(DustStyles.Get(base.Scene).EyeTextures);
            this.eyeTexture = eyeTextures[this.eyeTextureIndex % eyeTextures.Count];
            base.Entity.Scene.Add(this.eyes = new DustBunnyEyeballs(this));
            this.blink = new Coroutine(this.BlinkRoutine(), true);
            int enabled = 0;
            foreach (DustBunnyNode node in this.nodes)
            {
                if (node.enabled)
                {
                    enabled++;
                }
            }
            if (this.eyesMoveByRotation = (enabled < 4))
            {
                this.eyeLookRange = Vector2.Zero;
                if (nodes[0].enabled)
                {
                    this.eyeLookRange += new Vector2(-1F, -1F).SafeNormalize();
                }
                if (nodes[1].enabled)
                {
                    this.eyeLookRange += new Vector2(1F, -1F).SafeNormalize();
                }
                if (nodes[2].enabled)
                {
                    this.eyeLookRange += new Vector2(-1F, 1F).SafeNormalize();
                }
                if (nodes[3].enabled)
                {
                    this.eyeLookRange += new Vector2(1F, 1F).SafeNormalize();
                }
                if ((enabled > 0) && (this.eyeLookRange.Length() > 0F))
                {
                    this.eyeLookRange /= enabled;
                    this.eyeLookRange = this.eyeLookRange.SafeNormalize();
                }
                this.eyeTargetDirection = (this.eyeDirection = this.eyeLookRange);
            }
        }
        DustBunnyEdges.DustBunnyGraphicEstabledCounter++;
        this.established = true;
        if (ObjectUtils.IsNotNull(this.OnEstablish))
        {
            this.OnEstablish();
        }
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return 2F + Calc.Random.NextFloat(1.5F);
            this.leftEyeVisible = false;
            yield return 0.02F + Calc.Random.NextFloat(0.05F);
            this.rightEyeVisible = false;
            yield return 0.25F;
            DustBunnyGraphic dustGraphic = this;
            DustBunnyGraphic dustGraphic2 = this;
            dustGraphic.leftEyeVisible = true;
            dustGraphic2.rightEyeVisible = true;
        }
    }

    private void AddDustBunnyNode(Vector2 angle, bool enabled)
    {
        Vector2 vector = Vector2.One;
        int x = Math.Sign(angle.X);
        int y = Math.Sign(angle.Y);
        base.Entity.Collidable = false;
        if ((base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.Entity.X - 4F + (x * 16)), (int)(base.Entity.Y - 4F + (y * 4)), 8, 8))) || (base.Scene.CollideCheck<CustomDustBunny>(new Rectangle((int)(base.Entity.X - 4F + (x * 16)), (int)(base.Entity.Y - 4F + (y * 4)), 8, 8))))
        {
            vector.X = 4F;
        }
        if ((base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.Entity.X - 4F + (x * 4)), (int)(base.Entity.Y - 4F + (y * 16)), 8, 8))) || (base.Scene.CollideCheck<CustomDustBunny>(new Rectangle((int)(base.Entity.X - 4F + (x * 4)), (int)(base.Entity.Y - 4F + (y * 16)), 8, 8))))
        {
            vector.Y = 4F;
        }
        base.Entity.Collidable = true;
        DustBunnyNode node = new DustBunnyNode();
        node.@base = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(baseTexture));
        node.overlay = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(overlayTexture));
        node.rotation = Calc.Random.NextFloat((float)Math.PI * 2F);
        node.angle = angle * vector;
        node.enabled = enabled;
        this.nodes.Add(node);
    }

    public override void Render()
    {
        if (!this.inView)
        {
            return;
        }
        this.center.DrawCentered(this.renderPosition, this.dustBunny.tintColor, this.scale, this.rotationTimer);
        foreach (DustBunnyNode node in this.nodes)
        {
            if (node.enabled)
            {
                node.@base.DrawCentered(this.renderPosition + node.angle * this.scale, this.dustBunny.tintColor, this.scale, node.rotation);
                node.overlay.DrawCentered(this.renderPosition + node.angle * this.scale, this.dustBunny.tintColor, this.scale, 0F - node.rotation);
            }
        }
    }

    public void OnHitPlayer()
    {
        if (SaveData.Instance.Assists.Invincible)
        {
            return;
        }
        this.shakeTimer = 0.6F;
        if (this.dustBunny.hasEyes)
        {
            this.blink = null;
            this.leftEyeVisible = true;
            this.rightEyeVisible = true;
        }
    }

}

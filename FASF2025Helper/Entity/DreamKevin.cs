using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using System.Reflection;
using FASF2025Helper.Utils;
using ChroniaHelper.Cores;
using static Celeste.CrushBlock;
using System.Collections;
using Celeste.Mod.CommunalHelper.Entities;

namespace FASF2025Helper.Entities;

[Tracked]
[CustomEntity("ChroniaHelper/DreamKevin")]
public class DreamKevin : Solid
{
    private struct DreamParticle
    {
        public Vector2 Position;

        public int Layer;

        public Color Color;

        public float TimeOffset;
    }

    private DreamTunnelBlocker dreamBlocker;
    private DreamBlock dreamBlock;
    //public DreamBlock DreamBlock => dreamBlock;

    private bool inDreamBlockState => !(fieldGetter_dreamTunnelDashAttacking != null && (bool)fieldGetter_dreamTunnelDashAttacking(null));
    private static Func<object, object> fieldGetter_dreamTunnelDashAttacking;

    private Level level;
    private bool canActivate;
    private Vector2 crushDir;
    private List<MoveState> returnStack;
    private Coroutine attackCoroutine;
    private Axes axe;
    private bool canMoveVertically;
    private bool canMoveHorizontally;

    private SoundSource currentMoveLoopSfx;
    private SoundSource returnLoopSfx;

    // kevin render
    private Sprite face;
    private string nextFaceDirection;
    private List<Image> idleImages = new List<Image>();
    private List<Image> activeTopImages = new List<Image>();
    private List<Image> activeRightImages = new List<Image>();
    private List<Image> activeLeftImages = new List<Image>();
    private List<Image> activeBottomImages = new List<Image>();

    // dream render
    private MTexture[] particleTextures;
    private DreamParticle[] particles;
    private float animTimer;
    private readonly Color backgroundColor = Color.Black;
    private readonly float backgroundAlpha = 1f;

    public DreamKevin(Vector2 position, Vector2 size, Axes axe)
        : base(position, size.X, size.Y, true)
    {
        OnDashCollide = OnDashed;
        returnStack = new List<MoveState>();
        canActivate = true;
        this.axe = axe;

        attackCoroutine = new Coroutine();
        attackCoroutine.RemoveOnComplete = false;
        Add(attackCoroutine);

        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/Fangame/FASF2025/DreamKevin/block");
        MTexture idle;
        switch (axe)
        {
        default:
            idle = atlasSubtextures[3];
            canMoveHorizontally = (canMoveVertically = true);
            break;
        case Axes.Horizontal:
            idle = atlasSubtextures[1];
            canMoveHorizontally = true;
            canMoveVertically = false;
            break;
        case Axes.Vertical:
            idle = atlasSubtextures[2];
            canMoveHorizontally = false;
            canMoveVertically = true;
            break;
        }
        Add(face = GFX.SpriteBank.Create("dreamkevin_face"));
        face.Position = new Vector2(base.Width, base.Height) / 2f;
        face.Play("idle");
        face.OnLastFrame = delegate (string f)
        {
            if (f == "hit")
            {
                face.Play(nextFaceDirection);
            }
        };
        int num = (int)(base.Width / 8f) - 1;
        int num2 = (int)(base.Height / 8f) - 1;
        AddImage(idle, 0, 0, 0, 0, -1, -1);
        AddImage(idle, num, 0, 3, 0, 1, -1);
        AddImage(idle, 0, num2, 0, 3, -1, 1);
        AddImage(idle, num, num2, 3, 3, 1, 1);
        for (int i = 1; i < num; i++)
        {
            AddImage(idle, i, 0, Calc.Random.Choose(1, 2), 0, 0, -1);
            AddImage(idle, i, num2, Calc.Random.Choose(1, 2), 3, 0, 1);
        }
        for (int j = 1; j < num2; j++)
        {
            AddImage(idle, 0, j, 0, Calc.Random.Choose(1, 2), -1);
            AddImage(idle, num, j, 3, Calc.Random.Choose(1, 2), 1);
        }

        Add(new LightOcclude(0.2f));
        Add(returnLoopSfx = new SoundSource());

        animTimer = 0f;
        particleTextures = new MTexture[4]
        {
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
        };
    }

    public DreamKevin(EntityData data, Vector2 offset)
        : this(data.Position + offset, new Vector2(data.Width, data.Height), data.Enum("axes", Axes.Both))
    {

    }

    [LoadHook]
    public static void Load()
    {
        if (Md.CommunalHelperLoaded)
        {
            HookCommunalHelper();
        }

        On.Celeste.DreamBlock.FootstepRipple += DreamBlock_FootstepRipple;
    }

    private static void HookCommunalHelper()
    {
        fieldGetter_dreamTunnelDashAttacking = DelegateHelper.CreateFieldGetter(typeof(Celeste.Mod.CommunalHelper.DashStates.DreamTunnelDash), "dreamTunnelDashAttacking");
        if (fieldGetter_dreamTunnelDashAttacking == null)
            Logger.Error("FASF2025/DelegateHelper", "Failed to create field getter for CommunalHelper.DashStates.DreamTunnelDash.dreamTunnelDashAttacking");
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.DreamBlock.FootstepRipple -= DreamBlock_FootstepRipple;
    }

    private static void DreamBlock_FootstepRipple(On.Celeste.DreamBlock.orig_FootstepRipple orig, DreamBlock self, Vector2 position)
    {
        DreamKevin dreamKevin = self.SceneAs<Level>().Tracker.GetNearestEntity<DreamKevin>(self.Position);
        if (dreamKevin != null && dreamKevin.dreamBlock == self)
        {
            return;
        }
        orig(self, position);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();

        if (!Md.CommunalHelperLoaded)
        {
            Logger.Log("FASF2025", "DreamKevin require CommunalHelper 1.23.0 loaded. Removed self");
            RemoveSelf();
            return;
        }

        EntityData data = new()
        {
            Position = Position,
            Width = (int)Width,
            Height = (int)Height,
        };
        dreamBlocker = new DreamTunnelBlocker(data, Vector2.Zero)
        {
            BlockDreamTunnelDashes = true,
        };


        dreamBlock = new DreamBlock(Position, Width, Height, null, false, false);
        dreamBlock.Visible = false;

        Scene.Add(dreamBlock);
        Scene.Add(dreamBlocker);

        particles = new DreamParticle[(int)((double)base.Width / 8.0 * ((double)base.Height / 8.0) * 0.699999988079071)];
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
            particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
            particles[i].TimeOffset = Calc.Random.NextFloat();
            particles[i].Color = Color.LightGray * (0.5f + (float)particles[i].Layer / 2f * 0.5f);

            switch (particles[i].Layer)
            {
            case 0:
                particles[i].Color = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                break;
            case 1:
                particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                break;
            case 2:
                particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
                break;
            }           
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        dreamBlocker?.RemoveSelf();
        dreamBlocker = null;

        dreamBlock?.RemoveSelf();
        dreamBlock = null;
    }

    public override void Update()
    {
        base.Update();

        if (inDreamBlockState)
        {
            dreamBlock.Collidable = true;
            Collidable = false;
        }
        else
        {
            dreamBlock.Collidable = false;
            Collidable = true;
        }

        animTimer += 6f * Engine.DeltaTime;
    }

    public override void Render()
    {
        #region DreamPraticles
        Camera camera = SceneAs<Level>().Camera;

        if (Right >= camera.Left && Left <= camera.Right && Bottom >= camera.Top && Top <= camera.Bottom)
        {
            Draw.Rect(X, Y, Width, Height, backgroundColor * backgroundAlpha);

            Vector2 cameraPosition = camera.Position;
            for (int i = 0; i < particles.Length; i++)
            {
                int layer = particles[i].Layer;
                Vector2 particlePos = particles[i].Position + cameraPosition * (0.3f + 0.25f * layer);
                particlePos = PutInside(particlePos);

                MTexture texture;
                if (layer == 0)
                {
                    int frame = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                    texture = particleTextures[3 - frame];
                }
                else if (layer == 1)
                {
                    int frame = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                    texture = particleTextures[1 + frame];
                }
                else
                {
                    texture = particleTextures[2];
                }

                if (particlePos.X >= X + 2f && particlePos.Y >= Y + 2f && particlePos.X < Right - 2f && particlePos.Y < Bottom - 2f)
                    texture.DrawCentered(particlePos, particles[i].Color);

            }
        }
        #endregion

        base.Render();
    }

    private Vector2 PutInside(Vector2 pos)
    {
        if (pos.X > base.Right)
        {
            pos.X -= (float)Math.Ceiling((pos.X - base.Right) / base.Width) * base.Width;
        }
        else if (pos.X < base.Left)
        {
            pos.X += (float)Math.Ceiling((base.Left - pos.X) / base.Width) * base.Width;
        }
        if (pos.Y > base.Bottom)
        {
            pos.Y -= (float)Math.Ceiling((pos.Y - base.Bottom) / base.Height) * base.Height;
        }
        else if (pos.Y < base.Top)
        {
            pos.Y += (float)Math.Ceiling((base.Top - pos.Y) / base.Height) * base.Height;
        }
        return pos;
    }

    private void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0)
    {
        MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8);
        Vector2 vector = new Vector2(x * 8, y * 8);
        if (borderX != 0)
        {
            Image image = new Image(subtexture);
            image.Color = Color.Black;
            image.Color.A = 0;
            image.Position = vector + new Vector2(borderX, 0f);
            Add(image);
        }
        if (borderY != 0)
        {
            Image image2 = new Image(subtexture);
            image2.Color = Color.Black;
            image2.Color.A = 0;
            image2.Position = vector + new Vector2(0f, borderY);
            Add(image2);
        }
        Image image3 = new Image(subtexture);
        image3.Position = vector;
        Add(image3);
        idleImages.Add(image3);
        if (borderX != 0 || borderY != 0)
        {
            string directory = "objects/Fangame/FASF2025/DreamKevin";

            if (borderX < 0)
            {
                Image image4 = new Image(GFX.Game[directory + "/lit_left"].GetSubtexture(0, ty * 8, 8, 8));
                activeLeftImages.Add(image4);
                image4.Position = vector;
                image4.Visible = false;
                Add(image4);
            }
            else if (borderX > 0)
            {
                Image image5 = new Image(GFX.Game[directory + "/lit_right"].GetSubtexture(0, ty * 8, 8, 8));
                activeRightImages.Add(image5);
                image5.Position = vector;
                image5.Visible = false;
                Add(image5);
            }
            if (borderY < 0)
            {
                Image image6 = new Image(GFX.Game[directory + "/lit_top"].GetSubtexture(tx * 8, 0, 8, 8));
                activeTopImages.Add(image6);
                image6.Position = vector;
                image6.Visible = false;
                Add(image6);
            }
            else if (borderY > 0)
            {
                Image image7 = new Image(GFX.Game[directory + "/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8));
                activeBottomImages.Add(image7);
                image7.Position = vector;
                image7.Visible = false;
                Add(image7);
            }
        }
    }

    private void TurnOffImages()
    {
        foreach (Image activeLeftImage in activeLeftImages)
        {
            activeLeftImage.Visible = false;
        }
        foreach (Image activeRightImage in activeRightImages)
        {
            activeRightImage.Visible = false;
        }
        foreach (Image activeTopImage in activeTopImages)
        {
            activeTopImage.Visible = false;
        }
        foreach (Image activeBottomImage in activeBottomImages)
        {
            activeBottomImage.Visible = false;
        }
    }

    private void ActivateParticles(Vector2 dir)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        int num;
        if (dir == Vector2.UnitX)
        {
            direction = 0f;
            position = base.CenterRight - Vector2.UnitX;
            positionRange = Vector2.UnitY * (base.Height - 2f) * 0.5f;
            num = (int)(base.Height / 8f) * 4;
        }
        else if (dir == -Vector2.UnitX)
        {
            direction = (float)Math.PI;
            position = base.CenterLeft + Vector2.UnitX;
            positionRange = Vector2.UnitY * (base.Height - 2f) * 0.5f;
            num = (int)(base.Height / 8f) * 4;
        }
        else if (dir == Vector2.UnitY)
        {
            direction = (float)Math.PI / 2f;
            position = base.BottomCenter - Vector2.UnitY;
            positionRange = Vector2.UnitX * (base.Width - 2f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }
        else
        {
            direction = -(float)Math.PI / 2f;
            position = base.TopCenter + Vector2.UnitY;
            positionRange = Vector2.UnitX * (base.Width - 2f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }
        num += 2;
        level.Particles.Emit(P_Activate, num, position, positionRange, direction);
    }

    private DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (CanActivate(-direction))
        {
            Attack(-direction);
            return DashCollisionResults.Rebound;
        }
        return DashCollisionResults.NormalCollision;
    }

    private bool CanActivate(Vector2 direction)
    {
        return canActivate && crushDir != direction &&
               (direction.X == 0f || canMoveHorizontally) &&
               (direction.Y == 0f || canMoveVertically);
    }

    private void Attack(Vector2 direction)
    {
        Audio.Play("event:/game/06_reflection/crushblock_activate", Center);

        if (currentMoveLoopSfx != null)
        {
            currentMoveLoopSfx.Param("end", 1f);
            SoundSource sfx = currentMoveLoopSfx;
            Alarm.Set(this, 0.5f, () => sfx.RemoveSelf());
        }

        Add(currentMoveLoopSfx = new SoundSource());
        currentMoveLoopSfx.Position = new Vector2(Width, Height) / 2f;
        currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop");

        crushDir = direction;
        canActivate = false;

        face.Play("hit");
        TurnOffImages();
        ActivateParticles(crushDir);

        attackCoroutine.Replace(AttackSequence());
        ClearRemainder();

        bool uniqueDirection = returnStack.Count == 0 ||
             (returnStack[returnStack.Count - 1].Direction != direction &&
              returnStack[returnStack.Count - 1].Direction != -direction);

        if (uniqueDirection)
            returnStack.Add(new MoveState(Position, crushDir));

        if (crushDir.X < 0f)
        {
            foreach (Image activeLeftImage in activeLeftImages)
            {
                activeLeftImage.Visible = true;
            }
            nextFaceDirection = "left";
        }
        else if (crushDir.X > 0f)
        {
            foreach (Image activeRightImage in activeRightImages)
            {
                activeRightImage.Visible = true;
            }
            nextFaceDirection = "right";
        }
        else if (crushDir.Y < 0f)
        {
            foreach (Image activeTopImage in activeTopImages)
            {
                activeTopImage.Visible = true;
            }
            nextFaceDirection = "up";
        }
        else if (crushDir.Y > 0f)
        {
            foreach (Image activeBottomImage in activeBottomImages)
            {
                activeBottomImage.Visible = true;
            }
            nextFaceDirection = "down";
        }
/*        bool flag = true;
        if (returnStack.Count > 0)
        {
            MoveState moveState = returnStack[returnStack.Count - 1];
            if (moveState.Direction == direction || moveState.Direction == -direction)
            {
                flag = false;
            }
        }
        if (flag)
        {
            returnStack.Add(new MoveState(Position, crushDir));
        }*/
    }

    private IEnumerator AttackSequence()
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        StartShaking(0.4f);
        yield return 0.4f;

        canActivate = true;
        StopPlayerRunIntoAnimation = false;
        float speed = 0f;

        // 挤压移动阶段
        while (true)
        {
            speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);

            // 临时禁用果冻块的碰撞检测
            bool originalDreamBlockCollidable = dreamBlock.Collidable;
            bool originalKevinCollidable = Collidable;
            dreamBlock.Collidable = false;
            Collidable = true;
            bool collision = (crushDir.X == 0f) ?
                MoveVCheck(speed * crushDir.Y * Engine.DeltaTime) :
                MoveHCheck(speed * crushDir.X * Engine.DeltaTime);
            dreamBlock.Collidable = originalDreamBlockCollidable;
            Collidable = originalKevinCollidable;
            // 同步位置
            dreamBlock.Position = Position;
            dreamBlocker.Position = Position;

            if (Top >= level.Bounds.Bottom + 32)
            {
                RemoveSelf();
                yield break;
            }
            if (collision) break;

            if (Scene.OnInterval(0.02f))
            {
                Vector2 position;
                float direction;
                if (crushDir == Vector2.UnitX)
                {
                    position = new Vector2(Left + 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                    direction = (float)Math.PI;
                }
                else if (crushDir == -Vector2.UnitX)
                {
                    position = new Vector2(Right - 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                    direction = 0f;
                }
                else if (crushDir == Vector2.UnitY)
                {
                    position = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Top + 1f);
                    direction = -(float)Math.PI / 2f;
                }
                else
                {
                    position = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Bottom - 1f);
                    direction = (float)Math.PI / 2f;
                }
                level.Particles.Emit(P_Crushing, position, direction);
            }

            yield return null;
        }

        // 碰撞后处理
        FallingBlock fallingBlock = CollideFirst<FallingBlock>(Position + crushDir);
        if (fallingBlock != null) fallingBlock.Triggered = true;

        if (crushDir == -Vector2.UnitX)
        {
            Vector2 vector = new Vector2(0f, 2f);
            for (int i = 0; (float)i < Height / 8f; i++)
            {
                Vector2 vector2 = new Vector2(Left - 1f, Top + 4f + (float)(i * 8));
                if (!Scene.CollideCheck<Water>(vector2) && Scene.CollideCheck<Solid>(vector2))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector2 + vector, 0f);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector2 - vector, 0f);
                }
            }
        }
        else if (crushDir == Vector2.UnitX)
        {
            Vector2 vector3 = new Vector2(0f, 2f);
            for (int j = 0; (float)j < Height / 8f; j++)
            {
                Vector2 vector4 = new Vector2(Right + 1f, Top + 4f + (float)(j * 8));
                if (!Scene.CollideCheck<Water>(vector4) && Scene.CollideCheck<Solid>(vector4))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector4 + vector3, (float)Math.PI);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector4 - vector3, (float)Math.PI);
                }
            }
        }
        else if (crushDir == -Vector2.UnitY)
        {
            Vector2 vector5 = new Vector2(2f, 0f);
            for (int k = 0; (float)k < Width / 8f; k++)
            {
                Vector2 vector6 = new Vector2(Left + 4f + (float)(k * 8), Top - 1f);
                if (!Scene.CollideCheck<Water>(vector6) && Scene.CollideCheck<Solid>(vector6))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector6 + vector5, (float)Math.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector6 - vector5, (float)Math.PI / 2f);
                }
            }
        }
        else if (crushDir == Vector2.UnitY)
        {
            Vector2 vector7 = new Vector2(2f, 0f);
            for (int l = 0; (float)l < Width / 8f; l++)
            {
                Vector2 vector8 = new Vector2(Left + 4f + (float)(l * 8), Bottom + 1f);
                if (!Scene.CollideCheck<Water>(vector8) && Scene.CollideCheck<Solid>(vector8))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector8 + vector7, -(float)Math.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector8 - vector7, -(float)Math.PI / 2f);
                }
            }
        }

        Audio.Play("event:/game/06_reflection/crushblock_impact", Center);
        level.DirectionalShake(crushDir);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        StartShaking(0.4f);
        StopPlayerRunIntoAnimation = true;

        TurnOffImages();
        face.Play("hurt");

        if (currentMoveLoopSfx != null)
        {
            SoundSource sfx = currentMoveLoopSfx;
            currentMoveLoopSfx.Param("end", 1f);
            currentMoveLoopSfx = null;
            Alarm.Set(this, 0.5f, () => sfx.RemoveSelf());
        }

        crushDir = Vector2.Zero;

        // 返回阶段
        returnLoopSfx.Play("event:/game/06_reflection/crushblock_return_loop");
        yield return 0.4f;

        speed = 0f;
        float waypointSfxDelay = 0f;
        while (returnStack.Count > 0)
        {
            yield return null;
            StopPlayerRunIntoAnimation = false;
            MoveState move = returnStack[returnStack.Count - 1];
            speed = Calc.Approach(speed, 60f, 160f * Engine.DeltaTime);
            waypointSfxDelay -= Engine.DeltaTime;

            // 同步位置
            if (move.Direction.X != 0f)
            {
                MoveTowardsX(move.From.X, speed * Engine.DeltaTime);
                dreamBlock.MoveTowardsX(move.From.X, speed * Engine.DeltaTime);
            }
            if (move.Direction.Y != 0f)
            {
                MoveTowardsY(move.From.Y, speed * Engine.DeltaTime);
                dreamBlock.MoveTowardsY(move.From.Y, speed * Engine.DeltaTime);
            }
            dreamBlock.Position = Position;
            dreamBlocker.Position = Position;

            if ((move.Direction.X == 0f || ExactPosition.X == move.From.X) &&
                (move.Direction.Y == 0f || ExactPosition.Y == move.From.Y))
            {
                speed = 0f;
                returnStack.RemoveAt(returnStack.Count - 1);
                StopPlayerRunIntoAnimation = true;

                if (returnStack.Count == 0)
                {
                    returnLoopSfx.Stop();
                    if (waypointSfxDelay <= 0f)
                        Audio.Play("event:/game/06_reflection/crushblock_rest", Center);
                }
                else if (waypointSfxDelay <= 0f)
                {
                    Audio.Play("event:/game/06_reflection/crushblock_rest_waypoint", Center);
                }

                waypointSfxDelay = 0.1f;
                StartShaking(0.2f);
                yield return 0.2f;
            }
        }
    }

    private bool MoveHCheck(float amount)
    {
        if (MoveHCollideSolidsAndBounds(level, amount, thruDashBlocks: true))
        {
            // 同步果冻块位置
            dreamBlock.MoveHCollideSolidsAndBounds(level, amount, thruDashBlocks: true);

            if ((amount < 0f && Left <= level.Bounds.Left) ||
                (amount > 0f && Right >= level.Bounds.Right)) return true;

            for (int i = 1; i <= 4; i++)
            {
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector2 offset = new Vector2(Math.Sign(amount), i * sign);
                    if (!CollideCheck<Solid>(Position + offset))
                    {
                        MoveVExact(i * sign);
                        MoveHExact(Math.Sign(amount));

                        // 同步果冻块位置
                        dreamBlock.MoveVExact(i * sign);
                        dreamBlock.MoveHExact(Math.Sign(amount));

                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    private bool MoveVCheck(float amount)
    {
        if (MoveVCollideSolidsAndBounds(level, amount, thruDashBlocks: true, null, checkBottom: false))
        {
            // 同步果冻块位置
            dreamBlock.MoveVCollideSolidsAndBounds(level, amount, thruDashBlocks: true, null, checkBottom: false);

            if (amount < 0f && Top <= level.Bounds.Top) return true;

            for (int i = 1; i <= 4; i++)
            {
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector2 offset = new Vector2(i * sign, Math.Sign(amount));
                    if (!CollideCheck<Solid>(Position + offset))
                    {
                        MoveHExact(i * sign);
                        MoveVExact(Math.Sign(amount));

                        // 同步果冻块位置
                        dreamBlock.MoveVExact(i * sign);
                        dreamBlock.MoveHExact(Math.Sign(amount));

                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }
}


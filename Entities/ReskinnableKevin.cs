using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using FASF2025Helper.Entities;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/ReskinnableKevin")]
public class ReskinnableKevin : Solid
{
    public enum Axes
    {
        Both,
        Horizontal,
        Vertical
    }

    public struct MoveState
    {
        public Vector2 From;

        public Vector2 Direction;

        public MoveState(Vector2 from, Vector2 direction)
        {
            From = from;
            Direction = direction;
        }
    }

    public const float CrushSpeed = 240f;

    public const float CrushAccel = 500f;

    public const float ReturnSpeed = 60f;

    public const float ReturnAccel = 160f;

    public Color fill = Calc.HexToColor("62222b");

    public Level level;

    public bool canActivate;

    public Vector2 crushDir;

    public List<MoveState> returnStack;

    public Coroutine attackCoroutine;

    public bool canMoveVertically;

    public bool canMoveHorizontally;

    public bool chillOut;

    public bool giant;

    public Sprite face;

    public string nextFaceDirection;

    public List<Image> idleImages = new List<Image>();

    public List<Image> activeTopImages = new List<Image>();

    public List<Image> activeRightImages = new List<Image>();

    public List<Image> activeLeftImages = new List<Image>();

    public List<Image> activeBottomImages = new List<Image>();

    public SoundSource currentMoveLoopSfx;

    public SoundSource returnLoopSfx;

    public bool Submerged
    {
        get
        {
            return base.Scene.CollideCheck<Water>(new Rectangle((int)(base.Center.X - 4f), (int)base.Center.Y, 8, 4));
        }
    }

    public ReskinnableKevin(Vector2 position, float width, float height, Axes axes, EntityData data, bool chillOut = false)
        : base(position, width, height, safe: false)
    {
        dir = data.Attr("spriteDirectory", "objects/ChroniaHelper/reskinnableKevin/");
        fill = Calc.HexToColor(data.Attr("fillerColor", "62222b"));
        
        OnDashCollide = OnDashed;
        returnStack = new List<MoveState>();
        this.chillOut = chillOut;
        giant = base.Width >= 48f && base.Height >= 48f && chillOut;
        canActivate = true;
        attackCoroutine = new Coroutine();
        attackCoroutine.RemoveOnComplete = false;
        Add(attackCoroutine);
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(dir + "block");
        MTexture idle;
        switch (axes)
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

        string f = data.Attr("faceSpriteXML", "kevin_face");
        string bf = data.Attr("giantFaceSpriteXML", "big_kevin_face");
        Add(face = GFX.SpriteBank.Create(giant? bf: f));
        face.Position = new Vector2(base.Width, base.Height) / 2f;
        face.Play("idle");
        face.OnLastFrame = (string f) =>
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
        Add(new WaterInteraction(() => crushDir != Vector2.Zero));
    }
    private string dir;

    public ReskinnableKevin(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Enum("axes", Axes.Both), data, data.Bool("chillout"))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
        if (crushDir == Vector2.Zero)
        {
            face.Position = new Vector2(base.Width, base.Height) / 2f;
            if (CollideCheck<Player>(Position + new Vector2(-1f, 0f)))
            {
                face.X -= 1f;
            }
            else if (CollideCheck<Player>(Position + new Vector2(1f, 0f)))
            {
                face.X += 1f;
            }
            else if (CollideCheck<Player>(Position + new Vector2(0f, -1f)))
            {
                face.Y -= 1f;
            }
        }

        if (currentMoveLoopSfx != null)
        {
            currentMoveLoopSfx.Param("submerged", Submerged ? 1 : 0);
        }

        if (returnLoopSfx != null)
        {
            returnLoopSfx.Param("submerged", Submerged ? 1 : 0);
        }
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position += base.Shake;
        Draw.Rect(base.X + 2f, base.Y + 2f, base.Width - 4f, base.Height - 4f, fill);
        base.Render();
        Position = position;
    }

    public void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0)
    {
        MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8);
        Vector2 vector = new Vector2(x * 8, y * 8);
        if (borderX != 0)
        {
            Image image = new Image(subtexture);
            image.Color = Color.Black;
            image.Position = vector + new Vector2(borderX, 0f);
            Add(image);
        }

        if (borderY != 0)
        {
            Image image2 = new Image(subtexture);
            image2.Color = Color.Black;
            image2.Position = vector + new Vector2(0f, borderY);
            Add(image2);
        }

        Image image3 = new Image(subtexture);
        image3.Position = vector;
        Add(image3);
        idleImages.Add(image3);
        if (borderX != 0 || borderY != 0)
        {
            if (borderX < 0)
            {
                Image image4 = new Image(GFX.Game[dir + "lit_left"].GetSubtexture(0, ty * 8, 8, 8));
                activeLeftImages.Add(image4);
                image4.Position = vector;
                image4.Visible = false;
                Add(image4);
            }
            else if (borderX > 0)
            {
                Image image5 = new Image(GFX.Game[dir + "lit_right"].GetSubtexture(0, ty * 8, 8, 8));
                activeRightImages.Add(image5);
                image5.Position = vector;
                image5.Visible = false;
                Add(image5);
            }

            if (borderY < 0)
            {
                Image image6 = new Image(GFX.Game[dir + "lit_top"].GetSubtexture(tx * 8, 0, 8, 8));
                activeTopImages.Add(image6);
                image6.Position = vector;
                image6.Visible = false;
                Add(image6);
            }
            else if (borderY > 0)
            {
                Image image7 = new Image(GFX.Game[dir + "lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8));
                activeBottomImages.Add(image7);
                image7.Position = vector;
                image7.Visible = false;
                Add(image7);
            }
        }
    }

    public void TurnOffImages()
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

    public DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (CanActivate(-direction))
        {
            Attack(-direction);
            return DashCollisionResults.Rebound;
        }

        return DashCollisionResults.NormalCollision;
    }

    public bool CanActivate(Vector2 direction)
    {
        if (giant && direction.X <= 0f)
        {
            return false;
        }

        if (canActivate && crushDir != direction)
        {
            if (direction.X != 0f && !canMoveHorizontally)
            {
                return false;
            }

            if (direction.Y != 0f && !canMoveVertically)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public void Attack(Vector2 direction)
    {
        Audio.Play("event:/game/06_reflection/crushblock_activate", base.Center);
        if (currentMoveLoopSfx != null)
        {
            currentMoveLoopSfx.Param("end", 1f);
            SoundSource sfx = currentMoveLoopSfx;
            Alarm.Set(this, 0.5f, () =>
            {
                sfx.RemoveSelf();
            });
        }

        Add(currentMoveLoopSfx = new SoundSource());
        currentMoveLoopSfx.Position = new Vector2(base.Width, base.Height) / 2f;
        if (SaveData.Instance != null && SaveData.Instance.Name != null && SaveData.Instance.Name.StartsWith("FWAHAHA", StringComparison.InvariantCultureIgnoreCase))
        {
            currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop_covert");
        }
        else
        {
            currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop");
        }

        face.Play("hit");
        crushDir = direction;
        canActivate = false;
        attackCoroutine.Replace(AttackSequence());
        ClearRemainder();
        TurnOffImages();
        ActivateParticles(crushDir);
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

        bool flag = true;
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
        }
    }

    public void ActivateParticles(Vector2 dir)
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
            direction = MathF.PI;
            position = base.CenterLeft + Vector2.UnitX;
            positionRange = Vector2.UnitY * (base.Height - 2f) * 0.5f;
            num = (int)(base.Height / 8f) * 4;
        }
        else if (dir == Vector2.UnitY)
        {
            direction = MathF.PI / 2f;
            position = base.BottomCenter - Vector2.UnitY;
            positionRange = Vector2.UnitX * (base.Width - 2f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }
        else
        {
            direction = -MathF.PI / 2f;
            position = base.TopCenter + Vector2.UnitY;
            positionRange = Vector2.UnitX * (base.Width - 2f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }

        num += 2;
        level.Particles.Emit(CrushBlock.P_Activate, num, position, positionRange, direction);
    }

    public IEnumerator AttackSequence()
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        StartShaking(0.4f);
        yield return 0.4f;
        if (!chillOut)
        {
            canActivate = true;
        }

        StopPlayerRunIntoAnimation = false;
        bool slowing = false;
        float speed = 0f;
        while (true)
        {
            if (!chillOut)
            {
                speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
            }
            else if (slowing || CollideCheck<SolidTiles>(Position + crushDir * 256f))
            {
                speed = Calc.Approach(speed, 24f, 500f * Engine.DeltaTime * 0.25f);
                if (!slowing)
                {
                    slowing = true;
                    Alarm.Set(this, 0.5f, () =>
                    {
                        face.Play("hurt");
                        SoundSource soundSource = currentMoveLoopSfx;
                        if (soundSource != null)
                        {
                            soundSource = soundSource.Stop();
                        }

                        TurnOffImages();
                    });
                }
            }
            else
            {
                speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
            }

            bool flag = ((crushDir.X == 0f) ? MoveVCheck(speed * crushDir.Y * Engine.DeltaTime) : MoveHCheck(speed * crushDir.X * Engine.DeltaTime));
            if (Top >= (float)(level.Bounds.Bottom + 32))
            {
                RemoveSelf();
                yield break;
            }

            if (flag)
            {
                break;
            }

            if (Scene.OnInterval(0.02f))
            {
                Vector2 position;
                float direction;
                if (crushDir == Vector2.UnitX)
                {
                    position = new Vector2(Left + 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                    direction = MathF.PI;
                }
                else if (crushDir == -Vector2.UnitX)
                {
                    position = new Vector2(Right - 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                    direction = 0f;
                }
                else if (crushDir == Vector2.UnitY)
                {
                    position = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Top + 1f);
                    direction = -MathF.PI / 2f;
                }
                else
                {
                    position = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Bottom - 1f);
                    direction = MathF.PI / 2f;
                }

                level.Particles.Emit(CrushBlock.P_Crushing, position, direction);
            }

            yield return null;
        }

        FallingBlock fallingBlock = CollideFirst<FallingBlock>(Position + crushDir);
        if (fallingBlock != null)
        {
            fallingBlock.Triggered = true;
        }

        if (crushDir == -Vector2.UnitX)
        {
            Vector2 vector = new Vector2(0f, 2f);
            for (int i = 0; (float)i < Height / 8f; i++)
            {
                Vector2 vector2 = new Vector2(Left - 1f, Top + 4f + (float)(i * 8));
                if (!Scene.CollideCheck<Water>(vector2) && Scene.CollideCheck<Solid>(vector2))
                {
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector2 + vector, 0f);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector2 - vector, 0f);
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
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector4 + vector3, MathF.PI);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector4 - vector3, MathF.PI);
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
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector6 + vector5, MathF.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector6 - vector5, MathF.PI / 2f);
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
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector8 + vector7, -MathF.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, vector8 - vector7, -MathF.PI / 2f);
                }
            }
        }

        Audio.Play("event:/game/06_reflection/crushblock_impact", Center);
        level.DirectionalShake(crushDir);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        StartShaking(0.4f);
        StopPlayerRunIntoAnimation = true;
        SoundSource sfx = currentMoveLoopSfx;
        currentMoveLoopSfx.Param("end", 1f);
        currentMoveLoopSfx = null;
        Alarm.Set(this, 0.5f, () =>
        {
            sfx.RemoveSelf();
        });
        crushDir = Vector2.Zero;
        TurnOffImages();
        if (chillOut)
        {
            yield break;
        }

        face.Play("hurt");
        returnLoopSfx.Play("event:/game/06_reflection/crushblock_return_loop");
        yield return 0.4f;
        speed = 0f;
        float waypointSfxDelay = 0f;
        while (returnStack.Count > 0)
        {
            yield return null;
            StopPlayerRunIntoAnimation = false;
            MoveState moveState = returnStack[returnStack.Count - 1];
            speed = Calc.Approach(speed, 60f, 160f * Engine.DeltaTime);
            waypointSfxDelay -= Engine.DeltaTime;
            if (moveState.Direction.X != 0f)
            {
                MoveTowardsX(moveState.From.X, speed * Engine.DeltaTime);
            }

            if (moveState.Direction.Y != 0f)
            {
                MoveTowardsY(moveState.From.Y, speed * Engine.DeltaTime);
            }

            if ((moveState.Direction.X != 0f && ExactPosition.X != moveState.From.X) || (moveState.Direction.Y != 0f && ExactPosition.Y != moveState.From.Y))
            {
                continue;
            }

            speed = 0f;
            returnStack.RemoveAt(returnStack.Count - 1);
            StopPlayerRunIntoAnimation = true;
            if (returnStack.Count <= 0)
            {
                face.Play("idle");
                returnLoopSfx.Stop();
                if (waypointSfxDelay <= 0f)
                {
                    Audio.Play("event:/game/06_reflection/crushblock_rest", Center);
                }
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

    public bool MoveHCheck(float amount)
    {
        if (MoveHCollideSolidsAndBounds(level, amount, thruDashBlocks: true))
        {
            if (amount < 0f && base.Left <= (float)level.Bounds.Left)
            {
                return true;
            }

            if (amount > 0f && base.Right >= (float)level.Bounds.Right)
            {
                return true;
            }

            for (int i = 1; i <= 4; i++)
            {
                for (int num = 1; num >= -1; num -= 2)
                {
                    Vector2 vector = new Vector2(Math.Sign(amount), i * num);
                    if (!CollideCheck<Solid>(Position + vector))
                    {
                        MoveVExact(i * num);
                        MoveHExact(Math.Sign(amount));
                        return false;
                    }
                }
            }

            return true;
        }

        return false;
    }

    public bool MoveVCheck(float amount)
    {
        if (MoveVCollideSolidsAndBounds(level, amount, thruDashBlocks: true, null, checkBottom: false))
        {
            if (amount < 0f && base.Top <= (float)level.Bounds.Top)
            {
                return true;
            }

            for (int i = 1; i <= 4; i++)
            {
                for (int num = 1; num >= -1; num -= 2)
                {
                    Vector2 vector = new Vector2(i * num, Math.Sign(amount));
                    if (!CollideCheck<Solid>(Position + vector))
                    {
                        MoveHExact(i * num);
                        MoveVExact(Math.Sign(amount));
                        return false;
                    }
                }
            }

            return true;
        }

        return false;
    }
}

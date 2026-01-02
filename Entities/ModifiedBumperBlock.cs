using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using VivHelper;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/ModifiedBumperBlock")]
public class ModifiedBumperBlock : Solid
{
    public enum Axes
    {
        Both,
        Horizontal,
        Vertical
    }

    private bool canBumpHorizontally = false;
    private bool canBumpVertically = false;
    private bool canActivate = true;
    private Sprite face;
    private List<Image> idleImages = new List<Image>();
    private List<Image> activeTopImages = new List<Image>();
    private List<Image> activeRightImages = new List<Image>();
    private List<Image> activeLeftImages = new List<Image>();
    private List<Image> activeBottomImages = new List<Image>();

    /// <summary>
    /// Bump = 0, Bounce = 1, Normal = 2
    /// </summary>
    private int dashCollision = 0;
    /// <summary>
    /// Bump = 0, Bounce = 1, Normal = 2
    /// </summary>
    private int normalCollision = 2;

    public ModifiedBumperBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, false)
    {
        OnDashCollide = new DashCollision(OnDashed);

        Add(face = GFX.SpriteBank.Create("crystalline_flcc_bumperBlock_face"));
        face.Position = new Vector2(Width, Height) / 2f;
        face.Play("idle");
        face.OnLastFrame = (frame) =>
        {
            if (frame == "hit")
            {
                TurnOffImages();
                canActivate = true;
            }
        };

        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/CrystallineHelper/FLCC/bumperBlock/block");
        MTexture idle;

        Axes axes = data.Enum("axes", Axes.Both);
        switch (axes)
        {
            default:
                idle = atlasSubtextures[3];
                this.canBumpHorizontally = true;
                this.canBumpVertically = true;
                break;
            case Axes.Horizontal:
                idle = atlasSubtextures[1];
                this.canBumpHorizontally = true;
                this.canBumpVertically = false;
                break;
            case Axes.Vertical:
                idle = atlasSubtextures[2];
                this.canBumpHorizontally = false;
                this.canBumpVertically = true;
                break;
        }

        int num = (int)(Width / 8f) - 1;
        int num2 = (int)(Height / 8f) - 1;

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
            AddImage(idle, 0, j, 0, Calc.Random.Choose(1, 2), -1, 0);
            AddImage(idle, num, j, 3, Calc.Random.Choose(1, 2), 1, 0);
        }

        Add(new LightOcclude(0.2f));

        dashCollision = data.Int("onDashCollide", 0);
        normalCollision = data.Int("onNormalCollide", 2);
        if (normalCollision != 2)
        {
            OnCollide += OnCollideWithPlayer;
        }

        //upC = new Hitbox(Width, 2f, 0f, -2f);
        //downC = new Hitbox(Width, 2f, 0f, Height);
        //leftC = new Hitbox(2f, Height, -2f, 0);
        //rightC = new Hitbox(2f, Height, Width, 0f);
    }
    private Collider upC, downC, leftC, rightC;

    private void OnCollideWithPlayer(Vector2 dir)
    {
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player != null && canActivate)
        {
            Add(new Coroutine(WaitForOneFrameThenBouncePlayer(player, -dir)));
        }
    }

    private IEnumerator WaitForOneFrameThenBouncePlayer(Player player, Vector2 direction)
    {
        if(normalCollision == 1)
        {
            SpringBouncePlayer(player, direction);
        }
        else
        {
            BouncePlayer(player, direction);
        }
        
        yield break;
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position += Shake;
        Draw.Rect(X + 2f, Y + 2f, Width - 4f, Height - 4f, Calc.HexToColor("62222b"));
        base.Render();
        Position = position;
    }

    public override void Update()
    {
        base.Update();
        Player player = Scene.Tracker.GetEntity<Player>();

        if (player == null) { return; }

        //bool arg = player.Collider.Collide(upC) || player.Collider.Collide(downC)
        //    || player.Collider.Collide(leftC) || player.Collider.Collide(rightC);

        //if (arg)
        //{
        //    OnCollideWithPlayer(player.Center);
        //}

        bool argL = player.Right >= Left - 2f && player.Right <= Left && player.CenterY >= Math.Min(Top, Bottom) && player.CenterY <= Math.Max(Top, Bottom);
        bool argU = player.Bottom >= Top - 2f && player.Bottom <= Top && player.CenterX >= Math.Min(Left, Right) && player.CenterX <= Math.Max(Left, Right);
        bool argD = player.Top >= Bottom && player.Top <= Bottom + 2f && player.CenterX >= Math.Min(Left, Right) && player.CenterX <= Math.Max(Left, Right);
        bool argR = player.Left >= Right && player.Left <= Right + 2f && player.CenterY >= Math.Min(Top, Bottom) && player.CenterY <= Math.Max(Top, Bottom);

        if (argL)
        {
            OnCollideWithPlayer(Vc2.UnitX);
        }
        if (argU)
        {
            OnCollideWithPlayer(Vc2.UnitY);
        }
        if (argR)
        {
            OnCollideWithPlayer(-Vc2.UnitX);
        }
        if (argD)
        {
            OnCollideWithPlayer(-Vc2.UnitY);
        }
    }

    private void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0)
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
                Image image4 = new Image(GFX.Game["objects/CrystallineHelper/FLCC/bumperBlock/lit_left"].GetSubtexture(0, ty * 8, 8, 8));
                activeLeftImages.Add(image4);
                image4.Position = vector;
                image4.Visible = false;
                Add(image4);
            }
            else if (borderX > 0)
            {
                Image image5 = new Image(GFX.Game["objects/CrystallineHelper/FLCC/bumperBlock/lit_right"].GetSubtexture(0, ty * 8, 8, 8));
                activeRightImages.Add(image5);
                image5.Position = vector;
                image5.Visible = false;
                Add(image5);
            }

            if (borderY < 0)
            {
                Image image6 = new Image(GFX.Game["objects/CrystallineHelper/FLCC/bumperBlock/lit_top"].GetSubtexture(tx * 8, 0, 8, 8));
                activeTopImages.Add(image6);
                image6.Position = vector;
                image6.Visible = false;
                Add(image6);
            }
            else if (borderY > 0)
            {
                Image image7 = new Image(GFX.Game["objects/CrystallineHelper/FLCC/bumperBlock/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8));
                activeBottomImages.Add(image7);
                image7.Position = vector;
                image7.Visible = false;
                Add(image7);
            }
        }
    }

    private void TurnOffImages()
    {
        foreach (Image image in activeLeftImages)
            image.Visible = false;
        foreach (Image image in activeRightImages)
            image.Visible = false;
        foreach (Image image in activeTopImages)
            image.Visible = false;
        foreach (Image image in activeBottomImages)
            image.Visible = false;
    }

    private void TurnOnImages(List<Image> list)
    {
        foreach (Image image in list)
            image.Visible = true;
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
        SceneAs<Level>().Particles.Emit(CrushBlock.P_Activate, num, position, positionRange, direction);
    }

    private void BouncePlayer(Player player, Vector2 direction)
    {
        Audio.Play("event:/game/06_reflection/pinballbumper_hit", player.Center);
        face.Play("hit", true);

        Vector2 launchDir = player.ExplodeLaunch(player.Center - direction, false);
        SceneAs<Level>().DirectionalShake(launchDir, 0.15f);
        SceneAs<Level>().Displacement.AddBurst(player.Center, 0.3f, 8f, 32f, 0.8f, null, null);
        StartShaking(0.2f);
        ActivateParticles(direction);

        if (direction.X > 0f)
            TurnOnImages(activeRightImages);
        if (direction.X < 0f)
            TurnOnImages(activeLeftImages);
        if (direction.Y < 0f)
            TurnOnImages(activeTopImages);
        if (direction.Y > 0f)
            TurnOnImages(activeBottomImages);

        canActivate = false;
    }
    
    private void SpringBouncePlayer(Player player, Vc2 direction)
    {
        Audio.Play("event:/game/general/spring", player.Center);
        face.Play("hit", true);

        if (direction.X > 0f)
            player.SideBounce(Math.Sign((player.Center - direction).X), (player.Center - direction).X, (player.Center - direction).Y);
        if (direction.X < 0f)
            player.SideBounce(-Math.Sign((player.Center - direction).X), (player.Center - direction).X, (player.Center - direction).Y);
        if (direction.Y < 0f)
            player.Bounce((player.Center - direction).Y);
        if (direction.Y > 0f)
            player.Bounce((player.Center - direction).Y);
        
        SceneAs<Level>().DirectionalShake(player.Center - direction, 0.15f);
        SceneAs<Level>().Displacement.AddBurst(player.Center, 0.3f, 8f, 32f, 0.8f, null, null);
        StartShaking(0.2f);
        ActivateParticles(direction);

        if (direction.X > 0f)
            TurnOnImages(activeRightImages);
        if (direction.X < 0f)
            TurnOnImages(activeLeftImages);
        if (direction.Y < 0f)
            TurnOnImages(activeTopImages);
        if (direction.Y > 0f)
            TurnOnImages(activeBottomImages);

        canActivate = false;
    }

    private DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (canActivate && (direction.X == 0f || (direction.X != 0f && canBumpHorizontally)) && (direction.Y == 0f || (direction.Y != 0f && canBumpVertically)))
        {
            if(dashCollision == 1)
            {
                SpringBouncePlayer(player, -direction);
                return DashCollisionResults.NormalCollision;
            }
            else
            {
                BouncePlayer(player, -direction);
                return DashCollisionResults.Ignore;
            }
        }

        return DashCollisionResults.NormalCollision;
    }
}

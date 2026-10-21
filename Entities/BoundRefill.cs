using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System.Collections;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

[ExA.Credits("sunsetquasar for Femto Helper open source code")]
[CustomEntity("ChroniaHelper/BoundRefill")]
public class BoundRefill : CustomRefill
{
    private int direction;

    public class Bounder() : BaseComponent(false, false)
    {
        public int Direction;
        public Vc2 Horizontal;
        public Vc2 Vertical;
    }

    [SelectiveLoadHook]
    public static void LoadHooks()
    {
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    [SelectiveUnloadHook]
    public static void UnloadHooks()
    {
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);

        if (self.Get<Bounder>() is not { } b) return;

        b.RemoveSelf();
        
        // Fetch Direction
        int dir = b.Direction;

        if(dir == 4)
        {
            float angle = (Input.GetAimVector() * new Vc2(1f, -1f)).SafeNormalize(-Vc2.UnitY).Angle();

            switch (angle)
            {
                case var a when a >= -3f / 4 * float.Pi && a <= -1f / 4 * float.Pi:
                    dir = 3;
                    break;
                case var a when a >= 1f / 4 * float.Pi && a <= 3f / 4 * float.Pi:
                    dir = 0;
                    break;
                case var a when a > 3f / 4 * float.Pi || a < -3f / 4 * float.Pi:
                    dir = 1;
                    break;
                case var a when a > -1f / 4 * float.Pi || a < 1f / 4 * float.Pi:
                    dir = 2;
                    break;
                default:
                    dir = 0;
                    break;
            }
        }

        self.Add(new Coroutine(BoundRoutine(self, dir, b)));
    }

    public static IEnumerator BoundRoutine(Player player, int direction, Bounder b)
    {
        yield return null;

        switch (direction)
        {
            case 1: // Left
                player.Speed.X = -b.Horizontal.X;
                break;
            case 2: // Right
                player.Speed.X = b.Horizontal.X;
                break;
            case 3: // Down
                player.Speed.Y = b.Vertical.Y;
                break;
            default: // Up
                player.Speed.Y = -b.Vertical.Y;
                break;
        }
        player.StateMachine.ForceState(Player.StNormal);
        Audio.Play("event:/game/general/spring", player.Center);
    }

    private static readonly ParticleType _pShatter = new(global::Celeste.Refill.P_Shatter)
    {
        Color = Calc.HexToColor("ffc3fe"),
        Color2 = Calc.HexToColor("ffc3fe")
    };

    private static readonly ParticleType _pRegen = new(global::Celeste.Refill.P_Regen)
    {
        Color = Calc.HexToColor("b95fb7"),
        Color2 = Calc.HexToColor("b95fb7")
    };

    private static readonly ParticleType _pGlow = new(global::Celeste.Refill.P_Glow)
    {
        Color = Calc.HexToColor("b95fb7"),
        Color2 = Calc.HexToColor("b95fb7")
    };

    public BoundRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false), data.String("pulseFlag", ""))
    {
        Ldm.LoadHook<BoundRefill>();

        this.SetTexture(data.Attr("directory", "objects/FemtoHelper/boundRefill/").TrimEnd('/') + '/')
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => player.Get<Bounder>() is null)
            .SetOnCollect(OnCollect);

        RespawnTime = data.Float("respawnTime", 2.5f);
        direction = data.Int("direction", 0);

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);

        horizontal = data.Vector2("speedXOnHorizontal", "speedYOnHorizontal", new Vc2(240f, 140f));
        vertical = data.Vector2("speedXOnVertical", "speedYOnVertical", new Vc2(0, 300f));
    }
    private Vc2 horizontal, vertical;

    public BoundRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    private void OnCollect(Player player)
    {
        player.UseRefill(false);
        player.Add(new Bounder { 
            Direction = direction,
            Horizontal = horizontal,
            Vertical = vertical,
        });
    }
}

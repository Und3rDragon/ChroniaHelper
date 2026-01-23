using System;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(false)]
[CustomEntity("ChroniaHelper/KillerWall", "ChroniaHelper/SpringBlockWall")]
public class KillerWall : BaseSolid
{
    public enum Modes
    {
        Dash,
        FinalBoss,
        Crusher
    }

    private bool permanent;

    private EntityID id;

    private int entityID;

    private char tileType;

    private float width;

    private float height;

    private bool dashRebound, dashReboundRefill;

    public KillerWall(Vector2 position, EntityData data) : base(position, data, data.Bool("safe", true))
    {
        base.Depth = data.Int("depth");
        this.permanent = data.Bool("permanent");
        this.width = data.Width;
        this.height = data.Height;
        tileType = data.Char("tiletype", '3');
        //OnDashCollide = OnDashed;
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];

        base.topKillTimer = data.Float("topKillTimer", -1);
        base.bottomKillTimer = data.Float("bottomKillTimer", -1);
        base.leftKillTimer = data.Float("leftKillTimer", -1);
        base.rightKillTimer = data.Float("rightKillTimer", -1);

        // On dashed
        dashRebound = data.Bool("dashRebound", false);
        dashReboundRefill = data.Bool("dashReboundRefill", false);
        OnDashCollide = OnDashed;

        springBlockOverride = data.Bool("springBlockOverride", false);
    }


    public KillerWall(EntityData data, Vector2 offset) : this(data.Position + offset, data)
    {
    }

    public bool springBlockOverride;
    // On dashed
    public DashCollisionResults OnDashed(Player player, Vector2 dir)
    {
        if (dashRebound && !springBlockOverride)
        {
            Vector2 scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);

            if (dashReboundRefill)
            {
                player.RefillDash();
                player.RefillStamina();
            }
            //Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_1", Center);
            // Was a test sound (for the smash vibe), cannot use because of never ending event with unrelated SFX.
            return DashCollisionResults.Rebound;
        }
        return DashCollisionResults.NormalCollision;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        TileGrid tileGrid;
        //移除blendin判断，始终为blendin
        Level level = SceneAs<Level>();
        Rectangle tileBounds = level.Session.MapData.TileBounds;
        VirtualMap<char> solidsData = level.SolidsData;
        int x = (int)(base.X / 8f) - tileBounds.Left;
        int y = (int)(base.Y / 8f) - tileBounds.Top;
        int tilesX = (int)base.Width / 8;
        int tilesY = (int)base.Height / 8;
        tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
        Add(new EffectCutout());

        Add(tileGrid);
        Add(new TileInterceptor(tileGrid, highPriority: true));

    }


    public override void AfterUpdate()
    {
        TimedKill();

        if (springBlockOverride)
        {
            playerTouch = GetPlayerTouch();

            if (!Input.Grab.Check)
            {
                if (playerTouch == 1)
                {
                    OnTouch(Vc2.UnitY);
                }
                else if (playerTouch == 2)
                {
                    OnTouch(-Vc2.UnitY);
                }
                else if (playerTouch == 3)
                {
                    OnTouch(Vc2.UnitX);
                }
                else if (playerTouch == 4)
                {
                    OnTouch(-Vc2.UnitX);
                }
            }
        }
    }

    public override void Render()
    {
        base.Render();
        RenderDangerBorder();
    }

}

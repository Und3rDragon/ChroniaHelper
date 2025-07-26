using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Effects;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/OmniZipMover2")]
public class OmniZipMover2 : Solid
{
    // Omni Zip Mover 2 is a test subject that re-code the original OmniZipMover since the original one is turning too complicated to maintain

    #region Imported Components
    // Path Renderer from Communal Helper
    // I don't know how to code such a exquisite decorative entity
    public class PathRenderer : Entity
    {
        public class Segment
        {
            public bool Seen { get; set; }

            public Vector2 from, to;
            public Vector2 dir, twodir, perp;
            public float length;

            public Vector2 lineStartA, lineStartB;
            public Vector2 lineEndA, lineEndB;

            public Rectangle Bounds { get; }

            public Vector2 sparkAdd;

            public float sparkDirStartA, sparkDirStartB;
            public float sparkDirEndA, sparkDirEndB;
            public const float piOverEight = MathHelper.PiOver4 / 2f;
            public const float eightPi = 4 * MathHelper.TwoPi;

            public Segment(Vector2 from, Vector2 to)
            {
                this.from = from;
                this.to = to;

                dir = (to - from).SafeNormalize();
                twodir = 2 * dir;
                perp = dir.Perpendicular();
                length = Vector2.Distance(from, to);

                Vector2 threeperp = 3 * perp;
                Vector2 minusfourperp = -4 * perp;

                lineStartA = from + threeperp;
                lineStartB = from + minusfourperp;
                lineEndA = to + threeperp;
                lineEndB = to + minusfourperp;

                sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
                float angle = (from - to).Angle();
                sparkDirStartA = angle + piOverEight;
                sparkDirStartB = angle - piOverEight;
                sparkDirEndA = angle + MathHelper.Pi - piOverEight;
                sparkDirEndB = angle + MathHelper.Pi + piOverEight;

                Rectangle b = Util.Rectangle(from, to);
                b.Inflate(10, 10);

                Bounds = b;
            }

            public void Spark(Level level)
            {
                level.ParticlesBG.Emit(ZipMover.P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirStartA);
                level.ParticlesBG.Emit(ZipMover.P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirStartB);
                level.ParticlesBG.Emit(ZipMover.P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirEndA);
                level.ParticlesBG.Emit(ZipMover.P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirEndB);
            }

            public void Render(float percent, Color rope, Color lightRope, bool hide)
            {
                if (hide) { return; }
                Draw.Line(lineStartA, lineEndA, rope);
                Draw.Line(lineStartB, lineEndB, rope);

                for (float d = 4f - (percent * eightPi % 4f); d < length; d += 4f)
                {
                    Vector2 pos = dir * d;
                    Vector2 teethA = lineStartA + perp + pos;
                    Vector2 teethB = lineEndB - pos;
                    Draw.Line(teethA, teethA + twodir, lightRope);
                    Draw.Line(teethB, teethB - twodir, lightRope);
                }
            }

            public void RenderShadow(float percent, bool hide)
            {
                if (hide) { return; }
                Vector2 startA = lineStartA + Vector2.UnitY;
                Vector2 endB = lineEndB + Vector2.UnitY;

                Draw.Line(startA, lineEndA + Vector2.UnitY, Color.Black);
                Draw.Line(lineStartB + Vector2.UnitY, endB, Color.Black);

                for (float d = 4f - (percent * eightPi % 4f); d < length; d += 4f)
                {
                    Vector2 pos = dir * d;
                    Vector2 teethA = startA + perp + pos;
                    Vector2 teethB = endB - pos;
                    Draw.Line(teethA, teethA + twodir, Color.Black);
                    Draw.Line(teethB, teethB - twodir, Color.Black);
                }
            }
        }

        public Rectangle bounds;
        public Segment[] segments;

        public OmniZipMover2 zipMover;

        public Level level;

        public Color color, lightColor;

        public Vector2[] nodes;

        public bool hideRope, hideCog;

        public PathRenderer(OmniZipMover2 zipMover, Vector2[] nodes, Color color, Color lightColor)
        {
            this.zipMover = zipMover;

            this.nodes = new Vector2[nodes.Length];

            Vector2 offset = new(zipMover.MasterWidth / 2f, zipMover.MasterHeight / 2f);

            Vector2 prev = this.nodes[0] = nodes[0] + offset;
            Vector2 min = prev, max = prev;

            segments = new Segment[nodes.Length - 1];
            for (int i = 0; i < segments.Length; ++i)
            {
                Vector2 node = this.nodes[i + 1] = nodes[i + 1] + offset;
                segments[i] = new(node, prev);

                min = Util.Min(min, node);
                max = Util.Max(max, node);

                prev = node;
            }

            bounds = new((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
            bounds.Inflate(10, 10);

            this.color = color;
            this.lightColor = lightColor;

            Depth = Depths.SolidsBelow;

            hideRope = zipMover.hideRope;
            hideCog = zipMover.hideCog;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public void CreateSparks()
        {
            foreach (Segment seg in segments)
                if (!hideCog)
                    seg.Spark(level);
        }

        public override void Render()
        {
            Rectangle cameraBounds = level.Camera.GetBounds();

            if (!cameraBounds.Intersects(bounds))
                return;

            foreach (Segment seg in segments)
                if (seg.Seen = cameraBounds.Intersects(seg.Bounds))
                    seg.RenderShadow(zipMover.percent, hideRope);

            foreach (Segment seg in segments)
                if (seg.Seen)
                    seg.Render(zipMover.percent, color, lightColor, hideRope);

            float rotation = zipMover.percent * MathHelper.TwoPi;
            foreach (Vector2 node in nodes)
            {
                if (!hideCog)
                {
                    zipMover.cog.DrawCentered(node + Vector2.UnitY, Color.Black, 1f, rotation);
                    zipMover.cog.DrawCentered(node, Color.White, 1f, rotation);
                }

            }

            zipMover.DrawBorder();
        }
    }
    #endregion

    public PathRenderer pathRenderer;

    // params for path renderer
    private float MasterWidth, MasterHeight;
    private bool hideRope, hideCog;
    private float percent;
    private MTexture cog;

    // function for path renderer
    private void DrawBorder()
    {
        if (blackBorder)
            foreach (Hitbox extension in AllColliders)
                Draw.HollowRect(new Rectangle(
                    (int)(X + extension.Left - 1f + Shake.X),
                    (int)(Y + extension.Top - 1f + Shake.Y),
                    (int)extension.Width + 2,
                    (int)extension.Height + 2),
                    Color.Black);
    }
    private bool blackBorder;
    private Hitbox[] AllColliders;

    public OmniZipMover2(EntityData data, Vector2 offset, EntityID entityID) : this(entityID, data, offset) { }

    public OmniZipMover2(EntityID entityID, EntityData data, Vector2 position) : base(position, data.Width, data.Height, safe: false)
    {
        id = entityID;
        nodes = data.NodesWithPosition(position);

        Position = nodes[0];
        base.Collider = new Hitbox(Width, Height);
        MasterWidth = Width; MasterHeight = Height;

        // paramaters loading
        ropeColor = Calc.HexToColor(data.Attr("ropeColor", "663931"));
        ropeLightColor = Calc.HexToColor(data.Attr("ropeLightColor", "9b6157"));
        bgColor = Calc.HexToColor(data.Attr("backgroundColor", "000000"));

        wait = data.Bool("waitForPlayer", false);
        remember = data.Bool("rememberPosition", false);
        shake = data.Bool("shake", true);
        mode = (Modes)data.Int("mode", 1);
        startDelay = data.Float("startDelay", 0.1f);

        string[] speeds = data.Attr("moveTimes").Split(",",StringSplitOptions.TrimEntries);
        this.speeds = new float[speeds.Length] ;
        for(int i = 0; i< speeds.Length; i++)
        {
            float v = 0.5f; // default node moving time 0.5s
            float.TryParse(speeds[i], out v);
            this.speeds[i] = v;
        }

        string[] returnSpeeds = data.Attr("returnTimes").Split(",", StringSplitOptions.TrimEntries);
        this.returnSpeeds = new float[returnSpeeds.Length];
        for (int i = 0; i < returnSpeeds.Length; i++)
        {
            float v = 1f; // default node moving time 1s
            float.TryParse(returnSpeeds[i], out v);
            this.returnSpeeds[i] = v;
        }

        string[] nodeDelays = data.Attr("delays").Split(",", StringSplitOptions.TrimEntries);
        this.delays = new float[nodeDelays.Length];
        for(int i = 0; i < nodeDelays.Length; i++)
        {
            float t = 0.2f;
            float.TryParse(nodeDelays[i], out t);
            this.delays[i] = t;
        }

        string[] returnDelays = data.Attr("returnDelays").Split(",", StringSplitOptions.TrimEntries);
        this.returnDelays = new float[returnDelays.Length];
        for(int i = 0; i < returnDelays.Length; i++)
        {
            float t = 0.4f;
            float.TryParse(returnDelays[i], out t);
            this.returnDelays[i] = t;
        }

        eases = data.Attr("eases").Split(",", StringSplitOptions.TrimEntries);
        returnEases = data.Attr("returnEases").Split(",", StringSplitOptions.TrimEntries);

        // setting up sprites
        theme = data.Attr("theme", "objects/zipmover/");
        moon = theme == "objects/zipmover/moon/";
        string sPath, sId, sKey, sCorners;

        sPath = theme + "light";
        sId = theme + "block";
        sKey = theme + "innercog";
        sCorners = theme + "innerCorners";
        cog = GFX.Game[theme + "cog"];
        drawBorder = true;

        // cogs and streetlight
        innerCogs = GFX.Game.GetAtlasSubtextures(sKey);
        streetlight = new Sprite(GFX.Game, sPath)
        {
            Active = false
        };
        streetlight.Add("frames", "", 1f);
        streetlight.Play("frames");
        streetlight.SetAnimationFrame(1);
        streetlight.Position = new Vector2((Width / 2f) - (streetlight.Width / 2f), 0f);
        Add(bloom = new BloomPoint(1f, 6f)
        {
            Position = new Vector2(Width / 2f, 4f)
        });
        // inner corners
        if (!GFX.Game.HasAtlasSubtextures(sCorners))
        {
            sCorners = "objects/ChroniaHelper/omniZipMover/empty_preset";
        }
        // generate textures
        // >> texture generation needs to be re-coded

        Add(sfx = new SoundSource()
        {
            Position = new Vector2(Width, Height) / 2f
        });

        Add(seq = new Coroutine(Sequence()));
        Add(new LightOcclude());

        SurfaceSoundIndex = SurfaceIndex.Girder;
    }
    private EntityID id;
    private Vector2[] nodes;
    private Color ropeColor, ropeLightColor;
    private Coroutine seq;
    private bool remember = false;
    private enum Modes { None, Normal, Persistent, Cycle }
    private Modes mode = Modes.None;
    private bool shake;
    private float startDelay = 0.1f;
    private float[] speeds, returnSpeeds;
    private string[] eases, returnEases;
    private bool wait;
    private float[] delays, returnDelays;
    private bool drawBorder;
    private Color bgColor;
    private List<MTexture> innerCogs;
    private Sprite streetlight;
    private BloomPoint bloom;
    private SoundSource sfx;
    private string theme; private bool moon;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        // >> generate tiles here

        Add(streetlight);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(pathRenderer = new(this, nodes, ropeColor, ropeLightColor));
        pathRenderer.Position += new Vector2(Width / 2, Height / 2);
    }

    public override void Removed(Scene scene)
    {
        scene.Remove(pathRenderer);
        pathRenderer = null;
        base.Removed(scene);
    }

    public override void Update()
    {
        base.Update();

        bloom.Visible = streetlight.CurrentAnimationFrame != 0;
        bloom.Y = !moon ? streetlight.CurrentAnimationFrame * 3 : 9;
    }

    public override void Render()
    {
        Vector2 originalPosition = Position;
        Position += Shake;

        // >> render logic needs re-code

        base.Render();
        Position = originalPosition;
    }

    private int LoadIndex()
    {
        if (!ChroniaHelperSession.Zipmover_NodeIndex.ContainsKey(id))
        {
            return 0;
        }

        return ChroniaHelperSession.Zipmover_NodeIndex[id];
    }
    private bool LoadForward()
    {
        if (!ChroniaHelperSession.Zipmover_NextForward.ContainsKey(id))
        {
            return true;
        }

        return ChroniaHelperSession.Zipmover_NextForward[id];
    }
    private void SaveIndex(int index)
    {
        ChroniaHelperSession.Zipmover_NodeIndex.Enter(id, index);
    }
    private void SaveForward(bool dir)
    {
        ChroniaHelperSession.Zipmover_NextForward.Enter(id, dir);
    }

    private void Initiate()
    {
        ChroniaHelperSession.Zipmover_NextForward.Enter(id, true);
        ChroniaHelperSession.Zipmover_NodeIndex.Enter(id, 0);
        if (!remember)
        {
            ChroniaHelperSession.Zipmover_NodeIndex[id] = 0;
            ChroniaHelperSession.Zipmover_NextForward[id] = true;
        }

        triggered = false;
        sequenceStarted = false;
    }

    private bool triggered = false;
    private bool sequenceStarted = false;
    public IEnumerator Sequence()
    {
        Initiate();

        // done initiating, from now on start listening
        while (true)
        {
            // wait for a signal to activate
            while (!triggered || sequenceStarted)
            {
                yield return null;
                continue;
            }

            sequenceStarted = true;

            // decide target
            Vector2 from = nodes[LoadIndex()];
            int next = LoadIndex();

            if (LoadIndex() == nodes.Length - 1)
            {
                if (mode == Modes.None || mode == Modes.Persistent)
                {
                    triggered = false;
                    sequenceStarted = false;
                    continue;
                }
                else
                {
                    next = LoadIndex() - 1;
                    SaveForward(false);
                }
            }
            else
            {
                next = LoadIndex() + 1;
            }
            Vector2 to = nodes[next];

            // start moving
            // >> starting sound here
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            if (shake) { StartShaking(0.1f); }
            // >> streetlight on

            yield return startDelay;

            // don't know what this is
            //StopPlayerRunIntoAnimation = false;

            // set up the timer
            float timer = 0f;
            float setTimer = LoadForward() ? 
                speeds[Math.Min(LoadIndex(),speeds.Length - 1)] : 
                returnSpeeds[Math.Min(nodes.Length - 1 - LoadIndex(), returnSpeeds.Length - 1)];
            string ease = LoadForward() ?
                eases[Math.Min(LoadIndex(), eases.Length - 1)] :
                returnEases[Math.Min(nodes.Length - 1 - LoadIndex(), returnEases.Length - 1)];

            while(timer < setTimer)
            {
                // >> if wanna sync, we should send a signal here

                timer = Calc.Approach(timer, setTimer, Engine.DeltaTime);

                Vector2 target = Vector2.Lerp(from, to, EaseUtils.StringToEase(ease)(timer / setTimer));

                if (Scene.OnInterval(0.1f))
                    pathRenderer.CreateSparks();

                MoveTo(target);
            }
            // arrived and shake
            if (shake)
            {
                StartShaking(0.2f);
            }

            // arrived at new node, refresh state
            SaveIndex(next);
            if(LoadIndex() == nodes.Length - 1)
            {
                if(mode == Modes.Cycle || mode == Modes.Normal)
                {
                    SaveForward(false);
                }
            }
            if(LoadIndex() == 0)
            {
                SaveForward(true);
            }

            float delay = LoadForward() ? 
                delays[Math.Min(delays.Length - 1, LoadIndex())] : 
                returnDelays[Math.Min(returnDelays.Length - 1, nodes.Length - 1 - LoadIndex())];

            yield return delay; // node delay
            
            // if zipmover has to wait, then the sequence should reset triggered state here
            if (wait)
            {
                triggered = false;
            }

            sequenceStarted = false;
        }
    }

    // custom extended function
    public int GetPlayerTouch()
    {
        foreach (Player player in MapProcessor.level.Tracker.GetEntities<Player>())
        {
            if (CollideCheck(player, Position - Vector2.UnitY))
            {
                return 1; // up
            }
            if (CollideCheck(player, Position + Vector2.UnitY))
            {
                return 2; // down
            }
            if (player.Facing == Facings.Right && CollideCheck(player, Position - Vector2.UnitX))
            {
                return 3; // left
            }
            if (player.Facing == Facings.Left && CollideCheck(player, Position + Vector2.UnitX))
            {
                return 4; // right
            }
        }
        return 0;
    }
}

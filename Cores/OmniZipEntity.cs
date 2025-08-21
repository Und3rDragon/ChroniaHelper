using Celeste.Mod.Entities;
using System.Collections;
using System.Collections.Generic;
using System;
using ChroniaHelper.Utils;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using static On.Celeste.Player;
using System.IO;
using Line = ChroniaHelper.Utils.GeometryUtils.Line;

namespace ChroniaHelper.Entities;

[Tracked(false)]
public class OmniZipEntity : Entity
{
    
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

        public OmniZipEntity zipMover;

        public Level level;

        public Color color, lightColor;

        public Vector2[] nodes;

        public bool hideRope, hideCog;

        public PathRenderer(OmniZipEntity zipMover, Vector2[] nodes, Color color, Color lightColor)
        {
            this.zipMover = zipMover;

            this.nodes = new Vector2[nodes.Length];

            Vector2 offset = new(zipMover.Width / 2f, zipMover.Height / 2f);

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
    public PathRenderer pathRenderer;

    public Sprite streetlight;

    public bool drawBlackBorder;

    public SoundSource sfx;

    public bool permanent;
    public bool waits;
    public bool ticking;
    public Coroutine seq;

    public string themePath;


    public Vector2[] nodes;

    public MTexture cog;

    public float percent;

    //新增参数

    public float nodeDelay;
    public string[] delays;
    public string[] easing;
    public string[] speeds;
    public float speedmult;
    public int ticks;
    public float tickDelay;
    public bool synced;
    public string zmtag;
    public float startDelay;

    public List<Hitbox> AllColliders;

    public static Vector2 entityPos;

    //变更参数

    public Color backgroundColor;
    public Color ropeColor = Calc.HexToColor("663931");
    public Color ropeLightColor = Calc.HexToColor("9b6157");

    public string startSound = "event:/CommunalHelperEvents/game/zipMover/normal/start",
                   impactSound = "event:/CommunalHelperEvents/game/zipMover/normal/impact",
                   tickSound = "event:/CommunalHelperEvents/game/zipMover/normal/tick",
                   returnSound = "event:/CommunalHelperEvents/game/zipMover/normal/return",
                   finishSound = "event:/CommunalHelperEvents/game/zipMover/normal/finish";

    public bool startShake, nodeShake, returnShake, tickShake, permaShake;
    public bool hideRope, hideCog;

    public bool drawTank;

    //OnDash
    public bool dashable, onlyDash, dashableOnce, dashableRefill;

    // Return params
    public string[] returnDelays, returnSpeeds;
    public float irrespondingTime;

    public string[] returnEasing;

    // Glass Block
    public List<Line> lines = new List<Line>();

    public Color lineColor = Color.White;

    // Time Units
    public bool timeUnits;

    public OmniZipEntity(EntityData data)
        : this(data, data.Position)
    { }

    public OmniZipEntity(EntityData data, Vector2 position
        )
        : base(position)
    { }
    public bool sideflag;

    public bool customSkin;

    // OnDash parameters
    public bool triggered = false;
    public Vector2 scale = Vector2.One;
    public DashCollisionResults dcr = DashCollisionResults.NormalCollision;
    public DashCollisionResults OnDashed(Player player, Vector2 dir)
    {
        if (!dashable)
        {
            return dcr = DashCollisionResults.NormalCollision;
        }

        if (!triggered)
        {
            if (dashableOnce) { triggered = true; }

            scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
            if (dashableRefill)
            {
                player.RefillDash();
                player.RefillStamina();
            }
            //Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_1", Center);
            // Was a test sound (for the smash vibe), cannot use because of never ending event with unrelated SFX.
            return dcr = DashCollisionResults.Rebound;
        }
        return dcr = DashCollisionResults.NormalCollision;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        Add(streetlight);

        int num = (int)base.Width / 8;
        int num2 = (int)base.Height / 8;
        AddSide(new Vector2(0f, 0f), new Vector2(0f, -1f), num);
        AddSide(new Vector2(num - 1, 0f), new Vector2(1f, 0f), num2);
        AddSide(new Vector2(num - 1, num2 - 1), new Vector2(0f, 1f), num);
        AddSide(new Vector2(0f, num2 - 1), new Vector2(-1f, 0f), num2);
    }

    public float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }

    public void AddSide(Vector2 start, Vector2 normal, int tiles)
    {
        Vector2 vector = new Vector2(0f - normal.Y, normal.X);
        for (int i = 0; i < tiles; i++)
        {
            if (Open(start + vector * i + normal))
            {
                Vector2 vector2 = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                if (!Open(start + vector * (i - 1)))
                {
                    vector2 -= vector;
                }

                for (; i < tiles && Open(start + vector * i + normal); i++)
                {
                }

                Vector2 vector3 = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                if (!Open(start + vector * i))
                {
                    vector3 += vector;
                }

                lines.Add(new Line(vector2 + normal, vector3 + normal));
            }
        }
    }

    public bool Open(Vector2 tile)
    {
        Vector2 point = new Vector2(base.X + tile.X * 8f + 4f, base.Y + tile.Y * 8f + 4f);
        // Solid Check?
        /*
        if (!base.Scene.CollideCheck<SolidTiles>(point))
        {
            return !base.Scene.CollideCheck<GlassBlock>(point);
        }
        */

        return true; // default false
    }

    public Level level;
    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        scene.Add(pathRenderer = new(this, nodes, ropeColor, ropeLightColor));

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

        // OnDash
        dcr = DashCollisionResults.NormalCollision;
        scale = Calc.Approach(scale, Vector2.One, 3f * Engine.DeltaTime);

        streetlight.Scale = scale;
        Vector2 zeroCenter = new Vector2(Width, Height) / 2f;
        streetlight.Position = zeroCenter + (new Vector2(zeroCenter.X - streetlight.Width / 2f, 0) - zeroCenter) * scale;

    }


    public bool isShaking = false;
    public void StartShaking(float time)
    {
        float timer = time;
        isShaking = true;
        while (timer > 0)
        {
            timer -= Engine.DeltaTime;
        }
        isShaking = false;
    }


    public override void Render()
    {
        Vector2 originalPosition = Position;

        if (isShaking)
        {
            Vector2 dPos = Calc.Random.Range(new Vector2(-2f, -2f), new Vector2(2f, 2f));
            Position += dPos;
        }


        base.Render();
        Position = originalPosition;
    }

    public bool _IsShallowAtRectangle(Rectangle rectangle)
    {
        return base.Scene.CollideCheck<Solid>(new Vector2(rectangle.Left, base.Top + 8f), new Vector2(rectangle.Right, base.Top + 8f));
    }

    public void DrawBorder()
    {
        if (drawBlackBorder)
            foreach (Hitbox extension in AllColliders)
                Draw.HollowRect(new Rectangle(
                    (int)(X + extension.Left - 1f), // + Shake.X),
                    (int)(Y + extension.Top - 1f), // + Shake.Y),
                    (int)extension.Width + 2,
                    (int)extension.Height + 2),
                    Color.Black);
    }

    public void PlayStartSound()
    {
        //if (this.customSound)
        //{
        //    if (!string.IsNullOrEmpty(startSound.Trim()))
        //    {
        //        sfx.Play(this.startSound);
        //    }
        //}
        //else
        //{
        //    sfx.Play($"event:/CommunalHelperEvents/game/zipMover/{themePath}/start");
        //}
    }

    public void PlayImpactSound()
    {
        //if (customSound)
        //{
        //    if (!string.IsNullOrEmpty(impactSound.Trim()))
        //    {
        //        Audio.Play(this.impactSound, Center);
        //    }
        //}
        //else
        //{
        //    Audio.Play($"event:/CommunalHelperEvents/game/zipMover/{themePath}/impact", Center);
        //}
    }

    public void PlayTickSound()
    {
        //if (customSound)
        //{
        //    if (!string.IsNullOrEmpty(tickSound.Trim()))
        //    {
        //        Audio.Play(this.tickSound, Center);
        //    }
        //}
        //else
        //{
        //    Audio.Play($"event:/CommunalHelperEvents/game/zipMover/{themePath}/tick", Center);
        //}
    }

    public void PlayReturnSound()
    {
        //if (customSound)
        //{
        //    if (!string.IsNullOrEmpty(returnSound.Trim()))
        //    {
        //        sfx.Play(this.returnSound);
        //    }

        //}
        //else
        //{
        //    sfx.Play($"event:/CommunalHelperEvents/game/zipMover/{themePath}/return");
        //}
    }

    public void PlayFinishSound()
    {
        //if (customSound)
        //{
        //    if (!string.IsNullOrEmpty(finishSound.Trim()))
        //    {
        //        Audio.Play(this.finishSound, Center);
        //    }

        //}
        //else
        //{
        //    Audio.Play($"event:/CommunalHelperEvents/game/zipMover/{themePath}/finish", Center);
        //}
    }

    public bool ActivateArg()
    {
        return CollideCheck<Player>();
    }

    public string flagConditions;
    public string[][] flags;
    public bool conditionEmpty;
    public bool ActivateArg(int index, bool useNormal)
    {
        if (useNormal)
        {
            return ActivateArg();
        }

        if (conditionEmpty) { return ActivateArg(); }

        bool req = true;
        int n = index < flags.GetLength(0) ? index : flags.GetLength(0) - 1;
        foreach (var item in flags[n])
        {
            req = req ? level.Session.GetFlag(item) : false;
        }
        return req;
    }

    public void SetFlag(bool set)
    {
        level.Session.SetFlag($"ZipMoverSync:{zmtag}", set);
        if (sideflag) { level.Session.SetFlag(zmtag, set); }
    }

    public bool GetFlag()
    {
        bool ans = false;
        ans = level.Session.GetFlag($"ZipMoverSync:{zmtag}");
        if (sideflag) { ans = level.Session.GetFlag(zmtag); }

        return ans;
    }

    public IEnumerator Sequence()
    {
        // Infinite.
        while (true)
        {
            bool syncflag = false;

            //Activation Judgement
            if (ActivateArg())
            {
                syncflag = true;
                if (synced)
                {
                    SetFlag(true);
                }
            }

            if(synced && GetFlag()) { syncflag = true; }

            if (!syncflag)
            {
                yield return null;
                continue;
            }

            Vector2 from = nodes[0];
            Vector2 to;
            float at;

            // Player is riding.
            bool shouldCancel = false;
            int i;
            for (i = 1; i < nodes.Length; i++)
            {
                to = nodes[i];

                // Start shaking.


                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);

                if (this.startShake)
                {
                    StartShaking(0.1f); //起始抖动
                }
                // after shaking, activate the light as indicator
                streetlight.SetAnimationFrame(2);

                // then wait for the start delay
                var Tstart = i == 1 ? startDelay : 0f;
                yield return Tstart;

                // Start moving towards the target.

                //StopPlayerRunIntoAnimation = false;
                at = 0f;

                // zipmover starts moving
                while (at < 1f)
                {
                    yield return null;

                    // after starting, remove the flag
                    // for some reason, if it's executed before start delay, the other zipmovers can't receive the signal
                    if (syncflag)
                    {
                        syncflag = false;
                        if (synced) { SetFlag(false); }
                    }

                    // altering speed
                    if (i <= this.speeds.Length)
                    {
                        this.speedmult = this.speeds[i - 1].ParseFloat(1f);
                    }
                    if (this.speedmult <= 0f) { this.speedmult = -speedmult; }

                    at = Calc.Approach(at, 1f, Util.OZMTime(timeUnits, this.speedmult, false));

                    // changing easer
                    string ease;
                    if (i > easing.Length)
                    {
                        ease = easing[easing.Length - 1];
                    }
                    else { ease = easing[i - 1]; }
                    percent = EaseUtils.StringToEase(ease)(at);

                    Vector2 vector = Vector2.Lerp(from, to, percent);

                    if (Scene.OnInterval(0.1f))
                        pathRenderer.CreateSparks();

                    Position = vector;

                    /*
                    if (Scene.OnInterval(0.03f))
                        SpawnScrapeParticles();
                     */

                }

                bool last = i == nodes.Length - 1;


                // Arrived, will wait for 0.5 secs.

                float shaketime = 0.2f;
                if (this.nodeDelay == 0) { shaketime = Engine.DeltaTime; }

                if (this.nodeShake)
                {
                    StartShaking(shaketime); //抵达节点抖动
                }


                PlayImpactSound();

                var setFrame = 2;
                if (waits && !last) { setFrame = 1; }
                if (ticking && !last) { setFrame = 1; }
                if (permanent && last) { setFrame = 3; }
                streetlight.SetAnimationFrame(setFrame);

                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                // StopPlayerRunIntoAnimation = true;

                // node delay moved to the tails

                from = nodes[i];

                if (ticking && !last)
                {
                    triggered = false;
                    float tickTime = 0.0f;
                    int tickNum = 0;
                    // Args on each node
                    while (!ActivateArg() && tickNum < this.ticks)
                    {
                        yield return null;
                        streetlight.SetAnimationFrame(1 - (int)Math.Round(tickTime / this.tickDelay));


                        tickTime = Calc.Approach(tickTime, this.tickDelay, Engine.DeltaTime);
                        if (tickTime >= this.tickDelay)
                        {
                            tickTime = 0.0f;
                            ++tickNum;

                            PlayTickSound();

                            if (this.tickShake)
                            {
                                StartShaking(0.1f); //tick模式的抖动
                            }

                        }
                    }

                    if (tickNum == this.ticks && !ActivateArg())
                    {
                        shouldCancel = true;
                        break;
                    }
                }
                else if (waits && !last)
                {
                    triggered = false;
                    streetlight.SetAnimationFrame(1);
                    while (!ActivateArg())
                        yield return null;
                }
                else if (!conditionEmpty && !last)
                {
                    triggered = false;
                    streetlight.SetAnimationFrame(1);
                    while (!ActivateArg(i - 1, false))
                        yield return null;
                }
                streetlight.SetAnimationFrame(2);

                // Changed Subjects
                if (i <= this.delays.Length)
                {
                    this.nodeDelay = this.delays[i - 1].ParseFloat(0.2f);
                }
                else { this.nodeDelay = 0.2f; }
                nodeDelay.MakeAbs();

                yield return nodeDelay; // Delay on each node

            }

            if (!permanent)
            {
                // Following codes for return sequence
                int max = -1;
                for (i -= 2 - (shouldCancel ? 1 : 0); i >= 0; i--)
                {
                    max = i > max ? i : max;
                    to = nodes[i];

                    // Goes back to start with a speed that is four times slower.
                    // StopPlayerRunIntoAnimation = false;
                    streetlight.SetAnimationFrame(2);

                    PlayReturnSound();

                    at = 0f;
                    //Analyzing inputs here
                    float returnSpeed = max - i + 1 > returnSpeeds.Length ? returnSpeeds[returnSpeeds.Length - 1].ParseFloat(1f) : returnSpeeds[max - i].ParseFloat(1f);
                    returnSpeed = returnSpeed <= 0 ? -returnSpeed : returnSpeed;
                    string returnEase;
                    if (max - i + 1 > returnEasing.Length)
                    {
                        returnEase = returnEasing[returnEasing.Length - 1];
                    }
                    else
                    {
                        returnEase = returnEasing[max - i];
                    }
                    while (at < 1f)
                    {
                        yield return null;
                        at = Calc.Approach(at, 1f, Util.OZMTime(timeUnits, returnSpeed, true));// Return Speeds
                        percent = 1f - EaseUtils.StringToEase(returnEase)(at); // Return Ease

                        Vector2 position = Vector2.Lerp(from, to, EaseUtils.StringToEase(returnEase)(at)); // Return Ease
                        Position = position;
                    }

                    if (i != 0)
                        from = nodes[i];

                    if (this.returnShake)
                    {
                        StartShaking(0.2f); //返回及返回节点抖动
                    }

                    PlayFinishSound();
                    // Modify return delays
                    float returnDelay = max - i + 1 > returnDelays.Length ? returnDelays[returnDelays.Length - 1].ParseFloat(0.2f) : returnDelays[max - i].ParseFloat(0.2f);
                    returnDelay = returnDelay <= 0 ? -returnDelay : returnDelay;
                    yield return returnDelay; // Return Delays

                }

                // StopPlayerRunIntoAnimation = true;

                // OnDash set on return
                triggered = false;

                // Done, will not activate for [returnedIrrespondingTime] secs.
                streetlight.SetAnimationFrame(1);
                yield return irrespondingTime;
            }
            else
            {
                // Done, will never be activated again.

                if (this.permaShake)
                {
                    StartShaking(0.3f); //permanent模式抖动
                }


                PlayFinishSound();
                PlayTickSound();

                SceneAs<Level>().Shake(0.15f);
                streetlight.SetAnimationFrame(0);
                while (true)
                    yield return null;
            }

        }
    }
}

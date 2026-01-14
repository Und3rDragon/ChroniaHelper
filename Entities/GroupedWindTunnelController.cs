using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.DotNet.Cloning;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using static ChroniaHelper.Modules.ChroniaHelperSession;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/GroupedWindTunnelController")]
public class GroupedWindTunnelController : BaseEntity
{
    public GroupedWindTunnelController(EntityData data, Vc2 offset) : base(data, offset)
    {
        IDs = data.StringArray("groupIDs");
        Depth = data.Int("groupDepth", 9000);
        strength = data.Float("strength", 1f);
        affectPlayer = data.Bool("affectPlayer", true);
        particleStrength = data.Float("particleStrength", -1f);
        condition = data.Attr("condition");
        conditionType = (ConditionListener.ConditionType)data.Int("conditionType", 0);
        Add(conditionListener = new ConditionListener(condition, conditionType));
        angle = data.Float("angle", 0f);
        particleDensity = data.Float("particleDensity", 0.3f);
        colors = data.List<CColor>("colors", (i) => new(i));
        speedUp = data.Float("windUpSpeed", 1f);
        speedDown = data.Float("windDownSpeed", 0.6f);
        conditionMode = (ConditionMode)data.Int("conditionMode", 0);

        windSpeed = strength * Vc2.UnitX.Rotate(-Calc.DegToRad * angle);
        particleSpeed = (particleStrength >= 0 ? particleStrength : strength) * Vc2.UnitX.Rotate(-Calc.DegToRad * angle);
    }
    private string[] IDs;
    private float strength;
    private bool affectPlayer = true;
    private float particleStrength;
    private string condition;
    private ConditionListener.ConditionType conditionType;
    private float angle;
    private float particleDensity;
    private List<CColor> colors = new();
    private List<Particle> particles = new();
    private List<GroupedWindTunnel> members = new();
    private ColliderList colliders = new();
    private float speedUp = 1f, speedDown = 0.6f;
    private enum ConditionMode { Always = 0, Normal = 1, Inverted = 2 }
    private ConditionMode conditionMode = ConditionMode.Always;

    // setups
    private Vc2 windSpeed, particleSpeed;
    private int particleCount;
    private List<Point> pixels = new();
    private (Vc2, Vc2) basicRange = (Vc2.Zero, Vc2.Zero);
    private bool basicRangeModified = false;
    private Dictionary<WindMover, float> windPercents = new();
    private ConditionListener conditionListener;
    private bool conditionMet => conditionMode == ConditionMode.Always ? true : (conditionMode == ConditionMode.Normal ? conditionListener.state : !conditionListener.state);

    protected override void AwakeExecute(Scene scene)
    {
        basicRangeModified = false;

        foreach(var entity in MaP.level.Tracker.GetEntities<GroupedWindTunnel>())
        {
            var tunnel = entity as GroupedWindTunnel;

            if (IDs.Contains(tunnel.groupID))
            {
                members.Add(tunnel);
                Collider originalCollider = new Hitbox(tunnel.sizeData.Item3, tunnel.sizeData.Item4, tunnel.sizeData.Item1, tunnel.sizeData.Item2);
                Collider memberCollider = new Hitbox(tunnel.sizeData.Item3, tunnel.sizeData.Item4, tunnel.sizeData.Item1 - Position.X, tunnel.sizeData.Item2 - Position.Y);
                //Log.Info(tunnel.sizeData);
                colliders.Add(memberCollider);
                Vc2 v1 = originalCollider.TopLeft, v2 = originalCollider.BottomRight;
                int x1 = int.Min((int)v1.X, (int)v2.X), x2 = int.Max((int)v1.X, (int)v2.X);
                int y1 = int.Min((int)v1.Y, (int)v2.Y), y2 = int.Max((int)v1.Y, (int)v2.Y);
                for(int x = x1; x <= x2; x++)
                {
                    for(int y = y1; y <= y2; y++)
                    {
                        pixels.Add(new Point(x, y));
                    }
                }

                if (!basicRangeModified)
                {
                    basicRange = (v1, v2);
                    basicRangeModified = true;
                }
                else
                {
                    Vc2 p1 = basicRange.Item1, p2 = basicRange.Item2;
                    basicRange = (new Vc2(Math.Min(v1.X, p1.X), Math.Min(v1.Y, p1.Y)), new Vc2(Math.Max(v2.X, p2.X), Math.Max(v2.Y, p2.Y)));
                }

                tunnel.RemoveSelf();
            }
        }
        pixels = pixels.Distinct().ToList();

        particleCount = (int)(pixels.Count * particleDensity);
        particles.Clear();
        for(int i = 0; i < particleCount; i++)
        {
            Particle particle = new();
            particle.Percent = 0f;
            particle.Position = ((Func<Point, Vc2>)((i) => new Vc2(i.X, i.Y)))(pixels[Calc.Random.Next(pixels.Count)]);
            particle.Speed = particle.Percent * particleSpeed;
            //particle.Spin = Calc.Random.Range(0.25f, (float)Math.PI * 6f);
            particle.Color = colors[Calc.Random.Next(colors.Count)];
            particle.phase = 2 * MathF.PI * Calc.Random.NextFloat();
            particle.Color.alpha = MathF.Sin(particle.phase);

            particles.Add(particle);
        }

        Collider = colliders;
        //ColliderList list = Collider as ColliderList;
        //foreach (var collider in list.colliders)
        //{
        //    Log.Info(collider.TopLeft, collider.BottomRight);
        //}
    }

    protected override void UpdateExecute()
    {
        foreach (var particle in particles)
        {
            // 更新 Percent 和 Speed (现在修改是有效的!)
            particle.Percent = Calc.Approach(particle.Percent, conditionMet ? 1f : 0f, 
                Engine.DeltaTime * (conditionMet ? speedUp : speedDown));
            particle.Speed = particle.Percent * particleSpeed;
            particle.Position += particle.Speed * Engine.DeltaTime;
            particle.Color.alpha = MathF.Sin(Scene.TimeActive + particle.phase);

            // 快速边界检查
            int posX = (int)particle.Position.X, posY = (int)particle.Position.Y;
            if (!pixels.Contains(new Point(posX, posY)))
            {
                // 随机回退
                var p = pixels[Calc.Random.Next(pixels.Count)];
                particle.Position = new Vc2(p.X, p.Y);
                // 可选：重置速度方向
            }
        }

        foreach (WindMover component in Scene.Tracker.GetComponents<WindMover>())
        {
            // if arg is meet and this windmover is from player
            if (component.Entity is Player && !affectPlayer)
            {
                continue;
            }

            // normal logic
            if (component.Entity.CollideCheck(this))
            {
                if (windPercents.ContainsKey(component))
                {
                    windPercents[component] = Calc.Approach(windPercents[component], 1f, Engine.DeltaTime * speedUp);
                }
                else
                {
                    windPercents.Add(component, 0f);
                }
            }
            else
            {
                if (windPercents.ContainsKey(component))
                {
                    windPercents[component] = Calc.Approach(windPercents[component], 0f, Engine.DeltaTime * speedDown);
                    if (windPercents[component] == 0f)
                    {
                        windPercents.Remove(component);
                    }
                }
            }
        }

        foreach (WindMover component in windPercents.Keys)
        {
            if (component != null && component.Entity != null && component.Entity.Scene != null)
            {
                //对玩家产生影响的因素

                if (conditionMet)
                {
                    component.Move(windSpeed * 0.1f * Engine.DeltaTime * Ease.CubeInOut(windPercents[component]));
                }
            }
        }
    }

    public override void Render()
    {
        base.Render();
        
        foreach(var particle in particles)
        {
            Draw.LineAngle(particle.Position, particle.Speed.Angle(),
                Math.Max(1f, particle.Speed.Length() / 60f), particle.Color.Parsed());
        }
    }

    public class Particle
    {
        public Vector2 Position;
        public float Percent;
        public Vc2 Speed;
        public CColor Color;
        public float phase;
    }

    public void ResetAll(float percent = 0f)
    {
        for(int i = 0; i < particles.Count; i++)
        {
            Reset(particles[i], percent);
        }
    }

    public void Reset(Particle particle, float percent = 0f)
    {
        particle.Percent = percent;
        particle.Position = ((Func<Point, Vc2>)((i) => new Vc2(i.X, i.Y)))(pixels[Calc.Random.Next(pixels.Count)]);
        particle.Speed = Calc.Random.Range(4, 14) * Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
        //particle.Spin = Calc.Random.Range(0.25f, (float)Math.PI * 6f);
        particle.Color = colors[Calc.Random.Next(colors.Count)];
        particle.phase = 2 * MathF.PI * Calc.Random.NextFloat();
        particle.Color.alpha = MathF.Sin(particle.phase);
    }
}

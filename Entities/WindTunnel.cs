using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoHelper.Hooks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/WindTunnel")]
public class WindTunnel : Entity
{
    private static readonly float _baseAlpha = 0.7f;
    private static readonly char[] separators = { ',' };

    private readonly float _windUpTime = 0.5f;
    private readonly float _windDownTime = 0.2f;
    private readonly Dictionary<WindMover, float> _componentPercentages = new();
    private readonly float _loopWidth;
    private readonly float _loopHeight;
    private readonly float _strength;
    private readonly Direction _direction;
    private readonly Particle[] _particles;
    private float scale;
    private float _percent;
    private bool _speedingUp;
    private Vector2 _defaultWindSpeed;
    private Color[] _colors;

    //新增参数
    private bool status = false;
    private string flag;
    private Level level;
    private flagMode flagToggle;
    private bool affectPlayer;

    // particles
    private float particleStrength;
    private Vector2 defaultParticleSpeed;

    private float angle;

    public enum flagMode
    {
        AlwaysOn,
        FlagNeeded,
        FlagInverted,
    }

    public WindTunnel(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height,
              data.Float("strength", 1f), data.Attr("direction", "Up"),
              data.Attr("flag", "flag"),
              data.Bool("startActive", false),
              data.Attr("particleColors", "808080,545151,ada5a5"),
              data.Bool("showParticles", true),
              data.Attr("flagMode","AlwaysOn"),
              data.Bool("affectPlayer", true),
              data.Float("particleStrengthOverride", -1),
              data.Float("particleDensity", 1f)
              )
    {
    }

    public WindTunnel(Vector2 position, int width,
        int height, float strength, string direction,
        string flag, bool startActive,
        string particleColors, bool showParticles,
        string flagmode, bool affectPlayer, float particleStrength, float particleNum)
    {
        Depth = -1000;
        Position = position;
        _loopWidth = width;
        _loopHeight = height;

        _percent = 0f;
        _speedingUp = false;

        Collider = new Hitbox(width, height);
        _strength = strength;

        // whether the wind affects player
        this.affectPlayer = affectPlayer;
        this.particleStrength = particleStrength;

        if (float.TryParse(direction, out angle))
        {
            _defaultWindSpeed = new Vector2(_strength, 0).Rotate(-Calc.DegToRad * angle);
            defaultParticleSpeed = new Vector2(this.particleStrength, 0).Rotate(-Calc.DegToRad * angle);
        }
        else if(Enum.TryParse(direction, out _direction))
        {
            switch (_direction)
            {
                case Direction.Up:
                    angle = 90f;
                    _defaultWindSpeed = -Vector2.UnitY * _strength;
                    defaultParticleSpeed = -Vector2.UnitY * this.particleStrength;
                    break;
                case Direction.Down:
                    angle = -90f;
                    _defaultWindSpeed = Vector2.UnitY * _strength;
                    defaultParticleSpeed = Vector2.UnitY * this.particleStrength;
                    break;
                case Direction.Left:
                    angle = 180f;
                    _defaultWindSpeed = -Vector2.UnitX * _strength;
                    defaultParticleSpeed = -Vector2.UnitX * this.particleStrength;
                    break;
                case Direction.Right:
                    angle = 0f;
                    _defaultWindSpeed = Vector2.UnitX * _strength;
                    defaultParticleSpeed = Vector2.UnitX * this.particleStrength;
                    break;
            }
        }
        else
        {
            _defaultWindSpeed = -Vector2.UnitY * _strength;
            defaultParticleSpeed = -Vector2.UnitY * this.particleStrength;
        }


        int particlecount = showParticles ? (int)(width * height / 300 * particleNum) : 0;

        _colors = particleColors.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(str => Calc.HexToColor(str.Trim()))
            .ToArray();

        _particles = new Particle[particlecount];
        for (int i = 0; i < _particles.Length; i++)
        {
            Reset(i, Calc.Random.NextFloat(_baseAlpha));
        }

        //新增

        this.flag = flag;

        if(flagmode == "FlagInverted")
        {
            flagToggle = flagMode.FlagInverted;
        }
        else if (flagmode == "FlagNeeded")
        {
            flagToggle = flagMode.FlagNeeded;
        }
        else
        {
            flagToggle = flagMode.AlwaysOn;
        }
    }

    

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }


    private Vector2 _actualWindSpeed => _defaultWindSpeed * _percent;
    private Vector2 actualParticleSpeed => defaultParticleSpeed * _percent;


    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        PositionParticles();
    }

    public override void Update()
    {
        if(flagToggle == flagMode.FlagInverted)
        {
            if (!level.Session.GetFlag(this.flag))
            {
                _speedingUp = true;
            }
            else
            {
                _speedingUp = false;
            }
        }
        else if(flagToggle == flagMode.FlagNeeded)
        {
            if (level.Session.GetFlag(this.flag))
            {
                _speedingUp = true;
            }
            else
            {
                _speedingUp = false;
            }
        }
        else
        {
            _speedingUp = true;
        }

        if (_speedingUp && (_percent < 1f))
        {
            _percent = Calc.Approach(_percent, 1f, Engine.DeltaTime / 1f);
        }
        else if (!_speedingUp && (_percent > 0f))
        {
            _percent = Calc.Approach(_percent, 0f, Engine.DeltaTime / 1.5f);
        }

        PositionParticles();
        
        foreach (WindMover component in Scene.Tracker.GetComponents<WindMover>())
        {
            // if arg is meet and this windmover is from player
            if(component.Entity is Player && !affectPlayer)
            {
                continue;
            }

            // normal logic
            if (component.Entity.CollideCheck(this))
            {
                if (_componentPercentages.ContainsKey(component))
                {
                    _componentPercentages[component] = Calc.Approach(_componentPercentages[component], 1f, Engine.DeltaTime / _windUpTime);
                }
                else
                {
                    _componentPercentages.Add(component, 0f);
                }
            }
            else
            {
                if (_componentPercentages.ContainsKey(component))
                {
                    _componentPercentages[component] = Calc.Approach(_componentPercentages[component], 0.0f, Engine.DeltaTime / _windDownTime);
                    if (_componentPercentages[component] == 0f)
                    {
                        _componentPercentages.Remove(component);
                    }
                }
            }
        }

        foreach (WindMover component in _componentPercentages.Keys)
        {
            if (component != null && component.Entity != null && component.Entity.Scene != null)
            {
                //对玩家产生影响的因素
                
                if(flagToggle == flagMode.AlwaysOn)
                {
                    component.Move(_actualWindSpeed * 0.1f * Engine.DeltaTime * Ease.CubeInOut(_componentPercentages[component]));
                }
                else if (flagToggle == flagMode.FlagNeeded)
                {
                    if (level.Session.GetFlag(this.flag))
                    {
                        component.Move(_actualWindSpeed * 0.1f * Engine.DeltaTime * Ease.CubeInOut(_componentPercentages[component]));
                    }
                }
                else if(flagToggle == flagMode.FlagInverted)
                {
                    if (!level.Session.GetFlag(this.flag))
                    {
                        component.Move(_actualWindSpeed * 0.1f * Engine.DeltaTime * Ease.CubeInOut(_componentPercentages[component]));
                    }
                }
            }
        }

        base.Update();
    }

    public override void Render()
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            Vector2 particlePosition = default;
            particlePosition.X = Mod(_particles[i].Position.X, _loopWidth);
            particlePosition.Y = Mod(_particles[i].Position.Y, _loopHeight);
            float percent = _particles[i].Percent;
            float num = (!(percent < 0.7f)) ? Calc.ClampedMap(percent, 0.7f, 1f, 1f, 0f) : Calc.ClampedMap(percent, 0f, 0.3f);
            Draw.LineAngle(particlePosition + Position, -Calc.DegToRad * angle, scale, _colors[_particles[i].Color] * num);
        }
    }

    private void PositionParticles()
    {
        var particleSpeed = Vector2.Zero;
        if(this.particleStrength >= 0f)
        {
            particleSpeed = actualParticleSpeed;
        }
        else { particleSpeed = _actualWindSpeed; }

        bool flag = particleSpeed.Y == 0f;
        Vector2 zero = particleSpeed;
        scale = Math.Max(1f, particleSpeed.Length() / 60f);

        for (int i = 0; i < _particles.Length; i++)
        {
            if (_particles[i].Percent >= 1f)
            {
                Reset(i, 0f);
            }

            //float divisor = _direction switch
            //{
            //    Direction.Up => 4f,
            //    Direction.Down => 1.5f,
            //    _ => 1f,
            //};
            _particles[i].Percent += Engine.DeltaTime / _particles[i].Duration;
            _particles[i].Position += ((_particles[i].Direction * _particles[i].Speed) + zero / 4f) * Engine.DeltaTime;
            _particles[i].Direction.Rotate(_particles[i].Spin * Engine.DeltaTime);
        }
    }

    private float Mod(float x, float m)
    {
        return ((x % m) + m) % m;
    }

    private void Reset(int i, float p)
    {
        _particles[i].Percent = p;
        _particles[i].Position = new Vector2(Calc.Random.Range(0, _loopWidth), Calc.Random.Range(0, _loopHeight));
        _particles[i].Speed = Calc.Random.Range(4, 14);
        _particles[i].Spin = Calc.Random.Range(0.25f, (float)Math.PI * 6f);
        _particles[i].Duration = Calc.Random.Range(1f, 4f);
        _particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
        _particles[i].Color = Calc.Random.Next(_colors.Length);
    }

    public struct Particle
    {
        public Vector2 Position;
        public float Percent;
        public float Duration;
        public Vector2 Direction;
        public float Speed;
        public float Spin;
        public int Color;
    }
}

using ChroniaHelper.Cores;

namespace ChroniaHelper.Components;

public class StandardWave : BaseComponent
{
    public float frequency = 2.0f;
    public float amplitude = 4.0f;
    public float phase = 0.0f;
    public StandardWave(float frequency, float amplitude, float phase)
    {
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.phase = phase;
    }

    private float time = 0.0f;
    public override void Update()
    {
        time += Engine.DeltaTime;
    }

    public float Sin => amplitude * MathF.Sin(time * frequency + phase);
    public float Cos => amplitude * MathF.Cos(time * frequency + phase);
    public float Tan => amplitude * MathF.Tan(time * frequency + phase);
}

public class StandardWave2 : BaseComponent
{
    public Vector2 frequency = 2.0f * Vector2.One;
    public Vector2 amplitude = 4.0f * Vector2.One;
    public Vector2 phase = Vector2.Zero;
    public StandardWave2(Vector2 frequency, Vector2 amplitude, Vector2 phase)
    {
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.phase = phase;
    }

    private float time = 0.0f;
    public override void Update()
    {
        time += Engine.DeltaTime; 
    }

    private float WaveX => time * frequency.X + phase.X;
    private float WaveY => time * frequency.Y + phase.Y;

    public Vector2 Sin => amplitude * new Vector2(MathF.Sin(WaveX), MathF.Sin(WaveY));
    public Vector2 Cos => amplitude * new Vector2(MathF.Cos(WaveX), MathF.Cos(WaveY));
    public Vector2 Tan => amplitude * new Vector2(MathF.Tan(WaveX), MathF.Tan(WaveY));
}

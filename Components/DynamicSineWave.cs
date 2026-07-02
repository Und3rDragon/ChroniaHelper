using ChroniaHelper.Cores;

namespace ChroniaHelper.Components;

public class DynamicSineWave : BaseComponent
{
    public (float Min, float Max) AmpRange;
    public (float Min, float Max) FreqRange;
    public (float Min, float Max) PhaseRange;
    public bool UseSystemTime = false;

    private double _amplitude;
    private double _frequency;
    private double _phase;
    private double _phaseOffset = 0;      // 累计相位偏移（用于保持连续性）
    private double _lastValue = 0;

    private DateTime _startTime;
    private float _timer = 0f;

    public DynamicSineWave(
        (float, float) ampRange,
        (float, float) freqRange,
        (float, float) phaseRange,
        bool useSystemTime = false)
    {
        AmpRange = (Math.Min(ampRange.Item1, ampRange.Item2), Math.Max(ampRange.Item1, ampRange.Item2));
        FreqRange = (Math.Min(freqRange.Item1, freqRange.Item2), Math.Max(freqRange.Item1, freqRange.Item2));
        PhaseRange = (Math.Min(phaseRange.Item1, phaseRange.Item2), Math.Max(phaseRange.Item1, phaseRange.Item2));
        UseSystemTime = useSystemTime;

        _startTime = DateTime.Now;
        _amplitude = RandomInRange(AmpRange);
        _frequency = RandomInRange(FreqRange);
        _phase = RandomInRange(PhaseRange);
        _phaseOffset = 0;
        _lastValue = 0;
    }

    private double RandomInRange((float Min, float Max) range)
    {
        return Rd.RandomFloat(range.Min, range.Max);
    }

    private double GetCurrentTime()
    {
        if (UseSystemTime)
            return (DateTime.Now - _startTime).TotalSeconds;
        else
            return _timer;
    }

    public override void Update()
    {
        base.Update();
        _timer += Engine.DeltaTime;

        double t = GetCurrentTime();
        double rawAngle = _frequency * t + _phase - _phaseOffset;

        // 检测是否刚跨过零点（函数值符号变化）
        double currentValue = _amplitude * Math.Sin(rawAngle);
        bool crossedZero = (_lastValue > 0 && currentValue < 0) || (_lastValue < 0 && currentValue > 0);

        if (crossedZero)
        {
            // 计算精确的过零点相位偏移
            // 在过零点处，sin(angle) = 0，所以 angle = n * PI
            // 我们需要找到最近的 n*PI
            double n = Math.Round(rawAngle / Math.PI);
            double zeroAngle = n * Math.PI;
            
            // 保存当前的相位偏移，确保波形连续
            _phaseOffset += rawAngle - zeroAngle;

            // 随机生成新的参数
            _amplitude = RandomInRange(AmpRange);
            _frequency = RandomInRange(FreqRange);
            _phase = RandomInRange(PhaseRange);
        }

        _lastValue = currentValue;
    }

    public double GetValue()
    {
        double t = GetCurrentTime();
        double rawAngle = _frequency * t + _phase - _phaseOffset;
        return _amplitude * Math.Sin(rawAngle);
    }

    /// <summary>
    /// 获取当前原始角度（用于调试）
    /// </summary>
    public double GetRawAngle()
    {
        double t = GetCurrentTime();
        return _frequency * t + _phase - _phaseOffset;
    }

    /// <summary>
    /// 手动重置波形（强制在下一个零点重置）
    /// </summary>
    public void ResetAtNextZero()
    {
        // 强制将 lastValue 设为 0，让 Update 在下一帧检测到过零
        _lastValue = 0;
    }
}
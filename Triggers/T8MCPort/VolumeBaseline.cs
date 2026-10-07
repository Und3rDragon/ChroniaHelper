using ChroniaHelper.Utils;
using System;

namespace ChroniaHelper.Triggers.T8MCPort;

public class VolumeBaseline {
    private const float Epsilon = 0.00001f;
    public float BaseVolume { get; private set; }
    private float lastWritten;

    public VolumeBaseline(float initialVolume) {
        BaseVolume = initialVolume;
        lastWritten = initialVolume;
    }

    public float Apply(float currentVolume, float multiplier) {
        // Other entities/mods may change their own instance's volume.
        // Preserve that change instead of repeatedly multiplying our own output.
        if (Math.Abs(currentVolume - lastWritten) > Epsilon)
            BaseVolume = currentVolume;
        lastWritten = BaseVolume * multiplier.Clamp(0f, 1f);
        return lastWritten;
    }

    public float Restore(float currentVolume) {
        if (Math.Abs(currentVolume - lastWritten) > Epsilon)
            BaseVolume = currentVolume;
        lastWritten = BaseVolume;
        return BaseVolume;
    }
}

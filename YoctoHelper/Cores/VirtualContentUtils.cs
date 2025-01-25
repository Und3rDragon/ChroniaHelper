using ChroniaHelper;

namespace YoctoHelper.Cores;

public static class VirtualContentUtils
{

    public static VirtualRenderTarget DustBunny { get; set; }

    static VirtualContentUtils()
    {
        VirtualContentUtils.DustBunny = VirtualContentUtils.CreateRenderTarget("DustBunny");
    }

    public static VirtualRenderTarget CreateRenderTarget(string name)
    {
        return VirtualContent.CreateRenderTarget($"{ChroniaHelperModule.Name}.{name}", GameplayBuffers.Gameplay.Width, GameplayBuffers.Gameplay.Height, depth: false, preserve: true, multiSampleCount: 0);
    }

    public static VirtualRenderTarget CreateRenderTarget(string name, int width, int height, bool depth = false, bool preserve = true, int multiSampleCount = 0)
    {
        return VirtualContent.CreateRenderTarget($"{ChroniaHelperModule.Name}.{name}", width, height, depth, preserve, multiSampleCount);
    }

}

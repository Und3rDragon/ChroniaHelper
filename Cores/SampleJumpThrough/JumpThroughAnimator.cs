using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace ChroniaHelper.Cores.SampleJumpThrough;

/// <summary>
/// 跳穿板贴图动画组件：按固定间隔在图集序列间切换，
/// 并按格索引从当前图集中取出对应的贴图块绘制。
/// </summary>
public class JumpThroughAnimator : Component
{
    /// <summary>
    /// 单个贴图块的边长。
    /// </summary>
    private const int CellSize = 8;

    /// <summary>
    /// 相邻两帧之间的显示时长。
    /// </summary>
    private readonly float delay;

    /// <summary>
    /// 各帧图集，每项为一张完整的板体贴图。
    /// </summary>
    private readonly MTexture[] atlasFrames;

    /// <summary>
    /// 板体占据的格子数量。
    /// </summary>
    private readonly int cells;

    /// <summary>
    /// 格子是否沿水平方向排布：为 false 时沿垂直方向排布。
    /// </summary>
    private readonly bool horizontal;

    /// <summary>
    /// 绘制时整体旋转的角度。
    /// </summary>
    private readonly float rotation;

    /// <summary>
    /// 是否上下翻转绘制。
    /// </summary>
    private readonly bool flipY;

    /// <summary>
    /// 各格在水平方向上的额外偏移。
    /// </summary>
    private readonly float xOffset;

    /// <summary>
    /// 各格在垂直方向上的额外偏移。
    /// </summary>
    private readonly float yOffset;

    /// <summary>
    /// 每格的贴图块在所属图集中的列与行。
    /// </summary>
    private readonly Point[] blockPositions;

    /// <summary>
    /// 每格绘制时使用的颜色。
    /// </summary>
    private readonly Color color;

    /// <summary>
    /// 数值越大绘制越靠前。
    /// </summary>
    public float Depth;

    private float timer;
    private int frameIndex;

    public JumpThroughAnimator(
        MTexture[] atlasFrames, int cells, bool horizontal, float delay,
        Point[] blockPositions, float rotation = 0f, bool flipY = false,
        float xOffset = 0f, float yOffset = 0f, Color? color = null)
        : base(active: true, visible: true)
    {
        this.atlasFrames = atlasFrames;
        this.cells = cells;
        this.horizontal = horizontal;
        this.delay = delay;
        this.blockPositions = blockPositions;
        this.rotation = rotation;
        this.flipY = flipY;
        this.xOffset = xOffset;
        this.yOffset = yOffset;
        this.color = color ?? Color.White;
    }

    public override void Update()
    {
        base.Update();

        if (atlasFrames.Length <= 1 || delay <= 0f)
        {
            return;
        }

        timer += Engine.DeltaTime;

        while (timer >= delay)
        {
            timer -= delay;
            frameIndex = (frameIndex + 1) % atlasFrames.Length;
        }
    }

    public override void Render()
    {
        base.Render();

        if (Entity?.Scene == null || atlasFrames.Length == 0)
        {
            return;
        }

        MTexture frame = atlasFrames[frameIndex];
        Vector2 position = Entity.Position;

        // 与静态绘制保持一致：翻转通过负的纵向缩放实现
        Vector2 scale = new Vector2(1f, flipY ? -1f : 1f);

        for (int i = 0; i < cells; i++)
        {
            Point block = blockPositions[i];

            MTexture blockTexture = frame.GetSubtexture(
                block.X * CellSize, block.Y * CellSize, CellSize, CellSize);

            // 与静态绘制保持一致：以格子左上角为基准，绘制原点同为左上角
            Vector2 offset = horizontal
                ? new Vector2(i * CellSize, 0f)
                : new Vector2(0f, i * CellSize);

            Vector2 origin = Vector2.Zero;

            blockTexture.Draw(position + offset + new Vector2(xOffset, yOffset), origin, color, scale, rotation);
        }
    }
}

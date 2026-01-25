using ChroniaHelper;
using Microsoft.Xna.Framework.Graphics;
using System;
using YoctoHelper.Components;
using ChroniaHelper.Utils;

namespace YoctoHelper.Cores;

[Tracked(false)]
public class DustBunnyEdges : Entity
{

    private bool hasDustBunny { get; set; }

    private float noiseEase { get; set; }

    private Vector2 noiseFromPos { get; set; }

    private Vector2 noiseToPos { get; set; }

    private VirtualTexture dustNoiseFrom { get; set; }

    private VirtualTexture dustNoiseTo { get; set; }

    private static Effect FxDust { get; set; }

    public static int DustBunnyGraphicEstabledCounter { get; set; }

    static DustBunnyEdges()
    {
        DustBunnyEdges.FxDust = GFX.LoadFx("Dust");
    }

    public DustBunnyEdges()
    {
        base.Add(new BeforeRenderHook(this.BeforeRender));
        base.AddTag(Tags.Global | Tags.TransitionUpdate);
        base.Depth = Depths.Dust + 5;
    }

    public void BeforeRender()
    {
        if (base.Scene.Tracker.GetComponents<DustBunnyEdge>().Count <= 0)
        {
            return;
        }
        if ((ObjectUtils.IsNull(this.dustNoiseFrom)) || (this.dustNoiseFrom.IsDisposed))
        {
            this.CreateTextures();
        }
        Vector2 vector = this.FlooredCamera();
        Engine.Graphics.GraphicsDevice.Textures[1] = this.dustNoiseFrom.Texture_Safe;
        Engine.Graphics.GraphicsDevice.Textures[2] = this.dustNoiseTo.Texture_Safe;
        DustBunnyEdges.FxDust.Parameters["noiseEase"].SetValue(this.noiseEase);
        DustBunnyEdges.FxDust.Parameters["noiseFromPos"].SetValue(this.noiseFromPos + new Vector2(vector.X / 320F, vector.Y / 180F));
        DustBunnyEdges.FxDust.Parameters["noiseToPos"].SetValue(this.noiseToPos + new Vector2(vector.X / 320F, vector.Y / 180F));
        DustBunnyEdges.FxDust.Parameters["pixel"].SetValue(new Vector2(0.003125F, 1F / 180F));
        foreach (Color color in Md.Session.DustBunnyEdgeColor.Keys)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, (base.Scene as Level).Camera.Matrix);
            foreach (DustBunnyEdge dustBunnyEdge in Md.Session.DustBunnyEdgeColor[color])
            {
                if ((dustBunnyEdge.Visible) && (dustBunnyEdge.Entity.Visible))
                {
                    dustBunnyEdge.RenderDustBunny();
                }
            }
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.ResortDust);
            DustBunnyEdges.FxDust.Parameters["colors"].SetValue(new Vector3[] { color.ToVector3(), color.ToVector3(), color.ToVector3() });
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, DustBunnyEdges.FxDust, Matrix.Identity);
            Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }

    private void CreateTextures()
    {
        this.dustNoiseFrom = VirtualContent.CreateTexture("dust-noise-a", 128, 72, Color.White);
        this.dustNoiseTo = VirtualContent.CreateTexture("dust-noise-b", 128, 72, Color.White);
        Color[] array = new Color[this.dustNoiseFrom.Width * this.dustNoiseTo.Height];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new Color(Calc.Random.NextFloat(), 0F, 0F, 0F);
        }
        this.dustNoiseFrom.Texture_Safe.SetData(array);
        for (int j = 0; j < array.Length; j++)
        {
            array[j] = new Color(Calc.Random.NextFloat(), 0F, 0F, 0F);
        }
        this.dustNoiseTo.Texture_Safe.SetData(array);
    }

    private Vector2 FlooredCamera()
    {
        Vector2 position = (base.Scene as Level).Camera.Position;
        position.X = (int)Math.Floor(position.X);
        position.Y = (int)Math.Floor(position.Y);
        return position;
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        this.Dispose();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        this.Dispose();
    }

    public override void Update()
    {
        this.noiseEase = Calc.Approach(this.noiseEase, 1F, Engine.RawDeltaTime);
        if (this.noiseEase == 1F)
        {
            VirtualTexture dustNoiseFrom = this.dustNoiseFrom;
            this.dustNoiseFrom = this.dustNoiseTo;
            this.dustNoiseTo = dustNoiseFrom;
            this.noiseFromPos = this.noiseToPos;
            this.noiseToPos = new Vector2(Calc.Random.NextFloat(), Calc.Random.NextFloat());
            this.noiseEase = 0F;
        }
        DustBunnyEdges.DustBunnyGraphicEstabledCounter = 0;
    }

    public override void Render()
    {
        if (this.hasDustBunny)
        {
            Draw.SpriteBatch.Draw(VirtualContentUtils.DustBunny, this.FlooredCamera(), Color.White);
        }
    }

    public override void HandleGraphicsReset()
    {
        base.HandleGraphicsReset();
        this.Dispose();
    }

    private void Dispose()
    {
        if (ObjectUtils.IsNotNull(this.dustNoiseFrom))
        {
            this.dustNoiseFrom.Dispose();
        }
        if (ObjectUtils.IsNotNull(this.dustNoiseTo))
        {
            this.dustNoiseTo.Dispose();
        }
    }

}

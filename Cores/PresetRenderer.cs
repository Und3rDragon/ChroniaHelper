using Celeste.Mod.UI;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public class PresetRenderer
{
    public PresetRenderer(string bufferID, int bufferWidth = 1920, int bufferHeight = 1080)
    {
        BufferID = bufferID;
        BufferWidth = bufferWidth;
        BufferHeight = bufferHeight;
    }

    public VirtualRenderTarget Buffer;
    public string BufferID;
    public int BufferWidth = 1920, BufferHeight = 1080;
    public class SpriteBatchConfig
    {
        public static SpriteBatchConfig SubHUD;
        public static SpriteBatchConfig SubHUDRender;
        public static SpriteBatchConfig Gameplay;
        public static SpriteBatchConfig GameplayRender;
        static SpriteBatchConfig()
        {
            SubHUD = new SpriteBatchConfig();
            SubHUDRender = new SpriteBatchConfig()
            {
                effect = ColorGrade.Effect, 
                matrix = Engine.ScreenMatrix.M11 * 6 < 6 ? Matrix.Identity : Engine.ScreenMatrix
            };
            Gameplay = new SpriteBatchConfig();
            GameplayRender = new SpriteBatchConfig()
            {
                effect = ColorGrade.Effect,
                matrix = GameplayRenderer.instance.Camera.Matrix.M11 < 6 ? 
                    Matrix.Identity : GameplayRenderer.instance.Camera.Matrix,
            };
        }

        public SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
        public BlendState blendState = BlendState.AlphaBlend;
        public SamplerState samplerState = SamplerState.PointClamp;
        public DepthStencilState depthStencilState = DepthStencilState.Default;
        public RasterizerState rasterizerState = RasterizerState.CullNone;
        public Effect effect = null;
        public Matrix matrix = Matrix.Identity;

        public void BeginBatch()
        {
            Draw.SpriteBatch.Begin(spriteSortMode,
                blendState, samplerState,
                depthStencilState, rasterizerState,
                effect, matrix);
        }

        public void EndBatch()
        {
            Draw.SpriteBatch.End();
        }
    }
    public SpriteBatchConfig batchConfig = SpriteBatchConfig.SubHUD;
    public SpriteBatchConfig renderConfig = SpriteBatchConfig.SubHUDRender;

    public class BufferConfig
    {
        public static BufferConfig Default;
        static BufferConfig()
        {
            Default = new BufferConfig();
        }

        public Vc2 position = Vc2.Zero;
        public Rectangle? sourceRectangle = null;
        public CColor overlayColor = CColor.White;
        /// <summary>
        /// Unit: Rad
        /// </summary>
        public float rotation = 0;
        public Vc2 origin = Vc2.Zero;
        public float scale = 1;
        public SpriteEffects spriteEffect = SpriteEffects.None;
        public float layerDepth = 0;

        public void DrawBuffer(VirtualRenderTarget Buffer)
        {
            if(Buffer is null)
            {
                return;
            }

            Draw.SpriteBatch.Draw(Buffer, position, sourceRectangle,
                overlayColor.Parsed(),
                rotation, origin, scale,
                spriteEffect, layerDepth);
        }
    }
    public BufferConfig bufferConfig = BufferConfig.Default;

    /// <summary>
    /// This should be done in BeforeRender Hook
    /// </summary>
    public void SetupRenderTarget()
    {
        // Create a new render target for later renders
        if (Buffer?.Target is null)
        {
            Buffer = VirtualContent.CreateRenderTarget(BufferID, BufferWidth, BufferHeight);
        }

        // Change the render canvas to my own canvas
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
        // Clear up the canvas
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        // Start a new SpriteBatch
        batchConfig.BeginBatch();

        // Set up the render data
        createDrawData.InvokeAll();
        CreateDrawData();

        // End the Sprite Batch and start rendering
        batchConfig.EndBatch();
    }
    public ActionManager createDrawData = new();
    public virtual void CreateDrawData() { }

    public void SetupSubHUDRenderTarget()
    {
        // Create a new render target for later renders
        if (Buffer?.Target is null)
        {
            Buffer = VirtualContent.CreateRenderTarget(BufferID, BufferWidth, BufferHeight);
        }

        // Change the render canvas to my own canvas
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
        // Clear up the canvas
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        // Start a new SpriteBatch
        SpriteBatchConfig.SubHUD.BeginBatch();

        // Set up the render data
        createDrawData.InvokeAll();
        CreateDrawData();

        // End the Sprite Batch and start rendering
        SpriteBatchConfig.SubHUD.EndBatch();
    }

    public void HandleRender()
    {
        // SubHud SpriteBatch end and start rendering
        endRenderer.InvokeAll();
        EndRenderer();

        // Start a new SpriteBatch
        renderConfig.BeginBatch();

        // Send my canvas to the SpriteBatch
        bufferConfig.DrawBuffer(Buffer);

        // End the batch and start rendering
        endRenderer.InvokeAll();
        EndRenderer();

        // Recover to the normal render SpriteBatch
        startRenderer.InvokeAll();
        StartRenderer();
    }

    public ActionManager startRenderer = new(), endRenderer = new();
    public virtual void StartRenderer() { }
    public virtual void EndRenderer() { }

    public void SubHudRender()
    {
        // SubHud SpriteBatch end and start rendering
        SubHudRenderer.EndRender();

        // Start a new SpriteBatch
        SpriteBatchConfig.SubHUDRender.BeginBatch();

        // Send my canvas to the SpriteBatch
        BufferConfig.Default.DrawBuffer(Buffer);

        // End the batch and start rendering
        SubHudRenderer.EndRender();

        // Recover to the normal render SpriteBatch
        SubHudRenderer.BeginRender();
    }

    public void GameplayRender()
    {
        // SpriteBatch end and start rendering
        GameplayRenderer.End();

        // Start a new SpriteBatch
        SpriteBatchConfig.GameplayRender.BeginBatch();

        // Send my canvas to the SpriteBatch
        BufferConfig.Default.DrawBuffer(Buffer);

        // End the batch and start rendering
        GameplayRenderer.End();

        // Recover to the normal render SpriteBatch
        GameplayRenderer.Begin();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.UI;
using ChroniaHelper.Entities;
using ChroniaHelper.Settings;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Cores;

public class HDRenderEntity : BaseEntity
{
    public HDRenderEntity(EntityData d, Vc2 o): base(d, o)
    {
        nodes = d.NodesWithPosition(o);
        ID = d.ID;

        Prepare(d, o);

        Tag |= TagsExt.SubHUD;
        
        // Create a new render target for later renders
        Buffer = VirtualContent.CreateRenderTarget("ChroniaHelper_HDEntity_" + ID.ToString(), 1920, 1080);

        Add(new BeforeRenderHook(BeforeRender));
    }
    public VirtualRenderTarget Buffer;
    public Vc2 Parallax = Vc2.One;
    public Vc2 StaticScreen = new Vc2(160f, 90f);
    public CColor DrawColor = new CColor(Color.White);
    
    public virtual void Prepare(EntityData data, Vc2 offset) { }
    public void BeforeRender()
    {
        // Change the render canvas to my own canvas
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
        // Clear up the canvas
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        // Start a new SpriteBatch
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);

        // Set up the render data
        HDRender(); 

        // End the Sprite Batch and start rendering
        Draw.SpriteBatch.End();
    }
    
    /// <summary>
    /// Process the render data
    /// </summary>
    protected virtual void HDRender() { }
    
    /// <summary>
    /// Don't use this if the class is delegated to HDRendererEntity, use HDRender() instead
    /// </summary>
    public override void Render()
    {
        MTexture orDefault = GFX.ColorGrades.GetOrDefault((Scene as Level).lastColorGrade, GFX.ColorGrades["none"]);
        MTexture orDefault2 = GFX.ColorGrades.GetOrDefault((Scene as Level).Session.ColorGrade, GFX.ColorGrades["none"]);
        if ((Scene as Level).colorGradeEase > 0f && orDefault != orDefault2)
        {
            ColorGrade.Set(orDefault, orDefault2, (Scene as Level).colorGradeEase);
        }
        else
        {
            ColorGrade.Set(orDefault2);
        }
        
        // Normal Render
        base.Render();

        // SubHud SpriteBatch end and start rendering
        SubHudRenderer.EndRender();

        // Start a new SpriteBatch
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Engine.ScreenMatrix.M11 * 6 < 6 ? Matrix.Identity : Engine.ScreenMatrix);

        // Send my canvas to the SpriteBatch
        Draw.SpriteBatch.Draw(Buffer, Vc2.Zero, null, DrawColor.Parsed(),
            0, Vector2.Zero, 1,
            SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

        // End the batch and start rendering
        SubHudRenderer.EndRender();

        // Recover to the normal render SpriteBatch
        SubHudRenderer.BeginRender();
    }
    
    public Vc2 ParseLevelPositionToHDPosition(Vc2 position, Vc2 parallax, Vc2 staticScreen)
    {
        Vc2 normal = position - (MaP.cameraPos - MaP.levelPos) * parallax;

        return new Vc2(parallax.X == 0 ? StaticScreen.X : normal.X, parallax.Y == 0 ? StaticScreen.Y : normal.Y) * Cons.HDScale;
    }

    public Vc2 ParseGlobalPositionToHDPosition(Vc2 globalPosition, Vc2 parallax, Vc2 staticScreen)
    {
        Vc2 normal = (globalPosition - MaP.levelPos) - (MaP.cameraPos - MaP.levelPos) * parallax;
        
        return new Vc2(parallax.X == 0 ? StaticScreen.X : normal.X, parallax.Y == 0 ? StaticScreen.Y : normal.Y) * Cons.HDScale;
    }

    public override void Removed(Scene scene)
    {
        Buffer?.Dispose();
        
        base.Removed(scene);
    }
    public override void SceneEnd(Scene scene)
    {
        Buffer?.Dispose();
        
        base.SceneEnd(scene);
    }
}

using Celeste.Mod.UI;
using ChroniaHelper.Entities;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChroniaHelper.Utils.Miscs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Cores;

public class HDRenderEntity : BaseEntity
{
    public HDRenderEntity(EntityData d, Vc2 o): base(d, o)
    {
        nodes = d.NodesWithPosition(o);
        ID = d.ID;

        PrepareBeforeRenderHook(d, o);

        Tag |= TagsExt.SubHUD;

        Add(new BeforeRenderHook(BeforeRender));
    }
    public VirtualRenderTarget Buffer = null;
    public Vc2 Parallax = Vc2.One;
    public Vc2 StaticScreen = new Vc2(160f, 90f);
    public CColor DrawColor = new CColor(Color.White);

    public bool beforeRenderHookRunning = true;
    
    public virtual void PrepareBeforeRenderHook(EntityData data, Vc2 offset) { }

    [Credits("SSM24 for some technical bugfix")]
    public void BeforeRender()
    {
        if (!beforeRenderHookRunning)
        {
            Buffer = null;
            return;
        }

        // Create a new render target for later renders
        if (Buffer?.Target == null)
        {
            Buffer = VirtualContent.CreateRenderTarget("ChroniaHelper_HDEntity_" + ID.ToString(), 1920, 1080);
        }

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

        if(Buffer?.Target != null)
        {
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
    }
    
    public Vc2 ParseLevelPositionToHDPosition(Vc2 inLevelPosition, Vc2 parallax, Vc2 staticScreen)
    {
        Vc2 globalPosition = inLevelPosition + MaP.levelPos;
        Vc2 cameraPosition = MaP.cameraPos;
        Vc2 cameraCenter = cameraPosition + Miscs.Screen.Size * 0.5f;
        Vc2 screenPosition = parallax == Vc2.One ?
            (globalPosition - cameraPosition) * Cons.HDScale * (Cons.VanillaCanvas / Miscs.Screen.Size) :
            Cons.HDCanvas * ((parallax == Vc2.Zero ? staticScreen / Cons.VanillaCanvas : 0.5f * Vc2.One) + (globalPosition - cameraCenter) * parallax / Miscs.Screen.Size);
        // screenPosition is calculated based on 1080p canvas

        return screenPosition;
    }

    public Vc2 ParseGlobalPositionToHDPosition(Vc2 globalPosition, Vc2 parallax, Vc2 staticScreen)
    {
        Vc2 cameraPosition = MaP.cameraPos;
        Vc2 cameraCenter = cameraPosition + Miscs.Screen.Size * 0.5f;
        Vc2 screenPosition = parallax == Vc2.One ?
            (globalPosition - cameraPosition) * Cons.HDScale * (Cons.VanillaCanvas / Miscs.Screen.Size) :
            Cons.HDCanvas * ((parallax == Vc2.Zero ? staticScreen / Cons.VanillaCanvas : 0.5f * Vc2.One) + (globalPosition - cameraCenter) * parallax / Miscs.Screen.Size);
        // screenPosition is calculated based on 1080p canvas

        return screenPosition;
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

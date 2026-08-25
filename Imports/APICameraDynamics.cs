using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

[ModImportName("ExtendedCameraDynamics")] // registered in Module

public static class APICameraDynamics
{
    /// <returns> Whether the extended camera hooks are currently applied. </returns>
    public static bool extendedCameraHooksEnabled
        => ExtendedCameraHooksEnabled?.Invoke() ?? false;
    public static Func<bool> ExtendedCameraHooksEnabled;

    /// <summary> Multiplies the camera's interpolation by a fixed amount. </summary>
    public static Action<float> SetSnappingSpeed;

    #region Render Status

    /// <returns> The current width, in pixels, of the general render buffers. </returns>
    public static int bufferWidthOverride => BufferWidthOverride?.Invoke() ?? 1920;
    public static Func<int> BufferWidthOverride;

    /// <returns> The current height, in pixels, of the general render buffers. </returns>
    public static int  bufferHeightOverride => BufferHeightOverride?.Invoke() ?? 1080;
    public static Func<int> BufferHeightOverride;

    /// <returns> The current dimensions of the visible camera.</returns>
    public static Vector2 getCameraDimensions(Level level)
        => GetCameraDimensions?.Invoke(level) ?? level.Camera.Position;
    public static Func<Level, Vector2> GetCameraDimensions;

    /// <summary>
    /// Resizes the given buffer to the size of the general render buffers. <br></br>
    /// This will only resize the target if a resize is called for. <br></br>
    /// I suggest calling this in a relevant 'BeforeRender' hook if you're using a custom <see cref="VirtualRenderTarget"/> that needs to fill the 320x180 screen. <br></br>
    /// </summary>
    public static Action<VirtualRenderTarget> ResizeVirtualRenderTarget;

    public static Action<bool> SetRenderVerticalMirroring;

    #endregion

    #region Vanilla Zoom Method Replacements


    /// <summary>
    /// A drop-in replacement for <see cref="Level.ResetZoom()"/>
    /// This exists for completion's sake.
    /// <param name="level">The level object</param>
    /// </summary>
    public static Action<Level> Level_ResetZoom;

    /// <summary> A drop-in replacement for <see cref="Level.ZoomBack(float)"/> </summary>
    public static IEnumerator levelZoomBack(Level level, float duration)
        => Level_ZoomBack?.Invoke(level, duration);
    public static Func<Level, float, IEnumerator> Level_ZoomBack;

    /// <summary>
    /// A near-replacement for <see cref="Level.ZoomTo(Vector2, float, float)"/>. <br></br>
    /// The camera centers itself over <paramref name="worldFocusPoint"/>.
    /// </summary>
    /// <param name="worldFocusPoint">The position in world space to center the camer over.</param>
    /// <returns></returns>
    public static IEnumerator levelZoomToFocus(Level level, Vector2 worldFocusPoint, float zoom, float duration)
        => Level_ZoomToFocus?.Invoke(level, worldFocusPoint, zoom, duration);
    public static Func<Level, Vector2, float, float, IEnumerator> Level_ZoomToFocus;

    /// <summary>
    /// A near-replacement for <see cref="Level.ZoomTo(Vector2, float, float)"/>. <br></br>
    /// Allows you to specify a custom easer.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator levelZoomToCameraFocus(Level level, Vector2 worldFocusPoint, float zoom, float duration)
        => Level_ZoomToFocus?.Invoke(level, worldFocusPoint, zoom, duration);
    public static Func<Level, Vector2, float, float, Ease.Easer, IEnumerator> Level_ZoomToFocus_Eased;

    /// <summary>
    /// Zoom to a CameraReferenceFrame in the level denoted by easyKey
    /// </summary>
    /// <param name="level">The level object</param>
    /// <param name="easyKey">The easykey of a CameraReferenceFrame</param>
    /// <param name="duration">Time, in seconds, to zoom over.</param>
    /// <param name="ease">The Easer for the Zoom. Defaults to <see cref="Ease.SineInOut"></see> if left null.</param>
    /// <returns></returns>
    public static IEnumerator levelZoomToReferenceFrameKey(Level level, string easyKey, float duration,
        Ease.Easer ease = null)
        => Level_ZoomToReferenceFrameKey?.Invoke(level, easyKey, duration, ease);
    public static Func<Level, string, float, Ease.Easer, IEnumerator> Level_ZoomToReferenceFrameKey;


    /// <summary> <see cref="CameraReferenceFrame"/>s are placed in loenn to easily get camera positions for cutscenes and the like. You can get them here. </summary>
    public static Entity getCameraReferenceFrame(Level level, string easyKey) => Get_CameraReferenceFrame?.Invoke(level, easyKey);
    public static Func<Level, string, Entity> Get_CameraReferenceFrame;

    /// <summary> Zooms to a <see cref="CameraReferenceFrame"/> over duration. </summary>
    public static IEnumerator levelZoomToCameraReferenceFrame(Level level, Entity cameraReferenceFrame, float duration) => Level_ZoomToCameraReferenceFrame?.Invoke(level, cameraReferenceFrame, duration);
    public static Func<Level, Entity, float, IEnumerator> Level_ZoomToCameraReferenceFrame;

    public static Action<Level, object> Level_ForceZoomToCameraFocus;

    /// <returns> The zoom evaluated from <see cref="CameraZoomTrigger"/>s at <paramref name="worldPoint"/> </returns>
    public static float levelGetTriggerZoomAt(Level level, Vector2 worldPoint) =>
        Level_GetTriggerZoomAt?.Invoke(level, worldPoint) ?? 1f;
    public static Func<Level, Vector2, float> Level_GetTriggerZoomAt;

    #endregion

    #region CameraFocusTarget

    /// <returns>The CameraFocusTarget Component Type</returns>
    public static Type typeCameraFocusTarget => Type_CameraFocusTarget?.Invoke();
    public static Func<Type> Type_CameraFocusTarget;

    /// <summary>
    /// The game will try to keep all <see cref="CameraFocusTarget"/>s on screen. <br></br>
    /// For reference, the player has a weight of 1. <br></br>
    /// If all targets cannot fit on the screen, then the game will try and zoom out as far as the triggers allow. <br></br>
    /// If it still can't fit them, it will prioritize the player above all else.
    /// </summary>
    /// <param name="entityOffset">The offset relative from the entity to focus on.</param>
    /// <returns> A newly constructed <see cref="CameraFocusTarget"/> component. </returns>
    public static Component createCameraFocusTarget(Vector2 entityOffset, float weight)
        => Create_CameraFocusTarget?.Invoke(entityOffset, weight);
    public static Func<Vector2, float, Component> Create_CameraFocusTarget;

    /// <returns> An entity's <see cref="CameraFocusTarget"/>. </returns>
    public static Component getCameraFocusTarget(Entity ent) => Get_CameraFocusTarget?.Invoke(ent);
    public static Func<Entity, Component> Get_CameraFocusTarget;

    /// <summary> Set a <see cref="CameraFocusTarget"/>'s offset </summary>
    public static Action<Component, Vector2> CameraFocusTarget_SetOffset;

    /// <summary> Set a <see cref="CameraFocusTarget"/>'s weight </summary>
    public static Action<Component, float> CameraFocusTarget_SetWeight;

    /// <returns> All <see cref="CameraFocusTarget"/>s in the Level </returns>
    public static List<Component> trackedCameraFocusTarget(Level level) => Tracked_CameraFocusTarget?.Invoke(level);
    public static Func<Level, List<Component>> Tracked_CameraFocusTarget;


    #endregion

    #region CameraFocus (Struct)

    /// <summary>
    /// Creates a CameraFocus struct based on the current position 
    /// </summary>
    public static object create_CameraFocus_FromActiveCameraPos(Level level) => Create_CameraFocus_FromActiveCameraPos?.Invoke(level);
    public static Func<Level, object> Create_CameraFocus_FromActiveCameraPos;

    /// <summary>
    /// Creates a CameraFocus struct from world position and zoom
    /// </summary>
    public static object create_CameraFocus(Vector2 world_center, float zoom_factor) => Create_CameraFocus?.Invoke(world_center, zoom_factor);
    public static Func<Vector2, float, object> Create_CameraFocus;

    /// <summary>
    /// Interpolates between two CameraFocus structs
    /// </summary>
    /// <param name="focus_a">A CameraFocus struct</param>
    /// <param name="focus_b">A CameraFocus struct</param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static object cameraFocus_Lerp(object focus_a, object focus_b, float t) => CameraFocus_Lerp?.Invoke(focus_a, focus_b, t) ?? focus_b;
    public static Func<object, object, object, object> CameraFocus_Lerp;

    /// <summary>
    /// Zoom to a CameraFocus struct over duration.
    /// </summary>
    /// <param name="level">The level object</param>
    /// <param name="cameraFocus">The CameraFocus struct in question</param>
    /// <param name="duration">The length of the motion, in seconds</param>
    /// <returns></returns>
    public static IEnumerator level_ZoomToCameraFocus(Level level, object cameraFocus, float duration) => Level_ZoomToCameraFocus?.Invoke(level, cameraFocus, duration);
    public static Func<Level, object, float, IEnumerator> Level_ZoomToCameraFocus;

    #endregion

    #region Custom Lookout Sprites

    // A hook to modify / add sprites to the custom lookouts added.
    // Called by both.
    // > the Sprite is the lookout's sprite
    // > the Player is the player (duh)
    // >> return the anim prefix the lookout should use, null if it shouldn't use one.
    // # This shouldn't be needed with most skin mods, but for more advanced stuff this should be helpful! :)
    public static Dictionary<string, Func<Sprite, Player, string, string>> customLookoutHooks =>
        CustomLookoutHooks?.Invoke();
    public static Func<Dictionary<string, Func<Sprite, Player, string, string>>> CustomLookoutHooks;
    public static void hookCustomLookoutSprite(string id, Func<Sprite, Player, string, string> externalHook) => HookCustomLookoutSprite?.Invoke(id, externalHook);
    public static Action<string, Func<Sprite, Player, string, string>> HookCustomLookoutSprite;
    public static void unhookCustomLookoutSprite(string id) => UnhookCustomLookoutSprite?.Invoke(id);
    public static Action<string> UnhookCustomLookoutSprite;

    internal static string runCustomLookout(Sprite lookoutSprite, Player player, string prefix) => RunCustomLookout?.Invoke(lookoutSprite, player, prefix);
    internal static Func<Sprite, Player, string, string> RunCustomLookout;

    #endregion
}

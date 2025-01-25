using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

[ModImportName("ExtendedCameraDynamics")] // registered in Module

public static class CameraDynamicsImports
{
    public static Func<bool> ExtendedCameraHooksEnabled;
    
    public static Func<int> BufferWidthOverride;
    public static Func<int> BufferHeightOverride;

    public static Func<Level, Vector2> GetCameraDimensions;

    public static Action<bool> SetRenderVerticalMirroring;

    public static Func<Level, float, IEnumerator> Level_ZoomBack;
    public static Func<Level, Vector2, float, float, IEnumerator> Level_ZoomToFocus;

    public static Func<Level, string, Entity> Get_CameraReferenceFrame;

    public static Func<Level, Entity, float, IEnumerator> Level_ZoomToCameraReferenceFrame;
    public static Func<Level, Vector2, float> Level_GetTriggerZoomAt;

    public static Func<Vector2, float, Component> Create_CameraFocusTarget;
    public static Func<Entity, Component> Get_CameraFocusTarget;

    public static Action<Component, Vector2> CameraFocusTarget_SetOffset;
    public static Action<Component, float> CameraFocusTarget_SetWeight;

    public static Func<Level, List<Component>> Tracked_CameraFocusTarget;

    public static Func<Type> Type_CameraFocusTarget;

    public static Action<float> SetSnappingSpeed;
}

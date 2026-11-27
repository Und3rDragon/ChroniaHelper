using System;
using System.Net;
using System.Reflection;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Settings;

/// <summary>
/// 在使用 Another Loenn Plugin 的 Teleporter 传送之后暂停关卡，直到玩家有操作输入为止。
/// 仅在设置项 PauseTimeWhenTeleportInLoenn 打开时生效。
/// </summary>
public static class LoennTeleportPause
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Update += OnLevelUpdate;
        LoadTeleportRequestHook();
    }

    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Update -= OnLevelUpdate;
        teleportRequestHook?.Dispose();
        teleportRequestHook = null;
    }

    private static ILHook teleportRequestHook;
    private static volatile bool teleportRequested;
    private static Level frozenLevel;
    private static bool previousTimerStopped;

    private static void LoadTeleportRequestHook()
    {
        MethodInfo handleRequest = FindDebugRcType()?.GetMethod("HandleRequest", BindingFlags.NonPublic | BindingFlags.Static);

        if (handleRequest == null)
        {
            Log.Warn("[LoennTeleportPause] DebugRC HandleRequest not found, loenn teleport pause is unavailable.");
            return;
        }

        teleportRequestHook = new ILHook(handleRequest, InjectTeleportDetector);
    }

    private static Type FindDebugRcType()
    {
        Type type = typeof(Celeste.Mod.Everest).GetNestedType("DebugRC", BindingFlags.Public | BindingFlags.NonPublic);

        if (type != null)
        {
            return type;
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (Type candidate in types)
            {
                if (candidate.Name == "DebugRC")
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static void InjectTeleportDetector(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Action<HttpListenerContext>>(DetectTeleportRequest);
    }

    private static void DetectTeleportRequest(HttpListenerContext context)
    {
        if (!ChroniaHelperModule.Settings.PauseTimeWhenTeleportInLoenn)
        {
            return;
        }

        if (context?.Request?.Url == null)
        {
            return;
        }

        if (context.Request.Url.AbsolutePath != "/tp")
        {
            return;
        }

        teleportRequested = true;
    }

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        if (!ChroniaHelperModule.Settings.PauseTimeWhenTeleportInLoenn)
        {
            teleportRequested = false;

            if (frozenLevel != null)
            {
                Unfreeze(frozenLevel);
            }
        }
        else
        {
            if (frozenLevel != null && frozenLevel != self)
            {
                frozenLevel = null;
            }

            if (frozenLevel == null && self.Frozen && self.TimerStopped)
            {
                frozenLevel = self;
                previousTimerStopped = false;
            }

            if (frozenLevel == self && PlayerWantsToAct())
            {
                Unfreeze(self);
            }

            Player player = self.Tracker.GetEntity<Player>();

            if (teleportRequested && player != null)
            {
                teleportRequested = false;
                Freeze(self, player);
            }
        }

        orig(self);
    }

    private static bool PlayerWantsToAct()
    {
        return Input.Grab.Check
            || Input.Dash.Pressed
            || Input.Jump.Pressed
            || Input.MenuConfirm.Pressed
            || Input.MenuCancel.Pressed
            || Input.MoveX.Value != 0
            || Input.MoveY.Value != 0;
    }

    private static void Freeze(Level level, Player player)
    {
        frozenLevel = level;
        previousTimerStopped = level.TimerStopped;
        level.Camera.Position = level.GetFullCameraTargetAt(player, player.Position);
        level.Frozen = true;
        level.TimerStopped = true;
    }

    private static void Unfreeze(Level level)
    {
        level.Frozen = false;
        level.TimerStopped = previousTimerStopped;

        if (frozenLevel == level)
        {
            frozenLevel = null;
        }
    }
}

using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public static class GlobalCoroutines
{
    [ExA.LoadHook]
    public static void Load()
    {
        On.Monocle.Engine.Update += EngineUpdate;
    }
    [ExA.UnloadHook]
    public static void Unload()
    {
        On.Monocle.Engine.Update -= EngineUpdate;
    }

    public static void EngineUpdate(On.Monocle.Engine.orig_Update orig, Engine self, GameTime time)
    {
        orig(self, time);

        foreach(var i in Coroutines)
        {
            i.Value.Update();
            if (i.Value.Finished)
            {
                CoroutinesToRemove.Add(i.Key);
            }
        }
        foreach(var i in CoroutinesToRemove)
        {
            Coroutines.Remove(i);
        }
        CoroutinesToRemove.Clear();
    }

    private static Dictionary<string, Coroutine> Coroutines = new();
    private static List<string> CoroutinesToRemove = new();
    public static void Register(string register, IEnumerator routine)
    {
        Coroutines.Enter(register, new(routine));
    }

    public static void Register(string register, Coroutine routine)
    {
        Coroutines.Enter(register, routine);
    }
}

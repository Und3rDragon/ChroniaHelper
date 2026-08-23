using System.Collections;
using ChroniaHelper.References;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.LogicExpression;
using ChroniaHelper.Utils.MathExpression;
using ChroniaHelper.Utils.StopwatchSystem;
using TextCopy;

namespace ChroniaHelper.Settings;

public class Commands
{
    [Command("chronia_full_cheat", 
        "Enable Variant Mode and Cheat Mode for a save")]
    public static void EnableFullCheat()
    {
        MaP.currentSaveData?.Item2.CheatMode = true;
        MaP.currentSaveData?.Item2.VariantMode = true;
    }

    [Command("chronia_math_expression", 
        "Try parsing a string using ChroniaHelper MathExpression")]
    public static void TryMathExpression(string expression)
    {
        CommandLog.LogDivider($"Math Expression Result to: {expression}");
        expression.ParseMathExpression().LogCommand();
    }

    [Command("chronia_logic_expression", 
        "Try parsing a string using ChroniaHelper LogicExpression")]
    public static void TryLogicExpression(string expression)
    {
        CommandLog.LogDivider($"Logic Expression Result to: {expression}");
        expression.ParseLogicExpression().LogCommand();
    }

    [Command("chronia_get_keyboard_password_hash", 
        "Try getting the encrypted password for PasswordKeyboard")]
    public static void GenerateHashedPassword(string keyboardTag, 
        string password, bool caseSensitive = true)
    {
        CommandLog.LogDivider($"Password {password} encrypted for Keyboard {keyboardTag} {(caseSensitive?"with" : "without")} sensitive case");
        string hash = StringUtils.GetHashString(keyboardTag + password, caseSensitive);
        hash.LogCommand();
        ClipboardService.SetText(hash);
        "Password copied onto your clipboard".LogCommand(Color.Yellow);
    }

    [Command("chronia_get_password_hash", 
        "Try getting the encrypted keyboard-tag-combined password for PasswordKeyboard")]
    public static void GenerateHashedPassword(string parsedPassword, 
        bool caseSensitive = true)
    {
        CommandLog.LogDivider($"Password {parsedPassword} encrypted {(caseSensitive ? "with" : "without")} sensitive case");
        string hash = StringUtils.GetHashString(parsedPassword, caseSensitive);
        hash.LogCommand();
        ClipboardService.SetText(hash);
        "Password copied onto your clipboard".LogCommand(Color.Yellow);
    }
    
    [Command("chronia_all_flags", "List all flags")]
    public static void CommandAllFlags()
    {
        if (Engine.Scene is not Level) { return; }
        if (!Md.InstanceReady) { return; }

        Log.title("Listing all the current flags").LogCommand(Color.Yellow);
        Log.title("Level flags").LogCommand(Color.Cyan);
        foreach(var i in MaP.level.Session.Flags)
        {
            i.LogCommand();
        }

        Log.title("Flags per room").LogCommand(Color.Cyan);
        foreach (var i in Md.Session.flagsPerRoom)
        {
            i.LogCommand();
        }

        Log.title("Flags per death").LogCommand(Color.Cyan);
        foreach (var i in Md.Session.flagsPerDeath)
        {
            i.LogCommand();
        }

        Log.title("Global flags").LogCommand(Color.Cyan);
        foreach (var i in Md.SaveData.flags)
        {
            i.LogCommand();
        }

        if (Md.XaphanHelperLoaded)
        {
            Log.title("Xaphan Helper Global flags").LogCommand(Color.Cyan);
            foreach (var i in RefXaphanHelper.GlobalFlags)
            {
                i.LogCommand();
            }
        }

        CommandLog.LogDivider();
    }

    [Command("chronia_all_counters", "List all counters")]
    public static void CommandAllCounters()
    {
        if (Engine.Scene is not Level) { return; }
        if (!Md.InstanceReady) { return; }

        Log.title("Listing all the current counters").LogCommand(Color.Yellow);
        Log.title("Level counters").LogCommand(Color.Cyan);
        foreach (var i in MaP.level.Session.Counters)
        {
            $"{i.Key} = {i.Value}".LogCommand();
        }

        Log.title("Counters per room").LogCommand(Color.Cyan);
        foreach (var i in Md.Session.countersPerRoom)
        {
            $"{i.Key} = {i.Value}".LogCommand();
        }

        Log.title("Counters per death").LogCommand(Color.Cyan);
        foreach (var i in Md.Session.countersPerDeath)
        {
            $"{i.Key} = {i.Value}".LogCommand();
        }

        Log.title("Global counters").LogCommand(Color.Cyan);
        foreach (var i in Md.SaveData.counters)
        {
            $"{i.Key} = {i.Value}".LogCommand();
        }

        CommandLog.LogDivider();
    }

    [Command("chronia_all_sliders", "List all sliders")]
    public static void CommandAllSliders()
    {
        if (Engine.Scene is not Level) { return; }
        if (!Md.InstanceReady) { return; }

        Log.title("Listing all the current sliders").LogCommand(Color.Yellow);
        Log.title("Level sliders").LogCommand(Color.Cyan);
        foreach (var i in MaP.level.Session.Sliders)
        {
            $"{i.Value.Name} = {i.Value.Value}".LogCommand();
        }

        Log.title("Sliders per room").LogCommand(Color.Cyan);
        foreach (var i in Md.Session.slidersPerRoom)
        {
            $"{i.Key} = {i.Value}".LogCommand();
        }

        Log.title("Sliders per death").LogCommand(Color.Cyan);
        foreach (var i in Md.Session.slidersPerDeath)
        {
            $"{i.Key} = {i.Value}".LogCommand();
        }

        Log.title("Global sliders").LogCommand(Color.Cyan);
        foreach (var i in Md.SaveData.sliders)
        {
            $"{i.Key} = {i.Value}".LogCommand();
        }

        CommandLog.LogDivider();
    }

    [Command("chronia_charcode", 
        "Get the charcode index for a certain character\n" +
        "query: the series of chars you wanna query")]
    public static void CommandGetCharcode(string query)
    {
        string result = "";
        char[] c = query.ToArray();
        for(int i = 0; i < c.Length; i++)
        {
            if( i == 0)
            {
                result += $"{(int)c[i]}";
                continue;
            }

            result += $" {(int)c[i]}";
        }

        CommandLog.LogCommand(result, Color.Yellow);
    }

    [Command("chronia_all_keys", "Log all session keys")]
    public static void CommandAllSessionKeys()
    {
        CommandLog.LogDivider("Logging every session keys", Color.Yellow);
        
        CommandLog.LogDivider("All Session Keys", Color.Yellow);
        CommandLog.LogCommand("ID => Key Value", Color.Cyan);
        foreach(var i in Md.Session.keystrings)
        {
            CommandLog.LogCommand($"{i.Key} => {i.Value}");
        }

        CommandLog.LogDivider("All Global Keys", Color.Yellow);
        CommandLog.LogCommand("ID => Key Value", Color.Cyan);
        foreach (var i in Md.SaveData.keystrings)
        {
            CommandLog.LogCommand($"{i.Key} => {i.Value}");
        }

        CommandLog.LogDivider("All SaveData Keys", Color.Yellow);
        CommandLog.LogCommand("ID => Key Value", Color.Cyan);
        foreach (var i in Md.GlobalData.permaKeys)
        {
            CommandLog.LogCommand($"{i.Key} => {i.Value}");
        }
    }

    [Command("chronia_hud", "Set HUD state")]
    public static void CommandSetHUD(bool state = true)
    {
        Md.Settings.HUDMainControl = state;
    }
    
    public static IEnumerator DelayStopclockStart(Stopclock clock, float delay)
    {
        yield return delay.ClampMin(0);

        clock.Start();
    }

    [Command("chronia_stopclock", 
        "Set up a countup stopclock")]
    public static void CommandSetUpStopclock(bool startImmediately = true, 
        bool followLevelPause = true, float startDelay = 0f)
    {
        if (MaP.scene is not Level) { return; }

        Stopclock clock = new Stopclock(false, followPause: followLevelPause);
        clock.Register(Cons.CommandStopclockID, false);

        if (startImmediately)
        {
            if(startDelay <= 0f)
            {
                clock.Start();
            }
            else
            {
                MaP.dummyGlobal.Add(new Coroutine(DelayStopclockStart(clock, startDelay)));
            }
        }
    }

    [Command("chronia_stopclock_countdown", 
        "Set up a countdown stopclock\n" +
        "time: time format like \"5:0:0\"")]
    public static void CommandSetUpCountdown(string time = "5:0:0", 
        bool startImmediately = true, bool followLevelPause = true, float startDelay = 0f)
    {
        if (MaP.scene is not Level) { return; }

        Stopclock clock = new Stopclock(true, time: time, followPause: followLevelPause);
        clock.Register(Cons.CommandStopclockID, false);

        if (startImmediately)
        {
            if (startDelay <= 0f)
            {
                clock.Start();
            }
            else
            {
                MaP.dummyGlobal.Add(new Coroutine(DelayStopclockStart(clock, startDelay)));
            }
        }
    }

    [Command("chronia_stopclock_set_time", 
        "time: time formats like \"5:0:0\"")]
    public static void CommandSetStopclock(string time = "5:0:0")
    {
        if(Md.Session.IsNull()) { return; }
        
        if(Cons.CommandStopclockID.GetStopclock(out Stopclock clock))
        {
            clock.SetTime(time);
        }
    }

    [Command("chronia_stopclock_reset", "")]
    public static void CommandResetStopclock()
    {
        if (Md.Session.IsNull()) { return; }

        if (Cons.CommandStopclockID.GetStopclock(out Stopclock clock))
        {
            clock.Reset();
        }
    }
    
    [Command("chronia_stopclock_start", "")]
    public static void CommandStartStopclock(float delay = 0f)
    {
        if (Md.Session.IsNull()) { return; }

        if (Cons.CommandStopclockID.GetStopclock(out Stopclock clock))
        {
            if (delay <= 0f)
            {
                clock.Start();
            }
            else
            {
                MaP.dummyGlobal.Add(new Coroutine(DelayStopclockStart(clock, delay)));
            }
        }
    }

    [Command("chronia_hooks", "")]
    public static void _hooks(string inputs)
    {
        "Forced Loading Hooks".Print(ConsoleColor.Magenta);
        
        foreach (var i in Ldm.forceLoadingHooks)
        {
            string s = i.ToString();
            s.Print(ConsoleColor.DarkMagenta);
        }
        
        "Selective Loading Hooks".Print(ConsoleColor.Magenta);
        
        foreach (var i in Ldm.selectiveLoadingHooks)
        {
            string s = i.ToString();
            s.Print(ConsoleColor.DarkMagenta);
        }
    }

    [Command("chronia_seed", "Set ChroniaHelper Random Seed")]
    public static void _setSeed(string s)
    {
        if(int.TryParse(s, out int n))
        {
            Rd.RandomSeed = n;
            Rd.RefreshRandom();
            return;
        }

        if(float.TryParse(s, out float f))
        {
            Rd.RandomSeed = (int)f;
            Rd.RefreshRandom();
            return;
        }

        if (long.TryParse(s, out long l))
        {
            Rd.RandomSeed = (int)l;
            Rd.RefreshRandom();
            return;
        }

        if (double.TryParse(s, out double d))
        {
            Rd.RandomSeed = (int)d;
            Rd.RefreshRandom();
            return;
        }

        if (decimal.TryParse(s, out decimal D))
        {
            Rd.RandomSeed = (int)D;
            Rd.RefreshRandom();
            return;
        }

        Rd.RandomSeed = s.GetHashCode();
        Rd.RefreshRandom();
    }
    
    [Command("chronia_setflag",
        "string: name, bool: state, bool: global, bool: perDeath, bool: perRoom")]
    public static void _setFlag(string s, bool state = false, bool global = false, bool perDeath = false, bool perRoom = false)
    {
        s.SetFlag(state, global, perDeath, perRoom);
    }
}

public static class CommandLog
{
    public static void LogCommand(this object obj, Color? color = null)
    {
        Color c = color ?? Color.White;
        Engine.Commands.Log(obj, c);
    }

    public static void LogDivider(string title = "", Color? color = null)
    {
        Color c = color ?? Color.White;
        Engine.Commands.Log(title.IsNullOrEmpty() ? Log.divider : Log.title(title), c);
    }
}

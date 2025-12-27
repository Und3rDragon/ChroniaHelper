using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.XaphanHelper;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using TextCopy;
using static ChroniaHelper.Entities.PasswordKeyboard.PasswordKeyboard;

namespace ChroniaHelper.Settings;

public class Commands
{
    [Command("chronia_fullcheat", "Enable Variant Mode and Cheat Mode for a save")]
    public static void EnableFullCheat()
    {
        MaP.currentSaveData?.Item2.CheatMode = true;
        MaP.currentSaveData?.Item2.VariantMode = true;
    }

    [Command("chronia_try_math_expression", "Try parsing a string using ChroniaHelper MathExpression")]
    public static void TryMathExpression(string expression)
    {
        CommandLog.LogDivider($"Math Expression Result to: {expression}");
        expression.ParseMathExpression().LogCommand();
    }

    [Command("chronia_get_keyboard_password_hash", "Try getting the encrypted password for PasswordKeyboard")]
    public static void GenerateHashedPassword(string keyboardTag, string password, bool caseSensitive = true)
    {
        CommandLog.LogDivider($"Password {password} encrypted for Keyboard {keyboardTag} {(caseSensitive?"with" : "without")} sensitive case");
        string hash = StringUtils.GetHashString(keyboardTag + password, caseSensitive);
        hash.LogCommand();
        ClipboardService.SetText(hash);
        "Password copied onto your clipboard".LogCommand(Color.Yellow);
    }

    [Command("chronia_get_password_hash", "Try getting the encrypted keyboard-tag-combined password for PasswordKeyboard")]
    public static void GenerateHashedPassword(string parsedPassword, bool caseSensitive = true)
    {
        CommandLog.LogDivider($"Password {parsedPassword} encrypted {(caseSensitive ? "with" : "without")} sensitive case");
        string hash = StringUtils.GetHashString(parsedPassword, caseSensitive);
        hash.LogCommand();
        ClipboardService.SetText(hash);
        "Password copied onto your clipboard".LogCommand(Color.Yellow);
    }

    [Command("chronia_flag", "Set Flag")]
    public static void CommandFlag(string name, bool state = true, bool global = false, bool temporary = false)
    {
        if (Engine.Scene is not Level) { return; }
        if (!Md.InstanceReady) { return; }

        name.SetFlag(state, global, temporary);
    }

    [Command("chronia_flag_per_room", "Set up a flag that works only in one room")]
    public static void CommandRoomFlag(string name, bool state = true)
    {
        if (Engine.Scene is not Level) { return; }
        if (!Md.InstanceReady) { return; }

        name.SetFlag(state);
        if (state)
        {
            Md.Session.flagsPerRoom.Add(name);
        }
        else
        {
            Md.Session.flagsPerRoom.SafeRemove(name);
        }
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
            foreach (var i in XaphanModule.ModSaveData.GlobalFlags)
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

    [Command("chronia_get_charcode", "Get the charcode index for a certain character")]
    public static void CommandGetCharcode(string str)
    {
        string result = "";
        char[] c = str.ToArray();
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
        foreach(var i in Md.Session.sessionKeys)
        {
            CommandLog.LogCommand($"{i.Key} => {i.Value}");
        }

        CommandLog.LogDivider("All Global Keys", Color.Yellow);
        CommandLog.LogCommand("ID => Key Value", Color.Cyan);
        foreach (var i in Md.SaveData.globalKeys)
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

    [Command("chronia_enable_hud", "Set HUD")]
    public static void CommandSetHUD(bool state = true)
    {
        Md.Settings.HUDMainControl = state;
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

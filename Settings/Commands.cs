using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
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
        Engine.Commands.Log(Log.title(title), c);
    }
}

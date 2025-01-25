using System;
using System.Collections.Generic;
using ChroniaHelper;

namespace YoctoHelper.Cores;

public static class Log
{

    private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>()
    {
        {LogLevel.Info, ConsoleColor.Cyan },
        {LogLevel.Warn, ConsoleColor.Yellow },
        {LogLevel.Error, ConsoleColor.Red }
    };

    private static void Print(LogLevel level, string text)
    {
        Console.ForegroundColor = (Log.Colors.TryGetValue(level, out ConsoleColor color)) ? color : ConsoleColor.White;
        Logger.Log(level, ChroniaHelperModule.Name, text);
        Console.ResetColor();
    }

    public static void Info(object obj)
    {
        Log.Print(LogLevel.Info, (ObjectUtils.IsNull(obj)) ? "null" : obj.ToString());
    }

    public static void Warn(object obj)
    {
        Log.Print(LogLevel.Warn, (ObjectUtils.IsNull(obj)) ? "null" : obj.ToString());
    }

    public static void Error(object obj)
    {
        Log.Print(LogLevel.Error, (ObjectUtils.IsNull(obj)) ? "null" : obj.ToString());
    }

}

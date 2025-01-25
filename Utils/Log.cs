using System;
using System.Collections.Generic;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Utils;

public static class Log
{

    private const string Tag = "ChroniaHelper";

    private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>()
    {
        {LogLevel.Info, ConsoleColor.Cyan },
        {LogLevel.Warn, ConsoleColor.Yellow },
        {LogLevel.Error, ConsoleColor.Red }
    };

    private static void Output(LogLevel level, string text)
    {
        Console.ForegroundColor = Colors.TryGetValue(level, out ConsoleColor color) ? color : ConsoleColor.White;
        Logger.Log(level, Tag, text);
        Console.ResetColor();
    }

    public static void Info(object obj)
    {
        Output(LogLevel.Info, obj == null ? "null" : obj.ToString());
    }

    public static void Info(params object[] obj)
    {
        Output(LogLevel.Info, obj.ArrayToString());
    }

    public static void Warn(object obj)
    {
        Output(LogLevel.Warn, obj == null ? "null" : obj.ToString());
    }

    public static void Warn(params object[] obj)
    {
        Output(LogLevel.Warn, obj.ArrayToString());
    }

    public static void Error(object obj)
    {
        Output(LogLevel.Error, obj == null ? "null" : obj.ToString());
    }

    public static void Error(params object[] obj)
    {
        Output(LogLevel.Error, obj.ArrayToString());
    }

}

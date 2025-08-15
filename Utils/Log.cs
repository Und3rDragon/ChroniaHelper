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

    public static void Print(this object obj)
    {
        string text = obj == null ? "null" : obj.ToString();
        int color = 11;

        Console.ForegroundColor = (ConsoleColor)color;
        Logger.Log(LogLevel.Info, Tag, text);
        Console.ResetColor();
    }

    public static void Print(this object obj, int colorIndex, LogLevel level = LogLevel.Info)
    {
        string text = obj == null ? "null" : obj.ToString();
        int color = int.Clamp(colorIndex, 0, 15);

        Console.ForegroundColor = (ConsoleColor)color;
        Logger.Log(level, Tag, text);
        Console.ResetColor();
    }

    public static void Print(this object[] obj)
    {
        string text = obj.ArrayToString();
        int color = 11;

        Console.ForegroundColor = (ConsoleColor)color;
        Logger.Log(LogLevel.Info, Tag, text);
        Console.ResetColor();
    }

    public static void Print(this object[] obj, int colorIndex, LogLevel level = LogLevel.Info)
    {
        string text = obj.ArrayToString();
        int color = int.Clamp(colorIndex, 0, 15);

        Console.ForegroundColor = (ConsoleColor)color;
        Logger.Log(level, Tag, text);
        Console.ResetColor();
    }

    public static void Print(this object obj, ConsoleColor color, LogLevel level = LogLevel.Info)
    {
        string text = obj == null ? "null" : obj.ToString();

        Console.ForegroundColor = color;
        Logger.Log(level, Tag, text);
        Console.ResetColor();
    }

    public static void Print(this object[] obj, ConsoleColor color, LogLevel level = LogLevel.Info)
    {
        string text = obj.ArrayToString();

        Console.ForegroundColor = color;
        Logger.Log(level, Tag, text);
        Console.ResetColor();
    }

    public static void Print(ConsoleColor color, LogLevel level, params object[] obj)
    {
        string text = obj.ArrayToString();

        Console.ForegroundColor = color;
        Logger.Log(level, Tag, text);
        Console.ResetColor();
    }

    public enum LogMode { Info, Warn, Error }
    public static void Each<T>(ICollection<T> objs, LogMode mode = LogMode.Info)
    {
        foreach (var obj in objs)
        {
            if(mode == LogMode.Info)
            {
                Info(obj);
            }
            else if(mode == LogMode.Warn)
            {
                Warn(obj);
            }
            else if(mode == LogMode.Error)
            {
                Error(obj);
            }
        }
    }
}

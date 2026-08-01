using Celeste.Mod.Backdrops;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

public static class FormulaConverterUtils
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Update += LevelUpdate;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Update -= LevelUpdate;
    }

    public static void LevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        foreach(var item in Md.Session.FormulaConverterTimers)
        {
            item.SetSlider(item.GetSlider() + Engine.DeltaTime);
        }
    }
}

[CustomEntity("ChroniaHelper/FormulaConverter")]
[Tracked]
public class FormulaConverter : GeneralSetupController
{
    public FormulaConverter(EntityData data, Vc2 offset) : 
        base(data, offset)
    {
        targetName = data.Attr("targetName", "formulaCounter");
        timerSlider = data.Attr("timerSlider", "timer");
        isCounter = data.Bool("isCounter", true);
        formulaTimeFields = data.StringArray("formulaTimeFields");
        formulaExpressions = data.StringArray("formulaExpressions", '|');
        resetTimerWhenActivated = data.Bool("resetTimerWhenActivated", false);
    }
    private string targetName;
    private string timerSlider;
    private bool isCounter;
    private string[] formulaTimeFields;
    private string[] formulaExpressions;
    private bool resetTimerWhenActivated;

    public override void Execute() { }

    public override void ExecuteByUpdateState(bool current, bool last)
    {
        if(current != last && current)
        {
            // activated, start defining the current time index
            // and resetting timers

            Md.Session.FormulaConverterTimers.Add(timerSlider);

            formulaIndex = 0;

            if (resetTimerWhenActivated)
            {
                timerSlider.SetSlider(0f);
            }

            Indexing();
        }
    }

    public void Indexing()
    {
        float t = timerSlider.GetSlider();

        while (formulaIndex < formulaTimeFields.Length)
        {
            if (!formulaTimeFields[formulaIndex].HasValidContent())
            {
                break;
            }

            if (float.TryParse(formulaTimeFields[formulaIndex], out float f))
            {
                if (t >= f)
                {
                    formulaIndex++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                formulaIndex++;
            }
        }
    }

    private int formulaIndex = 0;
    public override void Update()
    {
        base.Update();

        Indexing();

        if(formulaIndex < formulaExpressions.Length)
        {
            double value = formulaExpressions[formulaIndex].ParseMathExpressionRaw(GetVariable);

            if (isCounter)
            {
                targetName.SetCounter((int)value);
            }
            else
            {
                targetName.SetSlider((float)value);
            }
        }
    }

    public double GetVariable(string variable)
    {
        if (variable == "e") { return Math.E; }
        if (new string[] { "pi", "PI", "Pi" }.Contains(variable)) { return Math.PI; }
        if (variable.ToLower() == "timer")
        {
            return timerSlider.GetSlider();
        }

        return MaP.level?.Session.GetSlider(variable) ?? 0f;
    }
}

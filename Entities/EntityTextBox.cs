using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using AsmResolver.IO;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/EntityTextbox")]
public class EntityTextBox : BaseEntity
{
    public EntityTextBox(EntityData d, Vc2 o) : base(d, o)
    {
        Tag = Tags.PauseUpdate | Tags.HUD;

        lineHeight = Dialog.Language.FontSize.LineHeight - 1;
        //Log.Info(lineHeight);
        float maxHeight = d.Float("maxHeight", 272f).GetAbs();
        float maxWidth = d.Float("maxWidth", 1688f).GetAbs();

        linesPerPage = (int)(maxHeight / lineHeight);

        string dialogID = d.Attr("dialog", "dialogID");
        string dialog = dialogID.StartsWith('#') ?
            Md.Session.keystrings[dialogID.TrimStart('#')] :
            Dialog.Get(dialogID, Dialog.Language);
        dialog = ProcessSessionData(dialog);
        text = FancyText.Parse(dialog, 
            (int)maxWidth, linesPerPage, 1f, null, Dialog.Language);

        index = 0;
        Start = 0;

        Add(listener = new FlagListener(d.Attr("operationFlag", "triggerDialog")));

        justification = new Vc2(d.Float("justifyX", 0.5f), d.Float("justifyY", 0.5f));
        scale = d.Float("scale", 1f);
    }
    private float lineHeight;
    private int linesPerPage;
    private FancyText.Text text;
    private int index = 0, Start = 0;
    private Coroutine runRoutine;
    private Vc2 justification = Vc2.One * 0.5f;
    private FlagListener listener;
    private float scale;

    public override void Update()
    {
        base.Update();

        listener.onEnable = () =>
        {
            runRoutine = new Coroutine(RunRoutine());
            runRoutine.UseRawDeltaTime = true;
            Add(runRoutine);
        };

        listener.onDisable = () =>
        {
            Remove(runRoutine);
            index = 0;
            Start = 0;
        };
    }

    // Status
    private List<FancyText.Node> Nodes => text.Nodes;
    private int Page;
    private char lastChar;

    public IEnumerator RunRoutine()
    {
        FancyText.Node last = null;
        float delayBuildup = 0f;
        while (index < Nodes.Count)
        {
            FancyText.Node current = Nodes[index];
            float delay = 0f;
            if (current is FancyText.NewPage)
            {
                Start = index + 1;
                Page++;
            }
            else if (current is FancyText.Wait)
            {
                delay = (current as FancyText.Wait).Duration;
            }
            else if (current is FancyText.Trigger)
            {
                FancyText.Trigger trigger = current as FancyText.Trigger;

                int num = trigger.Index;

                // trigger index?
            }
            else if (current is FancyText.Char)
            {
                FancyText.Char ch = current as FancyText.Char;
                lastChar = (char)ch.Character;

                bool flag = false;
                if (index - 5 > Start)
                {
                    for (int i = index; i < Math.Min(index + 4, Nodes.Count); i++)
                    {
                        if (Nodes[i] is FancyText.NewPage)
                        {
                            flag = true;
                        }
                    }
                }

                if (last != null && last is FancyText.NewPage)
                {
                    index--;
                    yield return 0.2f;
                    index++;
                }

                delay = ch.Delay + delayBuildup;
            }

            last = current;
            index++;
            if (delay < 0.016f)
            {
                delayBuildup += delay;
                continue;
            }

            delayBuildup = 0f;

            yield return delay;
        }

        Start = Nodes.Count;
    }

    public override void Render()
    {
        base.Render();
        
        int remainLines = 1;
        for (int i = Start; i < text.Nodes.Count; i++)
        {
            if (text.Nodes[i] is FancyText.NewLine)
            {
                remainLines++;
            }
            else if (text.Nodes[i] is FancyText.NewPage)
            {
                break;
            }
        }

        // Assist calculations from original
        float textEase = 1f;
        //Vector2 textRenderPos = new Vector2(textPaddingFromEdge, textPaddingFromEdge);
        //Vector2 actualTextSize = new Vector2(actualTextWidth, (float)linesPerPage * lineHeight * textEase);
        float assistiveScaling = ((remainLines >= 4) ? 0.75f : 1f);
        //float assistiveScaling = 1f;
        // The justify is for the text aligning
        text.DrawJustifyPerLine(
            (Position - MaP.cameraPos) * Cons.HDScale,
            justification, new Vector2(1f, textEase) * assistiveScaling * scale,
            textEase, Start, index);
    }

    public string ProcessSessionData(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 处理 {counter xxx n} 格式
        string result = Regex.Replace(input, @"\{counter\s+(\w+)(?:\s+(\d+))?\}", match =>
        {
            string name = match.Groups[1].Value;
            int minDigits = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;

            int value = name.GetCounter();
            return value.ToString(minDigits == 0 ? "" : $"D{minDigits}");
        });

        // 处理 {slider xxx m n} 格式
        result = Regex.Replace(result, @"\{slider\s+(\w+)(?:\s+(\d+))?(?:\s+(\d+))?\}", match =>
        {
            string name = match.Groups[1].Value;

            // 解析参数
            int intDigits = 0;  // m - 整数位最小位数
            int decimalPlaces = 2;  // n - 小数位最大位数

            if (match.Groups[2].Success)
            {
                intDigits = int.Parse(match.Groups[2].Value);

                if (match.Groups[3].Success)
                {
                    decimalPlaces = int.Parse(match.Groups[3].Value);
                }
                // 如果只有m，n默认为2
            }
            // 如果没有参数，保持默认值 (intDigits = 0, decimalPlaces = 2)

            float value = name.GetSlider();

            // 分离整数和小数部分
            int intPart = (int)Math.Floor(Math.Abs(value));
            float fractionalPart = Math.Abs(value) - intPart;

            // 处理整数部分（包括负号）
            string intStr = intPart.ToString();
            if (intDigits > intStr.Length)
            {
                intStr = intStr.PadLeft(intDigits, '0');
            }

            // 处理小数部分
            string fractionalStr;
            if (decimalPlaces == 0)
            {
                fractionalStr = "";
            }
            else
            {
                // 四舍五入到指定小数位
                fractionalPart = (float)Math.Round(fractionalPart, decimalPlaces);
                fractionalStr = fractionalPart.ToString($"F{decimalPlaces}").Split('.')[1];
            }

            // 组合最终结果
            string resultStr = value < 0 ? "-" + intStr : intStr;
            if (decimalPlaces > 0)
            {
                resultStr += "." + fractionalStr;
            }

            return resultStr;
        });

        return result;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Entities;

[WorkingInProgress]
public class EntityTextBox : BaseEntity
{
    public EntityTextBox(EntityData d, Vc2 o) : base(d, o)
    {
        Tag = Tags.PauseUpdate | Tags.HUD;

        font = Dialog.Language.Font;
        lineHeight = Dialog.Language.FontSize.LineHeight - 1;
        linesPerPage = (int)(240f / lineHeight);
        innerTextPadding = (272f - lineHeight * (float)linesPerPage) / 2f;
        maxLineWidthNoPortrait = 1688f - innerTextPadding * 2f;
        text = FancyText.Parse(Dialog.Get(dialog, Dialog.Language), 
            (int)maxLineWidthNoPortrait, linesPerPage, 0f, null, Dialog.Language);
        index = 0;
        Start = 0;
        runRoutine = new Coroutine(RunRoutine());
        runRoutine.UseRawDeltaTime = true;

        Add(runRoutine);
    }
    private PixelFont font;
    private float lineHeight;
    private int linesPerPage;
    private float innerTextPadding;
    private float maxLineWidthNoPortrait;
    private FancyText.Text text;
    private int index = 0, Start = 0;
    private Coroutine runRoutine;
    private string dialog;

    // Status
    private List<FancyText.Node> Nodes => text.Nodes;
    private int Page;
    private char lastChar;

    private Textbox original;

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

        //Start = Nodes.Count;
    }

    public override void Render()
    {
        base.Render();

        int num5 = 1;
        for (int i = Start; i < text.Nodes.Count; i++)
        {
            if (text.Nodes[i] is FancyText.NewLine)
            {
                num5++;
            }
            else if (text.Nodes[i] is FancyText.NewPage)
            {
                break;
            }
        }

        // Assist calculations from original
        float num = 1;
        Vc2 vector = Vc2.Zero;

        Vector2 vector2 = new Vector2(innerTextPadding, innerTextPadding);
        Vector2 vector3 = new Vector2(maxLineWidthNoPortrait, (float)linesPerPage * lineHeight * num) / 2f;
        float num6 = ((num5 >= 4) ? 0.75f : 1f);
        text.Draw(vector + vector2 + vector3, new Vector2(0.5f, 0.5f), new Vector2(1f, num) * num6, num, Start);
    }
}

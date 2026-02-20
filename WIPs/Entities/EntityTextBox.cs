using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.WIPs.Entities;

[WorkingInProgress("Display normal, but the Positioning is really weird")]
[CustomEntity("ChroniaHelper/EntityTextbox")]
public class EntityTextBox : BaseEntity
{
    public EntityTextBox(EntityData d, Vc2 o) : base(d, o)
    {
        Tag = Tags.PauseUpdate | Tags.HUD;

        font = Dialog.Language.Font;
        lineHeight = Dialog.Language.FontSize.LineHeight - 1;
        float actualTextHeight = 240f;
        float maxHeight = 272f;
        float maxWidth = 1688f;
        linesPerPage = (int)(actualTextHeight / lineHeight);
        textPaddingFromEdge = (maxHeight - actualTextHeight) / 2f;
        actualTextWidth = maxWidth - textPaddingFromEdge * 2f;
        text = FancyText.Parse(Dialog.Get(dialog, Dialog.Language), 
            (int)actualTextWidth, linesPerPage, 1f, null, Dialog.Language);
        index = 0;
        Start = 0;
        runRoutine = new Coroutine(RunRoutine());
        runRoutine.UseRawDeltaTime = true;

        //Add(runRoutine);
        Add(new FlagListener("dia"));
    }
    private PixelFont font;
    private float lineHeight;
    private int linesPerPage;
    private float textPaddingFromEdge;
    private float actualTextWidth;
    private FancyText.Text text;
    private int index = 0, Start = 0;
    private Coroutine runRoutine;
    private string dialog = "testE";

    public override void Update()
    {
        base.Update();

        foreach(Component comp in this.Components)
        {
            if(comp is FlagListener listener)
            {
                listener.onEnable = () =>
                {
                    Add(runRoutine);
                    Log.Info(Position);
                    Vector2 textPadding = new Vector2(textPaddingFromEdge, textPaddingFromEdge);
                    Vector2 linePadding = new Vector2(actualTextWidth, (float)linesPerPage * lineHeight) / 2f;
                    Log.Info(textPadding, linePadding);
                };
            }
        }
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
        Vc2 startPos = Position;
        Vector2 textRenderPos = new Vector2(textPaddingFromEdge, textPaddingFromEdge);
        Vector2 actualTextCenter = new Vector2(actualTextWidth, (float)linesPerPage * lineHeight * textEase) / 2f;
        float assistiveScaling = ((remainLines >= 4) ? 0.75f : 1f);
        // The justify is for the text aligning
        text.Draw(
            startPos + textRenderPos + actualTextCenter,
            new Vc2(0.5f, 0.5f), new Vector2(1f, textEase) * assistiveScaling,
            textEase, Start, index);
        //text.DrawJustifyPerLine(
        //    startPos + textRenderPos + actualTextCenter,
        //    new Vc2(0.5f, 0.5f), new Vector2(1f, textEase) * assistiveScaling,
        //    textEase, Start, index);
    }
}

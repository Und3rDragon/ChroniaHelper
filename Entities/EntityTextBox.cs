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
        text = FancyText.Parse(dialog, 
            (int)maxWidth, linesPerPage, 1f, null, Dialog.Language);

        index = 0;
        Start = 0;

        Add(listener = new FlagListener(d.Attr("operationFlag", "triggerDialog")));

        justification = new Vc2(d.Float("justifyX", 0.5f), d.Float("justifyY", 0.5f));
    }
    private float lineHeight;
    private int linesPerPage;
    private FancyText.Text text;
    private int index = 0, Start = 0;
    private Coroutine runRoutine;
    private Vc2 justification = Vc2.One * 0.5f;
    private FlagListener listener;

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
            (Position - MaP.cameraPos) * HDRenderEntity.HDScale,
            justification, new Vector2(1f, textEase) * assistiveScaling,
            textEase, Start, index);
    }
}

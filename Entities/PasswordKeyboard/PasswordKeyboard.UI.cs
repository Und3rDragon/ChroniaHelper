using System.Linq;
using Celeste.Mod.Core;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Input;

namespace ChroniaHelper.Entities.PasswordKeyboard;

partial class PasswordKeyboard
{
    public sealed class UI : Entity
    {
        #region stolen from original game
        private static Color unselectColor = Color.LightGray;
        private static Color selectColorA = Calc.HexToColor("84FF54");
        private static Color selectColorB = Calc.HexToColor("FCFF59");
        private static Color disableColor = Color.DarkSlateBlue;
        #endregion

        private readonly Config config;
        private readonly Action onExit;
        private readonly Func<string, bool> onTry;

        #region stolen from original game
        private readonly float optionsScale = 0.75f;

        private readonly string cancelText;
        private readonly string spaceText;
        private readonly string backspaceText;
        private readonly string acceptText;

        private readonly float cancelWidth;
        private readonly float spaceWidth;
        private readonly float backspaceWidth;
        private readonly float acceptWidth;
        private readonly float optionsTotalWidth;

        private readonly string[] letters;
        private readonly float widestLetterWidth;
        private readonly float lineHeight;
        private readonly float lineSpacing;
        private readonly float boxPadding;
        private readonly float boxWidth;
        private readonly float boxHeight;
        private Vector2 BoxTopLeft =>
            Position + new Vector2((1920f - boxWidth) / 2f, 360f + (680f - boxHeight) / 2f);

        private int line;
        private int index;
        private bool selectingOptions;
        private int optionsIndex;
        #endregion

        private readonly Wiggler wiggler;

        private string inputText = string.Empty;

        private int MaxInputLength = 12;

        public UI(Config config, Action onCancel, Func<string, bool> onTry)
        {
            this.AddTag(Tags.HUD);
            this.config = config;
            this.onExit = onCancel;
            this.onTry = onTry;

            #region stolen from original game
            float widestLineWidth;
            {
                string chars = Dialog.Clean("name_letters");
                letters = chars.Split('\n');
                widestLetterWidth = 0f;
                for (int i = 0; i < chars.Length; i++)
                {
                    float x = ActiveFont.Measure(chars[i]).X;
                    if (x > widestLetterWidth)
                        widestLetterWidth = x;
                }

                int widestLineCount = 0;
                foreach (string text in letters)
                {
                    if (text.Length > widestLineCount)
                    {
                        widestLineCount = text.Length;
                    }
                }
                widestLineWidth = widestLineCount * widestLetterWidth;
                lineHeight = ActiveFont.LineHeight;
                lineSpacing = ActiveFont.LineHeight * 0.1f;
            }

            {
                cancelText = Dialog.Clean("name_back", null);
                spaceText = Dialog.Clean("name_space", null);
                backspaceText = Dialog.Clean("name_backspace", null);
                acceptText = Dialog.Clean("name_accept", null);

                cancelWidth = ActiveFont.Measure(cancelText).X * optionsScale;
                spaceWidth = ActiveFont.Measure(spaceText).X * optionsScale;
                backspaceWidth = ActiveFont.Measure(backspaceText).X * optionsScale;
                acceptWidth = ActiveFont.Measure(acceptText).X * optionsScale * 1.25f;
                optionsTotalWidth = cancelWidth + spaceWidth + backspaceWidth + acceptWidth + widestLetterWidth * 3f;
            }

            {
                boxPadding = widestLetterWidth;
                boxWidth = Math.Max(widestLineWidth, optionsTotalWidth) + boxPadding * 2f;
                boxHeight = (letters.Length + 1) * lineHeight + letters.Length * lineSpacing + boxPadding * 3f;
            }
            #endregion

            wiggler = Wiggler.Create(0.25f, 4f);
            Add(wiggler);

            // character limit override
            MaxInputLength = config.LengthLimit;
        }

        private void Backspace()
        {
            if (inputText.Length >= 1)
                inputText = inputText[..^1];
        }

        private void Space()
        {
            if (!inputText.StartsWith(' '))
                inputText += " ";
        }

        private void Finish()
        {
            // clicking "enter"
            if (onTry.Invoke(inputText.Trim()))
            {
                // on exclusive mode and flag mode the screen will exit
                // but on normal mode, it won't
                Exit();
            }
        }

        private void Exit()
        {
            onExit?.Invoke();
        }

        private void DrawExtraUI()
        {
            DrawOptionText(inputText, new Vector2(960f, 148f), new Vector2(0.5f, 0.0f), Vector2.One * 3f, false);

            if (ChroniaHelperModule.Session.RemainingUses[config.entityID] > 0)
            {
                DrawOptionText($"You have {ChroniaHelperModule.Session.RemainingUses[config.entityID]} chances left",
                    new Vector2(368f, 110f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One,
                    false
                    );
            }
            else if(ChroniaHelperModule.Session.RemainingUses[config.entityID] == 0)
            {
                DrawOptionText("Keyboard Lockdown!",
                    new Vector2(368f, 110f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One,
                    false
                    );
            }

            // TODO
        }

        public void Clear()
        {
            inputText = string.Empty;
        }

        #region stolen from original game
        public override void Update()
        {
            base.Update();
            if (Input.MenuRight.Pressed && (optionsIndex < 3 || !selectingOptions) && (inputText.Length > 0 || !selectingOptions))
            {
                if (selectingOptions)
                {
                    optionsIndex = Math.Min(optionsIndex + 1, 3);
                }
                else
                {
                    do index = (index + 1) % letters[line].Length;
                    while (letters[line][index] == ' ');
                }
                wiggler.Start();
                Audio.Play(SFX.ui_main_rename_entry_roll);
            }
            else if (Input.MenuLeft.Pressed && (optionsIndex > 0 || !selectingOptions))
            {
                if (selectingOptions)
                {
                    optionsIndex = Math.Max(optionsIndex - 1, 0);
                }
                else
                {
                    do index = (index + letters[line].Length - 1) % letters[line].Length;
                    while (letters[line][index] == ' ');
                }
                wiggler.Start();
                Audio.Play(SFX.ui_main_rename_entry_roll);
            }
            else
            {
                if (Input.MenuDown.Pressed && !selectingOptions)
                {
                    int nextLine = line + 1;
                    while (true)
                    {
                        if (nextLine >= letters.Length)
                        {
                            selectingOptions = true;
                            break;
                        }
                        if (index < letters[nextLine].Length && letters[nextLine][index] != ' ')
                        {
                            line = nextLine;
                            break;
                        }
                        nextLine++;
                    }
                    if (selectingOptions)
                    {
                        float cursorOffset = index * widestLetterWidth;
                        float contentWidth = boxWidth - boxPadding * 2f;
                        optionsIndex = inputText.Length != 0 && cursorOffset >= cancelWidth + (contentWidth - cancelWidth - acceptWidth - backspaceWidth - spaceWidth - widestLetterWidth * 3f) / 2f
                            ? cursorOffset >= contentWidth - acceptWidth - backspaceWidth - widestLetterWidth * 2f
                            ? cursorOffset < contentWidth - acceptWidth - widestLetterWidth
                            ? 2 : 3 : 1 : 0;
                    }
                    wiggler.Start();
                    Audio.Play(SFX.ui_main_rename_entry_roll);
                }
                if ((Input.MenuUp.Pressed || (selectingOptions && inputText.Length <= 0 && optionsIndex > 0)) && (line > 0 || selectingOptions))
                {
                    if (selectingOptions)
                    {
                        line = letters.Length;
                        selectingOptions = false;
                        float contentWidth = boxWidth - boxPadding * 2f;
                        index = optionsIndex switch
                        {
                            0 => (int)(cancelWidth / 2f / widestLetterWidth),
                            1 => (int)((contentWidth - acceptWidth - backspaceWidth - spaceWidth / 2f - widestLetterWidth * 2f) / widestLetterWidth),
                            2 => (int)((contentWidth - acceptWidth - backspaceWidth / 2f - widestLetterWidth) / widestLetterWidth),
                            3 => (int)((contentWidth - acceptWidth / 2f) / widestLetterWidth),
                            _ => index,
                        };
                    }

                    line--;
                    while (line > 0 && !(index < letters[line].Length && letters[line][index] != ' '))
                        line--;
                    while (index >= letters[line].Length || letters[line][index] == ' ')
                        index--;

                    wiggler.Start();
                    Audio.Play(SFX.ui_main_rename_entry_roll);
                }
                else if (Input.MenuConfirm.Pressed)
                {
                    if (selectingOptions)
                    {
                        switch (optionsIndex)
                        {
                        case 0: Exit(); break;
                        case 1: Space(); break;
                        case 2: Backspace(); break;
                        case 3: Finish(); break;
                        }
                    }
                    else if (inputText.Length < MaxInputLength)
                    {
                        inputText += letters[line][index].ToString();
                        wiggler.Start();
                        Audio.Play(SFX.ui_main_rename_entry_char);
                    }
                    else
                    {
                        Audio.Play(SFX.ui_main_button_invalid);
                    }
                }
                else if (Input.MenuCancel.Pressed)
                {
                    if (inputText.Length > 0)
                        Backspace();
                    else
                        Exit();
                }
                else if (Input.Pause.Pressed || Input.ESC.Pressed)
                {
                    Exit();
                }
            }
        }

        public override void Render()
        {
            base.Render();
            Vector2 boxTopLeft = BoxTopLeft;
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.8f * 1.0f/*ease*/);
            Vector2 curPosition = boxTopLeft + new Vector2(boxPadding, boxPadding);
            int letterIndex = 0;
            foreach (string letter in letters)
            {
                for (int j = 0; j < letter.Length; j++)
                {
                    bool isSelected = letterIndex == line && j == index && !selectingOptions;
                    Vector2 scale = Vector2.One * (isSelected ? 1.2f : 1f);
                    Vector2 drawPosition = curPosition + new Vector2(widestLetterWidth, lineHeight) / 2f;
                    if (isSelected)
                        drawPosition += new Vector2(0f, wiggler.Value) * 8f;
                    DrawOptionText(letter[j].ToString(), drawPosition, new Vector2(0.5f, 0.5f), scale, isSelected);
                    curPosition.X += widestLetterWidth;
                }
                curPosition.X = boxTopLeft.X + boxPadding;
                curPosition.Y += lineHeight + lineSpacing;
                letterIndex++;
            }
            float wigglerValue = wiggler.Value * 8f;
            curPosition.Y = boxTopLeft.Y + boxHeight - lineHeight - boxPadding;
            Draw.Rect(curPosition.X, curPosition.Y - boxPadding * 0.5f, boxWidth - boxPadding * 2f, 4f, Color.White);

            bool selectedThis = selectingOptions && optionsIndex == 0;
            float optionWigglerValue = selectedThis ? wigglerValue : 0f;
            DrawOptionText(
                cancelText,
                curPosition + new Vector2(0f, lineHeight + optionWigglerValue),
                new Vector2(0f, 1f),
                Vector2.One * optionsScale,
                selectedThis,
                false
                );

            curPosition.X = boxTopLeft.X + boxWidth
                - backspaceWidth - widestLetterWidth - spaceWidth
                - widestLetterWidth - acceptWidth - boxPadding;
            selectedThis = (selectingOptions && optionsIndex == 1);
            optionWigglerValue = selectedThis ? wigglerValue : 0f;
            DrawOptionText(
                spaceText,
                curPosition + new Vector2(0f, lineHeight + optionWigglerValue),
                new Vector2(0f, 1f),
                Vector2.One * optionsScale,
                selectedThis,
                inputText.Length == 0
                );

            curPosition.X += spaceWidth + widestLetterWidth;
            selectedThis = (selectingOptions && optionsIndex == 2);
            optionWigglerValue = selectedThis ? wigglerValue : 0f;
            DrawOptionText(
                backspaceText,
                curPosition + new Vector2(0f, lineHeight + optionWigglerValue),
                new Vector2(0f, 1f),
                Vector2.One * optionsScale,
                selectedThis,
                inputText.Length <= 0
                );

            curPosition.X += backspaceWidth + widestLetterWidth;
            selectedThis = (selectingOptions && optionsIndex == 3);
            optionWigglerValue = selectedThis ? wigglerValue : 0f;
            DrawOptionText(
                acceptText,
                curPosition + new Vector2(0f, lineHeight + optionWigglerValue),
                new Vector2(0f, 1f),
                Vector2.One * optionsScale * 1.25f,
                selectedThis,
                inputText.Length < 1
                );

            // this is not the stolen part (apparently)
            DrawExtraUI();
        }

        private void DrawOptionText(
            string text,
            Vector2 at, Vector2 justify,
            Vector2 scale,
            bool selected, bool disabled = false
            )
        {
            Color textColor = (disabled ? disableColor : GetTextColor(selected));
            Color edgeColor = (disabled ? Color.Lerp(disableColor, Color.Black, 0.7f) : Color.Gray);
            ActiveFont.DrawEdgeOutline(text, at, justify, scale, textColor, 4f, edgeColor);

            Color GetTextColor(bool selected)
            {
                if (selected)
                {
                    return !CoreModule.Settings.AllowTextHighlight
                        ? selectColorA
                        : !Calc.BetweenInterval(Scene.TimeActive, 0.1f) ? selectColorB : selectColorA;
                }
                return unselectColor;
            }
        }
        #endregion
    }
}
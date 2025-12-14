using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using Celeste.Mod.Helpers;
using static Celeste.FancyText;

namespace ChroniaHelper.Settings;

public class CNCharacterAutoLining
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.FancyText.Parse += Parsetext;
        On.Celeste.FancyText.AddWord += TextAddWord;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.FancyText.Parse -= Parsetext;
        On.Celeste.FancyText.AddWord -= TextAddWord;
    }
    
    public static FancyText.Text Parsetext(On.Celeste.FancyText.orig_Parse orig, FancyText self)
    {
        return orig(self);
        
        //if (self.language.Id == "schinese")
        //{
        //    string[] array = Regex.Split(self.text, self.language.SplitRegex);
        //    string[] array2 = new string[array.Length];
        //    int num = 0;
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        if (!string.IsNullOrEmpty(array[i]))
        //        {
        //            array2[num++] = array[i];
        //        }
        //    }

        //    Stack<Color> stack = new Stack<Color>();
        //    Portrait[] array3 = new Portrait[2];
        //    for (int j = 0; j < num; j++)
        //    {
        //        if (array2[j] == "{")
        //        {
        //            j++;
        //            string text = array2[j++];
        //            List<string> list = new List<string>();
        //            for (; j < array2.Length && array2[j] != "}"; j++)
        //            {
        //                if (!string.IsNullOrWhiteSpace(array2[j]))
        //                {
        //                    list.Add(array2[j]);
        //                }
        //            }

        //            float result = 0f;
        //            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        //            {
        //                self.group.Nodes.Add(new Wait
        //                {
        //                    Duration = result
        //                });
        //                continue;
        //            }

        //            if (text[0] == '#')
        //            {
        //                string text2 = "";
        //                if (text.Length > 1)
        //                {
        //                    text2 = text.Substring(1);
        //                }
        //                else if (list.Count > 0)
        //                {
        //                    text2 = list[0];
        //                }

        //                if (string.IsNullOrEmpty(text2))
        //                {
        //                    if (stack.Count > 0)
        //                    {
        //                        self.currentColor = stack.Pop();
        //                    }
        //                    else
        //                    {
        //                        self.currentColor = self.defaultColor;
        //                    }

        //                    continue;
        //                }

        //                stack.Push(self.currentColor);
        //                switch (text2)
        //                {
        //                    case "red":
        //                        self.currentColor = Color.Red;
        //                        break;
        //                    case "green":
        //                        self.currentColor = Color.Green;
        //                        break;
        //                    case "blue":
        //                        self.currentColor = Color.Blue;
        //                        break;
        //                    default:
        //                        self.currentColor = Calc.HexToColor(text2);
        //                        break;
        //                }

        //                continue;
        //            }

        //            switch (text)
        //            {
        //                case "break":
        //                    self.CalcLineWidth();
        //                    self.currentPage++;
        //                    self.group.Pages++;
        //                    self.currentLine = 0;
        //                    self.currentPosition = 0f;
        //                    self.group.Nodes.Add(new NewPage());
        //                    continue;
        //                case "n":
        //                    self.AddNewLine();
        //                    continue;
        //                case ">>":
        //                    {
        //                        if (list.Count > 0 && float.TryParse(list[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
        //                        {
        //                            self.currentDelay = 0.01f / result2;
        //                        }
        //                        else
        //                        {
        //                            self.currentDelay = 0.01f;
        //                        }

        //                        continue;
        //                    }
        //            }

        //            if (text.Equals("/>>"))
        //            {
        //                self.currentDelay = 0.01f;
        //                continue;
        //            }

        //            if (text.Equals("anchor"))
        //            {
        //                if (Enum.TryParse<Anchors>(list[0], ignoreCase: true, out var result3))
        //                {
        //                    self.group.Nodes.Add(new Anchor
        //                    {
        //                        Position = result3
        //                    });
        //                }

        //                continue;
        //            }

        //            if (text.Equals("portrait") || text.Equals("left") || text.Equals("right"))
        //            {
        //                Portrait item;
        //                if (text.Equals("portrait") && list.Count > 0 && list[0].Equals("none"))
        //                {
        //                    item = new Portrait();
        //                    self.group.Nodes.Add(item);
        //                    continue;
        //                }

        //                if (text.Equals("left"))
        //                {
        //                    item = array3[0];
        //                }
        //                else if (text.Equals("right"))
        //                {
        //                    item = array3[1];
        //                }
        //                else
        //                {
        //                    item = new Portrait();
        //                    foreach (string item2 in list)
        //                    {
        //                        if (item2.Equals("upsidedown"))
        //                        {
        //                            item.UpsideDown = true;
        //                        }
        //                        else if (item2.Equals("flip"))
        //                        {
        //                            item.Flipped = true;
        //                        }
        //                        else if (item2.Equals("left"))
        //                        {
        //                            item.Side = -1;
        //                        }
        //                        else if (item2.Equals("right"))
        //                        {
        //                            item.Side = 1;
        //                        }
        //                        else if (item2.Equals("pop"))
        //                        {
        //                            item.Pop = true;
        //                        }
        //                        else if (item.Sprite == null)
        //                        {
        //                            item.Sprite = item2;
        //                        }
        //                        else
        //                        {
        //                            item.Animation = item2;
        //                        }
        //                    }
        //                }

        //                if (GFX.PortraitsSpriteBank.Has(item.SpriteId))
        //                {
        //                    List<SpriteDataSource> sources = GFX.PortraitsSpriteBank.SpriteData[item.SpriteId].Sources;
        //                    for (int num2 = sources.Count - 1; num2 >= 0; num2--)
        //                    {
        //                        XmlElement xML = sources[num2].XML;
        //                        if (xML != null)
        //                        {
        //                            if (item.SfxEvent == null)
        //                            {
        //                                item.SfxEvent = "event:/char/dialogue/" + xML.Attr("sfx", "");
        //                            }

        //                            if (xML.HasAttr("glitchy"))
        //                            {
        //                                item.Glitchy = xML.AttrBool("glitchy", defaultValue: false);
        //                            }

        //                            if (xML.HasChild("sfxs") && item.SfxExpression == 1)
        //                            {
        //                                foreach (object item3 in xML["sfxs"])
        //                                {
        //                                    if (item3 is XmlElement xmlElement && xmlElement.Name.Equals(item.Animation, StringComparison.InvariantCultureIgnoreCase))
        //                                    {
        //                                        item.SfxExpression = xmlElement.AttrInt("index");
        //                                        break;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //                self.group.Nodes.Add(item);
        //                array3[(item.Side > 0) ? 1u : 0u] = item;
        //                continue;
        //            }

        //            if (text.Equals("trigger") || text.Equals("silent_trigger"))
        //            {
        //                string text3 = "";
        //                for (int k = 1; k < list.Count; k++)
        //                {
        //                    text3 = text3 + list[k] + " ";
        //                }

        //                if (int.TryParse(list[0], out var result4) && result4 >= 0)
        //                {
        //                    self.group.Nodes.Add(new FancyText.Trigger
        //                    {
        //                        Index = result4,
        //                        Silent = text.StartsWith("silent"),
        //                        Label = text3
        //                    });
        //                }

        //                continue;
        //            }

        //            if (text.Equals("*"))
        //            {
        //                self.currentShake = true;
        //                continue;
        //            }

        //            if (text.Equals("/*"))
        //            {
        //                self.currentShake = false;
        //                continue;
        //            }

        //            if (text.Equals("~"))
        //            {
        //                self.currentWave = true;
        //                continue;
        //            }

        //            if (text.Equals("/~"))
        //            {
        //                self.currentWave = false;
        //                continue;
        //            }

        //            if (text.Equals("!"))
        //            {
        //                self.currentImpact = true;
        //                continue;
        //            }

        //            if (text.Equals("/!"))
        //            {
        //                self.currentImpact = false;
        //                continue;
        //            }

        //            if (text.Equals("%"))
        //            {
        //                self.currentMessedUp = true;
        //                continue;
        //            }

        //            if (text.Equals("/%"))
        //            {
        //                self.currentMessedUp = false;
        //                continue;
        //            }

        //            if (text.Equals("big"))
        //            {
        //                self.currentScale = 1.5f;
        //                continue;
        //            }

        //            if (text.Equals("/big"))
        //            {
        //                self.currentScale = 1f;
        //                continue;
        //            }

        //            if (text.Equals("s"))
        //            {
        //                int result5 = 1;
        //                if (list.Count > 0)
        //                {
        //                    int.TryParse(list[0], out result5);
        //                }

        //                self.currentPosition += 5 * result5;
        //                continue;
        //            }

        //            if (!text.Equals("savedata"))
        //            {
        //                continue;
        //            }

        //            if (SaveData.Instance == null)
        //            {
        //                if (list[0].Equals("name", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    self.AddWord("Madeline");
        //                }
        //                else
        //                {
        //                    self.AddWord("[SD:" + list[0] + "]");
        //                }
        //            }
        //            else if (list[0].Equals("name", StringComparison.OrdinalIgnoreCase))
        //            {
        //                if (!self.language.CanDisplay(SaveData.Instance.Name))
        //                {
        //                    self.AddWord(Dialog.Clean("FILE_DEFAULT", self.language));
        //                }
        //                else
        //                {
        //                    self.AddWord(SaveData.Instance.Name);
        //                }
        //            }
        //            else
        //            {
        //                FieldInfo field = typeof(SaveData).GetField(list[0]);
        //                self.AddWord(field.GetValue(SaveData.Instance).ToString());
        //            }
        //        }
        //        else
        //        {
        //            self.AddWord(array2[j]);
        //        }
        //    }

        //    self.CalcLineWidth();
        //    return self.group;
        //}
        //else
        //{
        //    return orig(self);
        //}
    }

    public static void TextAddWord(On.Celeste.FancyText.orig_AddWord orig, FancyText self, string word)
    {
        if(self.language.Id == "schinese" && Md.Settings.ChineseCharactersAutoLining)
        {
            word = Emoji.Apply(word);
            Vector2 vector = self.size.Measure(word);
            float num = vector.X * self.currentScale;
            if(num > (float)self.maxLineWidth)
            {
                float multiplier = num / self.maxLineWidth;
                int unit = (int)(word.Length / multiplier);
                for(int i = 0; i < (int)float.Ceiling(multiplier); i++)
                {
                    string s = word.Substring(i * unit, int.Min(unit, word.Length - i * unit));
                    if (!s.IsNullOrEmpty())
                    {
                        orig(self, s);
                        if(i < (int)float.Ceiling(multiplier) - 1)
                        {
                            self.AddNewLine();
                        }
                    }
                }
            }
            else if (self.currentPosition + num > (float)self.maxLineWidth)
            {
                self.AddNewLine();
                orig(self, word);
            }
            else
            {
                orig(self, word);
            }
        }
        else
        {
            orig(self, word);
        }
    }
}

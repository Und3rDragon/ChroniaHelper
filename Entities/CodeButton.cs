using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/CodeButton")]
public class CodeButton : PressButton
{
    public CodeButton(EntityData data, Vc2 offset) : base(data, offset)
    {
        Add(CaseImage = new Image(GFX.Game[data.Attr("buttonBaseTexture", "objects/ChroniaHelper/button/base00")]));
        //CaseImage.Position += new Vector2(-8, 10);
        CaseImage.Position -= new Vc2(CaseImage.Width, CaseImage.Height) * 0.5f;
        button = new Button(Position, data.Attr("buttonTexture", "objects/ChroniaHelper/button/button00"));
        Depth = data.Int("depth", 8020);
        button.Depth = Depth + 1;

        sessionKeyID = data.Attr("sessionKeyID", "buttonKey");
        buttonCode = data.Attr("buttonCode", "0");
        buttonMode = data.Int("buttonMode", 0);
        
        pressSound = data.Attr("pressSound");
        releaseSound = data.Attr("releaseSound");
    }
    private string sessionKeyID;
    private string buttonCode;
    /// <summary>
    /// 0 = Input, 1 = Enter, 2 = Backspace, 3 = Clear
    /// </summary>
    private int buttonMode;
    private string pressSound, releaseSound;

    public Image CaseImage { get; private set; }

    protected override void OnPress()
    {
        if (pressSound.IsNotNullOrEmpty())
        {
            Audio.Play(pressSound);
        }
        if (buttonMode == 1)
        {
            string result = Md.Session.keystrings.GetValueOrDefault(sessionKeyID, "");
            if (Md.Session.codeButtonTargets.ContainsKey(sessionKeyID))
            {
                if (result == Md.Session.codeButtonTargets[sessionKeyID].codeString)
                {
                    Md.Session.codeButtonTargets[sessionKeyID].flag.SetFlag(true);
                }
            }
            Md.Session.keystrings[sessionKeyID] = "";
        }
        else if (buttonMode == 2)
        {
            if (Md.Session.keystrings.ContainsKey(sessionKeyID))
            {
                var s = Md.Session.keystrings[sessionKeyID];

                if (s.Length > 0)
                {
                    Md.Session.keystrings[sessionKeyID] = s.Substring(0, s.Length - 1);
                }
            }
        }
        else if (buttonMode == 3)
        {
            Md.Session.keystrings[sessionKeyID] = "";
        }
        else
        {
            Md.Session.keystrings.Create(sessionKeyID, "");
            Md.Session.keystrings[sessionKeyID] += buttonCode;
        }
    }

    protected override void OnRelease()
    {
        if (releaseSound.IsNotNullOrEmpty())
        {
            Audio.Play(releaseSound);
        }
        string result = Md.Session.keystrings.GetValueOrDefault(sessionKeyID, "");
        if (Md.Session.codeButtonTargets.ContainsKey(sessionKeyID))
        {
            if (result != Md.Session.codeButtonTargets[sessionKeyID].codeString &&
                Md.Session.codeButtonTargets[sessionKeyID].deactivateFlagWhenNotStaisfied)
            {
                Md.Session.codeButtonTargets[sessionKeyID].flag.SetFlag(false);
            }
        }
    }
}

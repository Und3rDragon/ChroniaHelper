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
public class CodeButton : BaseSolid2
{
    private readonly Button _button;
    private bool _currentButtonState = false;
    private bool _previousButtonState = false;

    public CodeButton(EntityData data, Vc2 offset)
        : base(offset, data, 16, 6, false)
    {
        //Collider.Position.Y += 10;
        Collider = new Hitbox(16, 6, -8, 2);
        
        Add(CaseImage = new Image(GFX.Game[data.Attr("buttonBaseTexture", "objects/ChroniaHelper/button/base00")]));
        //CaseImage.Position += new Vector2(-8, 10);
        CaseImage.Position -= new Vc2(CaseImage.Width, CaseImage.Height) * 0.5f;
        _button = new Button(Position, data.Attr("buttonTexture", "objects/ChroniaHelper/button/button00"));
        Depth = data.Int("depth", 8020);
        _button.Depth = Depth + 1;

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

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(_button);
    }

    public override void Removed(Scene scene)
    {
        scene.Remove(_button);
        base.Removed(scene);
    }

    public override void Update()
    {
        base.Update();
        List<Entity> actors = Scene.Tracker.GetEntities<Actor>();
        _currentButtonState = false;
        foreach (Entity actor in actors)
        {
            if (Collide.Check(actor, _button))
            {
                _currentButtonState = true;
                break;
            }
        }

        if (_currentButtonState != _previousButtonState)
        {
            SendOutSignals(_currentButtonState);
        }

        int target = _currentButtonState ? (int)_button.imageY + 2 : (int)_button.imageY;
        _button.Image.Position.Y = Calc.Approach(_button.Image.Position.Y, target, 40f * Engine.DeltaTime);

        _previousButtonState = _currentButtonState;
    }

    private void SendOutSignals(bool state = true)
    {
        if (state)
        {
            if (pressSound.IsNotNullOrEmpty())
            {
                Audio.Play(pressSound);
            }
            if (buttonMode == 1)
            {
                string result = Md.Session.sessionKeys.GetValueOrDefault(sessionKeyID, "");
                if (Md.Session.codeButtonTargets.ContainsKey(sessionKeyID))
                {
                    if (result == Md.Session.codeButtonTargets[sessionKeyID].codeString)
                    {
                        Md.Session.codeButtonTargets[sessionKeyID].flag.SetFlag(true);
                    }
                }
                Md.Session.sessionKeys[sessionKeyID] = "";
            }
            else if (buttonMode == 2)
            {
                if (Md.Session.sessionKeys.ContainsKey(sessionKeyID))
                {
                    var s = Md.Session.sessionKeys[sessionKeyID];

                    if (s.Length > 0)
                    {
                        Md.Session.sessionKeys[sessionKeyID] = s.Substring(0, s.Length - 1);
                    }
                }
            }
            else if (buttonMode == 3)
            {
                Md.Session.sessionKeys[sessionKeyID] = "";
            }
            else
            {
                Md.Session.sessionKeys.Create(sessionKeyID, "");
                Md.Session.sessionKeys[sessionKeyID] += buttonCode;
            }
        }
        else
        {
            if (releaseSound.IsNotNullOrEmpty())
            {
                Audio.Play(releaseSound);
            }
            string result = Md.Session.sessionKeys.GetValueOrDefault(sessionKeyID, "");
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

    [Tracked]
    public class Button : Entity
    {
        public Button(Vector2 position, string image) : base(position)
        {
            Add(Image = new Image(GFX.Game[image]));
            Collider = new Hitbox(14, 7, -7, -2);
            //Image.Position.X -= 6;
            //Image.Position.Y += 10;
            Image.Position.X -= Image.Width * 0.5f;
            Image.Position.Y -= Image.Height * 0.5f;
            imageY = Image.Position.Y;
        }

        public float imageY;
        public Image Image { get; private set; }
    }
}

[Tracked(true)]
[CustomEntity("ChroniaHelper/CodeButtonTargetController")]
public class CodeButtonTargetController : BaseEntity
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Update += OnLevelUpdate;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Update -= OnLevelUpdate;
    }
    
    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Celeste.Level self)
    {
        orig(self);
        
        foreach(var item in Md.Session.codeButtonTargets)
        {
            if (!item.Value.needsEnterCheck)
            {
                if(Md.Session.sessionKeys.GetValueOrDefault(item.Key, "") == item.Value.codeString)
                {
                    item.Value.flag.SetFlag(true);
                }
                else if(item.Value.deactivateFlagWhenNotStaisfied)
                {
                    item.Value.flag.SetFlag(false);
                }
            }
        }
    }

    public CodeButtonTargetController(EntityData data, Vc2 offset) : base(data, offset)
    {
        sessionKeyID = data.Attr("sessionKeyID", "buttonKey");
        buttonCodeTarget = data.Attr("buttonCodeTarget", "000000");
        hitEnterNeeded = data.Bool("hitEnterToConfirm", true);
        targetFlag = data.Attr("targetFlag", "flag");
        deactivateFlagWhenNotSatisfied = data.Bool("deactivateFlagWhenNotSatisfied", false);
    }
    private string sessionKeyID;
    private string buttonCodeTarget;
    private bool hitEnterNeeded;
    private string targetFlag;
    private bool deactivateFlagWhenNotSatisfied;

    protected override void AddedExecute(Scene scene)
    {
        Ses.CodeButtonTarget target = new()
        {
            codeString = buttonCodeTarget,
            needsEnterCheck = hitEnterNeeded,
            flag = targetFlag,
            deactivateFlagWhenNotStaisfied = deactivateFlagWhenNotSatisfied,
        };
        Md.Session.codeButtonTargets[sessionKeyID] = target;
    }
}

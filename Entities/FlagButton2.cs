using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Modules;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/RealFlagSwitch2")]
public class FlagButton2 : Entity {
    
    // Default parameters
    public static ParticleType P_Fire;
    public static ParticleType P_FireWhite;
    public Switch Switch;
    public SoundSource touchSfx;
    public Sprite border = new Sprite(GFX.Game, "objects/ChroniaHelper/flagTouchSwitchNew/container");
    public Sprite icon = new Sprite(GFX.Game, "objects/ChroniaHelper/flagTouchSwitchNew/idle");
    public Color inactiveColor = Calc.HexToColor("5fcde4");
    public Color activeColor = Color.White;
    public Color finishColor = Calc.HexToColor("f141df");
    public float ease;
    public Wiggler wiggler;
    public Vector2 pulse = Vector2.One;
    public float timer;
    public BloomPoint bloom;

    public Level level;

    // Constants
    private ParticleType particle;

    // States

    private bool Activated()
    {
        // Check status
        return level.Session.GetFlag(flagID);
    }

    private bool Activated(bool set)
    {
        // Set and return status
        level.Session.SetFlag(flagID, set);
        return Activated();
    }

    // Inputs from Lonn

    private string flag, hitSound, completeSound, hideFlag;
    private string borderPath, iconPath;
    private bool persistent, smoke, toggle, interactable;
    private enum switchKind { touch, wall }
    private switchKind classify;

    private int ew, eh;
    private float iw, ih;
    private Vector2 pos;

    private int ID;
    private string flagID;
    

    public FlagButton2(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {

    }

    public FlagButton2(Vector2 position, EntityData data)
        : base(position)
    {
        
        base.Depth = 2000;
        ID = data.ID;
        // Tag = Tags.Global;

        // Inputs
        flag = data.Attr("flag");
        flagID = $"ChroniaButtonFlag-{flag}-ButtonID-{ID}";
        hitSound = data.Attr("hitSound");
        completeSound = data.Attr("completeSoundFromScene");
        hideFlag = data.Attr("hideIfFlag");

        string input = data.Attr("switch");
        if (input == "touchSwitch")
        {
            classify = switchKind.touch;
        }
        else
        {
            classify = switchKind.wall;
        }

        persistent = data.Bool("persistent");
        smoke = data.Bool("smoke");
        toggle = data.Bool("allowDisable");
        interactable = data.Bool("playerCanActivate");

        ew = data.Width;
        eh = data.Height;
        pos = data.Position;

        borderPath = data.Attr("borderTexture");
        iconPath = data.Attr("icon");

        inactiveColor = Calc.HexToColor(data.Attr("inactiveColor", "5FCDE4"));
        activeColor = Calc.HexToColor(data.Attr("activeColor", "FFFFFF"));
        finishColor = Calc.HexToColor(data.Attr("finishColor", "F141DF"));

        // Hitbox
        if (classify == switchKind.touch)
        {
            Collider = new Hitbox(16f, 16f, ew / 2 - 8f, eh / 2 - 8f);
            if (interactable)
            {
                Add(new PlayerCollider(OnPlayer, null, new Hitbox(30f, 30f, ew / 2 - 15f, eh / 2 - 15f)));
            }
            Add(new HoldableCollider(OnHoldable, new Hitbox(20f, 20f, ew / 2 - 10f, eh / 2 - 10f)));
            Add(new SeekerCollider(OnSeeker, new Hitbox(24f, 24f, ew / 2 - 12f, eh / 2 - 12f)));
        }
        else
        {
            Collider = new Hitbox(ew, eh);
            if (interactable)
            {
                Add(new PlayerCollider(OnPlayer, null, new Hitbox(ew, eh)));
            }
            Add(new HoldableCollider(OnHoldable, new Hitbox(ew, eh)));
            Add(new SeekerCollider(OnSeeker, new Hitbox(ew, eh)));
        }

        // Sprite
        // Border texture
        border = new Sprite(GFX.Game, borderPath);
        border.AddLoop("idle", "", data.Float("borderAnimation", 0.1f));
        Add(border);
        border.Play("idle");
        border.JustifyOrigin(0.5f, 0.5f);

        particle = new ParticleType(TouchSwitch.P_Fire)
        {
            Color = finishColor
        };

        // Setup the icon
        icon = new Sprite(GFX.Game, iconPath);
        icon.AddLoop("idle", "/idle", data.Float("iconIdleAnimation", 0.1f));
        icon.AddLoop("spin", "/spin", data.Float("iconSpinAnimation", 0.02f));
        icon.Add("finishing", "/finishing", data.Float("iconFinishingAnimation", 0.1f), "finished");
        icon.AddLoop("finished", "/finished", data.Float("iconFinishedAnimation", 0.1f));

        Add(icon);

        icon.Play("idle");
        icon.Color = inactiveColor;
        border.Color = icon.Color;
        ih = icon.Height;
        iw = icon.Width;
        icon.SetOrigin(-ew / 2 + icon.Width / 2, -eh / 2 + icon.Height / 2);

        Add(bloom = new BloomPoint(new Vector2(ew / 2, eh / 2), 0f, 16f)); // original 0, 16
        bloom.Alpha = 0f;

        Add(wiggler = Wiggler.Create(0.5f, 4f, v => {
            pulse = Vector2.One * (1f + v * 0.25f);
        }));

        Add(new VertexLight(Color.White, 0.8f, 16, 32));
        Add(touchSfx = new SoundSource());

        // Password Keyboard
        passwordID = data.Attr("passwordID");
        password = data.Attr("password");
        passwordProtected = !string.IsNullOrEmpty(passwordID) && !string.IsNullOrEmpty(password);
    }
    // Save or overwrite the existing values
    private bool passwordProtected = false; private string passwordID, password;
    
    public void TurnOn()
    {
        if (!Activated())
        {
            touchSfx.Play(hitSound);

            Activated(true);
            if (!persistent)
            {
                Md.Session.flagsPerRoom.Add(flagID);
            }

            // animation
            wiggler.Start();
            for (int i = 0; i < 32; i++)
            {
                float num = Calc.Random.NextFloat((float)Math.PI * 2f);
                level.Particles.Emit(particle, Position + new Vector2(ew / 2, eh / 2) + Calc.AngleToVector(num, 6f), num);
            }
            icon.Play("spin");
        }
    }

    public void TurnOff()
    {
        if (Activated())
        {
            touchSfx.Play(hitSound);

            Activated(false);
            
            level.Session.SetFlag(flag, false);
            level.Session.SetFlag($"playedSound_{flag}_button", false);

            // animation
            wiggler.Stop();
            icon.Play("idle");
        }
    }
    
    public bool IsCompleted()
    {
        return MaP.IsSwitchFlagCompleted(flag);
    }

    public bool inside = false;
    public void OnPlayer(Player player)
    {
        if (passwordProtected)
        {
            if (!Md.Session.Passkeyboard_Passwords.ContainsKey(passwordID)) { return; }
            else if (Md.Session.Passkeyboard_Passwords[passwordID] != password) { return; }
        }
        if (toggle)
        {
            if (!inside)
            {
                if (Activated()) { TurnOff(); }
                else { TurnOn(); }
            }
        }
        else
        {
            TurnOn();
        }
        if (IsCompleted())
        {
            if (!inside && !level.Session.GetFlag($"playedSound_{flag}_button"))
            {
                SoundEmitter.Play(completeSound);
                level.Session.SetFlag($"playedSound_{flag}_button", true);
            }
        }
    }

    public void OnHoldable(Holdable h)
    {
        TurnOn();
    }

    public void OnSeeker(Seeker seeker)
    {
        if (SceneAs<Level>().InsideCamera(Position, 10f))
        {
            TurnOn();
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        level = SceneAs<Level>();

        if (!persistent)
        {
            Md.Session.flagsPerRoom.Enter(flagID);
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        
        if (!persistent)
        {
            flagID.SetFlag(false);
            
            icon.Play("idle");
            finished = false;
        }

        // If not completed, we should reset the flag too
        if (!IsCompleted())
        {
            level.Session.SetFlag(flag, false);
            level.Session.SetFlag($"playedSound_{flag}_button", false);
        }
    }

    public override void Awake(Scene scene)
    {
        
        base.Awake(scene);


    }

    private bool finished = false;
    public override void Update()
    {
        if (!ChroniaFlagUtils.GetFlag(flagID) && !IsCompleted())
        {
            icon.Color = inactiveColor;
            border.Color = icon.Color;
            icon.Play("idle");
            Activated(false);
        }
        else if (ChroniaFlagUtils.GetFlag(flagID) && !IsCompleted())
        {
            icon.Color = activeColor;
            border.Color = icon.Color;
            icon.Play("spin");
        }

        if (CollideCheck<Player>())
        {
            inside = true;
        }
        else { inside = false; }

        timer += Engine.DeltaTime * 8f;
        ease = Calc.Approach(ease, (IsCompleted() || Activated()) ? 1f : 0f, Engine.DeltaTime * 2f);
        icon.Color = Color.Lerp(inactiveColor, IsCompleted() ? finishColor : activeColor, ease);
        icon.Color *= 0.5f + ((float)Math.Sin(timer) + 1f) / 2f * (1f - ease) * 0.5f + 0.5f * ease;
        border.Color = icon.Color;
        bloom.Alpha = ease;
        if (IsCompleted())
        {
            if (icon.CurrentAnimationID != "finishing" && icon.CurrentAnimationID != "finished")
            {
                icon.Play("finishing");
                finished = false;
            }
            else if (icon.CurrentAnimationID == "finished" && !finished)
            {
                wiggler.Start();
                icon.Play("finished");
                level.Displacement.AddBurst(Position + new Vector2(ew / 2, eh / 2), 0.6f, 4f, 28f, 0.2f);
                finished = true;
            }
            else if (base.Scene.OnInterval(0.03f))
            {
                Vector2 position = Position + new Vector2(ew /2, eh /2 + 1) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
                // emit particles depending on the entity
                if(classify == switchKind.touch) { level.ParticlesBG.Emit(particle, position); }
                else { level.ParticlesBG.Emit(particle, EdgePosition()); }
            }
            level.Session.SetFlag(flag, true);
        }

        base.Update();

        Md.Session.FlagButtonFrameIndex.Enter(ID, icon.CurrentAnimationFrame);
    }

    public Vector2 EdgePosition()
    {
        int def = Calc.Random.Range(0,4);
        int x, y;
        switch (def)
        {
            case 1:
                x = ew; y = Calc.Random.Range(0, eh); break;
            case 2:
                x = Calc.Random.Range(0, ew); y = eh; break;
            case 3:
                x = 0; y = Calc.Random.Range(0, eh); break;
            default:
                x = Calc.Random.Range(0, ew); y = 0; break;
        }
        return Position + new Vector2(x, y);
    }

    public override void Render()
    {

        if (this.classify == switchKind.touch)
        {
            //border.DrawCentered(Position + new Vector2(ew / 2, eh / 2) + new Vector2(0f, -1f), Color.Black);
            //border.DrawCentered(Position + new Vector2(ew / 2, eh / 2), icon.Color, pulse);
        }
        else
        {
            Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, new ChroniaColor(icon.Color, 0.7f).Parsed());
            Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Lerp(icon.Color, Calc.HexToColor("0a0a0a"), 0.5f) * 0.3f);
        }

        base.Render();
    }
}

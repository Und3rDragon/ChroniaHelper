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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/RealFlagSwitch", "ChroniaHelper/RealFlagSwitchAlt")]
public class FlagButton : Entity {
    
    // Default parameters
    public static ParticleType P_Fire;
    public static ParticleType P_FireWhite;
    public Switch Switch;
    public SoundSource touchSfx;
    public MTexture border = GFX.Game["objects/touchswitch/container"];
    public Sprite icon = new Sprite(GFX.Game, "objects/touchswitch/icon");
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

    private List<string> vanillanames = new List<string>
        { "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple" };
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

    private string flag, hitSound, switchSound, completeSound, hideFlag;
    private string borderTexture, iconPath;
    private bool vanilla;
    private bool persistent, smoke, inverted, toggle, interactable;
    private enum switchKind { touch, wall }
    private switchKind classify;
    private float idleInterval, spinInterval, onRate, finishRate;

    private int ew, eh;
    private float iw, ih;
    private Vector2 pos;

    private int ID;
    private string flagID;
    

    public FlagButton(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {

    }

    public FlagButton(Vector2 position, EntityData data)
        : base(position)
    {
        
        base.Depth = 2000;
        ID = data.ID;
        // Tag = Tags.Global;

        // Inputs
        flag = data.Attr("flag");
        flagID = $"ChroniaButtonFlag-{flag}-ButtonID-{ID}";
        hitSound = data.Attr("hitSound");
        switchSound = data.Attr("completeSoundFromSwitch");
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
        inverted = data.Bool("inverted");
        toggle = data.Bool("allowDisable");
        interactable = data.Bool("playerCanActivate");

        idleInterval = Math.Abs(data.Float("idleAnimDelay", 0.1f));
        spinInterval = Math.Abs(data.Float("spinAnimDelay", 0.1f));
        onRate = Math.Abs(data.Float("activatedAnimRate", 4f));
        finishRate = Math.Abs(data.Float("finishedAnimRate", 0.1f));

        ew = data.Width;
        eh = data.Height;
        pos = data.Position;

        borderTexture = data.Attr("borderTexture");
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
        if (!string.IsNullOrEmpty(borderTexture))
        {
            border = GFX.Game[borderTexture];
        }

        particle = new ParticleType(TouchSwitch.P_Fire)
        {
            Color = finishColor
        };

        // Setup the icon
        if (vanillanames.Contains(iconPath))
        {
            vanilla = true;
        }
        else { vanilla = false; }

        if (vanilla) { icon = new Sprite(GFX.Game, iconPath == "vanilla" ? "objects/touchswitch/icon" : $"objects/ChroniaHelper/flagTouchSwitch/{iconPath}/icon"); }
        else { icon = new Sprite(GFX.Game, iconPath); }

        Add(icon);
        if (vanilla)
        {
            icon.Add("idle", "", 0f, default(int));
            icon.Add("spin", "", 0.1f, new Chooser<string>("spin", 1f), 0, 1, 2, 3, 4, 5);
        }
        else
        {
            icon.AddLoop("idle", "", idleInterval);
            icon.AddLoop("spin", "", spinInterval);
        }

        icon.Play("spin");
        icon.Color = inactiveColor;
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

    public void FlagSave(string key)
    {
        if (ChroniaHelperModule.Session.switchFlag.ContainsKey(key))
        {
            ChroniaHelperModule.Session.switchFlag.Remove(key);
        }
        ChroniaHelperModule.Session.switchFlag.Add(key, Activated());
    }

    public void FlagSave(string key, bool overwrite)
    {
        if (ChroniaHelperModule.Session.switchFlag.ContainsKey(key))
        {
            ChroniaHelperModule.Session.switchFlag.Remove(key);
        }
        ChroniaHelperModule.Session.switchFlag.Add(key, overwrite);
    }
    // Get and override flag states
    public bool FlagLoad(string key)
    {
        if (ChroniaHelperModule.Session.switchFlag.ContainsKey(key))
        {
            return Activated(ChroniaHelperModule.Session.switchFlag[key]);
        }
        return Activated(false);
    }
    
    // Deal with flags in last room
    public void RegisterLastFlag()
    {
        if (ChroniaHelperModule.Session.lastRoom.Contains(flagID) && persistent)
        {
            ChroniaHelperModule.Session.lastRoom.Remove(flagID);
        }
        if (!ChroniaHelperModule.Session.lastRoom.Contains(flag))
        {
            ChroniaHelperModule.Session.lastRoom.Add(flag);
        }
        if (persistent) { return; }
        if (!ChroniaHelperModule.Session.lastRoom.Contains(flagID))
        {
            ChroniaHelperModule.Session.lastRoom.Add(flagID);
        }
    }

    public void ListFlagReset()
    {
        foreach (string item in ChroniaHelperModule.Session.lastRoom)
        {
            FlagSave(item, false);
            level.Session.SetFlag(item, false);
        }
    }

    public void DebugFlag()
    {
        foreach (string key in ChroniaHelperModule.Session.switchFlag.Keys)
        {
            Log.Info(key + " " + ChroniaHelperModule.Session.switchFlag[key]);
        }
    }

    public void TurnOn()
    {
        if (!Activated())
        {
            touchSfx.Play(hitSound);

            Activated(true);
            FlagSave(flagID);

            // animation
            wiggler.Start();
            for (int i = 0; i < 32; i++)
            {
                float num = Calc.Random.NextFloat((float)Math.PI * 2f);
                level.Particles.Emit(particle, Position + new Vector2(ew / 2, eh / 2) + Calc.AngleToVector(num, 6f), num);
            }
            icon.Rate = onRate;
        }
    }

    public void TurnOff()
    {
        if (Activated())
        {
            touchSfx.Play(hitSound);

            Activated(false);
            FlagSave(flagID);
            level.Session.SetFlag(flag, false);
            level.Session.SetFlag($"playedSound_{flag}_button", true);

            // animation
            wiggler.Stop();
            icon.Play("spin");
            icon.Rate = 1f;
        }
    }

    public bool IsCompleted(string flagIndex)
    {
        bool b = true;
        int count = 0;
        foreach (string key in ChroniaHelperModule.Session.switchFlag.Keys) {
            if (key.StartsWith($"ChroniaButtonFlag-{flagIndex}-ButtonID-"))
            {
                b = b ? ChroniaHelperModule.Session.switchFlag[key] : false;
                count++;
            }
        }
        if(count == 0) { return false; }
        return b;
    }

    public bool IsCompleted()
    {
        return IsCompleted(flag);
    }

    public bool inside = false;
    public void OnPlayer(Player player)
    {
        if (passwordProtected)
        {
            if (!ChroniaHelperModule.Session.Passwords.ContainsKey(passwordID)) { return; }
            else if (ChroniaHelperModule.Session.Passwords[passwordID] != password) { return; }
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

        // Register the flags in current room to see which should be reset once out of room
        RegisterLastFlag();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);

        // Reset the last room flags
        ListFlagReset();

        // If not persistent, reset the values
        if (!persistent)
        {
            FlagSave(flagID, false);
        }

        // If not completed, we should reset the flag too
        if (!IsCompleted())
        {
            level.Session.SetFlag(flag, false);
            level.Session.SetFlag($"playedSound_{flag}_button", false);
        }

        FlagLoad(flagID);
    }

    public override void Awake(Scene scene)
    {
        
        base.Awake(scene);


    }


    public override void Update()
    {
        if (CollideCheck<Player>())
        {
            inside = true;
        }
        else { inside = false; }

        timer += Engine.DeltaTime * 8f;
        ease = Calc.Approach(ease, (IsCompleted() || Activated()) ? 1f : 0f, Engine.DeltaTime * 2f);
        icon.Color = Color.Lerp(inactiveColor, IsCompleted() ? finishColor : activeColor, ease);
        icon.Color *= 0.5f + ((float)Math.Sin(timer) + 1f) / 2f * (1f - ease) * 0.5f + 0.5f * ease;
        bloom.Alpha = ease;
        if (IsCompleted())
        {
            if (icon.Rate > finishRate)
            {
                icon.Rate -= 2f * Engine.DeltaTime;
                if (icon.Rate <= finishRate)
                {
                    icon.Rate = finishRate;
                    wiggler.Start();
                    icon.Play("idle");
                    level.Displacement.AddBurst(Position + new Vector2(ew /2, eh / 2), 0.6f, 4f, 28f, 0.2f);
                }
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

        if (!ChroniaHelperModule.Session.touchSwitchFrame.Keys.Contains(ID))
        {
            ChroniaHelperModule.Session.touchSwitchFrame.Add(ID, icon.CurrentAnimationFrame);
        }
        else
        {
            ChroniaHelperModule.Session.touchSwitchFrame[ID] = icon.CurrentAnimationFrame;
        }
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
            border.DrawCentered(Position + new Vector2(ew / 2, eh / 2) + new Vector2(0f, -1f), Color.Black);
            border.DrawCentered(Position + new Vector2(ew / 2, eh / 2), icon.Color, pulse);
        }
        else
        {
            Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, ColorUtils.ColorCopy(icon.Color, 0.7f));
            Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Lerp(icon.Color, Calc.HexToColor("0a0a0a"), 0.5f) * 0.3f);
        }

        base.Render();
    }
}

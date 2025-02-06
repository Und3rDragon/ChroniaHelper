using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using FMOD;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using YamlDotNet.Core.Tokens;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SpriteEntity")]
public class SpriteEntity : Actor
{
    // assumption:
    // several commands controlling how this entity moves or acts
    public enum Command
    {
        Wait_Flag, Set_Flag, Play, Flag_Play, Wait, None, MoveTo, 
        Move, Alpha, Color, Scale, Rotate, Depth, Repeat, Move_Around, Rate, Origin,
        Ignore, Sound, Music, Hitbox, Bloom, Light,
        Bloom_Move, Bloom_Moveto, Bloom_Movearound, 
        Light_Move, Light_Moveto, Light_Movearound, Parallax, Render_Position_InRoom,
        Current_Frame, Camera_Offset,
        //done
        Holdable_Collider, Solid
        //undone
    }
    private Command execute = Command.None;
    public Dictionary<string, Command> GetCommand = new Dictionary<string, Command>()
    {
        { "setflag", Command.Set_Flag },
        { "moveto", Command.MoveTo },
        { "move", Command.Move },
        { "play", Command.Play },
        { "flagplay", Command.Flag_Play },
        { "wait", Command.Wait },
        { "waitflag", Command.Wait_Flag },
        { "alpha", Command.Alpha },
        { "color", Command.Color },
        { "scale", Command.Scale },
        { "rotate", Command.Rotate },
        { "depth", Command.Depth },
        { "repeat", Command.Repeat },
        { "movearound", Command.Move_Around },
        { "origin", Command.Origin },
        { "rate", Command.Rate },
        { "ignore", Command.Ignore },
        { "sound", Command.Sound },
        { "music", Command.Music },
        { "hitbox", Command.Hitbox },
        { "holdablecollider", Command.Holdable_Collider },
        { "bloom", Command.Bloom },
        { "light", Command.Light },
        { "bloommove", Command.Bloom_Move },
        { "bloommoveto", Command.Bloom_Moveto },
        { "bloommovearound", Command.Bloom_Movearound },
        { "lightmove", Command.Light_Move },
        { "lightmoveto", Command.Light_Moveto },
        { "lightmovearound", Command.Light_Movearound },
        { "parallax", Command.Parallax },
        { "renderpositioninroom", Command.Render_Position_InRoom },
        { "currentframe", Command.Current_Frame },
        { "cameraoffset", Command.Camera_Offset },
        { "solid", Command.Solid }
    };

    public Dictionary<Command, bool> IsRoutineRunning = new Dictionary<Command, bool>();

    public List<Command> NoSideRoutines = new List<Command>() {
        Command.None, Command.Set_Flag, Command.Play, Command.Flag_Play,
        Command.Wait, Command.Wait_Flag, Command.Repeat, Command.Ignore, Command.Sound,Command.Music,
        Command.Hitbox, Command.Holdable_Collider, Command.Current_Frame, Command.Solid
    };

    public SpriteEntity(EntityData data, Vector2 offset) : this(data.Position + offset, data) { }
    public SpriteEntity(Vector2 position, EntityData data) : base(position)
    {
        spriteName = data.Attr("xmlLabel", "SpriteEntity");
        sprite = GFX.SpriteBank.Create(spriteName);
        Add(sprite);
        // sprite center == entity position

        string command = data.Attr("commands");
        // commands syntax: "command1,param1,param2...;command2,param1,param2..."
        commands = command.Split(';', StringSplitOptions.TrimEntries); // commands stored
        // each command should looks like "command,xxx"

        base.Depth = data.Int("depth", 9500);

        Add(sfx = new SoundSource());
        Add(light = new VertexLight(Color.White, 0f, 32, 64));
        Add(bloom = new BloomPoint(0f, 0f));

        Add(holdable = new Holdable(0.3f));
        holdable.SpeedGetter = () => Speed;
        holdable.OnPickup = OnPickup;
        holdable.OnRelease = OnRelease;
        holdable.OnHitSpring = HitSpring;
        holdable.OnHitSeeker = HitSeeker;

        parallax = data.Float("parallax", 1f);
        camX = data.Float("camPositionX", 160f);
        camY = data.Float("camPositionY", 90f);

        solid = new Solid(Position, 0f, 0f, true);

        Add(mover = new StaticMover());
    }
    private string spriteName;
    private Sprite sprite;
    private string[] commands;
    private SoundSource sfx;
    private Holdable holdable;
    private VertexLight light;
    private BloomPoint bloom;
    private float parallax = 1f, camX = 160f, camY = 90f;
    private Solid solid; private Vector2 solidPos = Vector2.Zero;
    private StaticMover mover;

    public override void Added(Scene scene)
    {
        foreach (var command in Enum.GetValues<Command>())
        {
            IsRoutineRunning.Add(command, false);
        }

        base.Added(scene);

        MapProcessor.level.Add(solid);
        
        // start executing commands when entering the room
        Add(new Coroutine(Execution()));
    }

    public Dictionary<string, bool> ignoreFlags = new Dictionary<string, bool>();
    public Dictionary<string, List<int>> ignoreIndexes = new Dictionary<string, List<int>>();
    public List<int> alwaysIgnores = new List<int>();
    public bool ignoreActivated = false;

    // Holdable parameters
    private Vector2 Speed;

    public IEnumerator Execution()
    {
        ignoreActivated = false;
        for (int index = 0; index < commands.Length; index++)
        {
            if (ignoreActivated)
            {
                if (alwaysIgnores.Contains(index)) { continue; }
                bool shouldSkip = false;
                foreach (var flag in ignoreFlags.Keys)
                {
                    bool flagArg = ignoreFlags[flag] ? !MapProcessor.session.GetFlag(flag) : MapProcessor.session.GetFlag(flag);

                    if (flagArg && ignoreIndexes[flag].Contains(index)) { shouldSkip = true; }
                }
                if (shouldSkip) { continue; }
            }

            // split the commands, see if empty or invalid
            string[] commandLine = commands[index].Split(',', StringSplitOptions.TrimEntries);
            if (commandLine.Length == 0) { continue; }
            if (commandLine.Length == 1)
            {
                if (string.IsNullOrEmpty(commandLine[0])) { continue; }
            }

            // processing indicator data
            string indicator = commandLine[0].ToLower().Trim().RemoveAll("_");
            bool sideRoutine = indicator.Contains("passby ");
            indicator = indicator.RemoveAll("passby ");
            execute = GetCommand.GetValueOrDefault(indicator, Command.None);

            int segs = commandLine.Length;

            if (execute == Command.Repeat)
            {
                // valid syntax: "repeat, 0, (overrideFlag)"
                if (segs < 2) { continue; }

                int newIndex = int.TryParse(commandLine[1], out newIndex) ? newIndex : 0;
                newIndex.MakeAbs();

                bool hasOverride = false;
                string overrideFlag = string.Empty;
                if (segs >= 3)
                {
                    hasOverride = string.IsNullOrEmpty(commandLine[2]);
                }
                if (hasOverride)
                { overrideFlag = commandLine[2]; }

                if (hasOverride)
                {
                    if (!MapProcessor.session.GetFlag(overrideFlag))
                    {
                        index = newIndex - 1;
                    }
                }
                else
                {
                    index = newIndex - 1;
                }
            }

            else if (execute == Command.Play)
            {
                // valid syntax: "play,spriteName, (resetAnimation, random)"

                string playSprite = string.Empty;
                
                if (segs == 1) { IsRoutineRunning[execute] = false; continue; }
                if (segs >= 2) { playSprite = commandLine[1]; }
                bool random = false, reset = false;
                if (segs >= 3) { bool.TryParse(commandLine[2], out reset); }
                if (segs >= 4) { bool.TryParse(commandLine[3], out random); }

                sprite.Play(playSprite, reset, random);
            }

            else if (execute == Command.Flag_Play)
            {
                // valid syntax: "flagplay,flag,spriteName,(inverted, reset, random)"

                string playSprite = string.Empty, flag = string.Empty;
                
                if (segs < 3) { IsRoutineRunning[execute] = false; continue; }
                if (segs >= 3) { flag = commandLine[1]; playSprite = commandLine[2]; }
                bool inverted = false, reset = false, random = false;
                if (segs >= 4) { bool.TryParse(commandLine[3], out inverted); }
                if (segs >= 5) { bool.TryParse(commandLine[4], out reset); }
                if (segs >= 6) { bool.TryParse(commandLine[5], out random); }

                if (inverted && MapProcessor.session.GetFlag(flag))
                {
                    IsRoutineRunning[execute] = false; continue;
                }
                if (!inverted && !MapProcessor.session.GetFlag(flag))
                {
                    IsRoutineRunning[execute] = false; continue;
                }

                sprite.Play(playSprite, reset, random);
            }

            else if (execute == Command.Ignore)
            {
                // valid syntax: "ignore, ignoreFlag, inverted, num1,(num2,num3)"
                if (segs < 4) { continue; }

                string flag = string.Empty; bool inverted = false;
                List<int> indexes = new List<int>();
                if (segs >= 4)
                {
                    flag = commandLine[1];
                    bool.TryParse(commandLine[2], out inverted);
                    for (int i = 3; i < segs; i++)
                    {
                        int result;
                        if (int.TryParse(commandLine[i], out result))
                        {
                            indexes.Add(result);
                        }
                    }

                    if (string.IsNullOrEmpty(flag) && !inverted) { continue; }
                    else if (string.IsNullOrEmpty(flag) && inverted)
                    {
                        foreach (int i in indexes)
                        {
                            if (!alwaysIgnores.Contains(i)) { alwaysIgnores.Add(i); }
                        }
                    }
                    else
                    {
                        if (ignoreFlags.Keys.Contains(flag))
                        {
                            ignoreFlags[flag] = inverted;
                        }
                        else
                        {
                            ignoreFlags.Add(flag, inverted);
                        }

                        if (ignoreIndexes.Keys.Contains(flag)) { ignoreIndexes[flag] = indexes; }
                        else
                        {
                            ignoreIndexes.Add(flag, indexes);
                        }
                    }

                }

                ignoreActivated = true;
            }

            else if (execute == Command.Sound)
            {
                // valid syntax: "sound, sfx"
                if (segs < 2) { continue; }

                sfx.Play(commandLine[1]);
            }

            else if (execute == Command.Music)
            {
                // valid syntax: "music, eventName"
                if (segs < 2) { continue; }

                MapProcessor.session.Audio.Music.Event = SFX.EventnameByHandle(commandLine[1]);
            }

            else if (execute == Command.Hitbox)
            {
                // valid syntax: "hitbox, width, height, (x, y)"
                if (segs < 3) { continue; }
                float width = 16f, height = 16f, x = -8f, y = -8f;
                float.TryParse(commandLine[1], out width);
                float.TryParse(commandLine[2], out height);
                width.MakeAbs(); height.MakeAbs();

                if (segs >= 4)
                {
                    float.TryParse(commandLine[3], out x);
                }
                if (segs >= 5)
                {
                    float.TryParse(commandLine[4], out y);
                }

                base.Collider = new Hitbox(width, height, x, y);
            }

            else if (execute == Command.Holdable_Collider)
            {
                // valid syntax: "hitbox, width, height, (x, y)"
                if (segs < 3) { continue; }
                float width = 16f, height = 16f, x = -8f, y = -8f;
                float.TryParse(commandLine[1], out width);
                float.TryParse(commandLine[2], out height);
                width.MakeAbs(); height.MakeAbs();

                if (segs >= 4)
                {
                    float.TryParse(commandLine[3], out x);
                }
                if (segs >= 5)
                {
                    float.TryParse(commandLine[4], out y);
                }

                holdable.PickupCollider = new Hitbox(width, height, x, y);
            }

            else if (execute == Command.Current_Frame)
            {
                // valid syntax: "current_frame, frame index"
                if (segs < 2) { continue; }

                int setFrame = 0;
                int.TryParse(commandLine[1], out setFrame);
                int totalFrames = sprite.GetFrames(sprite.CurrentAnimationID).Length;
                setFrame = setFrame >= totalFrames ? totalFrames - 1 : setFrame;

                sprite.SetAnimationFrame(setFrame);
            }

            else if (execute == Command.Solid)
            {
                // syntax: "solid, x, y, width, height, (safe)"
                Vector2 size = Vector2.Zero;
                bool safe = true;

                if (segs < 5) { continue; }
                float.TryParse(commandLine[1], out solidPos.X);
                float.TryParse(commandLine[2], out solidPos.Y);
                float.TryParse(commandLine[3], out size.X);
                float.TryParse(commandLine[4], out size.Y);

                if (segs >= 6)
                {
                    bool.TryParse(commandLine[5], out safe);
                }

                MapProcessor.level.Remove(solid);
                solid = new Solid(Position + solidPos, size.X, size.Y, safe);
                MapProcessor.level.Add(solid);
            }

            else
            {
                if (sideRoutine && !NoSideRoutines.Contains(execute))
                {
                    Add(new Coroutine(Commanding(commandLine, execute, true)));
                }
                else
                {
                    yield return new SwapImmediately(Commanding(commandLine, execute, false));
                }
            }
        }

        //yield return null;
    }

    public IEnumerator Commanding(string[] commandLine, Command execute, bool isSideRoutine)
    {
        if (isSideRoutine && IsRoutineRunning[execute]) { IsRoutineRunning[execute] = false; yield break; }

        IsRoutineRunning[execute] = true;

        int segs = commandLine.Length;

        bool instant = false;

        if (execute == Command.Set_Flag)
        {
            string flagName = string.Empty; bool flagValue = true;
            // valid syntax: "setflag,flagName,value"
            if (segs == 1) { IsRoutineRunning[execute] = false; yield break; }
            if (segs >= 2) { flagName = commandLine[1]; }
            if (segs >= 3) { bool.TryParse(commandLine[2], out flagValue); }

            MapProcessor.session.SetFlag(flagName, flagValue);
        }

        else if (execute == Command.Wait_Flag)
        {
            // valid syntax: "waitflag,flag", "waitflag, flag, true"
            string flag = string.Empty;
            if (segs < 2) { IsRoutineRunning[execute] = false; yield break; }
            if (segs >= 2) { flag = commandLine[1]; }
            bool inverted = false;
            if (segs >= 3) { bool.TryParse(commandLine[2], out inverted); }

            while (true)
            {
                if (!inverted)
                {
                    if (MapProcessor.session.GetFlag(flag)) { break; }
                }
                else
                {
                    if (!MapProcessor.session.GetFlag(flag)) { break; }
                }
                yield return null;
            }

            IsRoutineRunning[execute] = false; yield break;
        }

        else if (execute == Command.Wait)
        {
            float timer = 0.1f;
            // valid syntax: "wait, time"
            if (segs <= 1) { IsRoutineRunning[execute] = false; yield break; }
            if (segs >= 2)
            {
                float.TryParse(commandLine[1], out timer);
                timer.MakeAbs();
            }

            while (timer > 0)
            {
                timer = Calc.Approach(timer, 0f, Engine.DeltaTime);
                yield return null;
            }

            IsRoutineRunning[execute] = false; yield break;
        }

        else if (execute == Command.Move || execute == Command.Light_Move || execute == Command.Bloom_Move)
        {
            // valid syntax: "move,3,5", "move,3,5,1", "move,3,5,1,cubein"
            float dx = 0f, dy = 0f, timer = 1f;
            if (segs < 3) { IsRoutineRunning[execute] = false; yield break; }
            if (segs >= 3)
            {
                float.TryParse(commandLine[1], out dx);
                float.TryParse(commandLine[2], out dy);
            }
            if (segs >= 4)
            {
                float.TryParse(commandLine[3], out timer);
                timer.MakeAbs();
                instant = timer.GetAbs() < Engine.DeltaTime;
            }
            Ease.Easer ease = Ease.Linear;
            if (segs >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4].ToLower());
            }

            Vector2 obj;
            switch (execute)
            {
                case Command.Move: obj = Position; break;
                case Command.Light_Move: obj = light.Position; break;
                case Command.Bloom_Move: obj = bloom.Position; break;
                default: obj = Position; break;
            }
            Vector2 pos1 = obj, pos2 = pos1 + new Vector2(dx, dy);
            if (instant) { 
                obj = pos2;

                switch (execute)
                {
                    case Command.Move: Position = obj; break;
                    case Command.Light_Move: light.Position = obj; break;
                    case Command.Bloom_Move: bloom.Position = obj; break;
                    default: Position = obj; break;
                }

                yield break; 
            }
            float percent = 0f;
            while (percent < 1f)
            {
                percent = Calc.Approach(percent, 1f, Engine.DeltaTime / timer);
                obj = Vector2.Lerp(pos1, pos2, ease(percent));

                switch (execute)
                {
                    case Command.Move: Position = obj; break;
                    case Command.Light_Move: light.Position = obj; break;
                    case Command.Bloom_Move: bloom.Position = obj; break;
                    default: obj = Position; break;
                }

                yield return null;
            }
        }

        else if (execute == Command.MoveTo || execute == Command.Light_Moveto || execute == Command.Bloom_Moveto)
        {
            // valid syntax: "moveto,3,5", "moveto,3,5,1", "moveto,3,5,1,cubein"
            float x = 0f, y = 0f, timer = 1f;
            if (segs < 3) { IsRoutineRunning[execute] = false; yield break; }
            if (segs >= 3)
            {
                float.TryParse(commandLine[1], out x);
                float.TryParse(commandLine[2], out y);
            }
            if (segs >= 4)
            {
                float.TryParse(commandLine[3], out timer);
                timer.MakeAbs();
                instant = timer.GetAbs() < Engine.DeltaTime;
            }
            Ease.Easer ease = Ease.Linear;
            if (segs >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4].ToLower());
            }

            Vector2 obj;
            switch (execute)
            {
                case Command.MoveTo: obj = Position; break;
                case Command.Light_Moveto: obj = light.Position; break;
                case Command.Bloom_Moveto: obj = bloom.Position; break;
                default: obj = Position; break;
            }
            Vector2 pos1 = obj, pos2 = new Vector2(x, y);
            if (instant) { 
                obj = pos2;

                switch (execute)
                {
                    case Command.MoveTo: Position = obj; break;
                    case Command.Light_Moveto: light.Position = obj; break;
                    case Command.Bloom_Moveto: bloom.Position = obj; break;
                    default: Position = obj; break;
                }

                yield break; 
            }
            float percent = 0f;
            while (percent < 1f)
            {
                percent = Calc.Approach(percent, 1f, Engine.DeltaTime / timer);
                obj = Vector2.Lerp(pos1, pos2, ease(percent));

                switch (execute)
                {
                    case Command.MoveTo: Position = obj; break;
                    case Command.Light_Moveto: light.Position = obj; break;
                    case Command.Bloom_Moveto: bloom.Position = obj; break;
                    default: Position = obj; break;
                }

                yield return null;
            }
        }

        else if (execute == Command.Move_Around || execute == Command.Light_Movearound || execute == Command.Bloom_Movearound)
        {
            // valid syntax: "movearound, center-dx, center-dy, deltaAngle, spinTime, (easing)"
            float dx = 0f, dy = 0f, angle = 45f, timer = 1f;
            if (segs < 5) { IsRoutineRunning[execute] = false; yield break; }
            if (segs >= 5)
            {
                float.TryParse(commandLine[1], out dx);
                float.TryParse(commandLine[2], out dy);
                if (dx == 0f && dy == 0f) { IsRoutineRunning[execute] = false; yield break; }

                float.TryParse(commandLine[3], out angle);
                float.TryParse(commandLine[4], out timer);
                timer.MakeAbs();
                instant = timer.GetAbs() < Engine.DeltaTime;
            }

            Ease.Easer ease = Ease.Linear;
            if (segs >= 6)
            {
                ease = EaseUtils.StringToEase(commandLine[5].ToLower());
            }

            Vector2 obj;
            switch (execute)
            {
                case Command.Move_Around: obj = Position; break;
                case Command.Light_Movearound: obj = light.Position; break;
                case Command.Bloom_Movearound: obj = bloom.Position; break;
                default: obj = Position; break;
            }
            float targetAngle = angle * Calc.DegToRad;
            Vector2 start = obj, center = start + new Vector2(dx, dy),
                needle = start - center, progressVector = needle;
            if (instant)
            {
                obj = center + needle.Rotate(targetAngle);

                switch (execute)
                {
                    case Command.Move_Around: Position = obj; break;
                    case Command.Light_Movearound: light.Position = obj; break;
                    case Command.Bloom_Movearound: bloom.Position = obj; break;
                    default: Position = obj; break;
                }

                yield break;
            }
            float progress = 0f;
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                progressVector = needle.Rotate(targetAngle * ease(progress));
                obj = center + progressVector;

                switch (execute)
                {
                    case Command.Move_Around: Position = obj; break;
                    case Command.Light_Movearound: light.Position = obj; break;
                    case Command.Bloom_Movearound: bloom.Position = obj; break;
                    default: Position = obj; break;
                }

                yield return null;
            }
        }

        else if (execute == Command.Alpha)
        {
            // valid syntax: "alpha,0.3", "alpha,0.3,1", "alpha,0.3,1,linear"
            float alpha = 1f;
            if (segs <= 1) { IsRoutineRunning[execute] = false; yield break; }

            if (segs >= 2) { float.TryParse(commandLine[1], out alpha); }
            alpha = float.Clamp(alpha, 0f, 1f);

            float timer = 0.1f;
            if (segs >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (segs >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3].ToLower());
            }

            float progress = 0f;
            Color from = sprite.Color, to = from * alpha;
            if (instant) { sprite.Color = to; yield break; }
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                sprite.Color = Color.Lerp(from, to, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Color)
        {
            // valid syntax: "color,ffffff", "color,ffffff,0.1", "color,ffffff,0.1,sinein"
            if (segs <= 1) { IsRoutineRunning[execute] = false; yield break; }

            Color color = Color.White;
            if (segs >= 2) { color = Calc.HexToColor(commandLine[1]); }

            float timer = 0.1f;
            if (segs >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (segs >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3].ToLower());
            }

            float progress = 0f;
            Color from = sprite.Color;
            if (instant) { sprite.Color = color; yield break; }
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                sprite.Color = Color.Lerp(from, color, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Scale)
        {
            // valid syntax: "scale,1,1", "scale,1,1,1", "scale,1,1,1,linear"
            if (segs < 3) { IsRoutineRunning[execute] = false; yield break; }

            float scaleX = 1f, scaleY = 1f;
            if (segs >= 3)
            {
                float.TryParse(commandLine[1], out scaleX);
                float.TryParse(commandLine[2], out scaleY);
            }

            float timer = 0.1f;
            if (segs >= 4) { float.TryParse(commandLine[3], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (segs >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4].ToLower());
            }

            float progress = 0f;
            Vector2 from = sprite.Scale, to = new Vector2(scaleX, scaleY);
            if (instant) { sprite.Scale = to; yield break; }
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                sprite.Scale = Vector2.Lerp(from, to, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Rotate)
        {
            // valid syntax: "rotate, angle, (rotateTime, easing, isDelta)"
            float angle = 0f;
            if (segs < 2) { IsRoutineRunning[execute] = false; yield break; }

            if (segs >= 2) { float.TryParse(commandLine[1], out angle); }

            float timer = Engine.DeltaTime;
            if (segs >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (segs >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3].ToLower());
            }

            bool isDelta = false;
            if (segs >= 5) { bool.TryParse(commandLine[4], out isDelta); }

            float progress = 0f;
            float from = sprite.Rotation, to = isDelta ? sprite.Rotation + Calc.DegToRad * angle : Calc.DegToRad * angle;
            if (instant) { sprite.Rotation = to; yield break; }
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                sprite.Rotation = Calc.LerpSnap(from, to, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Depth)
        {
            // valid syntax: "depth, 9500, (1, linear)"
            int depth = 9500;
            if (segs <= 1) { IsRoutineRunning[execute] = false; yield break; }

            if (segs >= 2) { int.TryParse(commandLine[1], out depth); }

            float timer = 0.1f;
            if (segs >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (segs >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3].ToLower());
            }

            float progress = 0f;
            int from = Depth;
            if (instant) { base.Depth = depth; yield break; }
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                base.Depth = (int)Calc.LerpSnap(from, depth, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Origin)
        {
            // valid syntax: "origin, originX, originY, (changeTime, easing)"
            if (segs < 3) { IsRoutineRunning[execute] = false; yield break; }
            Vector2 origin = Vector2.One * 0.5f;
            if (segs >= 3)
            {
                float.TryParse(commandLine[1], out origin.X);
                float.TryParse(commandLine[2], out origin.Y);
            }
            float timer = Engine.DeltaTime;
            if (segs >= 4)
            {
                float.TryParse(commandLine[3], out timer);
            }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (segs >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4].ToLower());
            }

            Vector2 from = (Vector2)sprite.Justify;
            if (instant) { sprite.Justify = origin; yield break; }
            float progress = 0f;
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                sprite.Justify = Vector2.Lerp(from, origin, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Rate)
        {
            // valid syntax: "rate,rate,(changeTime,easing)"
            if (segs < 2) { IsRoutineRunning[execute] = false; yield break; }

            float rate = 1f;
            float.TryParse(commandLine[1], out rate);

            float timer = Engine.DeltaTime;
            if (segs >= 3)
            {
                float.TryParse(commandLine[2], out timer);
            }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (segs >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3].ToLower());
            }

            float progress = 0f, from = sprite.Rate;
            if (instant) { sprite.Rate = rate; yield break; }
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                sprite.Rate = Calc.LerpSnap(from, rate, ease(progress), 0.01f);

                yield return null;
            }
        }

        else if (execute == Command.Light)
        {
            // valid syntax: "light, color, alpha, startFade, endFade, (time, easing)"
            if (segs < 5) { yield break; }

            Color color =  Color.White; 
            float alpha = 0f, timer = 0f; int startFade = 32, endFade = 64;  Ease.Easer ease = Ease.Linear;
            color = Calc.HexToColor(commandLine[1]);
            float.TryParse(commandLine[2], out alpha); float.Clamp(alpha, 0f, 1f);
            int.TryParse(commandLine[3], out startFade); startFade.MakeAbs();
            int.TryParse(commandLine[4], out endFade); endFade.MakeAbs();

            if (segs >= 6)
            {
                float.TryParse(commandLine[5], out timer); timer.MakeAbs();
                instant = timer == 0f;
            }
            if (segs >= 7)
            {
                ease = EaseUtils.StringToEase(commandLine[6].ToLower());
            }

            if (instant)
            {
                light = new VertexLight(color, alpha, startFade, endFade); yield break;
            }

            float progress = 0f;
            Color color0 = light.Color; 
            float alpha0 = light.Alpha; 
            float startFade0 = light.StartRadius, endFade0 = light.EndRadius;
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                light.Color = Color.Lerp(color0, color, ease(progress));
                light.Alpha = Calc.LerpSnap(alpha0, alpha, ease(progress), 0.01f);
                light.StartRadius = Calc.LerpSnap(startFade0, (float)startFade, ease(progress));
                light.EndRadius = Calc.LerpSnap(endFade0, (float)endFade, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Bloom)
        {
            // valid syntax: "bloom, alpha, radius, (time, easing)"
            if (segs < 3) { yield break; }

            float alpha = 0f, r = 0f, timer = 0f; Ease.Easer ease = Ease.Linear;
            float.TryParse(commandLine[1], out alpha); float.Clamp(alpha, 0f, 1f);
            float.TryParse(commandLine[2], out r); r.MakeAbs();

            if (segs >= 4)
            {
                float.TryParse(commandLine[3], out timer); timer.MakeAbs();
                instant = timer == 0f;
            }
            if (segs >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4].ToLower());
            }

            if (instant)
            {
                bloom = new BloomPoint(alpha, r); yield break;
            }

            float progress = 0f;
            float alpha0 = bloom.Alpha, r0 = bloom.Radius;
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                bloom.Alpha = Calc.LerpSnap(alpha0, alpha, ease(progress), 0.01f);
                bloom.Radius = Calc.LerpSnap(r0, r, ease(progress));

                yield return null;
            }
        }

        else if (execute == Command.Parallax)
        {
            // valid syntax: "parallax, value, (timer, easing)"
            if (segs < 2) { yield break; }

            float set = parallax;
            float.TryParse(commandLine[1], out set);

            float timer = 0f;
            if (segs >= 3)
            {
                float.TryParse(commandLine[2], out timer);
                timer.MakeAbs();
                instant = timer == 0f;
            }

            Ease.Easer ease = Ease.Linear;
            if (segs >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3]);
            }

            if (instant)
            {
                parallax = set;
                yield break;
            }

            float progress = 0f;
            float from = parallax, to = set;
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                parallax = Calc.LerpSnap(from,to, ease(progress), 0.001f);

                yield return null;
            }
        }

        else if (execute == Command.Render_Position_InRoom)
        {
            // valid syntax: "render_position_in_room, camX, camY, (timer, easing)"
            if (segs < 3) { yield break; }

            Vector2 camPos = new Vector2(camX, camY);
            float.TryParse(commandLine[1], out camPos.X);
            float.TryParse(commandLine[2], out camPos.Y);

            float timer = 0f;
            Ease.Easer ease = Ease.Linear;
            if (segs >= 4)
            {
                float.TryParse(commandLine[3], out timer);
                timer.MakeAbs();
                instant = timer == 0f;
            }
            if (segs >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4]);
            }

            if (instant)
            {
                camX = camPos.X; camY = camPos.Y; 
                yield break;
            }

            float progress = 0f;
            Vector2 from = new Vector2(camX, camY), to = camPos, pointer = from;
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                pointer = Vector2.Lerp(from, to, ease(progress));
                camX = pointer.X;
                camY = pointer.Y;

                yield return null;
            }
        }

        else if (execute == Command.Camera_Offset)
        {
            // syntax: "camera_offset, offsetX, offsetY, (changeTime, ease)"
            Vector2 offset = MapProcessor.level.CameraOffset;
            float offsetX = offset.X, offsetY = offset.Y, timer = 0f; Ease.Easer ease = Ease.Linear;

            if (segs < 3) { yield break; }
            float.TryParse(commandLine[1], out offsetX);
            float.TryParse(commandLine[2], out offsetY);

            if (segs >= 4)
            {
                float.TryParse(commandLine[3], out timer);
                timer.MakeAbs();
                instant = timer == 0f;
            }

            if (segs >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4]);
            }

            if (instant)
            {
                MapProcessor.level.CameraOffset = new Vector2(offsetX, offsetY);
                yield break;
            }

            float progress = 0f;
            Vector2 from = offset, to = new Vector2(offsetX, offsetY);
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                MapProcessor.level.CameraOffset = Vector2.Lerp(from, to, ease(progress));

                yield return null;
            }
        }

        IsRoutineRunning[execute] = false;
    }

    private void DebugInfo()
    {
        
    }

    public override void Render()
    {
        base.Render();
    }
    public override void Update()
    {
        base.Update();

        DebugInfo();

        Vector2 camPos = MapProcessor.level.Camera.Position;
        Vector2 center = camPos + new Vector2(camX, camY);
        sprite.RenderPosition = center + (Position - center) * parallax;

        solid.Position = Position + solidPos;
    }

    #region Holdable setups

    public void OnPickup()
    {
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
    }

    public void OnRelease(Vector2 force)
    {
        RemoveTag(Tags.Persistent);
        if (force.X != 0f && force.Y == 0f)
        {
            force.Y = -0.4f;
        }

        Speed = force * 200f;
        //if (Speed != Vector2.Zero)
        //{
        //    noGravityTimer = 0.1f;
        //}
    }

    public bool HitSpring(Spring spring)
    {
        if (!holdable.IsHeld)
        {
            if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
            {
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                //noGravityTimer = 0.15f;
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = 220f;
                Speed.Y = -80f;
                //noGravityTimer = 0.1f;
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = -220f;
                Speed.Y = -80f;
                //noGravityTimer = 0.1f;
                return true;
            }
        }

        return false;
    }

    public void HitSeeker(Seeker seeker)
    {
        if (!holdable.IsHeld)
        {
            Speed = (base.Center - seeker.Center).SafeNormalize(120f);
        }

        //Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
    }

    #endregion
}

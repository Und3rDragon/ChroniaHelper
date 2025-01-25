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
        Ignore,//done
        Sound, Music, Hitbox, Holdable_Collider//undone
    }
    private Command execute = Command.None;
    public Dictionary<string, Command> GetCommand = new Dictionary<string, Command>()
    {
        { "set_flag", Command.Set_Flag }, 
        { "setflag", Command.Set_Flag },
        { "moveto", Command.MoveTo },
        { "move", Command.Move },
        { "play", Command.Play },
        { "flag_play", Command.Flag_Play },
        { "flagplay", Command.Flag_Play },
        { "wait", Command.Wait },
        { "wait_flag", Command.Wait_Flag },
        { "waitflag", Command.Wait_Flag },
        { "alpha", Command.Alpha },
        { "color", Command.Color },
        { "scale", Command.Scale },
        { "rotate", Command.Rotate },
        { "depth", Command.Depth },
        { "repeat", Command.Repeat },
        { "move_around", Command.Move_Around },
        { "movearound", Command.Move_Around },
        { "origin", Command.Origin },
        { "rate", Command.Rate },
        { "ignore", Command.Ignore },
        { "sound", Command.Sound },
        { "music", Command.Music },
        { "hitbox", Command.Hitbox },
        { "holdable_collider", Command.Holdable_Collider },
        { "holdablecollider", Command.Holdable_Collider },
    };

    public Dictionary<Command, bool> IsRoutineRunning = new Dictionary<Command, bool>();

    public List<Command> NoSideRoutines = new List<Command>() {
        Command.None, Command.Set_Flag, Command.Play, Command.Flag_Play,
        Command.Wait, Command.Wait_Flag, Command.Repeat, Command.Ignore, Command.Sound,Command.Music,
        Command.Hitbox, Command.Holdable_Collider
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

        Add(holdable = new Holdable(0.3f));
        holdable.SpeedGetter = () => Speed;
        holdable.OnPickup = OnPickup;
        holdable.OnRelease = OnRelease;
        holdable.OnHitSpring = HitSpring;
        holdable.OnHitSeeker = HitSeeker;
    }
    private string spriteName;
    private Sprite sprite;
    private string[] commands;
    private SoundSource sfx;
    private Holdable holdable;

    public override void Added(Scene scene)
    {
        foreach (var command in Enum.GetValues<Command>())
        {
            IsRoutineRunning.Add(command, false);
        }

        base.Added(scene);
        
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

            string[] commandLine = commands[index].Split(',', StringSplitOptions.TrimEntries);
            if (commandLine.Length == 0) { continue; }
            string indicator = commandLine[0].ToLower().Trim();
            bool sideRoutine = indicator.StartsWith("passby ");
            indicator = sideRoutine ? indicator.RemoveFirst("passby ") : indicator;
            execute = GetCommand.Keys.Contains(indicator) ? GetCommand[indicator] : Command.None;

            if (execute == Command.Repeat)
            {
                // valid syntax: "repeat, 0, overrideFlag"
                if (commandLine.Length < 3) { continue; }

                int newIndex = int.TryParse(commandLine[1], out newIndex) ? newIndex : 0;
                newIndex.MakeAbs();
                string overrideFlag = commandLine[2];

                if (!MapProcessor.session.GetFlag(overrideFlag))
                {
                    index = newIndex - 1;
                }
            }
            else if (execute == Command.Ignore)
            {
                // valid syntax: "ignore, ignoreFlag, inverted, num1,(num2,num3)"
                if (commandLine.Length < 4) { continue; }

                string flag = string.Empty; bool inverted = false;
                List<int> indexes = new List<int>();
                if (commandLine.Length >= 4)
                {
                    flag = commandLine[1];
                    bool.TryParse(commandLine[2], out inverted);
                    for (int i = 3; i < commandLine.Length; i++)
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
                if (commandLine.Length < 2) { continue; }
                
                sfx.Play(commandLine[1]);
            }
            else if (execute == Command.Music)
            {
                // valid syntax: "music, eventName"
                if (commandLine.Length < 2) { continue; }

                MapProcessor.session.Audio.Music.Event = SFX.EventnameByHandle(commandLine[1]);
            }
            else if (execute == Command.Hitbox)
            {
                // valid syntax: "hitbox, width, height, (x, y)"
                if (commandLine.Length < 3) { continue; }
                float width = 16f, height = 16f, x = -8f, y = -8f;
                float.TryParse(commandLine[1], out width);
                float.TryParse(commandLine[2], out height);
                width.MakeAbs(); height.MakeAbs();

                if (commandLine.Length >= 4)
                {
                    float.TryParse(commandLine[3], out x);
                }
                if(commandLine.Length >= 5)
                {
                    float.TryParse(commandLine[4], out y);
                }

                base.Collider = new Hitbox(width, height, x, y);
            }
            else if (execute == Command.Holdable_Collider)
            {
                // valid syntax: "hitbox, width, height, (x, y)"
                if (commandLine.Length < 3) { continue; }
                float width = 16f, height = 16f, x = -8f, y = -8f;
                float.TryParse(commandLine[1], out width);
                float.TryParse(commandLine[2], out height);
                width.MakeAbs(); height.MakeAbs();

                if (commandLine.Length >= 4)
                {
                    float.TryParse(commandLine[3], out x);
                }
                if (commandLine.Length >= 5)
                {
                    float.TryParse(commandLine[4], out y);
                }

                holdable.PickupCollider = new Hitbox(width, height, x, y);
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

        bool instant = false;

        if (execute == Command.Set_Flag)
        {
            string flagName = string.Empty; bool flagValue = true;
            // valid syntax: "setflag,flagName,value"
            if (commandLine.Length == 1) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 2) { flagName = commandLine[1]; }
            if (commandLine.Length >= 3) { bool.TryParse(commandLine[2], out flagValue); }

            MapProcessor.session.SetFlag(flagName, flagValue);
        }
        else if (execute == Command.Play)
        {
            string playSprite = string.Empty;
            // valid syntax: "play,spriteName"
            if (commandLine.Length == 1) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 2) { playSprite = commandLine[1]; }

            sprite.Play(playSprite);
        }
        else if (execute == Command.Flag_Play)
        {
            string playSprite = string.Empty, flag = string.Empty;
            // valid syntax: "flagplay,flag,spriteName,(inverted)"
            if (commandLine.Length < 3) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 3) { flag = commandLine[1]; playSprite = commandLine[2]; }
            bool inverted = false;
            if (commandLine.Length >= 4) { bool.TryParse(commandLine[3], out inverted); }

            if (inverted && MapProcessor.session.GetFlag(flag))
            {
                IsRoutineRunning[execute] = false; yield break;
            }
            if (!inverted && !MapProcessor.session.GetFlag(flag))
            {
                IsRoutineRunning[execute] = false; yield break;
            }
            sprite.Play(playSprite);
        }
        else if (execute == Command.Wait_Flag)
        {
            // valid syntax: "waitflag,flag", "waitflag, flag, true"
            string flag = string.Empty;
            if (commandLine.Length < 2) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 2) { flag = commandLine[1]; }
            bool inverted = false;
            if (commandLine.Length >= 3) { bool.TryParse(commandLine[2], out inverted); }

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
            if (commandLine.Length <= 1) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 2)
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
        else if (execute == Command.Move)
        {
            // valid syntax: "move,3,5", "move,3,5,1", "move,3,5,1,cubein"
            float dx = 0f, dy = 0f, timer = 1f;
            if (commandLine.Length < 3) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 3)
            {
                float.TryParse(commandLine[1], out dx);
                float.TryParse(commandLine[2], out dy);
            }
            if (commandLine.Length >= 4)
            {
                float.TryParse(commandLine[3], out timer);
                timer.MakeAbs();
                instant = timer.GetAbs() < Engine.DeltaTime;
            }
            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4].ToLower());
            }

            Vector2 pos1 = Position, pos2 = pos1 + new Vector2(dx, dy);
            if (instant) { Position = pos2; yield break; }
            float percent = 0f;
            while (percent < 1f)
            {
                percent = Calc.Approach(percent, 1f, Engine.DeltaTime / timer);
                Position = Vector2.Lerp(pos1, pos2, ease(percent));

                yield return null;
            }
        }
        else if (execute == Command.MoveTo)
        {
            // valid syntax: "moveto,3,5", "moveto,3,5,1", "moveto,3,5,1,cubein"
            float x = 0f, y = 0f, timer = 1f;
            if (commandLine.Length < 3) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 3)
            {
                float.TryParse(commandLine[1], out x);
                float.TryParse(commandLine[2], out y);
            }
            if (commandLine.Length >= 4)
            {
                float.TryParse(commandLine[3], out timer);
                timer.MakeAbs();
                instant = timer.GetAbs() < Engine.DeltaTime;
            }
            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 5)
            {
                ease = EaseUtils.StringToEase(commandLine[4].ToLower());
            }

            Vector2 pos1 = Position, pos2 = new Vector2(x, y);
            if (instant) { Position = pos2; yield break; }
            float percent = 0f;
            while (percent < 1f)
            {
                percent = Calc.Approach(percent, 1f, Engine.DeltaTime / timer);
                Position = Vector2.Lerp(pos1, pos2, ease(percent));

                yield return null;
            }
        }
        else if (execute == Command.Alpha)
        {
            // valid syntax: "alpha,0.3", "alpha,0.3,1", "alpha,0.3,1,linear"
            float alpha = 1f;
            if (commandLine.Length <= 1) { IsRoutineRunning[execute] = false; yield break; }

            if (commandLine.Length >= 2) { float.TryParse(commandLine[1], out alpha); }
            alpha = float.Clamp(alpha, 0f, 1f);

            float timer = 0.1f;
            if (commandLine.Length >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 4)
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
            if (commandLine.Length <= 1) { IsRoutineRunning[execute] = false; yield break; }

            Color color = Color.White;
            if (commandLine.Length >= 2) { color = Calc.HexToColor(commandLine[1]); }

            float timer = 0.1f;
            if (commandLine.Length >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 4)
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
            if (commandLine.Length < 3) { IsRoutineRunning[execute] = false; yield break; }

            float scaleX = 1f, scaleY = 1f;
            if (commandLine.Length >= 3)
            {
                float.TryParse(commandLine[1], out scaleX);
                float.TryParse(commandLine[2], out scaleY);
            }

            float timer = 0.1f;
            if (commandLine.Length >= 4) { float.TryParse(commandLine[3], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 5)
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
            if (commandLine.Length < 2) { IsRoutineRunning[execute] = false; yield break; }

            if (commandLine.Length >= 2) { float.TryParse(commandLine[1], out angle); }

            float timer = Engine.DeltaTime;
            if (commandLine.Length >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3].ToLower());
            }

            bool isDelta = false;
            if (commandLine.Length >= 5) { bool.TryParse(commandLine[4], out isDelta); }

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
            if (commandLine.Length <= 1) { IsRoutineRunning[execute] = false; yield break; }

            if (commandLine.Length >= 2) { int.TryParse(commandLine[1], out depth); }

            float timer = 0.1f;
            if (commandLine.Length >= 3) { float.TryParse(commandLine[2], out timer); }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 4)
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
        else if (execute == Command.Move_Around)
        {
            // valid syntax: "movearound, center-dx, center-dy, deltaAngle, spinTime, (easing)"
            float dx = 0f, dy = 0f, angle = 45f, timer = 1f;
            if (commandLine.Length < 5) { IsRoutineRunning[execute] = false; yield break; }
            if (commandLine.Length >= 5)
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
            if (commandLine.Length >= 6)
            {
                ease = EaseUtils.StringToEase(commandLine[5].ToLower());
            }

            float targetAngle = angle * Calc.DegToRad;
            Vector2 start = Position, center = start + new Vector2(dx, dy),
                needle = start - center, progressVector = needle;
            if (instant) { Position = center + needle.Rotate(targetAngle); yield break; }
            float progress = 0f;
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                progressVector = needle.Rotate(targetAngle * ease(progress));
                Position = center + progressVector;

                yield return null;
            }
        }
        else if (execute == Command.Origin)
        {
            // valid syntax: "origin, originX, originY, (changeTime, easing)"
            if (commandLine.Length < 3) { IsRoutineRunning[execute] = false; yield break; }
            Vector2 origin = Vector2.One * 0.5f;
            if (commandLine.Length >= 3)
            {
                float.TryParse(commandLine[1], out origin.X);
                float.TryParse(commandLine[2], out origin.Y);
            }
            float timer = Engine.DeltaTime;
            if (commandLine.Length >= 4)
            {
                float.TryParse(commandLine[3], out timer);
            }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 5)
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
            if (commandLine.Length < 2) { IsRoutineRunning[execute] = false; yield break; }

            float rate = 1f;
            float.TryParse(commandLine[1], out rate);

            float timer = Engine.DeltaTime;
            if (commandLine.Length >= 3)
            {
                float.TryParse(commandLine[2], out timer);
            }
            timer.MakeAbs();
            instant = timer.GetAbs() < Engine.DeltaTime;

            Ease.Easer ease = Ease.Linear;
            if (commandLine.Length >= 4)
            {
                ease = EaseUtils.StringToEase(commandLine[3].ToLower());
            }

            float progress = 0f, from = sprite.Rate;
            if (instant) { sprite.Rate = rate; yield break; }
            while (progress < 1f)
            {
                progress = Calc.Approach(progress, 1f, Engine.DeltaTime / timer);
                sprite.Rate = Calc.LerpSnap(from, rate, ease(progress));

                yield return null;
            }
        }

        IsRoutineRunning[execute] = false;
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

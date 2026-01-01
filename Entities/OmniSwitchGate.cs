using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using YamlDotNet.Core.Tokens;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/OmniSwitchGate")]
public class OmniSwitchGate : Solid
{
    private readonly Vector2[] nodes;
    private readonly string flag;
    private readonly bool canReturn;
    private readonly float shakeTime;
    private readonly float[] moveTime;
    private readonly float[] moveTime2;
    private readonly Ease.Easer[] easers;
    private readonly Ease.Easer[] easers2;
    private readonly CColor inactiveColor;
    private readonly CColor activeColor;
    private readonly CColor finishColor;
    private readonly float[] pauseTimes;
    private readonly float[] pauseTimes2; 
    private readonly bool smoke;
    private readonly string moveSound;
    private readonly string finishedSound;
    private readonly string shatterSound, shatterShakeSound;
    private readonly string shatterDebris;

    private Sprite icon;
    private MTexture texture;
    private Vector2 iconOffset;
    private Wiggler wiggler;
    private SoundSource openSfx;
    
    private bool persistent;

    private float beforeShatter = -1f;

    // indicators
    private bool moving;
    private bool cancelMoving;
    private int targetNodeIndex;
    private int currentNodeIndex;
    private bool shattered = false;

    public OmniSwitchGate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
    {
        // parse all options
        nodes = data.NodesWithPosition(offset);
        flag = data.Attr("flag");
        canReturn = data.Bool("canReturn", true);
        shakeTime = data.Float("shakeTime", 0.5f);
        
        data.Attr("moveTime").Split(',',StringSplitOptions.TrimEntries).ApplyTo(out string[] t1);
        List<float> _t1 = new();
        for(int i = 0; i < t1.Length; i++)
        {
            _t1.Add(t1[i].ParseFloat(0.5f).GetAbs());
        }
        moveTime = _t1.ToArray();

        data.Attr("returnMoveTime").Split(',', StringSplitOptions.TrimEntries).ApplyTo(out string[] t2);
        List<float> _t2 = new();
        for (int i = 0; i < t2.Length; i++)
        {
            _t2.Add(t2[i].ParseFloat(2f).GetAbs());
        }
        moveTime2 = _t2.ToArray();

        data.Attr("pauseTime").Split(',', StringSplitOptions.TrimEntries).ApplyTo(out string[] t3);
        List<float> _t3 = new();
        for (int i = 0; i < t3.Length; i++)
        {
            _t3.Add(t3[i].ParseFloat(0.2f).GetAbs());
        }
        pauseTimes = _t3.ToArray();

        data.Attr("returnPauseTime").Split(',', StringSplitOptions.TrimEntries).ApplyTo(out string[] t4);
        List<float> _t4 = new();
        for (int i = 0; i < t4.Length; i++)
        {
            _t4.Add(t4[i].ParseFloat(0.2f).GetAbs());
        }
        pauseTimes2 = _t4.ToArray();

        data.Attr("easers", "CubeOut").Split(',',StringSplitOptions.TrimEntries).ApplyTo(out string[] e);
        List<Ease.Easer> _e = new();
        for(int i = 0; i < e.Length; i++)
        {
            _e.Add(EaseUtils.StringToEase(e[i]));
        }
        easers = _e.ToArray();
        
        data.Attr("returnEasers", "CubeOut").Split(',', StringSplitOptions.TrimEntries).ApplyTo(out string[] e2);
        List<Ease.Easer> _e2 = new();
        for (int i = 0; i < e2.Length; i++)
        {
            _e2.Add(EaseUtils.StringToEase(e2[i]));
        }
        easers2 = _e2.ToArray();

        inactiveColor = data.GetChroniaColor("inactiveColor", Calc.HexToColor("5fcde4"));
        activeColor = data.GetChroniaColor("activeColor", Color.White);
        finishColor = data.GetChroniaColor("finishColor", Calc.HexToColor("f141df"));
        string iconName = data.Attr("icon", "objects/switchgate/icon");

        smoke = data.Bool("smoke", true);

        moveSound = data.Attr("moveSound", defaultValue: "event:/game/general/touchswitch_gate_open");
        finishedSound = data.Attr("finishedSound", defaultValue: "event:/game/general/touchswitch_gate_finish");
        shatterSound = data.Attr("shatterShakeSound", "event:/game/general/fallblock_shake");
        shatterSound = data.Attr("shatterSound", "event:/game/general/wall_break_stone");
        
        // initialize the icon
        icon = new Sprite(GFX.Game, iconName);
        icon.Add("spin", "", 0.1f, "spin");
        icon.Play("spin");
        icon.Rate = 0f;
        icon.Color = inactiveColor.Parsed();
        icon.Position = (iconOffset = new Vector2(Width / 2f, Height / 2f));
        icon.CenterOrigin();
        Add(icon);

        // initialize the gate texture
        string blockSpriteName = data.Attr("sprite", "block");
        texture = GFX.Game["objects/switchgate/" + blockSpriteName];

        // initialize other components
        Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f) {
            icon.Scale = Vector2.One * (1f + f);
        }));
        Add(openSfx = new SoundSource());
        Add(new LightOcclude(0.5f));

        persistent = data.Bool("persistent", true);

        currentNodeIndex = 0;
        targetNodeIndex = 1;

        beforeShatter = data.Float("beforeShatterTime", -1f);

        shatterDebris = data.Attr("shatterDebris", "debris/9");

        shattered = false;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (flag.GetFlag() && persistent)
        {
            currentNodeIndex = nodes.Length - 1;
            targetNodeIndex = nodes.Length - 2;
            MoveTo(nodes.Last());
            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
            icon.Color = finishColor.Parsed();
        }

        Add(new Coroutine(activator()));
    }
    
    public override void Render()
    {
        if (!Position.InView(new Vc2(Width, Height))) return;

        int widthInTiles = (int)Collider.Width / 8 - 1;
        int heightInTiles = (int)Collider.Height / 8 - 1;

        Vector2 renderPos = new Vector2(Position.X + Shake.X, Position.Y + Shake.Y);
        Texture2D baseTexture = texture.Texture.Texture;
        int clipBaseX = texture.ClipRect.X;
        int clipBaseY = texture.ClipRect.Y;

        Rectangle clipRect = new Rectangle(clipBaseX, clipBaseY, 8, 8);

        for (int i = 0; i <= widthInTiles; i++)
        {
            clipRect.X = clipBaseX + ((i < widthInTiles) ? i == 0 ? 0 : 8 : 16);
            for (int j = 0; j <= heightInTiles; j++)
            {
                int tilePartY = (j < heightInTiles) ? j == 0 ? 0 : 8 : 16;
                clipRect.Y = tilePartY + clipBaseY;
                Draw.SpriteBatch.Draw(baseTexture, renderPos, clipRect, Color.White);
                renderPos.Y += 8f;
            }
            renderPos.X += 8f;
            renderPos.Y = Position.Y + Shake.Y;
        }

        icon.Position = iconOffset + Shake;
        icon.DrawOutline();

        base.Render();
    }

    public override void Update()
    {
        base.Update();
    }
    
    private IEnumerator activator()
    {
        while (true)
        {
            bool cond1 = currentNodeIndex == 0 && !flag.GetFlag();
            bool cond2 = currentNodeIndex == nodes.MaxIndex() && flag.GetFlag();
            bool cond3 = !flag.GetFlag() && !canReturn;
            while (cond1 || cond2 || cond3)
            {
                cond1 = currentNodeIndex == 0 && !flag.GetFlag();
                cond2 = currentNodeIndex == nodes.MaxIndex() && flag.GetFlag();
                cond3 = !flag.GetFlag() && !canReturn;
                
                yield return null;
            }

            if (shattered) { yield break; }
            
            yield return new SwapImmediately(sequence(nodes[targetNodeIndex]));
        }
    }
    
    private IEnumerator sequence(Vector2 node)
    {
        while (moving)
        {
            // cancel the current move, and wait for the move to be effectively cancelled
            cancelMoving = true;
            yield return null;
        }
        cancelMoving = false;
        moving = true;

        Vector2 start = nodes[currentNodeIndex];
        wiggler.Stop();
        icon.Scale = Vector2.One;

        if (node != start)
        {
            if (cancelMoving)
            {
                moving = false;
                yield break;
            }

            openSfx.Play(moveSound);

            // shake
            if (shakeTime > 0f)
            {
                StartShaking(shakeTime);
                while (icon.Rate < 1f)
                {
                    var lastColor = icon.Color;
                    icon.Color = Color.Lerp(lastColor, activeColor.Parsed(), icon.Rate);
                    icon.Rate += Engine.DeltaTime / shakeTime;
                    yield return null;
                }
            }
            else
            {
                icon.Rate = 1f;
                icon.Color = activeColor.Parsed();
            }

            if (cancelMoving)
            {
                moving = false;
                yield break;
            }

            bool forward = targetNodeIndex >= currentNodeIndex;

            // move and emit particles
            int particleAt = 0;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, 
                forward? easers.TryGet(currentNodeIndex, Ease.CubeOut) : easers2.TryGet(nodes.MaxIndex() - currentNodeIndex, Ease.CubeOut), 
                forward? moveTime.TryGet(currentNodeIndex, 0.5f) : moveTime2.TryGet(nodes.MaxIndex() - currentNodeIndex, 2f), 
                start: true);
            bool waiting = true;
            tween.OnUpdate = delegate (Tween t) {
                MoveTo(Vector2.Lerp(start, node, t.Eased));
                
                if (Scene.OnInterval(0.1f))
                {
                    particleAt++;
                    particleAt %= 2;
                    for (int x = 0; (float)x < Width / 8f; x++)
                    {
                        for (int y = 0; (float)y < Height / 8f; y++)
                        {
                            if ((x + y) % 2 == particleAt)
                            {
                                SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind, Position + new Vector2(x * 8, y * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                            }
                        }
                    }
                }
            };
            tween.OnComplete = (t) => { waiting = false; };
            Add(tween);

            // wait for the move to be done.
            while (waiting)
            {
                if (cancelMoving)
                {
                    tween.Stop();
                    Remove(tween);
                    moving = false;
                    yield break;
                }
                yield return null;
            }
            Remove(tween);


            bool wasCollidable = Collidable;
            // collide dust particles on the left
            if (node.X <= start.X)
            {
                Vector2 add = new Vector2(0f, 2f);
                for (int tileY = 0; tileY < Height / 8f; tileY++)
                {
                    Vector2 collideAt = new Vector2(Left - 1f, Top + 4f + (tileY * 8));
                    Vector2 noCollideAt = collideAt + Vector2.UnitX;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, (float)Math.PI);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, (float)Math.PI);
                    }
                }
            }

            // collide dust particles on the rigth
            if (node.X >= start.X)
            {
                Vector2 add = new Vector2(0f, 2f);
                for (int tileY = 0; tileY < Height / 8f; tileY++)
                {
                    Vector2 collideAt = new Vector2(Right + 1f, Top + 4f + (tileY * 8));
                    Vector2 noCollideAt = collideAt - Vector2.UnitX * 2f;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, 0f);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, 0f);
                    }
                }
            }

            // collide dust particles on the top
            if (node.Y <= start.Y)
            {
                Vector2 add = new Vector2(2f, 0f);
                for (int tileX = 0; tileX < Width / 8f; tileX++)
                {
                    Vector2 collideAt = new Vector2(Left + 4f + (tileX * 8), Top - 1f);
                    Vector2 noCollideAt = collideAt + Vector2.UnitY;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, -(float)Math.PI / 2f);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, -(float)Math.PI / 2f);
                    }
                }
            }

            // collide dust particles on the bottom
            if (node.Y >= start.Y)
            {
                Vector2 add = new Vector2(2f, 0f);
                for (int tileX = 0; tileX < Width / 8f; tileX++)
                {
                    Vector2 collideAt = new Vector2(Left + 4f + (tileX * 8), Bottom + 1f);
                    Vector2 noCollideAt = collideAt - Vector2.UnitY * 2f;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, (float)Math.PI / 2f);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, (float)Math.PI / 2f);
                    }
                }
            }
            Collidable = wasCollidable;

            Audio.Play(finishedSound, Position);

            // shake after arriving at destination
            StartShaking(0.2f);
            while (icon.Rate > 0f && !cancelMoving)
            {
                icon.Color = Color.Lerp(activeColor.Parsed(), targetNodeIndex == nodes.MaxIndex() ? finishColor.Parsed() : inactiveColor.Parsed(), 1f - icon.Rate);
                icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
            }
            if (cancelMoving)
            {
                moving = false;
                yield break;
            }

            icon.Rate = 0f;
            icon.SetAnimationFrame(0);

            // animate the icon with particles
            wiggler.Start();
            wasCollidable = Collidable;
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(Center) && smoke)
            {
                for (int i = 0; i < 32; i++)
                {
                    float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                    SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, Position + iconOffset + Calc.AngleToVector(angle, 4f), angle);
                }
            }
            Collidable = wasCollidable;

            currentNodeIndex = targetNodeIndex;
        }
        else
        {
            // we are "moving" without changing positions: just animate the icon.
            icon.Rate = 1f;
            while (icon.Rate > 0f && !cancelMoving)
            {
                var lastColor = icon.Color;
                icon.Color = Color.Lerp(lastColor, targetNodeIndex > 0 ? finishColor.Parsed() : inactiveColor.Parsed(), 1f - icon.Rate);
                icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
            }
            icon.Color = finishColor.Parsed();
            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
        }

        // check if the block should continue moving, in case it is configured not to skip nodes.
        if(currentNodeIndex < nodes.MaxIndex() && flag.GetFlag())
        {
            targetNodeIndex = currentNodeIndex + 1;
        }
        if(currentNodeIndex > 0 && !flag.GetFlag() && canReturn)
        {
            targetNodeIndex = currentNodeIndex - 1;
        }
        
        if (currentNodeIndex != targetNodeIndex)
        {
            // wait at position for the configured time.
            bool forward = targetNodeIndex >= currentNodeIndex;
            float delay = forward ? pauseTimes.TryGet(currentNodeIndex, 0.2f) : pauseTimes2.TryGet(nodes.MaxIndex() - currentNodeIndex, 0.2f);
            while (delay > 0f && !cancelMoving)
            {
                delay -= Engine.DeltaTime;
                yield return null;
            }

            if (cancelMoving)
            {
                moving = false;
                yield break;
            }

            // then move to the next node.
        }
        
        if(currentNodeIndex == nodes.MaxIndex() && beforeShatter > 0f)
        {
            yield return shatterSequence(beforeShatter);
        }

        moving = false;
    }

    // this is heavily inspired by Vortex Helper by catapillie, and also requires Vortex Helper to fully work.
    private IEnumerator shatterSequence(float shatterDelay)
    {
        //if (!Md.VortexHelperLoaded)
        //{
        //    // error postcards are nicer than crashes!
        //    Audio.SetMusic(null);
        //    LevelEnter.ErrorMessage = "{big}Oops!{/big}{n}To use {# F94A4A}Shatter Flag Switch Gates{#}, you need to have {# d678db}Vortex Helper{#} installed!";
        //    LevelEnter.Go(new Session(SceneAs<Level>().Session.Area), fromSaveData: false);
        //    yield break;
        //}
        
        openSfx.Play(shatterShakeSound);
        
        for (int k = 0; k < 32; k++)
        {
            float num = Calc.Random.NextFloat((float)Math.PI * 2f);
            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, Position + iconOffset + Calc.AngleToVector(num, 4f), num);
        }
        openSfx.Stop();

        yield return shatterDelay;
        
        Audio.Play(shatterSound, Center);
        Audio.Play(finishedSound, Center);

        for (int i = 0; i < Width / 8f; i++)
        {
            for (int j = 0; j < Height / 8f; j++)
            {
                Debris debris = new Debris().orig_Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '1').BlastFrom(Center);
                DynData<Debris> debrisData = new DynData<Debris>(debris);
                debrisData.Get<Image>("image").Texture = GFX.Game[shatterDebris];
                Scene.Add(debris);
            }
        }
        DestroyStaticMovers();
        RemoveSelf();

        shattered = true;
    }
}

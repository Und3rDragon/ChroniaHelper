using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Runtime.InteropServices;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities {

    // adaptable switch gate base thing *HEAVILY* based on and inspired by flag switch gates from maddie's helping hand
    // code is probably a bit different now due to me changing a bunch to make it more consistent with the other stuff in this mod/my current coding style and to help myself understand it more, but ultimately it is copied from there
    // https://github.com/maddie480/MaddieHelpingHand/blob/master/Entities/FlagSwitchGate.cs

    // not meant to be used as an entity by itself, but rather as a base for more interesting variations
    // originally made around early 2022 as an excuse to better understand inheritance/polymorphism/virtual methods/whatever its called and to mess around with celeste modding in general more

    // this code along with the two variations of gate blocks are all from Sorbet Helper thanks to the open-sourced codes
    // thanks to the entity concept I can modify it and adding up some features I expected from it when the features I require could be absurd

    [Tracked(true)]
    public class GateBlock : Solid {
        public bool Triggered { get; private set; }

        protected readonly int entityId;
        protected Vector2 start;
        protected Vector2[] nodes;

        protected bool atNode;
        protected Color fillColor;
        protected Vector2 scale = Vector2.One;
        protected Vector2 offset;
        public Vector2 Scale => scale;
        public Vector2 Offset => offset + Shake;

        protected readonly Sprite icon;
        protected readonly Vector2 iconOffset;
        protected readonly Wiggler finishIconScaleWiggler;
        protected readonly SoundSource openSfx;

        protected readonly bool smoke;
        protected readonly Color inactiveColor;
        protected readonly Color activeColor;
        protected readonly Color finishColor;
        protected readonly string moveSound;
        protected readonly string finishedSound;

        protected readonly float shakeTime;
        private float moveTime;
        protected readonly bool moveEased;
        protected readonly bool allowReturn;
        protected readonly bool persistent;
        protected readonly string onActivateFlag;

        public bool VisibleOnCamera { get; private set; } = true;

        protected readonly ParticleType P_RecoloredFire;
        protected readonly ParticleType P_Activate;
        protected readonly ParticleType P_ActivateReturn;

        private bool collidableBackup;
        public GateBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false) {
            //if (data.Nodes.Length > 0) {
            //    node = data.Nodes[0] + offset;
            //}
            nodes = data.NodesWithPosition(offset);

            shakeTime = data.Float("shakeTime", 0.5f);
            moveTimes = data.Attr("moveTime", "1.8").Split(',',StringSplitOptions.TrimEntries);
            moveEased = data.Bool("moveEased", true);
            allowReturn = data.Bool("allowReturn", false);
            persistent = data.Bool("persistent", false);
            entityId = data.ID;
            onActivateFlag = data.Attr("linkTag", "");

            smoke = data.Bool("smoke", true);
            inactiveColor = Calc.HexToColor(data.Attr("inactiveColor", "5FCDE4"));
            activeColor = Calc.HexToColor(data.Attr("activeColor", "FFFFFF"));
            finishColor = Calc.HexToColor(data.Attr("finishColor", "F141DF"));
            moveSound = data.Attr("moveSound", "event:/sorbethelper/sfx/gateblock_open");
            finishedSound = data.Attr("finishedSound", "event:/sorbethelper/sfx/gateblock_finish");

            string iconSprite = data.Attr("iconSprite", "switchgate/icon");

            // set up icon
            icon = new Sprite(GFX.Game, "objects/" + iconSprite);
            Add(icon);
            icon.Add("spin", "", 0.1f, "spin");
            icon.Play("spin");
            icon.Rate = 0f;
            icon.Color = fillColor = inactiveColor;
            icon.Position = iconOffset = new Vector2(data.Width / 2f, data.Height / 2f);
            icon.CenterOrigin();
            Add(finishIconScaleWiggler = Wiggler.Create(0.5f, 4f, f => {
                icon.Scale = Vector2.One * (1f + f);
            }));

            Add(openSfx = new SoundSource());
            Add(new LightOcclude(0.5f));

            P_RecoloredFire = new ParticleType(TouchSwitch.P_Fire) {
                Color = finishColor
            };

            P_Activate = new(Seeker.P_HitWall) {
                Color = inactiveColor,
                Color2 = Color.Lerp(inactiveColor, Color.White, 0.75f),
                ColorMode = ParticleType.ColorModes.Blink,
            };
            P_ActivateReturn = new(P_Activate) {
                Color = finishColor,
                Color2 = Color.Lerp(finishColor, Color.White, 0.75f),
                ColorMode = ParticleType.ColorModes.Blink,
            };

            easers = data.Attr("ease", "CubeOut").Split(',',StringSplitOptions.TrimEntries);
            nodeDelays = data.Attr("nodeDelays").Split(',', StringSplitOptions.TrimEntries);
            returnDelays = data.Attr("returnDelays").Split(',', StringSplitOptions.TrimEntries);
        }
        private Ease.Easer ease;
        private string[] moveTimes, easers, nodeDelays, returnDelays;
        private float nodeDelay, returnDelay;

        public void Activate() {
            Triggered = true;

            if (!string.IsNullOrEmpty(onActivateFlag)) {
                SceneAs<Level>().Session.SetFlag(onActivateFlag);
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            GateBlockOutlineRenderer.TryCreateRenderer(scene);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            start = Position;

            if (persistent && SceneAs<Level>().Session.GetFlag("flag_sorbetHelper_gateBlock_persistent_" + entityId)) {
                atNode = true;

                if (allowReturn)
                    Add(new Coroutine(BackAndForthSequence()));
                else
                    Triggered = true;

                MoveTo(nodes[0]);
                icon.Rate = 0f;
                icon.SetAnimationFrame(0);
                icon.Color = finishColor;
                fillColor = allowReturn ? finishColor : new((int)(finishColor.R * 0.7f), (int)(finishColor.G * 0.67f), (int)(finishColor.B * 0.8f), 255);
            } else {
                atNode = false;

                if (allowReturn)
                    Add(new Coroutine(BackAndForthSequence()));
                else
                    Add(new Coroutine(Sequence()));
            }
        }

        public override void Update() {
            VisibleOnCamera = InView((Scene as Level).Camera);

            // ease scale and hitOffset towards their default values
            scale.X = Calc.Approach(scale.X, 1f, Engine.DeltaTime * 4f);
            scale.Y = Calc.Approach(scale.Y, 1f, Engine.DeltaTime * 4f);
            offset.X = Calc.Approach(offset.X, 0f, Engine.DeltaTime * 15f);
            offset.Y = Calc.Approach(offset.Y, 0f, Engine.DeltaTime * 15f);

            base.Update();
        }

        public override void Render() {
            if (!VisibleOnCamera)
                return;

            // only render the icon, any unique gate blocks should handle visuals themselves
            Vector2 iconScale = icon.Scale;
            icon.Scale *= Scale;

            icon.Position = iconOffset + Offset;
            icon.DrawOutline();

            base.Render();

            icon.Scale = iconScale;
        }

        public virtual void RenderOutline() {
            // outline rendering should be implemented per gate block type
        }

        public IEnumerator BackSequence()
        {
            Vector2 moveFrom = nodes[nodes.Length - 1]; // nodes.Length - 1 is the last node
            Vector2 moveTo;
            Color fromColor, toColor;

            if (!atNode)
            {
                moveTo = nodes[0];

                fromColor = inactiveColor;
                toColor = finishColor;

                // according to log, this section will be excuted on level load
            }
            else
            {
                moveTo = nodes.Length - 2 >= 0 ? nodes[nodes.Length - 2] : Position;

                fromColor = finishColor;
                toColor = inactiveColor;

                // according to log, this section will be executed after the sequence done
            }
            // when touching an entity, this process will only be done once at the level start
            // then it's the gate block end
            // which means, at first atNode == false
            // after the gate block reached the end, atNode == true

            // darken finished no return fill color a bit
            Color toFillColorNoReturn = new((int)(toColor.R * 0.7f), (int)(toColor.G * 0.67f), (int)(toColor.B * 0.8f), 255);

            // wait until triggered
            while (!Triggered)
            {
                yield return null;
            }

            if (persistent)
                SceneAs<Level>().Session.SetFlag("flag_sorbetHelper_gateBlock_persistent_" + entityId, !atNode);
            // flag adaptable with Sorbet Helper original codes

            yield return 0.1f;

            // animate the icon
            openSfx.Play(moveSound);
            if (shakeTime > 0f)
            {
                StartShaking(shakeTime);
                while (icon.Rate < 1f)
                {
                    icon.Color = fillColor = Color.Lerp(fromColor, activeColor, icon.Rate);
                    icon.Rate += Engine.DeltaTime / shakeTime;
                    yield return null;
                }
            }
            else
            {
                icon.Color = fillColor = activeColor;
                icon.Rate = 1f;
            }

            yield return 0.1f; // start delay?

            moveFrom = nodes[nodes.Length - 1];

            // these will has to be executed when nodes.Length - 2 >= 0
            // hence nodes.Length >= 2
            for (int i = nodes.Length - 2, index = 0; i >= 0; i--, index++)
            {
                moveTo = nodes[i];

                // move the gate block, emitting particles along the way
                float at = 0f, percent;
                Vector2 placing;
                int particleAt = 0;
                while (at < 1f)
                {
                    yield return null;

                    int n = index < moveTimes.Length ? index : moveTimes.Length - 1;
                    if (!float.TryParse(moveTimes[n], out moveTime)) { moveTime = 1.8f; }
                    at = Calc.Approach(at, 1f, Engine.DeltaTime / moveTime);

                    n = index < easers.Length ? index : easers.Length - 1;
                    percent = EaseUtils.StringToEase(easers[n])(at);
                    placing = Vector2.Lerp(moveFrom, moveTo, percent);
                    MoveTo(placing);

                    // generating particles
                    if (Scene.OnInterval(0.1f))
                    {
                        particleAt++;
                        particleAt %= 2;
                        for (int tileX = 0; tileX < Width / 8f; tileX++)
                        {
                            for (int tileY = 0; tileY < Height / 8f; tileY++)
                            {
                                if ((tileX + tileY) % 2 == particleAt)
                                {
                                    SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind,
                                        Position + new Vector2(tileX * 8, tileY * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                                }
                            }
                        }
                    }
                }

                // changing the tween to ease lerp codes

                collidableBackup = Collidable;
                Collidable = false;

                #region collide particles
                // collide dust particles on the left
                if (moveTo.X <= moveFrom.X)
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

                // collide dust particles on the right
                if (moveTo.X >= moveFrom.X)
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
                if (moveTo.Y <= moveFrom.Y)
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
                if (moveTo.Y >= moveFrom.Y)
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
                #endregion

                Collidable = collidableBackup;

                moveFrom = nodes[i];

                // node delay processing
                if (i > 0) // no delays on the first node
                {
                    if(returnDelays.Length == 0) { returnDelay = 0; }
                    else if (index < returnDelays.Length)
                    {
                        if (!float.TryParse(returnDelays[index], out returnDelay)) { returnDelay = 0; }
                    }
                    else
                    {
                        if (!float.TryParse(returnDelays[returnDelays.Length - 1], out returnDelay)) { returnDelay = 0; }
                    }

                    yield return returnDelay;
                }

            }

            // moving is over
            Audio.Play(finishedSound, Position);
            StartShaking(0.2f);
            while (icon.Rate > 0f)
            {
                icon.Color = Color.Lerp(activeColor, toColor, 1f - icon.Rate);
                fillColor = Color.Lerp(activeColor, allowReturn ? toColor : toFillColorNoReturn, 1f - icon.Rate);

                icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
            }
            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
            finishIconScaleWiggler.Start();

            // emit fire particles if the block is not behind a solid
            collidableBackup = Collidable;
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(Center) && smoke)
            {
                for (int i = 0; i < 32; i++)
                {
                    float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                    SceneAs<Level>().ParticlesFG.Emit(P_RecoloredFire, Position + iconOffset + Calc.AngleToVector(angle, 4f), toColor, angle);
                }
            }
            Collidable = collidableBackup;

            atNode = !atNode;
            if (allowReturn)
                Triggered = false;
        }

        // (somewhat) stolen from maddie helping hand
        // modifed, but... how did it work anyways???
        private IEnumerator BackAndForthSequence()
        {
            while (true)
            {
                IEnumerator seq = Sequence();

                while (seq.MoveNext())
                {
                    yield return seq.Current;
                }

                seq = BackSequence();

                while (seq.MoveNext())
                {
                    yield return seq.Current;
                }
            }
        }

        public IEnumerator Sequence() {
            Vector2 moveFrom = Position;
            Vector2 moveTo;
            Color fromColor, toColor;

            if (!atNode)
            {
                moveTo = nodes[0];

                fromColor = inactiveColor;
                toColor = finishColor;

                // according to log, this section will be excuted on level load
            }
            else
            {
                moveTo = start;

                fromColor = finishColor;
                toColor = inactiveColor;

                // according to log, this section will be executed after the sequence done
            }
            // when touching an entity, this process will only be done once at the level start
            // then it's the gate block end
            // which means, at first atNode == false
            // after the gate block reached the end, atNode == true

            // darken finished no return fill color a bit
            Color toFillColorNoReturn = new((int)(toColor.R * 0.7f), (int)(toColor.G * 0.67f), (int)(toColor.B * 0.8f), 255);

            // wait until triggered
            while (!Triggered)
            {
                yield return null;
            }

            if (persistent)
                SceneAs<Level>().Session.SetFlag("flag_sorbetHelper_gateBlock_persistent_" + entityId, !atNode);
                // flag adaptable with Sorbet Helper original codes

            yield return 0.1f;

            // animate the icon
            openSfx.Play(moveSound);
            if (shakeTime > 0f)
            {
                StartShaking(shakeTime);
                while (icon.Rate < 1f)
                {
                    icon.Color = fillColor = Color.Lerp(fromColor, activeColor, icon.Rate);
                    icon.Rate += Engine.DeltaTime / shakeTime;
                    yield return null;
                }
            }
            else
            {
                icon.Color = fillColor = activeColor;
                icon.Rate = 1f;
            }

            yield return 0.1f; // start delay?

            moveFrom = nodes[0];

            for (int i = 1, index = 0; i < nodes.Length; i++, index++)
            {
                moveTo = nodes[i];

                /*
                int particleAt = 0;
                Tween moveTween = Tween.Create(Tween.TweenMode.Oneshot, moveEased ? Ease.CubeOut : null, moveTime + (moveEased ? 0.2f : 0f), start: true);
                moveTween.OnUpdate = tweenArg =>
                {
                    // position moving
                    MoveTo(Vector2.Lerp(moveFrom, moveTo, tweenArg.Eased));

                    if (Scene.OnInterval(0.1f))
                    {
                        particleAt++;
                        particleAt %= 2;
                        for (int tileX = 0; tileX < Width / 8f; tileX++)
                        {
                            for (int tileY = 0; tileY < Height / 8f; tileY++)
                            {
                                if ((tileX + tileY) % 2 == particleAt)
                                {
                                    SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind,
                                        Position + new Vector2(tileX * 8, tileY * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                                }
                            }
                        }
                    }

                    // to be fixed, the output here shows that the position will suddenly advance to the next node
                    // when the moving is done
                    // solution : Maybe use percentages like Omni Zip Mover?
                };
                Add(moveTween);
                */

                // move the gate block, emitting particles along the way
                float at = 0f, percent;
                Vector2 placing;
                int particleAt = 0;
                while (at < 1f)
                {
                    yield return null;

                    int n = index < moveTimes.Length ? index : moveTimes.Length - 1;
                    if (!float.TryParse(moveTimes[n], out moveTime)) { moveTime = 1.8f; }
                    at = Calc.Approach(at, 1f, Engine.DeltaTime / moveTime);

                    n = index < easers.Length ? index : easers.Length - 1;
                    percent = EaseUtils.StringToEase(easers[n])(at);
                    placing = Vector2.Lerp(moveFrom, moveTo, percent);
                    MoveTo(placing);

                    // generating particles
                    if (Scene.OnInterval(0.1f))
                    {
                        particleAt++;
                        particleAt %= 2;
                        for (int tileX = 0; tileX < Width / 8f; tileX++)
                        {
                            for (int tileY = 0; tileY < Height / 8f; tileY++)
                            {
                                if ((tileX + tileY) % 2 == particleAt)
                                {
                                    SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind,
                                        Position + new Vector2(tileX * 8, tileY * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                                }
                            }
                        }
                    }
                }

                // changing the tween to ease lerp codes

                //float moveTimeLeft = moveTime;
                //while (moveTimeLeft > 0f)
                //{
                //    yield return null;
                //    moveTimeLeft -= Engine.DeltaTime;
                //}

                collidableBackup = Collidable;
                Collidable = false;

                #region collide particles
                // collide dust particles on the left
                if (moveTo.X <= moveFrom.X)
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

                // collide dust particles on the right
                if (moveTo.X >= moveFrom.X)
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
                if (moveTo.Y <= moveFrom.Y)
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
                if (moveTo.Y >= moveFrom.Y)
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
                #endregion

                Collidable = collidableBackup;

                moveFrom = nodes[i];

                // node delay processing
                if (i < nodes.Length - 1) // no delays on the last node
                {
                    if(nodeDelays.Length == 0) { nodeDelay = 0; }
                    else if(index < nodeDelays.Length)
                    {
                        if (!float.TryParse(nodeDelays[index], out nodeDelay)) { nodeDelay = 0; }
                    }
                    else
                    {
                        if (!float.TryParse(nodeDelays[nodeDelays.Length - 1], out nodeDelay)) { nodeDelay = 0; }
                    }

                    yield return nodeDelay;
                }
            }

            // moving is over
            Audio.Play(finishedSound, Position);
            StartShaking(0.2f);
            while (icon.Rate > 0f)
            {
                icon.Color = Color.Lerp(activeColor, toColor, 1f - icon.Rate);
                fillColor = Color.Lerp(activeColor, allowReturn ? toColor : toFillColorNoReturn, 1f - icon.Rate);

                icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
            }
            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
            finishIconScaleWiggler.Start();

            // emit fire particles if the block is not behind a solid
            collidableBackup = Collidable;
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(Center) && smoke)
            {
                for (int i = 0; i < 32; i++)
                {
                    float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                    SceneAs<Level>().ParticlesFG.Emit(P_RecoloredFire, Position + iconOffset + Calc.AngleToVector(angle, 4f), toColor, angle);
                }
            }
            Collidable = collidableBackup;

            atNode = !atNode;
            if (allowReturn)
                Triggered = false;
        }


        // i love stealing vanilla code peaceline
        public void ActivateParticles() {
            Vector2 dir = nodes[0] - start;
            if (atNode)
                dir = -dir;

            float direction = Calc.Angle(dir);
            Vector2 position;
            Vector2 positionRange;
            int num;

            dir = dir.FourWayNormal();
            if (dir == Vector2.UnitX) {
                position = CenterRight - Vector2.UnitX;
                positionRange = Vector2.UnitY * (Height - 2f) * 0.5f;
                num = (int)(Height / 8f) * 4;
            } else if (dir == -Vector2.UnitX) {
                position = CenterLeft + Vector2.UnitX;
                positionRange = Vector2.UnitY * (Height - 2f) * 0.5f;
                num = (int)(Height / 8f) * 4;
            } else if (dir == Vector2.UnitY) {
                position = BottomCenter - Vector2.UnitY;
                positionRange = Vector2.UnitX * (Width - 2f) * 0.5f;
                num = (int)(Width / 8f) * 4;
            } else {
                position = TopCenter + Vector2.UnitY;
                positionRange = Vector2.UnitX * (Width - 2f) * 0.5f;
                num = (int)(Width / 8f) * 4;
            }
            num += 2;

            (Scene as Level).Particles.Emit(atNode ? P_ActivateReturn : P_Activate, num, position, positionRange, direction);
        }

        protected void DrawNineSlice(MTexture texture, Color color) {
            // completely stolen from maddie's helping hand
            // probably much more performant than anything i could make so its mostly unchanged apart from adding scaling
            int widthInTiles = (int)Collider.Width / 8 - 1;
            int heightInTiles = (int)Collider.Height / 8 - 1;

            Vector2 renderPos = new Vector2(Position.X + Offset.X, Position.Y + Offset.Y);
            Vector2 blockCenter = renderPos + new Vector2(Collider.Width / 2f, Collider.Height / 2f);
            Texture2D baseTexture = texture.Texture.Texture;
            int clipBaseX = texture.ClipRect.X;
            int clipBaseY = texture.ClipRect.Y;

            Rectangle clipRect = new Rectangle(clipBaseX, clipBaseY, 8, 8);

            for (int i = 0; i <= widthInTiles; i++) {
                clipRect.X = clipBaseX + ((i < widthInTiles) ? i == 0 ? 0 : 8 : 16);
                for (int j = 0; j <= heightInTiles; j++) {
                    int tilePartY = (j < heightInTiles) ? j == 0 ? 0 : 8 : 16;
                    clipRect.Y = tilePartY + clipBaseY;
                    Draw.SpriteBatch.Draw(baseTexture, blockCenter + ((renderPos + new Vector2(4, 4) - blockCenter) * Scale), clipRect, color, 0f, new Vector2(4, 4), Scale, SpriteEffects.None, 0f);
                    renderPos.Y += 8f;
                }

                renderPos.X += 8f;
                renderPos.Y = Position.Y + Offset.Y;
            }
        }

        private bool InView(Camera camera) =>
            X < camera.Right + 16f && X + Width > camera.Left - 16f && Y < camera.Bottom + 16f && Y + Height > camera.Top - 16f;

        [Tracked]
        private class GateBlockOutlineRenderer : Entity {
            private static bool rendererJustCreated = false;

            public GateBlockOutlineRenderer() : base() {
                Depth = 1;
                Tag = Tags.Persistent;
            }

            public override void Render() {
                var blocks = Scene.Tracker.GetEntities<GateBlock>();

                foreach (GateBlock block in blocks) {
                    if (block.Visible && block.VisibleOnCamera) {
                        block.RenderOutline();
                    }
                }
            }

            public override void Awake(Scene scene) {
                base.Awake(scene);

                rendererJustCreated = false;
            }

            public static void TryCreateRenderer(Scene scene) {
                if (!rendererJustCreated && scene.Tracker.GetEntities<GateBlockOutlineRenderer>().Count == 0) {
                    scene.Add(new GateBlockOutlineRenderer());
                    rendererJustCreated = true;
                }
            }
        }
    }
}

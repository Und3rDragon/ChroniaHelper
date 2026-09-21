using Celeste.Mod.Entities;
using ChroniaHelper.Cores.SampleJumpThrough;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using static ChroniaHelper.Cores.ExtendedAttributes;

namespace ChroniaHelper.Entities;

[CustomEntity(
    "ChroniaHelper/SampleJumpThroughLeft = CreateLeft",
    "ChroniaHelper/SampleJumpThroughRight = CreateRight"
)]
[Tracked]
[ExA.Credits("Maddie for Sideways and Upside-down Jumpthru codes" +
    "pixelator for VortexHelper attached jumpthru codes")]
public class SampleJumpThroughSideways : Entity {

    private static ILHook hookOnUpdateSprite;
    private static ILHook hookOnOrigUpdate;

    private static MethodInfo playerJumpthruBoostBlockedCheck = typeof(Player).GetMethod("JumpThruBoostBlockedCheck", BindingFlags.Instance | BindingFlags.NonPublic);

    private static bool hooksActive = false;

    [SelectiveLoadHook]
    public static void Load() {
        activateHooks();
    }

    [SelectiveUnloadHook]
    public static void Unload() {
        deactivateHooks();
    }

    private static void activateHooks() {
        if (hooksActive) {
            return;
        }
        hooksActive = true;

        // implement the basic collision between actors/platforms and sideways jumpthrus.
        IL.Celeste.Actor.MoveHExact += addSidewaysJumpthrusInHorizontalMoveMethods;
        IL.Celeste.Platform.MoveHExactCollideSolids += addSidewaysJumpthrusInHorizontalMoveMethods;

        // block "climb hopping" on top of sideways jumpthrus, because this just looks weird.
        On.Celeste.Player.ClimbHopBlockedCheck += onPlayerClimbHopBlockedCheck;

        // mod collide checks to include sideways jumpthrus, so that the player behaves with them like with walls.
        IL.Celeste.Player.WallJumpCheck += modCollideChecks; // allow player to walljump off them
        IL.Celeste.Player.NormalUpdate += modCollideChecks; // get the wall slide effect
        IL.Celeste.Player.ClimbCheck += modCollideChecks; // allow player to climb on them
        IL.Celeste.Player.ClimbBegin += modCollideChecks; // if not applied, the player will clip through jumpthrus if trying to climb on them
        IL.Celeste.Player.ClimbUpdate += modCollideChecks; // when climbing, jumpthrus are handled like walls
        IL.Celeste.Player.SlipCheck += modCollideChecks; // make climbing on jumpthrus not slippery
        IL.Celeste.Player.OnCollideH += modCollideChecks; // handle dashes against jumpthrus properly, without "shifting" down
        IL.Celeste.Seeker.OnCollideH += modCollideChecks; // make seekers bump against jumpthrus, instead of vibrating at maximum velocity
        hookOnOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), modCollideChecks); // patch wall retention to include sideways jumpthru detection as well

        // don't make Madeline duck when dashing against a sideways jumpthru
        On.Celeste.Player.DuckFreeAt += preventDuckWhenDashingAgainstJumpthru;

        // one extra hook that kills the player momentum when hitting a jumpthru so that they don't get "stuck" on them.
        On.Celeste.Player.NormalUpdate += onPlayerNormalUpdate;

        On.Celeste.SurfaceIndex.GetPlatformByPriority += modSurfaceIndexGetPlatformByPriority;

        // have the push animation when Madeline runs against a jumpthru for example
        hookOnUpdateSprite = new ILHook(typeof(Player).GetMethod("orig_UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance), modCollideChecks);
    }

    private static void deactivateHooks() {
        if (!hooksActive) {
            return;
        }
        hooksActive = false;

        IL.Celeste.Actor.MoveHExact -= addSidewaysJumpthrusInHorizontalMoveMethods;
        IL.Celeste.Platform.MoveHExactCollideSolids -= addSidewaysJumpthrusInHorizontalMoveMethods;

        On.Celeste.Player.ClimbHopBlockedCheck -= onPlayerClimbHopBlockedCheck;

        IL.Celeste.Player.WallJumpCheck -= modCollideChecks;
        IL.Celeste.Player.NormalUpdate -= modCollideChecks;
        IL.Celeste.Player.ClimbCheck -= modCollideChecks;
        IL.Celeste.Player.ClimbBegin -= modCollideChecks;
        IL.Celeste.Player.ClimbUpdate -= modCollideChecks;
        IL.Celeste.Player.SlipCheck -= modCollideChecks;
        IL.Celeste.Player.OnCollideH -= modCollideChecks;
        IL.Celeste.Seeker.OnCollideH -= modCollideChecks;
        hookOnOrigUpdate?.Dispose();

        On.Celeste.Player.DuckFreeAt -= preventDuckWhenDashingAgainstJumpthru;

        On.Celeste.Player.NormalUpdate -= onPlayerNormalUpdate;

        On.Celeste.SurfaceIndex.GetPlatformByPriority -= modSurfaceIndexGetPlatformByPriority;

        hookOnUpdateSprite?.Dispose();
    }

    private class FakeCollidingSolid : Solid {
        public FakeCollidingSolid() : base(Vector2.Zero, 0, 0, false) { }
    }

    private static void addSidewaysJumpthrusInHorizontalMoveMethods(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Entity>("CollideFirst"))) {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<Solid, Entity, int, Solid>>((orig, self, moveH) => {
                if (orig != null) return orig;

                int moveDirection = Math.Sign(moveH);
                bool movingLeftToRight = moveH > 0;
                if (getCollisionWithSidewaysJumpthruWhileMoving(self, moveDirection, movingLeftToRight) is SampleJumpThroughSideways jumpThru) {
                    FakeCollidingSolid fakeSolid = new FakeCollidingSolid();
                    if (self is Player player && player.DashAttacking) {
                        fakeSolid.OnDashCollide = jumpThru.OnDashCollide;
                    }
                    return fakeSolid; // so Celeste will call that callback and not discard return value.
                }

                return null;
            });
        }
    }

    private static SampleJumpThroughSideways getCollisionWithSidewaysJumpthruWhileMoving(Entity self, int moveDirection, bool movingLeftToRight) {
        // check if colliding with a sideways jumpthru
        SampleJumpThroughSideways jumpThru = self.CollideFirstOutside<SampleJumpThroughSideways>(self.Position + Vector2.UnitX * moveDirection);
        if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight && (!(self is Seeker) || !jumpThru.letSeekersThrough)) {
            // there is a sideways jump-thru and we are moving in the opposite direction => collision
            return jumpThru;
        }

        return null;
    }

    private static bool onPlayerClimbHopBlockedCheck(On.Celeste.Player.orig_ClimbHopBlockedCheck orig, Player self) {
        bool vanillaCheck = orig(self);
        if (vanillaCheck)
            return vanillaCheck;

        // block climb hops on jumpthrus because those look weird
        return self.CollideCheckOutside<SampleJumpThroughSideways>(self.Position + Vector2.UnitX * (int) self.Facing);
    }

    private static void modCollideChecks(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        // create a Vector2 temporary variable
        VariableDefinition checkAtPositionStore = new VariableDefinition(il.Import(typeof(Vector2)));
        il.Body.Variables.Add(checkAtPositionStore);

        bool isClimb = il.Method.Name.Contains("Climb");
        bool isWallJump = il.Method.Name.Contains("WallJump") || il.Method.Name.Contains("NormalUpdate");

        while (cursor.Next != null) {
            Instruction next = cursor.Next;

            // we want to replace all CollideChecks with solids here.
            if (next.OpCode == OpCodes.Call && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {

                callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: false);

                // mod the result
                cursor.EmitDelegate<Func<bool, Entity, Vector2, bool>>((orig, self, checkAtPosition) => {
                    // we still want to check for solids...
                    if (orig) {
                        return true;
                    }

                    // if we are not checking a side, this certainly has nothing to do with jumpthrus.
                    if (self.Position.X == checkAtPosition.X)
                        return false;

                    return entityCollideCheckWithSidewaysJumpthrus(self, checkAtPosition, isClimb, isWallJump);
                });
            }

            if (next.OpCode == OpCodes.Callvirt && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Scene::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {

                callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: true);

                cursor.EmitDelegate<Func<bool, Scene, Vector2, bool>>((orig, self, vector) => {
                    if (orig) {
                        return true;
                    }
                    return sceneCollideCheckWithSidewaysJumpthrus(self, vector, isClimb, isWallJump);
                });
            }

            cursor.Index++;
        }
    }

    private static bool preventDuckWhenDashingAgainstJumpthru(On.Celeste.Player.orig_DuckFreeAt orig, Player self, Vector2 at) {
        if (orig(self, at)) {
            // check for collisions against sideways jumpthrus while ducked.
            Collider origHitbox = self.Collider;
            self.Collider = new Hitbox(8f, 6f, -4f, -6f); // duck hitbox
            bool result = !entityCollideCheckWithSidewaysJumpthrus(self, at, false, false);
            self.Collider = origHitbox;

            return result;
        }
        return false;
    }

    private static void callOrigMethodKeepingEverythingOnStack(ILCursor cursor, VariableDefinition checkAtPositionStore, bool isSceneCollideCheck) {
        // store the position in the local variable
        cursor.Emit(OpCodes.Stloc, checkAtPositionStore);
        cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);

        // let vanilla call CollideCheck
        cursor.Index++;

        // reload the parameters
        cursor.Emit(OpCodes.Ldarg_0);
        if (isSceneCollideCheck) {
            cursor.Emit(OpCodes.Call, typeof(Entity).GetProperty("Scene").GetGetMethod());
        }

        cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);
    }

    private static bool entityCollideCheckWithSidewaysJumpthrus(Entity self, Vector2 checkAtPosition, bool isClimb, bool isWallJump) {
        // our entity collides if this is with a jumpthru and we are colliding with the solid side of it.
        // we are in this case if the jumpthru is left to right (the "solid" side of it is the right one)
        // and we are checking the collision on the left side of the player for example.
        bool collideOnLeftSideOfPlayer = (self.Position.X > checkAtPosition.X);
        SampleJumpThroughSideways jumpthru = self.CollideFirstOutside<SampleJumpThroughSideways>(checkAtPosition);
        return jumpthru != null && (self is Player || self is Seeker) && (jumpthru.AllowLeftToRight == collideOnLeftSideOfPlayer
            && (!isWallJump || jumpthru.allowWallJumping) && (!isClimb || jumpthru.allowClimbing))
            && jumpthru.Bottom >= self.Top + checkAtPosition.Y - self.Position.Y + 3;
    }

    private static bool sceneCollideCheckWithSidewaysJumpthrus(Scene self, Vector2 vector, bool isClimb, bool isWallJump) {
        SampleJumpThroughSideways jumpthru;
        if ((jumpthru = self.CollideFirst<SampleJumpThroughSideways>(vector)) != null) {
            return (!isWallJump || jumpthru.allowWallJumping) && (!isClimb || jumpthru.allowClimbing);
        }
        return false;
    }

    private static int onPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self) {
        int result = orig(self);

        // kill speed if player is going towards a jumpthru.
        if (self.Speed.X != 0) {
            bool movingLeftToRight = self.Speed.X > 0;
            SampleJumpThroughSideways jumpThru = self.CollideFirstOutside<SampleJumpThroughSideways>(self.Position + Vector2.UnitX * Math.Sign(self.Speed.X));
            if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight) {
                self.Speed.X = 0;
            }
        }

        return result;
    }

    private static Platform modSurfaceIndexGetPlatformByPriority(On.Celeste.SurfaceIndex.orig_GetPlatformByPriority orig, List<Entity> platforms) {
        // if vanilla already has platforms to get the sound index from, use those.
        if (platforms.Count != 0) {
            return orig(platforms);
        }

        // check if we are climbing a sideways jumpthru.
        Player player = Engine.Scene.Tracker.GetEntity<Player>();
        if (player != null) {
            SampleJumpThroughSideways jumpThru = player.CollideFirst<SampleJumpThroughSideways>(player.Center + Vector2.UnitX * (float) player.Facing);
            if (jumpThru != null && jumpThru.surfaceIndex != -1) {
                // yes we are! pass it off as a Platform so that the game can get its surface index later.
                return new WallSoundIndexHolder(jumpThru.surfaceIndex);
            }
        }

        return orig(platforms);
    }

    // this is a dummy Platform that is just here to hold a wall surface sound index, that the game will read.
    // it isn't actually used as a platform!
    private class WallSoundIndexHolder : Platform {
        private int wallSoundIndex;

        public WallSoundIndexHolder(int wallSoundIndex) : base(Vector2.Zero, false) {
            this.wallSoundIndex = wallSoundIndex;
        }

        public override void MoveHExact(int move) {
            throw new NotImplementedException();
        }

        public override void MoveVExact(int move) {
            throw new NotImplementedException();
        }

        public override int GetWallSoundIndex(Player player, int side) {
            return wallSoundIndex;
        }
    }

    // ======== Begin of entity code ========

    public static Entity CreateLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new SampleJumpThroughSideways(entityData, offset, left: true);

    public static Entity CreateRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new SampleJumpThroughSideways(entityData, offset, left: false);

    private int lines;
    private string overrideTexture;
    private float animationDelay;
    private int surfaceIndex = -1;

    public bool AllowLeftToRight;

    private bool allowClimbing;
    private bool allowWallJumping;

    private bool letSeekersThrough;

    private bool pushPlayer;
    private bool cornerCorrect;
        private bool attached;

        /// <summary>
        /// 粘连组件，用于在玩家抓住板面时把触发传递给宿主实体。
        /// </summary>
        private StaticMover staticMover;

        /// <summary>
        /// 贴板冲刺时的自定义碰撞回调。
        /// </summary>
        public DashCollision OnDashCollide;

    // 粘连时用一块临时可碰撞的实体承担推挤与携带，平时不参与碰撞
    private Solid playerInteractingSolid;

    private Vector2 shakeOffset = Vector2.Zero;

    public SampleJumpThroughSideways(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool allowClimbing, bool allowWallJumping, bool letSeekersThrough, int surfaceIndex, bool pushPlayer, bool cornerCorrect, bool attached)
        : this(position, height, allowLeftToRight, overrideTexture, animationDelay, allowClimbing, allowWallJumping, letSeekersThrough, surfaceIndex, pushPlayer, attached) {

        this.cornerCorrect = cornerCorrect;
    }

    public SampleJumpThroughSideways(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool allowClimbing, bool allowWallJumping, bool letSeekersThrough, int surfaceIndex, bool pushPlayer, bool attached)
        : this(position, height, allowLeftToRight, overrideTexture, animationDelay, allowClimbing, allowWallJumping, letSeekersThrough, surfaceIndex, attached) {

        this.pushPlayer = pushPlayer;
    }

    public SampleJumpThroughSideways(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool allowClimbing, bool allowWallJumping, bool letSeekersThrough, int surfaceIndex, bool attached)
        : this(position, height, allowLeftToRight, overrideTexture, animationDelay, allowClimbing, allowWallJumping, letSeekersThrough, attached) {

        this.surfaceIndex = surfaceIndex;
    }

    public SampleJumpThroughSideways(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool allowClimbing, bool allowWallJumping, bool letSeekersThrough, bool attached)
        : this(position, height, allowLeftToRight, overrideTexture, animationDelay, attached) {

        this.allowClimbing = allowClimbing;
        this.allowWallJumping = allowWallJumping;
        this.letSeekersThrough = letSeekersThrough;
    }

    public SampleJumpThroughSideways(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool attached)
       : base(position) {

        Ldm.LoadHook(typeof(SampleJumpThroughSideways));

        lines = height / 8;
        AllowLeftToRight = allowLeftToRight;
        Depth = -60;
        this.overrideTexture = overrideTexture;
        this.animationDelay = animationDelay;
        this.attached = attached;

        float hitboxOffset = 0f;
        if (AllowLeftToRight)
            hitboxOffset = 3f;

        Collider = new Hitbox(5f, height, hitboxOffset, 0);

        if (attached) {
            // 该实体仅在随宿主移动时参与碰撞，使玩家被压到或攀附时表现正常
            playerInteractingSolid = new Solid(Position, Width, Height, safe: false);
            playerInteractingSolid.Collidable = false;
            playerInteractingSolid.Visible = false;
            if (!AllowLeftToRight) {
                playerInteractingSolid.Position.X += 3f;
            }

            staticMover = new StaticMoverWithLiftSpeed() {
                SolidChecker = solid => {
                    if (!solid.CollideRect(new Rectangle((int) X, (int) Y - 1, (int) Width, (int) Height + 2))) {
                        return false;
                    }

                    // 记录所粘附的宿主，供触发传递使用
                    staticMover.Platform = solid;
                    return true;
                },
                OnMove = move => SampleJumpThroughUtils.SidewaysJumpthruOnMove(this, playerInteractingSolid, !AllowLeftToRight, move),
                OnShake = onShake,
                OnSetLiftSpeed = liftSpeed => playerInteractingSolid.LiftSpeed = liftSpeed
            };
            Add(staticMover);
        }
    }

    public SampleJumpThroughSideways(EntityData data, Vector2 offset, bool left)
        : this(data.Position + offset, data.Height, !left, data.Attr("texture", "default"), data.Float("animationDelay", 0f),
              data.Bool("allowClimbing", true), data.Bool("allowWallJumping", true), data.Bool("letSeekersThrough", false), data.Int("surfaceIndex", -1),
              data.Bool("pushPlayer", false), data.Bool("cornerCorrect"), data.Bool("attached", false)) { }

    public override void Added(Scene scene) {
        base.Added(scene);

        if (attached && playerInteractingSolid != null) {
            // 把承担碰撞的临时实体一并加入场景
            scene.Add(playerInteractingSolid);
        }
    }

    private void onShake(Vector2 move) {
        shakeOffset += move;
        playerInteractingSolid.ShakeStaticMovers(move);
    }

    public override void Awake(Scene scene) {

        AreaData areaData = AreaData.Get(scene);
        string jumpthru = areaData.Jumpthru;
        if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default")) {
            jumpthru = overrideTexture;
        }

        // 贴图名可能带编号序列：不带编号时直接命中，带编号时取第一帧
        MTexture[] atlasFrames = GFX.Game.GetAtlasSubtextures("objects/jumpthru/" + jumpthru).ToArray();
        MTexture mTexture = atlasFrames.Length > 0
            ? atlasFrames[0]
            : GFX.Game["objects/jumpthru/" + jumpthru];

        int num = mTexture.Width / 8;

        // 逐格决定使用图集中的哪一块，动画与静态绘制共用同一套取块规则
        Point[] blocks = new Point[lines];

        for (int i = 0; i < lines; i++) {
            int xTilePosition;
            int yTilePosition;
            if (i == 0) {
                xTilePosition = 0;
                yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, -1f))) ? 1 : 0);
            } else if (i == lines - 1) {
                xTilePosition = num - 1;
                yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, 1f))) ? 1 : 0);
            } else {
                xTilePosition = 1 + Calc.Random.Next(num - 2);
                yTilePosition = Calc.Random.Choose(0, 1);
            }

            blocks[i] = new Point(xTilePosition, yTilePosition);
        }

        // 板体沿垂直方向排布，绘制时整体旋转；朝右时另需水平偏移一格
        float rotation = (float) (Math.PI / 2);
        float xOffset = AllowLeftToRight ? 8f : 0f;
        bool flipY = !AllowLeftToRight;

        if (animationDelay > 0f) {
            if (atlasFrames.Length > 0) {
                Add(new JumpThroughAnimator(
                    atlasFrames, lines, horizontal: false, animationDelay, blocks,
                    rotation: rotation, flipY: flipY, xOffset: xOffset)
                {
                    Depth = Depth
                });
            }
        } else {
            for (int i = 0; i < lines; i++) {
                Image image = new Image(mTexture.GetSubtexture(blocks[i].X * 8, blocks[i].Y * 8, 8, 8));
                image.Y = i * 8;
                image.Rotation = rotation;

                if (AllowLeftToRight)
                    image.X = 8;
                else
                    image.Scale.Y = -1;

                Add(image);
            }
        }
    }

    public override void Update() {
        base.Update();

        Player p = CollideFirst<Player>();

        // 玩家抓住板面时，把触发传递给所粘附的实体
        if (attached) {
            bool climbing = SampleJumpThroughUtils.GetPlayerClimbing(this, !AllowLeftToRight) != null;

            if (climbing) {
                staticMover?.TriggerPlatform();
            }
        }

        if (p == null)
            return;

        // if we are supposed to push the player and the player is hitting us...
        if (pushPlayer) {
            DynData<Player> playerData = new DynData<Player>(p);
            if (AllowLeftToRight) {
                // player is moving right, not on the ground, not climbing, not blocked => push them to the right
                if (p.Speed.X >= 0f && !playerData.Get<bool>("onGround") && (p.StateMachine.State != 1 || playerData.Get<int>("lastClimbMove") == -1)
                    && !((bool) playerJumpthruBoostBlockedCheck.Invoke(p, new object[0]))) {

                    p.MoveH(40f * Engine.DeltaTime);
                }
            } else {
                // player is moving left, not on the ground, not climbing, not blocked => push them to the left
                if (p.Speed.X <= 0f && !playerData.Get<bool>("onGround") && (p.StateMachine.State != 1 || playerData.Get<int>("lastClimbMove") == -1)
                    && !((bool) playerJumpthruBoostBlockedCheck.Invoke(p, new object[0]))) {

                    p.MoveH(-40f * Engine.DeltaTime);
                }
            }
        }

        if (cornerCorrect && (p.StateMachine.State == Player.StDash || p.StateMachine.State == Player.StRedDash) && Math.Abs(p.DashDir.X) < 0.1f) {
            if (AllowLeftToRight && Right - p.Left <= 6f)
                p.MoveHExact((int) (Right - p.Left));
            else if (!AllowLeftToRight && p.Right - Left <= 6f)
                p.MoveHExact((int) (Left - p.Right));
        }
    }

    public override void Render() {
        Position += shakeOffset;
        base.Render();
        Position -= shakeOffset;
    }
}
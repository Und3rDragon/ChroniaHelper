using ChroniaHelper.Cores;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace ChroniaHelper.Components;

[Tracked(false)]
public class SolidModifierComponent : BaseComponent
{
    [ExA.SelectiveLoadHook]
    public static void Load()
    {
        On.Celeste.GameLoader.Begin += LateInitialize;
        On.Celeste.Player.IsRiding_Solid += Player_IsRiding_Solid;
        On.Celeste.Player.ClimbJump += Player_ClimbJump;
        On.Celeste.Player.WallJump += Player_WallJump;
        On.Celeste.Player.Update += Player_Update;
        IL.Celeste.Player.WallJumpCheck += Player_WallJumpCheck;
        On.Monocle.StateMachine.Update += StateMachine_Update;
    }
    [ExA.SelectiveUnloadHook]
    public static void Unload()
    {
        On.Celeste.GameLoader.Begin += LateInitialize;
        On.Celeste.Player.IsRiding_Solid -= Player_IsRiding_Solid;
        On.Celeste.Player.ClimbJump -= Player_ClimbJump;
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
        On.Monocle.StateMachine.Update -= StateMachine_Update;
    }

    private static void StateMachine_Update(On.Monocle.StateMachine.orig_Update orig, StateMachine self)
    {
        orig.Invoke(self);
        if (self.Entity is Player)
        {
            Md.Session.currentActiveSolidModifier = null;
        }
    }

    private static bool Player_IsRiding_Solid(On.Celeste.Player.orig_IsRiding_Solid orig, Player self, Solid solid)
    {
        bool flag;
        if (orig.Invoke(self, solid))
        {
            flag = true;
        }
        else
        {
            SolidModifierComponent solidModifierComponent = solid.Get<SolidModifierComponent>();
            if (solidModifierComponent != null)
            {
                if (solidModifierComponent.bufferClimbJump && solidModifierComponent.HasBeenBufferJumpedOn)
                {
                    solidModifierComponent.HasBeenBufferJumpedOn = false;
                    flag = true;
                }
                else
                {
                    flag = (solidModifierComponent.triggerClimbOnTouch && self.CollideCheck(solid, self.Position + Vector2.UnitX * (float)self.Facing)) || (solidModifierComponent.OnTouchFromBelow && self.CollideCheck(solid, self.Position - Vector2.UnitY));
                }
            }
            else
            {
                flag = false;
            }
        }
        return flag;
    }

    private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        if (Md.Session.currentActiveSolidModifier == null)
        {
            orig.Invoke(self);
            return;
        }
        Md.Session.currentActiveSolidModifier.HasBeenBufferJumpedOn = true;
        orig.Invoke(self);
        if (Md.Session.currentActiveSolidModifier.CornerBoostBlock == -2 || Md.Session.currentActiveSolidModifier.CornerBoostRetention)
        {
            DynamicData.For(self).Set("wallSpeedRetained", self.Speed.X);
        }
        Md.Session.currentActiveSolidModifier = null;
    }

    private static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        if (Md.Session.currentActiveSolidModifier != null)
        {
            Md.Session.currentActiveSolidModifier.HasBeenBufferJumpedOn = true;
            Md.Session.currentActiveSolidModifier = null;
        }
        orig.Invoke(self, dir);
    }

    private static void Player_WallJumpCheck(ILContext il)
    {
        ILCursor ilcursor = new ILCursor(il);
        VariableDefinition variableDefinition = new VariableDefinition(il.Import(typeof(Vector2)));
        il.Body.Variables.Add(variableDefinition);
        ILCursor ilcursor2 = ilcursor;
        MoveType moveType = MoveType.Before;
        Func<Instruction, bool>[] array = new Func<Instruction, bool>[1];
        array[0] = delegate (Instruction i)
        {
            bool flag;
            if (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
            {
                MethodReference methodReference = i.Operand as MethodReference;
                flag = ((methodReference != null) ? methodReference.FullName : null) == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)";
            }
            else
            {
                flag = false;
            }
            return flag;
        };
        if (ilcursor2.TryGotoNext(moveType, array))
        {
            ilcursor.Emit(OpCodes.Stloc, variableDefinition);
            ilcursor.Emit(OpCodes.Ldloc, variableDefinition);
            ILCursor ilcursor3 = ilcursor;
            MoveType moveType2 = MoveType.After;
            Func<Instruction, bool>[] array2 = new Func<Instruction, bool>[1];
            array2[0] = delegate (Instruction i)
            {
                bool flag2;
                if (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
                {
                    MethodReference methodReference2 = i.Operand as MethodReference;
                    flag2 = ((methodReference2 != null) ? methodReference2.FullName : null) == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)";
                }
                else
                {
                    flag2 = false;
                }
                return flag2;
            };
            ilcursor3.GotoNext(moveType2, array2);
            ilcursor.Emit(OpCodes.Ldarg_0);
            ilcursor.Emit(OpCodes.Ldarg_1);
            ilcursor.Emit(OpCodes.Ldloc, variableDefinition);
            ilcursor.Emit(OpCodes.Call, typeof(SolidModifierComponent).GetMethod("ModifyWallJumpCheck"));
        }
    }

    public static bool ModifyWallJumpCheck(bool prev, Player player, int dir, Vector2 legacyAt)
    {
        List<Component> list;
        bool flag;
        if (!player.Scene.Tracker.Components.TryGetValue(typeof(SolidModifierComponent), out list) || list.Count == 0)
        {
            flag = prev;
        }
        else if (Enumerable.Contains<string>(SolidModifierComponent.LegacyMapSIDs, player.SceneAs<Level>().Session.Area.SID))
        {
            if (prev)
            {
                flag = true;
            }
            else
            {
                bool flag2 = false;
                Vector2 position = player.Position;
                player.Position = legacyAt;
                foreach (Component component in list)
                {
                    Solid solid = component.Entity as Solid;
                    if (solid != null)
                    {
                        flag2 |= SolidModifierComponent.LegacyCollideCheck(solid, player, (component as SolidModifierComponent).CornerBoostBlock);
                        if (flag2)
                        {
                            break;
                        }
                    }
                }
                player.Position = position;
                flag = flag2;
            }
        }
        else
        {
            flag = SolidModifierComponent.WJ_CollideCheck(list, player, dir, false) || prev;
        }
        return flag;
    }

    public static Func<Player, int, int> player_WallJumpCheck_getNum;
    public static bool WJ_CollideCheck(List<Component> q, Player player, int dir, bool includeOrig)
    {
        Func<Player, int, int> player_WallJumpCheck_getNum = SolidModifierComponent.player_WallJumpCheck_getNum;
        int num = Math.Max((player_WallJumpCheck_getNum != null) ? player_WallJumpCheck_getNum(player, dir) : 3, (int)Math.Ceiling((double)(Math.Abs(player.Speed.X) * Engine.DeltaTime)) + 1);
        Scene scene = player.Scene;
        int num2 = (includeOrig ? 4 : 0);
        if (num >= num2)
        {
            foreach (Component component in q)
            {
                SolidModifierComponent solidModifierComponent = (SolidModifierComponent)component;
                Solid solid = solidModifierComponent.Entity as Solid;
                if (solid != null)
                {
                    int num3 = ((solidModifierComponent.CornerBoostBlock > 0) ? solidModifierComponent.CornerBoostBlock : num);
                    for (int i = num2; i <= num3; i++)
                    {
                        if (player.ClimbBoundsCheck(dir) && !ClimbBlocker.EdgeCheck(scene, player, dir * i) && Collide.Check(player, solid, player.Position + Vector2.UnitX * (float)(dir * i)))
                        {
                            Md.Session.currentActiveSolidModifier = solidModifierComponent;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        return false;
    }

    private static bool LegacyCollideCheck(Solid solid, Player player, int a)
    {
        bool flag = false;
        int num = ((a < 0) ? Math.Max(3, (int)Math.Ceiling((double)(Math.Abs(player.Speed.X) * Engine.DeltaTime))) : 3);
        for (int i = 0; i <= num; i++)
        {
            flag |= ((a > 1) ? solid.CollideCheck(player, solid.Position - Vector2.UnitX * (float)((int)player.Facing * i)) : player.CollideCheck(solid, player.Position + Vector2.UnitX * (float)((int)player.Facing * i)));
            if (flag)
            {
                break;
            }
        }
        return flag;
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        if (self.Scene != null)
        {
            List<Component> list;
            if (self.Scene.Tracker.Components.TryGetValue(typeof(SolidModifierComponent), out list))
            {
                foreach (Component component in list)
                {
                    ((SolidModifierComponent)component).HasBeenBufferJumpedOn = false;
                }
            }
            orig.Invoke(self);
        }
    }

    public SolidModifierComponent(int cornerBoostBlock, bool bufferClimbJump, bool triggerClimbOnTouch, bool onTouchFromBelow = false)
        : base(true, false)
    {
        this.CornerBoostBlock = -cornerBoostBlock;
        this.bufferClimbJump = bufferClimbJump;
        this.triggerClimbOnTouch = triggerClimbOnTouch;
        this.OnTouchFromBelow = onTouchFromBelow;

        Ldm.LoadHookWithMethod(this, Load);
    }

    public SolidModifierComponent(SolidModifierComponent c)
        : base(true, false)
    {
        this.CornerBoostBlock = c.CornerBoostBlock;
        this.bufferClimbJump = c.bufferClimbJump;
        this.triggerClimbOnTouch = c.triggerClimbOnTouch;
        this.OnTouchFromBelow = c.OnTouchFromBelow;

        Ldm.LoadHookWithMethod(this, Load);
    }

    public override string ToString()
    {
        return string.Format("SMC: {0}\nBuffer: {1}\tTouch: {2}\nBelow: {3}\n", new object[] { this.CornerBoostBlock, this.bufferClimbJump, this.triggerClimbOnTouch, this.OnTouchFromBelow });
    }

    private static readonly string[] LegacyMapSIDs = new string[] { "MOCE/issy/MOCE_CPCL", "MOCE/issy/MOCE_LXVI", "Tardigrade/WaterbearMountain/WaterbearMountain", "Cabob/AmberTerminus/1-AmberTerminus", "Cabob/AmberTerminus/1-AmberTerminus-B", "AnarchyCollab2022/1-Submissions/02_ellatas", "AnarchyCollab2022/1-Submissions/22_pogseal", "AnarchyCollab2022/1-Submissions/ZZ_heartside", "WinterCollab2021/1-Maps/Jackal", "WinterCollab2021/1-Maps/Jackal_KAERRA" };

    public int CornerBoostBlock;

    public bool CornerBoostRetention;

    public bool bufferClimbJump;

    public bool triggerClimbOnTouch;

    public bool OnTouchFromBelow;

    public bool Legacy;

    public bool HasBeenBufferJumpedOn;

    public static void LateInitialize(On.Celeste.GameLoader.orig_Begin orig, GameLoader self)
    {
        orig.Invoke(self);
        SolidModifierComponent.player_WallJumpCheck_getNum = delegate (Player player, int dir)
        {
            int num = 3;
            bool flag = player.DashAttacking && player.DashDir.X == 0f && player.DashDir.Y == -1f;
            bool flag2 = flag;
            if (flag2)
            {
                Spikes.Directions directions = ((dir <= 0) ? Spikes.Directions.Right : Spikes.Directions.Left);
                foreach (Entity entity in player.Scene.Tracker.GetEntities<Spikes>())
                {
                    Spikes spikes = (Spikes)entity;
                    bool flag3 = spikes.Direction == directions && player.CollideCheck(spikes, player.Position + Vector2.UnitX * (float)dir * 5f);
                    if (flag3)
                    {
                        flag = false;
                        break;
                    }
                }
            }
            bool flag4 = flag;
            if (flag4)
            {
                num = 5;
            }
            return num;
        };
    }
}
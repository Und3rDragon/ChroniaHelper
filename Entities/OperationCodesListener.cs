using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Lis = ChroniaHelper.Entities.OperationCodesListener.OperationCodeData.Listener;
using OPC = ChroniaHelper.Entities.OperationCodesListener.OperationCodeData.OperationCode;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/OperationCodesListener")]
public class OperationCodesListener : BaseEntity
{
    public OperationCodeData data = new();
    public OperationCodesListener(EntityData d, Vc2 o) : base(d, o)
    {
        data = new();
        
        data.InterceptLength = d.Int("interceptLength", 5);

        data.TargetOperations = new();
        string[] sequence = d.Attr("targetSequence").Split(";", StringSplitOptions.TrimEntries);
        foreach (var seq in sequence)
        {
            OPC code = OPC.None;
            string[] buttons = seq.Split(",", StringSplitOptions.TrimEntries);
            foreach (var button in buttons)
            {
                int parse = button.ParseInt(-1).Clamp(-1, 13);
                if (parse == -1) { code |= OPC.None; continue; }

                code |= data.Encode(parse);
            }

            data.TargetOperations.Add(code);
        }

        data.Console = d.Bool("logOperationsInConsole", false);

        data.ListenMode = (OperationCodeData.Listener)d.Int("listener", 1);

        data.Flag = d.Attr("flag");

        Tag = Tags.TransitionUpdate;
    }

    protected override void AddedExecute(Scene scene)
    {
        Md.Session.operationCodeListeners.Enter(ID, data);
    }

    protected override void RemovedExecute(Scene scene)
    {
        Md.Session.operationCodeListeners.SafeRemove(this.ID);
    }
    
    public class OperationCodeData
    {
        [Flags]
        public enum Listener
        {
            None = 0,
            Press = 1,
            Hold = 2,
        }
        [Flags]
        public enum OperationCode
        {
            None = 0,
            ESC = 1 << 0,
            Pause = 1 << 1,
            MenuLeft = 1 << 2,
            MenuRight = 1 << 3,
            MenuUp = 1 << 4,
            MenuDown = 1 << 5,
            MenuConfirm = 1 << 6,
            MenuCancel = 1 << 7,
            MenuJournal = 1 << 8,
            QuickRestart = 1 << 9,
            Jump = 1 << 10,
            Dash = 1 << 11,
            Grab = 1 << 12,
            Talk = 1 << 13,
            CrouchDash = 1 << 14,
        }
        [YamlIgnore]
        public Func<VirtualButton, bool> Press = (button) => button.Pressed;
        [YamlIgnore]
        public Func<VirtualButton, bool> Holding = (button) => button.Check;
        [YamlIgnore]
        public List<VirtualButton> Ref = new()
        {
            Input.ESC,
            Input.Pause,
            Input.MenuLeft,
            Input.MenuRight,
            Input.MenuUp,
            Input.MenuDown,
            Input.MenuConfirm,
            Input.MenuJournal,
            Input.QuickRestart,
            Input.Jump,
            Input.Dash,
            Input.Grab,
            Input.Talk,
            Input.CrouchDash,
        };
        [YamlIgnore]
        public Dictionary<VirtualButton, OperationCode> Encoder = new()
        {
            { Input.ESC, OperationCode.ESC },
            { Input.Pause, OperationCode.Pause },
            { Input.MenuLeft, OperationCode.MenuLeft },
            { Input.MenuRight, OperationCode.MenuRight },
            { Input.MenuUp, OperationCode.MenuUp },
            { Input.MenuDown, OperationCode.MenuDown },
            { Input.MenuConfirm, OperationCode.MenuConfirm },
            { Input.MenuJournal, OperationCode.MenuJournal },
            { Input.QuickRestart, OperationCode.QuickRestart },
            { Input.Jump, OperationCode.Jump },
            { Input.Dash, OperationCode.Dash },
            { Input.Grab, OperationCode.Grab },
            { Input.Talk, OperationCode.Talk },
            { Input.CrouchDash, OperationCode.CrouchDash },
        };
        public OperationCodeData() { }
        public string Flag { get; set; } = "flag";
        public Listener ListenMode { get; set; } = Listener.Press;
        public bool Console { get; set; } = false;
        public int InterceptLength { get; set; } = 5;
        public List<OperationCode> TargetOperations { get; set; } = new();

        [YamlIgnore]
        public OperationCode opCode1, opCode2, _opCode1, _opCode2;
        [YamlIgnore]
        public List<OperationCode> operationsA = new(), operationsB = new(), recordA = new(), recordB = new();
        
        public OperationCode Encode(VirtualButton button)
        {
            return Encoder[button];
        }

        public OperationCode Encode(int button)
        {
            int parse = button.ParseInt(-1).Clamp(-1, 13);
            if (parse == -1) { return OPC.None; }

            return Encoder[Ref[parse]];
        }
    }
    
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Update += LevelUpdate;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Update -= LevelUpdate;
    }
    
    public static void LevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        
        foreach (var op in Md.Session.operationCodeListeners.Values)
        {
            op.opCode1 = op.opCode2 = OPC.None;

            if ((op.ListenMode & OperationCodeData.Listener.Press) != 0)
            {
                foreach (var item in op.Ref)
                {
                    if (op.Press(item))
                    {
                        op.opCode1 |= op.Encoder[item];
                    }
                }

                if (op.operationsA.Count < op.InterceptLength)
                {
                    if (op.opCode1 != OPC.None && op.opCode1 != op._opCode1)
                    {
                        op.operationsA.Add(op.opCode1);
                    }
                }
                else if (op.operationsA.Count == op.InterceptLength)
                {
                    if (op.opCode1 != OPC.None && op.opCode1 != op._opCode1)
                    {
                        op.operationsA.Remove(op.operationsA[0]);
                        op.operationsA.Add(op.opCode1);
                    }
                }
                else
                {
                    if (op.opCode1 != OPC.None && op.opCode1 != op._opCode1)
                    {
                        op.operationsA.Add(op.opCode1);
                    }
                    while (op.operationsA.Count > op.InterceptLength)
                    {
                        op.operationsA.Remove(op.operationsA[0]);
                    }
                }
            }

            if ((op.ListenMode & Lis.Hold) != 0)
            {
                foreach (var item in op.Ref)
                {
                    if (op.Holding(item))
                    {
                        op.opCode2 |= op.Encoder[item];
                    }
                }

                if (op.operationsB.Count < op.InterceptLength)
                {
                    if (op.opCode2 != OPC.None && op.opCode2 != op._opCode2)
                    {
                        op.operationsB.Add(op.opCode2);
                    }
                }
                else if (op.operationsB.Count == op.InterceptLength)
                {
                    if (op.opCode2 != OPC.None && op.opCode2 != op._opCode2)
                    {
                        op.operationsB.Remove(op.operationsB[0]);
                        op.operationsB.Add(op.opCode2);
                    }
                }
                else
                {
                    if (op.opCode2 != OPC.None && op.opCode2 != op._opCode2)
                    {
                        op.operationsB.Add(op.opCode2);
                    }
                    while (op.operationsB.Count > op.InterceptLength)
                    {
                        op.operationsB.Remove(op.operationsB[0]);
                    }
                }
            }

            if (op.recordA.Count == op.TargetOperations.Count && op.recordA.Count > 0)
            {
                if (op.Console)
                {
                    Log.Info("Success on Press!");
                }
                self.Session.SetFlag(op.Flag, true);
                op.recordA.Clear();
            }
            else if (op.opCode1 != OPC.None && op.opCode1 != op._opCode1)
            {
                if (op.opCode1 == op.TargetOperations[op.recordA.Count])
                {
                    op.recordA.Add(op.opCode1);
                }
                else
                {
                    op.recordA.Clear();
                }
            }

            if (op.recordB.Count == op.TargetOperations.Count && op.recordB.Count > 0)
            {
                if (op.Console)
                {
                    Log.Warn("Success on Hold!");
                }
                self.Session.SetFlag(op.Flag, true);
                op.recordB.Clear();
            }
            else if (op.opCode2 != OPC.None && op.opCode2 != op._opCode2)
            {
                if (op.opCode2 == op.TargetOperations[op.recordB.Count])
                {
                    op.recordB.Add(op.opCode2);
                }
                else
                {
                    op.recordB.Clear();
                }
            }

            op._opCode1 = op.opCode1;
            op._opCode2 = op.opCode2;


            if (op.Console && self.OnInterval(0.1f))
            {
                Log.Info("Match on Press:");
                Log.Each(op.recordA, LogLevel.Info);
                Log.Info("Match on Hold:");
                Log.Each(op.recordB, LogLevel.Warn);
                Log.Warn(Log.divider);

                Log.Info("Intercepted on Press:");
                Log.Each(op.operationsA, LogLevel.Info);
                Log.Info("Intercepted on Hold:");
                Log.Each(op.operationsB, LogLevel.Warn);
                Log.Divider(LogLevel.Error);
            }
        }
    }
}

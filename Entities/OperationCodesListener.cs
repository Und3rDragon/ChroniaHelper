using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using Microsoft.Xna.Framework.Input;
using YamlDotNet.Core.Events;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/OperationCodesListener")]
public class OperationCodesListener : BaseEntity
{
    public OperationCodesListener(EntityData d, Vc2 o) : base(d, o)
    {
        listener = d.Int("interceptLength", 5);

        target = new();
        string[] sequence = d.Attr("targetSequence").Split(";", StringSplitOptions.TrimEntries);
        foreach(var seq in sequence)
        {
            OperationCode code = OperationCode.None;
            string[] buttons = seq.Split(",", StringSplitOptions.TrimEntries);
            foreach(var button in buttons)
            {
                int parse = button.ParseInt(-1).Clamp(-1, 13);
                if(parse == -1) { code |= OperationCode.None; continue; }
                
                code |= Encoder[Ref[parse]];
            }

            target.Add(code);
        }

        console = d.Bool("logOperationsInConsole", false);

        listenMode = (Listener)d.Int("listener", 1);

        flag = d.Attr("flag");

        Tag = Tags.TransitionUpdate;
    }
    private string flag;
    private Listener listenMode = Listener.Press;
    private bool console = false;
    private int listener = 5;
    public List<OperationCode> target = new();
    public List<OperationCode> operationsA = new(), operationsB = new(), recordA = new(), recordB = new();

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

    public Func<VirtualButton, bool> Press = (button) => button.Pressed;
    //{
    //    { Input.ESC, Input.ESC.Pressed },
    //    { Input.Pause, Input.Pause.Pressed },
    //    { Input.MenuLeft, Input.MenuLeft.Pressed },
    //    { Input.MenuRight, Input.MenuRight.Pressed },
    //    { Input.MenuUp, Input.MenuUp.Pressed },
    //    { Input.MenuDown, Input.MenuDown.Pressed },
    //    { Input.MenuConfirm, Input.MenuConfirm.Pressed },
    //    { Input.MenuJournal, Input.MenuJournal.Pressed },
    //    { Input.QuickRestart, Input.QuickRestart.Pressed },
    //    { Input.Jump, Input.Jump.Pressed },
    //    { Input.Dash, Input.Dash.Pressed },
    //    { Input.Grab, Input.Grab.Pressed },
    //    { Input.Talk, Input.Talk.Pressed },
    //    { Input.CrouchDash, Input.CrouchDash.Pressed },
    //};

    public Func<VirtualButton, bool> Holding = (button) => button.Check;
    //{
    //    { Input.ESC, Input.ESC.Check },
    //    { Input.Pause, Input.Pause.Check },
    //    { Input.MenuLeft, Input.MenuLeft.Check },
    //    { Input.MenuRight, Input.MenuRight.Check },
    //    { Input.MenuUp, Input.MenuUp.Check },
    //    { Input.MenuDown, Input.MenuDown.Check },
    //    { Input.MenuConfirm, Input.MenuConfirm.Check },
    //    { Input.MenuJournal, Input.MenuJournal.Check },
    //    { Input.QuickRestart, Input.QuickRestart.Check },
    //    { Input.Jump, Input.Jump.Check },
    //    { Input.Dash, Input.Dash.Check },
    //    { Input.Grab, Input.Grab.Check },
    //    { Input.Talk, Input.Talk.Check },
    //    { Input.CrouchDash, Input.CrouchDash.Check },
    //};

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

    private OperationCode code1 = OperationCode.None, 
        _code1 = OperationCode.None, 
        code2 = OperationCode.None, 
        _code2 = OperationCode.None;
    protected override void AddedExecute(Scene scene)
    {
        _code1 = _code2 = OperationCode.None;
    }
    protected override void UpdateExecute()
    {
        code1 = code2 = OperationCode.None;
        
        if((listenMode & Listener.Press) != 0)
        {
            foreach (var item in Encoder)
            {
                if (Press(item.Key))
                {
                    code1 |= Encoder[item.Key];
                }
            }

            if (operationsA.Count < listener)
            {
                if (code1 != OperationCode.None && code1 != _code1)
                {
                    operationsA.Add(code1);
                }
            }
            else if (operationsA.Count == listener)
            {
                if (code1 != OperationCode.None && code1 != _code1)
                {
                    operationsA.Remove(operationsA[0]);
                    operationsA.Add(code1);
                }
            }
            else
            {
                if (code1 != OperationCode.None && code1 != _code1)
                {
                    operationsA.Add(code1);
                }
                while (operationsA.Count > listener)
                {
                    operationsA.Remove(operationsA[0]);
                }
            }
        }
        
        if((listenMode & Listener.Hold) != 0)
        {
            foreach (var item in Encoder)
            {
                if (Holding(item.Key))
                {
                    code2 |= Encoder[item.Key];
                }
            }

            if (operationsB.Count < listener)
            {
                if (code2 != OperationCode.None && code2 != _code2)
                {
                    operationsB.Add(code2);
                }
            }
            else if (operationsB.Count == listener)
            {
                if (code2 != OperationCode.None && code2 != _code2)
                {
                    operationsB.Remove(operationsB[0]);
                    operationsB.Add(code2);
                }
            }
            else
            {
                if (code2 != OperationCode.None && code2 != _code2)
                {
                    operationsB.Add(code2);
                }
                while (operationsB.Count > listener)
                {
                    operationsB.Remove(operationsB[0]);
                }
            }
        }

        if (recordA.Count == target.Count && recordA.Count > 0)
        {
            SuccessA();
        }
        else if (code1 != OperationCode.None && code1 != _code1)
        {
            if(code1 == target[recordA.Count])
            {
                recordA.Add(code1);
            }
            else
            {
                recordA.Clear();
            }
        }

        if (recordB.Count == target.Count && recordB.Count > 0)
        {
            SuccessB();
        }
        else if (code2 != OperationCode.None && code2 != _code2)
        {
            if (code2 == target[recordB.Count])
            {
                recordB.Add(code2);
            }
            else
            {
                recordB.Clear();
            }
        }

        _code1 = code1;
        _code2 = code2;
        
        
        if (console)
        {
            Log.Info("Match on Press:");
            Log.Each(recordA, LogLevel.Info);
            Log.Info("Match on Hold:");
            Log.Each(recordB, LogLevel.Warn);
            Log.Warn(Log.divider);

            Log.Info("Intercepted on Press:");
            Log.Each(operationsA, LogLevel.Info);
            Log.Info("Intercepted on Hold:");
            Log.Each(operationsB, LogLevel.Warn);
            Log.Divider(LogLevel.Error);
        }
    }
    
    private void SuccessA()
    {
        if (console)
        {
            Log.Error("success on press");
        }
        if (flag.IsNotNullOrEmpty())
        {
            flag.SetFlag(true);
        }
    }
    
    private void SuccessB()
    {
        if (console)
        {
            Log.Error("success on hold");
        }
        if (flag.IsNotNullOrEmpty())
        {
            flag.SetFlag(true);
        }
    }
}

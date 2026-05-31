using System.Collections;
using System.Reflection.Metadata.Ecma335;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using FMOD.Studio;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetSessionValueSequenceController")]
public class SetSessionValueSequenceController : GeneralSetupController
{
    public SetSessionValueSequenceController(EntityData data, Vc2 offset) : base(data, offset)
    {
        sequences = data.Attr("sequence");
        DecryptSequence();
        global = data.Bool("global", false);
    }
    private string sequences;
    private bool global;

    private enum Operations
    {
        None = 0, 
        Start = 1, End = 1, 
        Command = 2,
        SubCommand = 3,
        Number = 4,
        Bool = 5,
        ValueFrom = 6,
        FlagDeclaration = 7,
        CounterDeclaration = 7,
        SliderDeclaration = 7,
        Name = 8,
    }

    private struct OpData
    {
        public Operations Operation;
        public object Value = null;

        public OpData(Operations op, object v = null)
        {
            Operation = op;
            Value = v;
        }
    }
    private List<OpData> seq = new();
    public void DecryptSequence()
    {
        seq.Clear();

        int label = 0;
        bool commandBreak = false;
        seq.Add(new OpData(Operations.Start));
        int charIndex = 0;

        for (charIndex = 0; charIndex < sequences.Length; charIndex++)
        {
            commandBreak = false;
            switch (sequences[charIndex])
            {
                case ';':
                    seq.Add(new OpData(Operations.Command));
                    commandBreak = true;
                    break;
                case ',':
                    seq.Add(new OpData(Operations.SubCommand));
                    commandBreak = true;
                    break;
                case '=':
                    seq.Add(new OpData(Operations.ValueFrom));
                    commandBreak = true;
                    break;
                default:
                    break;
            }

            if (commandBreak)
            {
                if(label <= charIndex - 1)
                {
                    StringToOperation(sequences.Substring(label, charIndex - label));
                }

                label = charIndex + 1;
                continue;
            }
        }

        if (label <= charIndex - 1)
        {
            StringToOperation(sequences.Substring(label, charIndex - label));
        }

        seq.Add(new OpData(Operations.End));
    }

    public void StringToOperation(string input)
    {
        string _input = input.Trim();

        List<string> trueStates = new() { "true", "t" };
        List<string> falseState = new() { "false", "f" };

        if(int.TryParse(_input, out int n))
        {
            seq.Add(new OpData(Operations.Number, n));
        }
        else if(float.TryParse(_input, out float f))
        {
            seq.Add(new OpData(Operations.Number, f));
        }
        else if (_input.ToLower().StartsWith("flag:"))
        {
            seq.Add(new OpData(Operations.FlagDeclaration, _input.Remove(0, 5)));
        }
        else if (_input.ToLower().StartsWith("slider:"))
        {
            seq.Add(new OpData(Operations.SliderDeclaration, _input.Remove(0, 7)));
        }
        else if (_input.ToLower().StartsWith("counter:"))
        {
            seq.Add(new OpData(Operations.CounterDeclaration, _input.Remove(0, 8)));
        }
        else if (trueStates.Contains(_input.ToLower()))
        {
            seq.Add(new OpData(Operations.Bool, true));
        }
        else if (falseState.Contains(_input.ToLower()))
        {
            seq.Add(new OpData(Operations.Bool, false));
        }
        else
        {
            seq.Add(new OpData(Operations.Name, _input));
        }
    }

    public override void ApplyValue()
    {
        PrepareSequence();
    }
    
    public void PrepareSequence()
    {
        if (global)
        {
            MaP.dummyGlobal.Add(new Coroutine(MainSequence()));
        }
        else
        {
            Add(new Coroutine(MainSequence()));
        }
    }
    
    public IEnumerator MainSequence()
    {
        // Start Sorting Commands
        int n = 0;
        int left = 0, right = 0;
        while (n < seq.Count)
        {
            var s = seq[n];

            if (s.Operation == Operations.Start)
            {
                // Index + 1, set left barrier and repeat
                left = 1; n++; 
                continue;
            }
            else if (s.Operation == Operations.End)
            {
                // The sequence is ended, parse final command
                right = seq.Count - 2;
                if(left <= right)
                {
                    yield return new SwapImmediately(ParseCommand(left, right));
                }
                // Set right barrier and quit
                break;
            }
            else if (s.Operation == Operations.Command)
            {
                // Found primary command, set right barrier
                right = n - 1;
                // If the barrier is valid, this range would represent a main command
                if (left <= right)
                {
                    yield return new SwapImmediately(ParseCommand(left, right));
                }
                // Push the left barrier to the right of the current index
                left = n + 1;
            }

            n++;
        }

        yield return null;
    }

    public IEnumerator ParseCommand(int a, int b)
    {
        int n = a;
        int left = a, right = a;
        List<int> toAssign = new();

        while (n >= a && n <= b)
        {
            var s = seq[n];

            if(s.Operation == Operations.SubCommand)
            {
                // Found a sub, set right barrier
                right = n - 1;
                // If valid, parse sub-command
                if(left <= right)
                {
                    yield return new SwapImmediately(ParseSubCommand(left, right));
                }
                //Then set the left barrier
                left = n + 1;
            }

            // If ignoring the logic above, this means this command is only consists of one signle sub-command
            // Therefore the logic below should only handle one single sub-command

            // Matching a string
            if((int)s.Operation >= 7)
            {
                // The command cannot be made by just a name ("name")
                if(n == b && n > a)
                {
                    // Check if it's the source value? ("= name")
                    if (seq[n - 1].Operation == Operations.ValueFrom)
                    {
                        yield return new SwapImmediately(AssignValues(toAssign, n));
                    }
                }
                else if(n < b)
                {
                    // Check if it's a valid assign equation ("name =")
                    if (seq[n + 1].Operation == Operations.ValueFrom)
                    {
                        toAssign.Add(n);
                    }
                }
            }
            // Matching a number
            if(s.Operation == Operations.Number)
            {
                // This command only has a number value? ("number")
                if(n == a && n == b)
                {
                    yield return (float)s.Value;
                }
                // Check if it's the source value? ("= number")
                else if(n == b && n > a)
                {
                    // This represents the source value
                    if (seq[n - 1].Operation == Operations.ValueFrom)
                    {
                        yield return new SwapImmediately(AssignValues(toAssign, n));
                    }
                }
            }
            // Matching a bool
            if (s.Operation == Operations.Bool)
            {
                // The command cannot be made by just a bool ("bool")
                // Check if it's the source value? ("= bool")
                if (n == b && n > a)
                {
                    // This represents the source value
                    if (seq[n - 1].Operation == Operations.ValueFrom)
                    {
                        yield return new SwapImmediately(AssignValues(toAssign, n));
                    }
                }
            }

            n++;
        }

        yield return null;
    }

    public IEnumerator ParseSubCommand(int a, int b)
    {
        int n = a;
        List<int> toAssign = new();

        while (n >= a && n <= b)
        {
            var s = seq[n];

            // Matching a string
            if ((int)s.Operation >= 7)
            {
                // The command cannot be made by just a name ("name")
                if (n == b && n > a)
                {
                    // Check if it's the source value? ("= name")
                    if (seq[n - 1].Operation == Operations.ValueFrom)
                    {
                        yield return new SwapImmediately(AssignValues(toAssign, n));
                    }
                }
                else if (n < b)
                {
                    // Check if it's a valid assign equation ("name =")
                    if (seq[n + 1].Operation == Operations.ValueFrom)
                    {
                        toAssign.Add(n);
                    }
                }
            }
            // Matching a number
            if (s.Operation == Operations.Number)
            {
                // This command only has a number value? ("number")
                if (n == a && n == b)
                {
                    yield return (float)s.Value;
                }
                // Check if it's the source value? ("= number")
                else if (n == b && n > a)
                {
                    // This represents the source value
                    if (seq[n - 1].Operation == Operations.ValueFrom)
                    {
                        yield return new SwapImmediately(AssignValues(toAssign, n));
                    }
                }
            }
            // Matching a bool
            if (s.Operation == Operations.Bool)
            {
                // The command cannot be made by just a bool ("bool")
                // Check if it's the source value? ("= bool")
                if (n == b && n > a)
                {
                    // This represents the source value
                    if (seq[n - 1].Operation == Operations.ValueFrom)
                    {
                        yield return new SwapImmediately(AssignValues(toAssign, n));
                    }
                }
            }

            n++;
        }

        yield return null;
    }

    public IEnumerator AssignValues(List<int> targets, int value)
    {
        var source = seq[value];

        foreach(var i in targets)
        {
            var tar = seq[i];

            if (tar.Operation == Operations.Name)
            {
                string name = tar.Value.ToString();
                
                if(source.Operation == Operations.Number)
                {
                    if(source.Value is int)
                    {
                        name.SetCounter((int)source.Value);
                    }
                    else
                    {
                        name.SetSlider((float)source.Value);
                    }
                }
                else if(source.Operation == Operations.Bool)
                {
                    name.SetFlag((bool)source.Value);
                }
            }
            else if(tar.Operation == Operations.FlagDeclaration)
            {
                string name = tar.Value.ToString();

                if (source.Operation == Operations.Number)
                {
                    name.SetFlag((float)source.Value != 0);
                }
                else if (source.Operation == Operations.Bool)
                {
                    name.SetFlag((bool)source.Value);
                }
            }
            else if(tar.Operation == Operations.CounterDeclaration)
            {
                string name = tar.Value.ToString();

                if (source.Operation == Operations.Number)
                {
                    name.SetCounter((int)source.Value);
                }
                else if (source.Operation == Operations.Bool)
                {
                    name.SetCounter((bool)source.Value ? 1 : 0);
                }
            }
            else if (tar.Operation == Operations.SliderDeclaration)
            {
                string name = tar.Value.ToString();

                if (source.Operation == Operations.Number)
                {
                    name.SetSlider((float)source.Value);
                }
                else if (source.Operation == Operations.Bool)
                {
                    name.SetSlider((bool)source.Value ? 1f : 0);
                }
            }
        }

        yield return null;
    }
}

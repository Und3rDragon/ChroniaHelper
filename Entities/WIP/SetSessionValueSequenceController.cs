using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using FMOD.Studio;

namespace ChroniaHelper.Entities.WIP;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetSessionValueSequenceController")]
[WorkingInProgress]
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

        seq.Add(new OpData(Operations.Start));
        int charIndex = 0;
        string current = "";

        for (charIndex = 0; charIndex < sequences.Length; charIndex++)
        {
            char c = sequences[charIndex];

            if (c == ';')
            {
                if (current.HasValidContent())
                {
                    StringToOperation(current);
                }
                current = "";
                seq.Add(new OpData(Operations.Command));
                continue;
            }
            else if (c == ',')
            {
                if (current.HasValidContent())
                {
                    StringToOperation(current);
                }
                current = "";
                seq.Add(new OpData(Operations.SubCommand));
                continue;
            }
            else if (c == '=')
            {
                if (current.HasValidContent())
                {
                    StringToOperation(current);
                }
                current = "";
                seq.Add(new OpData(Operations.ValueFrom));
                continue;
            }

            current += c;
        }

        if (current.HasValidContent())
        {
            StringToOperation(current);
        }

        seq.Add(new OpData(Operations.End));
    }

    public void StringToOperation(string input)
    {
        string _input = input.Trim();

        List<string> trueStates = new() { "true", "t" };
        List<string> falseState = new() { "false", "f" };

        if (int.TryParse(_input, out int n))
        {
            seq.Add(new OpData(Operations.Number, n));
        }
        else if (float.TryParse(_input, out float f))
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

    public override void Execute()
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

    public void AssignValues(List<int> targets, int value)
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
                else if(source.Operation == Operations.FlagDeclaration)
                {
                    name.SetFlag(source.Value.ToString().GetFlag());
                }
                else if(source.Operation == Operations.CounterDeclaration)
                {
                    name.SetCounter(source.Value.ToString().GetCounter());
                }
                else if(source.Operation == Operations.SliderDeclaration)
                {
                    name.SetSlider(source.Value.ToString().GetSlider());
                }
                else if(source.Operation == Operations.Name)
                {
                    if (MaP.sliders.ContainsKey(source.Value.ToString()))
                    {
                        name.SetSlider(source.Value.ToString().GetSlider());
                    }
                    else if(MaP.level.Session.Counters.TryGet(
                        (c) => c.Key == source.Value.ToString(), 
                        out List<Session.Counter> n))
                    {
                        name.SetCounter(source.Value.ToString().GetCounter());
                    }
                    else
                    {
                        name.SetFlag(source.Value.ToString().GetFlag());
                    }
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
                else if (source.Operation == Operations.FlagDeclaration)
                {
                    name.SetFlag(source.Value.ToString().GetFlag());
                }
                else if (source.Operation == Operations.CounterDeclaration)
                {
                    name.SetFlag(source.Value.ToString().GetCounter() != 0);
                }
                else if (source.Operation == Operations.SliderDeclaration)
                {
                    name.SetFlag(source.Value.ToString().GetSlider() != 0);
                }
                else if (source.Operation == Operations.Name)
                {
                    if (MaP.sliders.ContainsKey(source.Value.ToString()))
                    {
                        name.SetFlag(source.Value.ToString().GetSlider() != 0);
                    }
                    else if (MaP.level.Session.Counters.TryGet(
                        (c) => c.Key == source.Value.ToString(),
                        out List<Session.Counter> n))
                    {
                        name.SetFlag(source.Value.ToString().GetCounter() != 0);
                    }
                    else
                    {
                        name.SetFlag(source.Value.ToString().GetFlag());
                    }
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
                else if (source.Operation == Operations.FlagDeclaration)
                {
                    name.SetCounter(source.Value.ToString().GetFlag() ? 1 : 0);
                }
                else if (source.Operation == Operations.CounterDeclaration)
                {
                    name.SetCounter(source.Value.ToString().GetCounter());
                }
                else if (source.Operation == Operations.SliderDeclaration)
                {
                    name.SetCounter((int)source.Value.ToString().GetSlider());
                }
                else if (source.Operation == Operations.Name)
                {
                    if (MaP.sliders.ContainsKey(source.Value.ToString()))
                    {
                        name.SetCounter((int)source.Value.ToString().GetSlider());
                    }
                    else if (MaP.level.Session.Counters.TryGet(
                        (c) => c.Key == source.Value.ToString(),
                        out List<Session.Counter> n))
                    {
                        name.SetCounter(source.Value.ToString().GetCounter());
                    }
                    else
                    {
                        name.SetCounter(source.Value.ToString().GetFlag() ? 1 : 0);
                    }
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
                else if (source.Operation == Operations.FlagDeclaration)
                {
                    name.SetSlider(source.Value.ToString().GetFlag() ? 1f : 0);
                }
                else if (source.Operation == Operations.CounterDeclaration)
                {
                    name.SetSlider((float)source.Value.ToString().GetCounter());
                }
                else if (source.Operation == Operations.SliderDeclaration)
                {
                    name.SetSlider(source.Value.ToString().GetSlider());
                }
                else if (source.Operation == Operations.Name)
                {
                    if (MaP.sliders.ContainsKey(source.Value.ToString()))
                    {
                        name.SetSlider(source.Value.ToString().GetSlider());
                    }
                    else if (MaP.level.Session.Counters.TryGet(
                        (c) => c.Key == source.Value.ToString(),
                        out List<Session.Counter> n))
                    {
                        name.SetSlider((float)source.Value.ToString().GetCounter());
                    }
                    else
                    {
                        name.SetSlider(source.Value.ToString().GetFlag() ? 1f : 0);
                    }
                }
            }
        }
    }

    public IEnumerator MainSequence()
    {
        int index = 0;
        int seqLength = seq.Count;

        while (index < seqLength)
        {
            // Skip the Start operation
            if (seq[index].Operation == Operations.Start)
            {
                index++;
                continue;
            }

            // End with End operation
            if (seq[index].Operation == Operations.End)
            {
                break;
            }

            // Command: represents a new command is coming
            if (seq[index].Operation == Operations.Command)
            {
                index++;
                continue;
            }

            // Find the end of this command
            int commandStart = index;
            int commandEnd = FindCommandEnd(index, seqLength);

            // And execute this command
            IEnumerator result = ParseCommandSegment(commandStart, commandEnd);
            if (result != null)
            {
                yield return new SwapImmediately(result);
            }

            // Move on
            index = commandEnd + 1;
        }

        yield return null;
    }

    private int FindCommandEnd(int start, int seqLength)
    {
        for (int i = start + 1; i < seqLength; i++)
        {
            if (seq[i].Operation == Operations.Command || seq[i].Operation == Operations.End)
            {
                return i - 1;
            }
        }
        return seqLength - 1;
    }

    public IEnumerator ParseCommandSegment(int start, int end)
    {
        if (start > end) yield break;

        // Standalone Number: Delay
        if (start == end && seq[start].Operation == Operations.Number)
        {
            float delay = ConvertToFloat(seq[start].Value);
            yield return new SwapImmediately(TryDelay(delay));
            yield break;
        }

        // Subcommands
        List<(int subStart, int subEnd)> subRanges = new List<(int, int)>();
        int subStart = start;

        for (int i = start; i <= end; i++)
        {
            if (seq[i].Operation == Operations.SubCommand)
            {
                // Save the current index range of the subcommand
                if (subStart <= i - 1)
                {
                    subRanges.Add((subStart, i - 1));
                }
                subStart = i + 1;
            }
        }
        // Add one last subcommand
        if (subStart <= end)
        {
            subRanges.Add((subStart, end));
        }

        // Process subcommands
        foreach (var range in subRanges)
        {
            yield return new SwapImmediately(ParseSubCommand(range.subStart, range.subEnd));
        }

        yield return null;
    }

    public IEnumerator ParseSubCommand(int start, int end)
    {
        if (start > end) yield break;

        // Search for all indexes that's not ValueFrom
        List<int> elementIndices = new List<int>();
        for (int i = start; i <= end; i++)
        {
            if (seq[i].Operation != Operations.ValueFrom)
            {
                elementIndices.Add(i);
            }
        }

        if (elementIndices.Count == 0) yield break;

        // Check if it's valid source?
        if (elementIndices.Count == 1)
        {
            var op = seq[elementIndices[0]];
            // Standalone Number or Name has no meaning here, which can be ignored
            yield break;
        }

        // Chain assignments
        List<int> targets = new List<int>();
        int sourceIndex = -1;

        // Set the last value as Source
        sourceIndex = elementIndices[elementIndices.Count - 1];

        // The rest is set as Targets
        for (int i = 0; i < elementIndices.Count - 1; i++)
        {
            targets.Add(elementIndices[i]);
        }

        // Start the assignment
        if (sourceIndex >= 0 && targets.Count > 0)
        {
            AssignValues(targets, sourceIndex);
        }

        yield return null;
    }

    private float ConvertToFloat(object value)
    {
        if (value is int intVal) return intVal;
        if (value is float floatVal) return floatVal;
        return 0f;
    }

    private IEnumerator TryDelay(float seconds)
    {
        yield return seconds;
    }
}

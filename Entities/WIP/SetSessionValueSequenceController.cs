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
        int index = 0;
        int seqLength = seq.Count;

        while (index < seqLength)
        {
            // 找到下一个命令的起始位置（跳过 Start 和 Command）
            while (index < seqLength && (seq[index].Operation == Operations.Start || seq[index].Operation == Operations.Command))
            {
                index++;
            }

            if (index >= seqLength || seq[index].Operation == Operations.End)
            {
                break;
            }

            // 找到这个命令的结束位置
            int commandStart = index;
            int commandEnd = FindCommandEnd(index, seqLength);

            // 解析并执行这个命令
            IEnumerator result = ParseCommandSegment(commandStart, commandEnd);
            if (result != null)
            {
                yield return new SwapImmediately(result);
            }

            // 移动到下一个命令
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

        // 情况6: 单独的 Number（延迟命令）
        if (start == end && seq[start].Operation == Operations.Number)
        {
            float delay = ConvertToFloat(seq[start].Value);
            yield return new SwapImmediately(TryDelay(delay));
            yield break;
        }

        // 收集这个片段中的所有非 SubCommand 元素
        List<OpData> elements = new List<OpData>();
        for (int i = start; i <= end; i++)
        {
            if (seq[i].Operation != Operations.SubCommand)
            {
                elements.Add(seq[i]);
            }
        }

        if (elements.Count == 0) yield break;

        // 构建 targets 列表和 source 位置
        // 格式: [target, ValueFrom, target, ValueFrom, ..., source]
        List<int> targetIndices = new List<int>();
        int sourceIndex = -1;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Operation == Operations.ValueFrom)
            {
                // ValueFrom 前面的元素是目标
                if (i > 0)
                {
                    targetIndices.Add(i - 1);
                }
            }
            else
            {
                // 检查这个元素后面是否有 ValueFrom
                bool hasValueFromAfter = (i + 1 < elements.Count && elements[i + 1].Operation == Operations.ValueFrom);

                if (!hasValueFromAfter)
                {
                    // 没有后续的 ValueFrom，这是值来源
                    sourceIndex = i;
                    break;
                }
            }
        }

        // 执行赋值
        if (sourceIndex >= 0 && targetIndices.Count > 0)
        {
            // 获取 source 对应的原始终端索引
            int actualSourceIndex = FindOriginalIndex(start, end, elements[sourceIndex]);

            // 获取所有 target 对应的原始终端索引
            List<int> actualTargetIndices = new List<int>();
            foreach (int ti in targetIndices)
            {
                actualTargetIndices.Add(FindOriginalIndex(start, end, elements[ti]));
            }

            AssignValues(actualTargetIndices, actualSourceIndex);
        }

        yield return null;
    }

    private int FindOriginalIndex(int segmentStart, int segmentEnd, OpData targetElement)
    {
        for (int i = segmentStart; i <= segmentEnd; i++)
        {
            if (seq[i].Operation == targetElement.Operation &&
                Equals(seq[i].Value, targetElement.Value))
            {
                return i;
            }
        }
        return -1;
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
    }
}

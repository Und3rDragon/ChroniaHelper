using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using FMOD;
using YoctoHelper.Cores;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagStringTrigger")]
public class FlagStringTrigger : FlagManageTrigger
{

    private enum Execution
    {
        None,
        AddAsPrefix,
        AddAsSuffix,
        RemoveFirst,
        RemoveLast,
        RemoveAll,
        ReplaceFirst,
        ReplaceLast,
        ReplaceAll,
        Backspace,
        Delete,
        Clear
    }
    private Execution execution = Execution.None;

    public FlagStringTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        baseString = data.Attr("baseString");
        hasBase = !string.IsNullOrEmpty(baseString);
        newString = data.Attr("newString");
        execution = data.Enum<Execution>("executor",0);
        leaveReset = data.Bool("leaveReset", false);
        onlyOnce = data.Bool("onlyOnce", false);
    }
    private string baseString;
    private string newString;
    private HashSet<string> flags, oldFlags, newFlags;
    private bool hasBase;

    protected override void OnEnterExecute(Player player)
    {
        oldFlags = new HashSet<string>();
        newFlags = new HashSet<string>();
        flags = new HashSet<string>();
        oldFlags = flags = GetFlags();
        //foreach (var item in flags)
        //{
        //    Utils.Log.Info(item);
        //}
        //Utils.Log.Info("-----------level flags");

        foreach (var item in flags)
        {
            switch (execution)
            {
                case Execution.AddAsPrefix:
                    if ((hasBase && item.Contains(baseString)) || (!hasBase))
                    {
                        flags.Remove(item);

                        newFlags.Add(newString + item);
                    }
                    break;
                case Execution.AddAsSuffix:
                    if ((hasBase && item.Contains(baseString)) || (!hasBase))
                    {
                        flags.Remove(item);

                        newFlags.Add(item + newString);
                    }
                    break;
                case Execution.RemoveFirst:
                    if (hasBase && item.Contains(baseString))
                    {
                        flags.Remove(item);

                        int n = item.IndexOf(baseString);
                        newFlags.Add(item.Remove(n, baseString.Length));
                    }
                    break;
                case Execution.RemoveLast:
                    if (hasBase && item.Contains(baseString))
                    {
                        flags.Remove(item);

                        string a = item;
                        int index = a.IndexOf(baseString);
                        int start = a.IndexOf(baseString) + baseString.Length;
                        a = a.Substring(start);
                        while (a.Contains(baseString))
                        {
                            start = a.IndexOf(baseString) + baseString.Length;
                            index += start;
                            a = a.Substring(start);
                        }
                        newFlags.Add(item.Remove(index, baseString.Length));
                    }
                    break;
                case Execution.RemoveAll:
                    if(hasBase && item.Contains(baseString))
                    {
                        flags.Remove(item);

                        newFlags.Add(item.Replace(baseString, ""));
                    }
                    break;
                case Execution.ReplaceFirst:
                    if(hasBase && item.Contains(baseString)){
                        flags.Remove(item);

                        int n = item.IndexOf(baseString);
                        string s1 = item.Substring(0, n + baseString.Length);
                        string s2 = item.Substring(n + baseString.Length);
                        string f = s1.Replace(baseString, newString) + s2;
                        newFlags.Add(f);
                    }
                    break;
                case Execution.ReplaceLast: // !!
                    if (hasBase && item.Contains(baseString))
                    {
                        flags.Remove(item);

                        string a = item;
                        int index = a.IndexOf(baseString);
                        int start = a.IndexOf(baseString) + baseString.Length;
                        a = a.Substring(start);
                        while (a.Contains(baseString))
                        {
                            start = a.IndexOf(baseString) + baseString.Length;
                            index += start;
                            a = a.Substring(start);
                        }
                        string s1 = item.Substring(0, index);
                        string s2 = item.Substring(index);
                        newFlags.Add(s1 + s2.Replace(baseString, newString));
                    }
                    break;
                case Execution.ReplaceAll:
                    if(hasBase && item.Contains(baseString))
                    {
                        flags.Remove(item);

                        newFlags.Add(item.Replace(baseString, newString));
                    }
                    break;
                case Execution.Backspace:
                    if((hasBase && item.StartsWith(baseString)) || !hasBase)
                    {
                        flags.Remove(item);

                        string s = item;
                        if(s.Length <= 1) { break; }
                        s = s.Substring(0, s.Length - 1);
                        newFlags.Add(s);
                    }
                    break;
                case Execution.Delete:
                    if((hasBase && item.EndsWith(baseString)) || !hasBase)
                    {
                        flags.Remove(item);

                        string s = item;
                        if(s.Length <= 1) { break; }
                        s = s.Substring(1, s.Length - 1);
                        newFlags.Add(s);
                    }
                    break;
                case Execution.Clear:
                    if(hasBase && item.StartsWith(baseString))
                    {
                        flags.Remove(item);

                        newFlags.Add(baseString);
                    }
                    else if (!hasBase)
                    {
                        flags.Clear();

                        newFlags.Clear();
                    }
                    break;
                default:
                    break;

            }
        }
        foreach (var item in newFlags)
        {
            flags.Add(item);
        }
        //foreach (var item in flags)
        //{
        //    Utils.Log.Info(item);
        //}
        //Utils.Log.Info("-----------new set flags");
        base.Clear();
        SetFlags(flags);

    }

    protected override void LeaveReset(Player player)
    {
        base.Clear();
        SetFlags(oldFlags);
    }

}
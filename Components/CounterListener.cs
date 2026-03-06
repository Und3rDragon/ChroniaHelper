using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class CounterListener :  StateListener
{
    public CounterListener(string name, bool inverted, params int[] targets)
    {
        this.Name = name;
        this.Inverted = inverted;
        this.References = targets.ToList();
    }
    public string Name;
    public List<int> References = new();
    public bool Inverted = false;

    public CounterListener(string name, bool inverted, string expression)
    {
        this.Name = name;
        this.Inverted = inverted;
        string[] s = expression.Split(',', StringSplitOptions.TrimEntries);
        foreach(var item in s)
        {
            if (int.TryParse(item, out int n))
            {
                References.Add(n);
                continue;
            }

            if (item.Contains('-'))
            {
                string[] s1 = item.Split('-', StringSplitOptions.TrimEntries);
                List<int> indexes = new();
                foreach(var num in s1)
                {
                    if(int.TryParse(num, out int n1))
                    {
                        indexes.Add(n1);
                    }
                }

                if (indexes.Count == 0) { continue; }

                if (indexes.Count == 1) { References.Add(indexes[0]); }

                int m1 = indexes.GetMinItem(n => n);
                int m2 = indexes.GetMaxItem(n => n);

                for(int i = m1; i <= m2; i++)
                {
                    References.Add(i);
                }

                continue;
            }
        }
    }

    private int n = 0;
    protected override bool GetState()
    {
        n = Name.GetCounter();
        return (Inverted && !References.Contains(n)) ||
            (!Inverted && References.Contains(n));
    }
}

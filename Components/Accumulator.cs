using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class Accumulator<T> :  BaseComponent 
    where T : INumber<T>
{
    public Accumulator(T original)
    {
        OriginalValue = original;
    }
    public T OriginalValue;
    public List<T> Accumulations = new();

    public T Parsed()
    {
        T n = OriginalValue;
        foreach(T i in Accumulations)
        {
            n += i;
        }

        return n;
    }
}

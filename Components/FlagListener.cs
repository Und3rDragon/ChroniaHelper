using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Components;

public class FlagListener : StateListener
{
    public FlagListener(string flag)
    {
        this.flag = flag;
    }
    public string flag;

    protected override bool GetState()
    {
        return flag.GetFlag();
    }
}

public class FlagsListener : StateListener
{
    public FlagsListener(string flags, string separator = ",", string invertChar = "!")
    {
        this.flags = flags;
        this.separator = separator;
        this.invertChar = invertChar;
    }
    public string flags, separator, invertChar;

    protected override bool GetState()
    {
        return flags.GetGeneralFlags(separator, invertChar);
    }
}

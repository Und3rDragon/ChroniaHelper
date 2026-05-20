using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Components.StateListeners;

public class FlagListener : StateListener
{
    public FlagListener(string flag)
    {
        this.Flag = flag;
    }
    public string Flag;

    protected override bool GetState()
    {
        if (!Flag.HasValidContent())
        {
            return true;
        }

        return Flag.GetFlag();
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
        if (!flags.HasValidContent())
        {
            return true;
        }
        return flags.GetGeneralFlags(separator, invertChar);
    }
}

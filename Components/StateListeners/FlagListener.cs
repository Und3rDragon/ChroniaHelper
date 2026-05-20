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
    public FlagListener(string flag, bool fallback = true)
    {
        this.Flag = flag;
        this.Fallback = fallback;
    }
    public string Flag;
    public bool Fallback = true;

    protected override bool GetState()
    {
        if (!Flag.HasValidContent())
        {
            return Fallback;
        }

        return Flag.GetFlag();
    }
}

public class FlagsListener : StateListener
{
    public FlagsListener(string flags, string separator = ",", string invertChar = "!",
        bool fallback = true)
    {
        this.flags = flags;
        this.separator = separator;
        this.invertChar = invertChar;
        this.Fallback = fallback;
    }
    public string flags, separator, invertChar;
    public bool Fallback = true;

    protected override bool GetState()
    {
        if (!flags.HasValidContent())
        {
            return Fallback;
        }
        return flags.GetGeneralFlags(separator, invertChar);
    }
}

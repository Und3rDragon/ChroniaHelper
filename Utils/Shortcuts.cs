using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Modules;

namespace ChroniaHelper.Utils;

public static class Shortcuts
{
    public static Dictionary<string, ChroniaFlag> ChroniaFlags(this object obj)
    {
        return ChroniaHelperSaveData.ChroniaFlags;
    }

}

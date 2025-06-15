using System.Diagnostics;
using System.Reflection.Emit;

namespace ChroniaHelper.Modules;

public class ChroniaHelperSettings : EverestModuleSettings
{

    public enum Pages { WikiSet }

    public Pages page { get; set; } = Pages.WikiSet;

    [SettingSubMenu]
    public class WikiSet
    {
        
    }

}

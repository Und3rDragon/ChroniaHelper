using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils.StopwatchSystem;

public partial class Stopclock
{
    public static void Debug(string additionalTagInfo = "")
    {
        Md.Session.Stopclocks.EachDo((e) =>
        {
            Log.Info($"[{additionalTagInfo}] Session ({e.Key}): {e.Value.completed}, {e.Value.FormattedTime}");
        });
        Md.SaveData.stopclocks.EachDo((e) =>
        {
            Log.Info($"[{additionalTagInfo}] Global ({e.Key}): {e.Value.completed}, {e.Value.FormattedTime}");
        });
        Log.Error("_____________________________");
    }

    public void SelfDebug()
    {
        // 添加实例标识
        Log.Info($"[Instance {GetHashCode()}] {FormattedTime}");
    }
}

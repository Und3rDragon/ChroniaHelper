using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Triggers;

public class FlagSerialTrigger : Trigger
{
    public FlagSerialTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        ID = data.ID;

    }
    private int ID;
    private string activeFlag, serialFlag, targetSymbol = "&";
    private float interval;
    private int startIndex, totalIndexes;

    public IEnumerator SerialProcessor()
    {
        while (!MapProcessor.level.Session.GetFlag(activeFlag))
        {
            yield return null;
        }

        for(int i = startIndex; i < startIndex + totalIndexes; i++)
        {
            for(int j = startIndex; j < startIndex + totalIndexes; j++)
            {
                MapProcessor.level.Session.SetFlag(serialFlag.Replace(targetSymbol, j.ToString()), false);
            }
            MapProcessor.session.SetFlag(serialFlag.Replace(targetSymbol, i.ToString()), true);

            yield return interval;
        }
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        MapProcessor.globalEntityDummy.Add(new Coroutine());
    }
}

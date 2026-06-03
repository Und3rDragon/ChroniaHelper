using System.Collections;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/SetFlagSequenceController")]
public class SetFlagSequenceController : GeneralSetupController
{
    public SetFlagSequenceController(EntityData data, Vc2 offset) : base(data, offset)
    {
        flags = data.StringArray("flagSequence", ';');
        global = data.Bool("global", false);
    }
    private string[] flags;
    private bool global;

    public override void ApplyValue()
    {
        PrepareSequence();
    }
    
    public void PrepareSequence()
    {
        if (global)
        {
            MaP.dummyGlobal.Add(new Coroutine(SetFlagSequence()));
        }
        else
        {
            Add(new Coroutine(SetFlagSequence()));
        }
    }
    
    public IEnumerator SetFlagSequence()
    {
        for(int i = 0; i < flags.Length; i++)
        {
            bool isNumber = float.TryParse(flags[i], out var delay);

            if (isNumber)
            {
                yield return delay;
                continue;
            }
            
            flags[i].SetGeneralFlags(",", "!", "*", "?");

            yield return null;
        }
    }
}

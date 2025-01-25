using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagDateTrigger")]
public class FlagDateTrigger : FlagManageTrigger
{

    private HashSet<string> flags;

    public FlagDateTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        requirements = data.Attr("requirements");
        flag = FlagUtils.Parse(data.Attr("setTrueFlags"));
        notflag = FlagUtils.Parse(data.Attr("setFalseFlags"));
        onStay = data.Bool("activeOnStay", true);
        enterIfFlag = FlagUtils.Parse(data.Attr("enterIfFlag"));
        ID = data.ID;
    }
    private string[] flag, notflag;
    private string requirements;
    private bool onStay;
    private int ID;

    private string[] format = { "year", "month", "day", "hour", "minute", "second", "millisecond" };

    private bool IsQualified()
    {
        string[] groups = requirements.Split(';', StringSplitOptions.TrimEntries);

        // check all formulas true will return true, otherwise go false and end this thing
        foreach (string group in groups)
        {
            string[] parameters = group.Split(',', StringSplitOptions.TrimEntries);
            string indicator = "";
            bool inverted = false;
            if (parameters.Length > 0)
            {
                indicator = parameters[0].Trim().TrimStart('!').ToLower();
                inverted = parameters[0].StartsWith('!');
            }

            if (parameters.Length == 1)
            {
                // if length is 1, normally it's considered ignored (true)
                // but if it's a reversed condition, then it's a false
                if (inverted) { return false; }

            }
            else if(parameters.Length == 2)
            {
                bool check = true;

                // if the first slot isn't in the list, condition ignored (true)
                
                // if first slot is in the list, start parsing second slot
                if (format.Contains(indicator))
                {
                    int digit;

                    // if parsing failed, condition ignored (true)
                    if (int.TryParse(parameters[1], out digit))
                    {
                        // parsing success, check time
                        check.TryNegative(indicator.CheckTime() >= digit);
                    }
                }

                // checking complete, check reversed condition
                if (inverted)
                {
                    check = !check;
                }

                if (!check) { return false; }
            }
            else if (parameters.Length >= 3)
            {
                bool check = true;
                // if first slot isn't in the list, condition ignored (true)

                // if first slot is in the list, start parsing the other slots
                if (format.Contains(indicator)){
                    int digit1, digit2;
                    if (int.TryParse(parameters[1], out digit1))
                    {
                        // there is a second slot, check for third slot
                        if(int.TryParse(parameters[2], out digit2))
                        {
                            // parsing success, digits 1 and 2 are settled
                            if (digit1 <= digit2) {

                                check.TryNegative(indicator.CheckTime() >= digit1 && indicator.CheckTime() < digit2);
                            }
                            else
                            {
                                // on special case, digit1 > digit2, consider range loop
                                bool flag1 = indicator.CheckTime() >= digit1 && indicator.CheckTime() <= indicator.CheckTimeLimit(),
                                    flag2 = indicator.CheckTime() >= 0 && indicator.CheckTime() < digit2;
                                check.TryNegative(flag1 || flag2);
                            }
                        }
                        else
                        {
                            // there is a second slot, but there is no third slot
                            check.TryNegative(indicator.CheckTime() >= digit1);
                        }
                    }
                    else
                    {
                        // there is no second slot, check for third slot
                        if (int.TryParse(parameters[2], out digit2))
                        {
                            check.TryNegative(indicator.CheckTime() < digit2);
                        }
                        
                        // both digits are invalid, condition ignored (true)
                    }
                }

                // done checking formulas, check reverse conditions
                if (inverted)
                {
                    check = !check;
                }

                if (!check) { return false; }
            }
            // other invalid conditions are ignored (true)
        }
        // every group of conditions are met, return true
        return true;
    }

    protected override void OnEnterExecute(Player player)
    {
        this.flags = base.GetFlags();

        if (IsQualified())
        {
            base.Remove(this.notflag);
            base.Add(this.flag);
        }
    }

    protected override void OnStayExecute(Player player)
    {
        if (IsQualified() && onStay)
        {
            base.Remove(this.notflag);
            base.Add(this.flag);
        }
    }

    protected override void LeaveReset(Player player)
    {
        base.SetFlags(this.flags);
    }

}

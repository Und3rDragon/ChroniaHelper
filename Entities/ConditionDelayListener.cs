using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;
using VivHelper.Entities;

namespace ChroniaHelper.Entities;

public class ConditionDelayListenerOperator
{
    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Update += OnLevelUpdate;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Update -= OnLevelUpdate;
    }
    
    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        
        foreach(var item in Md.Session.listeningConditions)
        {
            var listener = item.Value;
            bool state = listener.condition.CheckCondition((ConditionUtils.ConditionMode)listener.usingExpression);
            
            if (listener.constant == 1)
            {
                if(state != Md.Session.listeningConditionLastState.SafeGet(item.Key, false))
                {
                    Md.Session.listeningConditionTimerState[item.Key] = true;
                }
            }
            else if(listener.constant == 2)
            {
                if (state && !Md.Session.listeningConditionLastState.SafeGet(item.Key, false))
                {
                    Md.Session.listeningConditionTimerState[item.Key] = true;
                }
            }
            else if(listener.constant == 3)
            {
                if (!state && Md.Session.listeningConditionLastState.SafeGet(item.Key, false))
                {
                    Md.Session.listeningConditionTimerState[item.Key] = true;
                }
            }
            else
            {
                Md.Session.listeningConditionTimerState[item.Key] = state;
            }

            if (Md.Session.listeningConditionTimerState.SafeGet(item.Key, false))
            {
                Md.Session.listeningConditionTimer.Create(item.Key, 0f);
                Md.Session.listeningConditionTimer[item.Key] += Engine.DeltaTime;
            }
            
            if (Md.Session.listeningConditionTimer.SafeGet(item.Key, 0f) >= listener.time.GetAbs() &&
                Md.Session.listeningConditionTimerState.SafeGet(item.Key, false))
            {
                Md.Session.listeningConditionTimerState[item.Key] = false;
                Md.Session.listeningConditionTimer[item.Key] = 0f;

                if (listener.flagOperation == 1)
                {
                    listener.flag.SetFlag(false);
                }
                else if (listener.flagOperation == 2)
                {
                    listener.flag.SetFlag(!listener.flag.GetFlag());
                }
                else
                {
                    listener.flag.SetFlag(true);
                }
            }

            Md.Session.listeningConditionLastState[item.Key] = state;
        }
    }
}

[Tracked(true)]
[CustomEntity("ChroniaHelper/ConditionDelayListener")]
public class ConditionDelayListener : BaseEntity
{
    public ConditionDelayListener(EntityData data, Vc2 offset) : base(data, offset)
    {
        conditionTag = data.Attr("listenerTag", "conditionListener");
        listener = new()
        {
            condition = data.Attr("condition", "slider == 0"),
            constant = data.Int("operationMode", 0),
            flagOperation = data.Int("flagOperationMode", 0),
            usingExpression = data.Int("usingExpression", 0),
            time = data.Float("delayOrInterval", 0f),
            flag = data.Attr("flag", "flag"),
        };
    }
    private string conditionTag;
    Ses.SessionConditionListener listener;

    protected override void AddedExecute(Scene scene)
    {
        Md.Session.listeningConditions[conditionTag] = listener;
    }
}

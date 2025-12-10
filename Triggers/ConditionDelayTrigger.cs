using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Triggers.TriggerExtension;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Triggers;

[Tracked(false)]
[CustomEntity("ChroniaHelper/ConditionDelayTrigger")]
public class ConditionDelayTrigger : BaseTrigger
{
    public ConditionDelayTrigger(EntityData d, Vc2 o) : base(d, o)
    {
        ID = d.ID;
        onEnterCondition = d.Attr("onEnterCondition");
        onStayCondition = d.Attr("onStayCondition");
        onLeaveCondition = d.Attr("onLeaveCondition");
        onEnterUseExpression = d.Bool("onEnterUseExpression", false);
        onStayUseExpression = d.Bool("onStayUseExpression", false);
        onLeaveUseExpression = d.Bool("onLeaveUseExpression", false);
        onEnterDelay = d.Float("onEnterDelay", 0f).GetAbs();
        onStayInterval = d.Float("onStayInterval", 0f).GetAbs();
        onLeaveDelay = d.Float("onLeaveDelay", 0f).GetAbs();
        triggerCovered = d.Bool("triggerCoveredTriggers", false);
        targetIDs = d.Attr("targetTriggerIDs");
        useEnter = d.Bool("useOnEnterCondition", false);
        useStay = d.Bool("useOnStayCondition", false);
        useLeave = d.Bool("useOnLeaveCondition", false);
    }
    public int ID;
    public string onEnterCondition, onStayCondition, onLeaveCondition;
    public bool useEnter, useStay, useLeave;
    public bool onEnterUseExpression = false, onStayUseExpression = false, onLeaveUseExpression = false;
    public float onEnterDelay = 0f, onStayInterval = 0f, onLeaveDelay = 0f;
    public bool triggerCovered = false;
    public string targetIDs;

    // current states
    private bool onEnter = false, onStay = false, onLeave = false;
    // last states
    private bool _onEnter = false, _onLeave = false;
    // timer running
    private bool _tEnter = false, _tStay = false, _tLeave = false;
    // timers
    private float tEnter = 0, tStay = 0, tLeave = 0;
    public override void Update()
    {
        base.Update();

        if (!triggersLocated) { return; }

        onEnter = false;
        onStay = false;
        onLeave = false;

        if (onEnterCondition.IsNotNullOrEmpty() && useEnter)
        {
            if (onEnterUseExpression)
            {
                onEnter = onEnterCondition.ParseMathExpression() != 0f;
            }
            else
            {
                onEnter = true;
                onEnterCondition.Split(',', StringSplitOptions.TrimEntries).EachDo((flag) =>
                {
                    string name = flag;
                    bool inverted = false;
                    if (inverted = flag.StartsWith('!'))
                    {
                        name = flag.TrimStart('!');
                    }
                    onEnter.TryNegative(inverted ? !name.GetFlag() : name.GetFlag());
                });
            }
        }
        else
        {
            onEnter = useEnter;
        }

        if (onStayCondition.IsNotNullOrEmpty() && useStay)
        {
            if (onStayUseExpression)
            {
                onStay = onStayCondition.ParseMathExpression() != 0f;
            }
            else
            {
                onStay = true;
                onStayCondition.Split(',', StringSplitOptions.TrimEntries).EachDo((flag) =>
                {
                    string name = flag;
                    bool inverted = false;
                    if (inverted = flag.StartsWith('!'))
                    {
                        name = flag.TrimStart('!');
                    }
                    onStay.TryNegative(inverted ? !name.GetFlag() : name.GetFlag());
                });
            }
        }
        else
        {
            onStay = useStay;
        }

        if (onLeaveCondition.IsNotNullOrEmpty() && useStay)
        {
            if (onLeaveUseExpression)
            {
                onLeave = onLeaveCondition.ParseMathExpression() != 0f;
            }
            else
            {
                onLeave = true;
                onLeaveCondition.Split(',', StringSplitOptions.TrimEntries).EachDo((flag) =>
                {
                    string name = flag;
                    bool inverted = false;
                    if (inverted = flag.StartsWith('!'))
                    {
                        name = flag.TrimStart('!');
                    }
                    onLeave.TryNegative(inverted ? !name.GetFlag() : name.GetFlag());
                });
            }
        }
        else
        {
            onLeave = useLeave;
        }

        if (!_onEnter && onEnter) { _tEnter = true; }
        if (onStay) { _tStay = true; }
        else { _tStay = false; tStay = 0f; }
        if (!_onLeave && onLeave) { _tLeave = true; }

        if (_tEnter) { tEnter += Engine.DeltaTime; }
        if(_tStay) { tStay += Engine.DeltaTime; }
        if (_tLeave) { tLeave += Engine.DeltaTime; }
        
        if(PUt.TryGetPlayer(out Player player))
        {
            if (tEnter >= onEnterDelay && _tEnter)
            {
                _tEnter = false;
                tEnter = 0f;
                EnterTriggers(player);
            }
            if (tStay >= onStayInterval && onStay)
            {
                tStay = 0f;
                StayTriggers(player);
            }
            if (tLeave >= onLeaveDelay && _tLeave)
            {
                _tLeave = false;
                tLeave = 0f;
                LeaveTriggers(player);
            }
        }
        
        _onEnter = onEnter;
        _onLeave = onLeave;
    }
    
    private void EnterTriggers(Player player)
    {
        foreach(var item in targetTriggers.ToHashSet())
        {
            item?.OnEnter(player);
        }
    }
    
    private void StayTriggers(Player player)
    {
        foreach (var item in targetTriggers.ToHashSet())
        {
            item?.OnStay(player);
        }
    }
    
    private void LeaveTriggers(Player player)
    {
        foreach (var item in targetTriggers.ToHashSet())
        {
            item?.OnLeave(player);
        }
    }

    private bool triggersLocated = false;
    public override void Awake(Scene scene)
    {
        _tEnter = false;
        _tLeave = false;
        _onEnter = false;
        _onLeave = false;
        triggersLocated = false;
        targetTriggers.Clear();
        if (triggerCovered)
        {
            FindCoveredTriggers();
        }
        FindTargetTriggers();
        triggersLocated = true;

        base.Awake(scene);
    }
    
    private List<Trigger> targetTriggers = new();
    public void FindCoveredTriggers()
    {
        var triggers = MaP.level.Tracker.GetEntities<Trigger>();
        
        foreach(var entity in triggers)
        {
            if (entity is TriggerExtension.TriggerExtension) { continue; }
            if (entity is TriggerExtension.TriggerExtensionTarget) { continue; }

            var trigger = entity as Trigger;

            if (CollideCheck(trigger))
            {
                targetTriggers.Add(trigger);
            }
        }
    }
    
    public void FindTargetTriggers()
    {
        List<int> targets = new();
        this.targetIDs.Split(',', StringSplitOptions.TrimEntries).EachDo((s) =>
        {
            if (s.ParseInt(out int n))
            {
                targets.Add(n);
            }
        });

        var triggers = MaP.level.Tracker.GetEntities<Trigger>();

        foreach (var entity in triggers)
        {
            if (entity is TriggerExtension.TriggerExtension) { continue; }
            if (entity is TriggerExtension.TriggerExtensionTarget) { continue; }
            
            var trigger = entity as Trigger;

            if (targets.Contains(trigger.SourceData.ID))
            {
                targetTriggers.Add(trigger);
            }
        }
    }
}

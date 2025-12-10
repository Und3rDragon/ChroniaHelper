local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/ConditionDelayTrigger",
	placements =
	{
		name = "trigger",
		data =
		{
			onEnterCondition = "",
            onStayCondition = "",
            onLeaveCondition = "",
            onEnterDelay = 0,
            onStayInterval = 0,
            onLeaveDelay = 0,
            targetTriggerIDs = "",
            triggerCoveredTriggers = false,
            useOnEnterCondition = false,
            useOnStayCondition = false,
            useOnLeaveCondition = false,
            onEnterUseExpression = false,
            onStayUseExpression = false,
            onLeaveUseExpression = false,
		}
	},
	fieldInformation = {
		
	},
	triggerText = function(room, entity)
		local base = "Condition Delay"
        
		return base
	end
}
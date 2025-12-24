local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/ConditionDelayTrigger",
	placements =
	{
		name = "trigger",
		data =
		{
            chroniaMathExpession = "See tooltip",
			frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",
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
            onEnterUseExpression = 0,
            onStayUseExpression = 0,
            onLeaveUseExpression = 0,
		}
	},
    fieldOrder = {
		"_x", "_y", "x", "y", "_id", "_name",
		"chroniaMathExpession", "frostSessionExpression",
	},
	fieldInformation = {
        chroniaMathExpession = {
			editable = false,
		},
		onEnterUseExpression ={
            options = {
				["Flags"] = 0, ["ChroniaMathExpression"] = 1, ["FrostSessionExpression"] = 2
			},
            editable = false,
		},
        onStayUseExpression ={
            options = {
				["Flags"] = 0, ["ChroniaMathExpression"] = 1, ["FrostSessionExpression"] = 2
			},
            editable = false,
		},
        onLeaveUseExpression ={
            options = {
				["Flags"] = 0, ["ChroniaMathExpression"] = 1, ["FrostSessionExpression"] = 2
			},
            editable = false,
		},
	},
	triggerText = function(room, entity)
		local base = "Condition Delay"
        
		return base
	end,
    associatedMods = function(entity)
		if entity.onEnterUseExpression == 2 or entity.onStayUseExpression == 2 or entity.onLeaveUseExpression == 2 then
			return {"FrostHelper", "ChroniaHelper"}
        end
        
        return {"ChroniaHelper"}
    end
}
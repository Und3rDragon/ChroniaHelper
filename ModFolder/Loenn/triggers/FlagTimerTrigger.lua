local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/FlagTimerTrigger",
	placements =
	{
		name = "FlagTimerTrigger",
		data =
		{
			setups = "flagA,1;flagB,2",
			range = 0,
			mode = 0,
			onlyOnce = false,
		},
	},
	fieldInformation = {
		setups = {
			fieldType = "list",
			elementSeparator = ";",
			elementOptions = {
				fieldType = "list",
				maximumElements = 2,
				minimumElements = 2,
			},
		},
		range = {
			options = {
				["room"] = 0,
				["map"] = 1,
				["saves"] = 2,
			},
			editable = false,
		},
		mode = {
			options = {
				["set"] = 0,
				["add"] = 1,
				["minus"] = 2,
			},
			editable = false,
		},
	},
	triggerText = function(room, entity)
		local base = "flag timer"
		return base
	end,
}
local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/CounterFadeTrigger",
	placements =
	{
		name = "trigger",
		data =
		{
			counterName = "",
			fadeFrom = "",
			fadeTo = 0,
			positionMode = "NoEffect",
			easing = "Linear",
		}
	},
	fieldInformation = {
		fadeTo = {fieldType = "integer"},
		positionMode = require("mods").requireFromPlugin("helpers.field_options").positionMode,
		easing = require("mods").requireFromPlugin("helpers.field_options").easeMode,
	},
	triggerText = function(room, entity)
		local base = "counter fade"
		
		if entity.counterName ~= nil then
			base = base .."\n(" ..entity.counterName .. ")"
		end
		
		return base
	end
}
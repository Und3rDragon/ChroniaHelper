local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/SliderFadeTrigger",
	placements =
	{
		name = "trigger",
		data =
		{
			sliderName = "",
			fadeFrom = "",
			fadeTo = 0,
			positionMode = "NoEffect",
			easing = "Linear",
		}
	},
	fieldInformation = {
		positionMode = require("mods").requireFromPlugin("helpers.field_options").positionMode,
		easing = require("mods").requireFromPlugin("helpers.field_options").easeMode,
	},
	triggerText = function(room, entity)
		local base = "slider fade"
		
		if entity.sliderName ~= nil then
			base = base .."\n(" ..entity.sliderName .. ")"
		end
		return base
	end
}
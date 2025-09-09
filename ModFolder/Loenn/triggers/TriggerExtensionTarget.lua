local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/TriggerExtensionTarget",
	placements =
	{
		name = "TriggerExtensionTarget",
		data =
		{
			extensionTag = "",
		}
	},
	fieldInformation = {
		
	},
	triggerText = function(room, entity)
		local base = "Trigger Extension Target"
		local additional = ""
		if entity.extensionTag ~= "" then
			additional = " (Tag = " .. entity.extensionTag .. ")"
		end

		return base .. additional
	end
}
local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/TriggerExtension",
	placements =
	{
		name = "TriggerExtension",
		data =
		{
			extensionTag = "",
		}
	},
	triggerText = function(room, entity)
		local base = "Trigger Extension"
		return base
	end
}
local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/TriggerExtension",
	placements =
	{
		name = "TriggerExtension",
		data =
		{
			extensionTag = "",
			overrideID = -1,
		}
	},
	fieldInformation = {
		overrideID = {
			fieldType = "integer",
			options = {
				["disabled"] = -1
			},
		},
	},
	triggerText = function(room, entity)
		local base = "Trigger Extension"
		local additional = ""
		if entity.overrideID >= 0 then
			additional = " (ID = " .. tostring(entity.overrideID) .. ")"
		elseif entity.extensionTag ~= "" then
			additional = " (Tag = " .. entity.extensionTag .. ")"
		end

		return base .. additional
	end
}
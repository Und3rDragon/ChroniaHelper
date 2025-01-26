local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/FlagTrigger",
	placements =
	{
		name = "FlagTrigger",
		data =
		{
			flag = "flag",
			set = true,
			temporary = false,
			global = false,
		}
	},
	fieldInformation = {
		flag = {
			fieldType = "list",
			allowEmpty = false,
		},
	},
	triggerText = function(room, entity)
		local base = "flag\n(" ..entity.flag .. ")"
		if entity.set == false then
			base = "inverted\nflag (!" ..entity.flag .. ")"
		end
		if entity.temporary then
			base = "temporary " .. base
		end
		if entity.global then
			base = "global " .. base
		end
		return base
	end
}
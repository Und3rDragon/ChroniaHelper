local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/FlagTrigger",
	placements =
	{
		name = "FlagTrigger",
		data =
		{
			flag = "flag",
			extensionTag = "",
			set = true,
			temporary = false,
			global = false,
			resetOnLeave = false,
			ignoreUnchanged = true,
			onStay = false,
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
			base = "inverted\nflag (" ..entity.flag .. ")"
		end
		if entity.ignoreUnchanged then
			base = "safe " .. base
		end
		if entity.resetOnLeave then
			base = "inzone " .. base
		end
		if entity.temporary then
			base = "temporary " .. base
		end
		if entity.global then
			base = "global " .. base
		end
		if entity.onStay then
			base = base .. "(active on stay)"
		end
		return base
	end
}
local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/FlagArrayTrigger",
	placements =
	{
		name = "FlagArrayTrigger",
		data =
		{
			enterIfFlag = "",
			flags = "flag",
			intervals = "0.1",
			positionMode = "NoEffect",
		}
	},
	fieldInformation = {
		flag = {
			fieldType = "list",
			allowEmpty = false,
		},
		intervals = {
			fieldType = "list",
			allowEmpty = false,
		},
		positionMode = {
            options = {
                "NoEffect",
                "RightToLeft",
                "LeftToRight",
                "BottomToTop",
                "TopToBottom",
                "VerticalCenter",
                "HorizontalCenter",
            },
            editable = false,
        },
	},
	triggerText = function(room, entity)
		local base = "flag array\n(" ..entity.flags .. ")"
		
		return base
	end
}
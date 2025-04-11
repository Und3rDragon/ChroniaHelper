local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/FlagSerialTrigger",
	placements =
	{
		name = "FlagSerialTrigger",
		data =
		{
			enterIfFlag = "",
			serialFlag = "flag_&",
			targetSymbol = "&",
			positionMode = "NoEffect",
			interval = 0.1,
			startIndex = 0,
			steps = 10,
			staircase = false,
		}
	},
	fieldInformation = {
		startIndex = {
			fieldType = "integer",
		},
		steps = {
			fieldType = "integer",
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
		local base = "flag serial\n(" ..entity.serialFlag .. ")"

		return base
	end
}
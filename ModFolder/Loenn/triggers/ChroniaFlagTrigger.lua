local drawableText = require("structs.drawable_text")

--return {}

return{
	name = "ChroniaHelper/ChroniaFlagTrigger",
	placements =
	{
		name = "FlagTrigger",
		data =
		{
            Name = "flag",
			Active = false,
            Global = false,
            ResetOnDeath = false,
            ResetOnReload = false,
            ResetOnTransition = false,
            RemoveWhenReset = true,
            Force = false,
            Timed = -1,
            DefaultResetState = 0,
            Tags = "",
            CustomData = "",
            PresetTags = "",
		}
	},
	fieldInformation = {
        Name = {
            allowsEmpty = false,
		},
        DefaultResetState = {
            options = {
                ["False"] = 0,
				["True"] = 1,
				["Reversed Active"] = 2,
			},
            editable = false,
		},
	},
	triggerText = "Chronia Flag",
}
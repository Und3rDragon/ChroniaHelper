local drawableText = require("structs.drawable_text")

--return {}

return{
	name = "ChroniaHelper/ChroniaCounterTrigger",
	placements =
	{
		name = "trigger",
		data =
		{
            Name = "";
            Value = 0,
            Global = false,
            ResetOnDeath = false,
            ResetOnTransition = false,
            RemoveWhenReset = true,
            DefaultValue = 0,
			Timer = -1,
		}
	},
	fieldInformation = {
        Name = {
            allowsEmpty = false,
		},
        Value = {
			fieldType = "integer",
		},
		DefaultValue = {
			fieldType = "integer",
		}
	},
	triggerText = "Chronia Counter",
}
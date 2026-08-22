local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagDictionaryTrigger",
    placements =
    {
        name = "FlagDictionaryTrigger",
        data =
        {
            flagDictionary = "flag1,!flag2>>flag3;flag4,!flag5>>flag6,!*flag7",
            listenOnStay = false,
            coverScreen = false,
        },
    },
    fieldInformation =
    {
        flagDictionary = {
            fieldType = "list_scroll",
            elementSeparator = ";",
            elementOptions = {
                fieldType = "list_scroll",
                elementSeparator = ">>",
            },
        },
    },
    triggerText = function(room, entity)
		local base = "Flag Dictionary"
		return base
	end,
}
local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagRegexOperation",
    placements =
    {
        name = "trigger",
        data =
        {
            contains = "",
            startsWith = "",
            endsWith = "",
            mode = 0,
            parameter = "",
        }
    },
    fieldInformation =
    {
        enterMode = fieldOptions.enterMode,
        enterDelay =
        {
            minimumValue = 0
        },
        leaveMode = fieldOptions.leaveMode,
        leaveDelay =
        {
            minimumValue = 0
        },
        mode = {
            fieldType = "integer",
            options = {
                ["Off"] = 0,
                ["Replace"] = 1,
                ["Count"] = 2,
            },
            editable = false,
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset"
    }
}
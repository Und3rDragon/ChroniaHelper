local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagFilterOperation",
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
                ["Set as flag value"] = 1,
                ["Equals to Chronia Helper Expression not 0"] = 2,
                ["Equals to Frost Session Expression not 0"] = 3,
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
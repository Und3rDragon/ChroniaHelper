local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagSwitchTrigger",
    placements =
    {
        name = "FlagSwitchTrigger",
        data =
        {
            flagA = "",
            flagB = "",
            defaultFlag = "None",
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            leaveMode = "Any",
            leaveDelay = 0,
            onlyOnce = false,
            leaveReset = false
        }
    },
    fieldInformation =
    {
        defaultFlag =
        {
            options =
            {
                "None",
                "FlagA",
                "FlagB"
            },
            editable = false
        },
        enterMode = fieldOptions.enterMode,
        enterDelay =
        {
            minimumValue = 0
        },
        leaveMode = fieldOptions.leaveMode,
        leaveDelay =
        {
            minimumValue = 0
        }
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "flagA",
        "flagB",
        "defaultFlag",
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
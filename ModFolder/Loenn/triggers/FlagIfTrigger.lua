local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagIfTrigger",
    placements =
    {
        name = "FlagIfTrigger",
        data =
        {
            ifFlag = "",
            trueFlag = "",
            falseFlag = "",
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            leaveMode = "Any",
            leaveDelay = 0,
            onlyOnce = false,
            leaveReset = false,
            deleteIfFlag = false
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
        }
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "ifFlag",
        "trueFlag",
        "falseFlag",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset",
        "deleteIfFlag"
    }
}
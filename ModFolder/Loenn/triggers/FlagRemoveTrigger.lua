local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        return "Flag Remove (" .. entity.flag .. ")"
    end,
    name = "ChroniaHelper/FlagRemoveTrigger",
    placements =
    {
        name = "FlagRemoveTrigger",
        data =
        {
            flag = "",
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
        "flag",
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
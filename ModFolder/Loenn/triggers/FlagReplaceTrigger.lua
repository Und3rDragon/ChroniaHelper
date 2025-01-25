local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        return "Flag Replace (" .. entity.oldFlag .. ") to (" .. entity.newFlag .. ")"
    end,
    name = "ChroniaHelper/FlagReplaceTrigger",
    placements =
    {
        name = "FlagReplaceTrigger",
        data =
        {
            oldFlag = "",
            newFlag = "",
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            leaveMode = "Any",
            leaveDelay = 0,
            onlyOnce = false,
            leaveReset = false,
            strict = false
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
        "oldFlag",
        "newFlag",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset",
        "strict"
    }
}
local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        local base = "Random Packed Flag" .. entity.label
        return base
    end,
    name = "ChroniaHelper/RandomPackedFlagTrigger",
    placements =
    {
        name = "trigger",
        data =
        {
            label = "pack",
            onlyOnce = false,
        },
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
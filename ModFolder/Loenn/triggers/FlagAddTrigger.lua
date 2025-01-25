local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        local base = "Flag Add (" .. entity.flag .. ")"
        if entity.onlyOnce then
            base = base .. " (Once)"
        end
        if entity.leaveReset then
            base = base .. " (Leave Reset)"
        end
        return base
    end,
    name = "ChroniaHelper/FlagAddTrigger",
    placements =
    {
        name = "FlagAddTrigger",
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
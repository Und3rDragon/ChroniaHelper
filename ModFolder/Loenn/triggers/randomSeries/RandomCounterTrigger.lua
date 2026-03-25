local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        local base = "Random Counter" .. " (" .. entity.counter .. ")"
        return base
    end,
    name = "ChroniaHelper/RandomCounterTrigger",
    placements =
    {
        name = "trigger",
        data =
        {
            counter = "counter",
            value1 = 0,
            value2 = 1,
            interval = 1,
            enterDelay = 0,
            continuously = false,
            onlyOnce = true,
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
        value1 = {
            fieldType = "integer",
        },
        value2 = {
            fieldType = "integer",
        }
    },
}
local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        local base = "Random Slider" .. " (" .. entity.slider .. ")"
        return base
    end,
    name = "ChroniaHelper/RandomSliderTrigger",
    placements =
    {
        name = "trigger",
        data =
        {
            slider = "slider",
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
        }
    },
}
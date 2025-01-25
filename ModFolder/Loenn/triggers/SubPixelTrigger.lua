local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        return "SubPixel (" .. entity.subPixelX .. "," .. entity.subPixelY .. ")"
    end,
    name = "ChroniaHelper/SubPixelTrigger",
    placements =
    {
        name = "SubPixelTrigger",
        data =
        {
            subPixelX = 0,
            subPixelY = 0,
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            enterIfFlag = "",
            enterSound = "",
            onlyOnce = false,
            stay = true
        }
    },
    fieldInformation =
    {
        enterMode = fieldOptions.enterMode,
        enterDelay =
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
        "subPixelX",
        "subPixelY",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "enterIfFlag",
        "enterSound",
        "onlyOnce",
        "stay"
    }
}
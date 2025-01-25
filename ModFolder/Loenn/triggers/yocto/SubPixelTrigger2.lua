local baseTrigger = require("mods").requireFromPlugin("helpers.base_trigger")

local trigger = {
    triggerText = function(room, entity)
        return "SubPixel (" .. entity.subPixelX .. "," .. entity.subPixelY .. ")"
    end,
    name = "ChroniaHelper/SubPixelTrigger2",
    placements =
    {
        name = "Sub Pixel Trigger 2",
        data =
        {
            subPixelX = 0,
            subPixelY = 0,
            stay = false
        }
    },
    fieldInformation = { },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "subPixelX",
        "subPixelY",
        "enterFrom",
        "flagsForEnter",
        "leaveFrom",
        "flagsForLeave",
        "onlyOnce",
        "stay"
    }
}

baseTrigger.invoke(trigger)

return trigger
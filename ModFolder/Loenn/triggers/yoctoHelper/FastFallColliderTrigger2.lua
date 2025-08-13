local baseTrigger = require("mods").requireFromPlugin("helpers.base_trigger")

local trigger = {
    name = "ChroniaHelper/FastFallColliderTrigger2",
    placements =
    {
        name = "Fast Fall Collider Trigger 2",
        data = { }
    },
    fieldInformation = { },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "enterFrom",
        "flagsForEnter",
        "leaveFrom",
        "flagsForLeave",
        "onlyOnce",
        "revertOnLeave",
        "revertOnDeath"
    }
}

baseTrigger.invoke(trigger)

return trigger
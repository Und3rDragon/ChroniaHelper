local baseTrigger = require("mods").requireFromPlugin("helpers.base_trigger")

local trigger = {
    name = "ChroniaHelper/KeepDashSpeedTrigger",
    placements =
    {
        name = "Keep Dash Speed Trigger",
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
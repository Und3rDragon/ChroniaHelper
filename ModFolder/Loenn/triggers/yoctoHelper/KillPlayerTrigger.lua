local baseTrigger = require("mods").requireFromPlugin("helpers.base_trigger")

local trigger = {
    name = "ChroniaHelper/KillPlayerTrigger",
    placements =
    {
        name = "Kill Player Trigger",
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
        "onlyOnce"
    }
}

baseTrigger.invoke(trigger)

return trigger
local trigger = {}

trigger.name = "T8MC/EventVolumeFadeTrigger"

trigger.fieldOrder = {
    "x", "y", "width", "height", "eventPaths",
    "volumeFrom", "volumeTo", "direction", "restoreOnLeave"
}

trigger.fieldInformation = {
    direction = require("mods").requireFromPlugin("consts.field_options").positionMode,
    eventPaths = {
        elementOptions = {
            "event:/game/05_mirror_temple/redbooster_move",
            "event:/game/05_mirror_temple/redbooster_enter,event:/game/05_mirror_temple/redbooster_dash,event:/game/05_mirror_temple/redbooster_move,event:/game/05_mirror_temple/redbooster_end,event:/game/05_mirror_temple/redbooster_reappear",
        },
        editable = true,
        fieldType = "list_scroll",
    },
}

trigger.placements = {
    name = "trigger",
    data = {
        width = 64, 
        height = 64,
        eventPaths = "event:/game/05_mirror_temple/redbooster_move",
        volumeFrom = 1.0, 
        volumeTo = 0.0,
        direction = "LeftToRight", 
        restoreOnLeave = false
    }
}

return trigger

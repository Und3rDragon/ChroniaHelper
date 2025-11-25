local vivUtil = require('mods').requireFromPlugin('helpers.vivUtilsMig')

local playerPlayback = {}

-- Read from disk instead?
local baseGameTutorials = {
    "combo", "superwalljump", "too_close", "too_far",
    "wavedash", "wavedashppt"
}

playerPlayback.name = "ChroniaHelper/CustomPlayback"
playerPlayback.depth = 0
playerPlayback.sprite = function(room,entity)
    local spr = require('structs.drawable_sprite').fromTexture("characters/player_playback/sitDown00")        
    spr:setColor({0.54, 0.157, 0.157, 0.8})
    spr:setPosition(entity.x, entity.y)
    spr:setJustification(0.5,1)
    return spr    
end
playerPlayback.nodeLineRenderType = "line"
playerPlayback.nodeLimits = {0, 2}
playerPlayback.fieldInformation = {
    tutorial = {
        options = baseGameTutorials
    },
    Color = {fieldType = "color", allowEmpty = true},
    SpeedMultiplier = {fieldType = "number", options = {
        {"2x", 2.0},
        {"1x", 1.0},
        {"0.5x", 0.5},
        {"0.333x", 0.3333333},
        {"0.25x", 0.25},
        {"0.2x", 0.2},
        {"0.1x", 0.1}
    }},
    activeFlag = {
        allowEmpty = true,
    },
    displayFlag = {
        allowEmpty = true,
    },
    sprite = {
        options = {
            "Madeline", "MadelineNoBackpack", "Badeline", "MadelineAsBadeline", "Playback"
        },
        editable = false,
    },
}
playerPlayback.placements = {
    name = "main",
    data = {
        tutorial = "",
        Delay = 1.0,
        --StartActive = true,
        SpeedMultiplier = 1.0,
        --CustomStringID = "",
        Color = "",
        activeFlag = "",
        displayFlag = "",
        sprite = "Madeline",
    }
}

return playerPlayback
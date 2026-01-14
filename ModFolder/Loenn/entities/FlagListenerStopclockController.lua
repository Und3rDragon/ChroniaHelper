local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/FlagListenerStopclockController"

controller.placements = {
    name = "controller",
    data = {
        stopclockName = "stopclock",
        flag = "flag",
        time = "00:05:000",
        countdown = true,
        followLevelPause = true,
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y"
}

controller.fieldInformation = 
{
    
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Stopclock", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
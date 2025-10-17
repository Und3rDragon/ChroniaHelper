local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/StopclockFlagController"

controller.placements = {
    name = "controller",
    data = {
        stopclockTags = "",
        global = false,
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
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/StopclockFlag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
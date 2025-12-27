local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/CameraGrid"

controller.placements = {
    name = "grid",
    data = {
        Introduction = "",
        indicatorAlpha = 1,
    }
}

controller.ignoredFields = {
    "_x", "_y", "x", "y"
}

controller.fieldInformation = 
{
    Introduction = {
        options = {}, editable = false,
    },
    indicatorAlpha = {
        minimumValue = 0.0,
        maximumValue = 1.0,
    },
}

function controller.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/CameraGrid", entity)
    iconSprite:setColor({1,1,1,entity.indicatorAlpha})
    
    table.insert(sprite, iconSprite)
    return sprite
end

return controller
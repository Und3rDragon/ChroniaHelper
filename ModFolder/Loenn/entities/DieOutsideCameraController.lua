local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/DieOutsideCameraController"

controller.placements = {
    name = "controller",
    data = {
        x = 0,
        y = 0,
        flag = "ChroniaHelper_DieOutsideCamera",
    },
}

controller.ignoredFields = {
    
}

controller.fieldInformation = 
{
    
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/DieOutCam", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
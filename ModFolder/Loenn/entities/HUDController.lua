local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/HUDController"

controller.placements = {
    name = "controller",
    data = {
        condition = "y < 90",
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y", "_id", "_name"
}

controller.fieldInformation = 
{
    condition = {
        options = {
            "y < 90",
            "x < 160",
            "(x < 160) && (y < 90)",
            "all",
        },
        editable = true,
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/HUD", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
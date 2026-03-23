local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/FlagPacker"

controller.placements = {
    name = "controller",
    data = {
        flags = "flagA,!flagB,*flagC",
        label = "pack",
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y"
}

controller.fieldInformation = 
{
    flags = {
        fieldType = "list",
        minimumElements = 1,
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/FlagPacker", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
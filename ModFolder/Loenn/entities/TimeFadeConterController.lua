local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/TimeFadeCounterController"

controller.placements = {
    name = "controller",
    data = {
        flag = "",
        counterName = "",
        targetValue = 0,
        duration = 1,
        easeMode = "Linear",
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y", "_name", "_id"
}

controller.fieldInformation = 
{
    targetValue = {fieldType = "integer"},
    easeMode = require("mods").requireFromPlugin("helpers.field_options").easeMode
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Stopclock", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
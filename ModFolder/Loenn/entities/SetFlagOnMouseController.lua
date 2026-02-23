local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/SetFlagOnMouseController"

controller.placements = {
    name = "controller",
    data = {
        mouseMode = 0,
        flags = "flag",
        flagMode = 0,
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y"
}

controller.fieldInformation = 
{
    mouseMode = {
        fieldType = "integer",
        options = {
            ["On Left Button Click"] = 0,
            ["On Right Button Click"] = 1,
            ["On Middle Button Click"] = 2,
            ["On Left Button Holding"] = 3,
            ["On Right Button Holding"] = 4,
            ["On Middle Button Holding"] = 5,
        },
        editable = false,
    },
    flags = {
        fieldType = "list",
        minimumElements = 1,
    },
    flagMode = {
        fieldType = "integer",
        options = {
            ["Flag On"] = 0,
            ["Flag Off"] = 1,
            ["Flag Switch"] = 2,
        },
        editable = false,
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/MouseFlag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
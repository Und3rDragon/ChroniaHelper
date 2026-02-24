local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/CounterCassetteBlockManager"

controller.placements = {
    name = "controller",
    data = {
        flag = "flag",
        mode = 2,
        targetCassetteBlockCounter = "counterCassetteBlockCounter",
        operation = 0,
        number = 1,
        maxCassetteBlockIndex = -1,
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y", "_id", "_name"
}

controller.fieldInformation = 
{
    mode = {
        fieldType = "integer",
        options = {
            ["On Flag Enable"] = 0,
            ["On Flag Disable"] = 1,
            ["On Flag Change"] = 2,
        },
        editable = false,
    },
    operation = {
        fieldType = "integer",
        options = {
            ["Add"] = 0,
            ["Minus"] = 1,
            ["Multiply"] = 2,
            ["Divide"] = 3,
        },
        editable = false,
    },
    number = {
        fieldType = "integer",
        minimumValue = 1,
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/CounterCassetteBlockManager", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
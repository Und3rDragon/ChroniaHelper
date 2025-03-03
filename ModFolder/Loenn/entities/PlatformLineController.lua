local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local platfromLineController = {}

platfromLineController.depth = -10000000

platfromLineController.name = "ChroniaHelper/PlatformLineController"

platfromLineController.placements = {
    {
        name = "platfromLineController",
        data = {
            edgeColor = "2a1923",
            centerColor = "160b12",
            renderMode = 0,
        }
    }
}

platfromLineController.fieldInformation = 
{
    renderMode = {
        options = {
            ["Override All"] = 3,
            ["Moving Platform Lines"] = 1,
            ["Sinking Platform Lines"] = 2,
            ["Disabled"] = 0,
        },
        editable = false,
    },
    edgeColor = {
        fieldType = "color",
    },
    centerColor = {
        fieldType = "color",
    },
}

function platfromLineController.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/PlatformLineController", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

function HexToColor(hex)
    local r = tonumber(hex:sub(1, 2), 16)
    local g = tonumber(hex:sub(3, 4), 16)
    local b = tonumber(hex:sub(5, 6), 16)
    local rgb = {r / 255, g / 255, b / 255}
    return rgb
end

return platfromLineController
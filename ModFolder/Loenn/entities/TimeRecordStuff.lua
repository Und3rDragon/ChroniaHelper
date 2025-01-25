local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local timeRecordStuff = {}

timeRecordStuff.depth = -10000000

timeRecordStuff.name = "ChroniaHelper/TimerRecordStuff"

timeRecordStuff.placements = {
    {
        name = "timeRecordStuff",
        data = {
            text =  "",
            recordID = "",
            color = "ffd700",
            showMilliseconds = false,
            showUnits = false
        }
    }
}

timeRecordStuff.canResize = {false, false}

timeRecordStuff.fieldInformation = 
{
    width = {
        editable = false
    },
    height = {
        editable = false
    },
    color = {
        fieldType = "color",
    },
}

function timeRecordStuff.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Timer", entity):setPosition(entity.x + 8, entity.y + 8)

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

return timeRecordStuff
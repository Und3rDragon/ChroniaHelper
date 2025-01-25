local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local customTimer = {}

customTimer.depth = -10000000

customTimer.name = "ChroniaHelper/CustomTimer"

customTimer.placements = {
    {
        name = "CustomTimer",
        data = {
            text =  "",
            timeLimit = "00:30:00.000";
            overTimeLimitColor = "ffffff",
            underTimeLimitColor = "ffd700",
            defaultTextColor = "ffffff",
            invaildColor = "ff4500",
            completeFlag = "",
            useRawTime = false,
            checkTimeLimit = true,
            checkIsValidRun = true,
            showMilliseconds = false,
            showUnits = false
        }
    }
}

customTimer.canResize = {false, false}

customTimer.fieldInformation = 
{
    width = {
        editable = false
    },
    height = {
        editable = false
    },
    overTimeLimitColor = {
        fieldType = "color",
    },
    underTimeLimitColor = {
        fieldType = "color",
    },
    invaildColor = {
        fieldType = "color",
    },
    defaultTextColor = {
        fieldType = "color",
    },
}

function customTimer.sprite(room, entity)
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

return customTimer
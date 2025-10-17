local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local renderer = {
    name = "ChroniaHelper/StopclockRenderer"
}

renderer.depth = -10000000

renderer.placements = {
    name = "renderer",
    data = {
        depth = -10000000,
        sourcePath = "ChroniaHelper/StopclockFonts/font",
        stopclockTag = "stopclock",
        parallaxX = 1,
        parallaxY = 1,
        screenX = 160,
        screenY = 90,
        renderMode = 0,
        positionAlign = 0,
        segmentAlign = 0,
        segmentDistance = 8,
    }, 
    --nodeLimits = {0,2}
}

renderer.fieldInformation = {
    positionAlign = {
        options = {
            ["center"] = 5,
            ["top left"] = 1,
            ["top center"] = 2,
            ["top right"] = 3,
            ["center left"] = 4,
            ["center right"] = 6,
            ["bottom left"] = 7,
            ["bottom center"] = 8,
            ["bottom right"] = 9,
        },
        editable = false,
    },
    segmentAlign = {
        options = {
            ["center"] = 5,
            ["top left"] = 1,
            ["top center"] = 2,
            ["top right"] = 3,
            ["center left"] = 4,
            ["center right"] = 6,
            ["bottom left"] = 7,
            ["bottom center"] = 8,
            ["bottom right"] = 9,
        },
        editable = false,
    },
    renderMode = {
        options = {
            ["Compact"] = 0,
            ["Equal Distance"] = 1,
        },
        editable = false,
    }
}

renderer.selection = function (room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

function renderer.sprite(room, entity)
    local sprite = {}
    
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Stopclock", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return renderer
local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local renderer = {
    name = "ChroniaHelper/MessageDisplayer"
}

renderer.depth = -10000000

renderer.placements = {
    name = "renderer",
    data = {
        depth = -10000000,
        message = "",
        lineOriginX = 1,
        lineOriginY = 1,
        letterOriginX = 0.5,
        letterOriginY = 0.5,
        letterDistance = 0,
        originX = 0.5,
        originY = 0.5,
        screenX = 160,
        screenY = 90,
        renderMode = 0,
        lineDistance = 0,
        fontColor = "ffffff",
    }, 
    --nodeLimits = {0,2}
}

renderer.fieldInformation = {
    segmentDistance = {
        fieldType = "integer",
    },
    sourcePath = {
        options = {
            "ChroniaHelper/StopclockFonts/font",
            "ChroniaHelper/StopclockFonts/fontB",
            ["(Different Sized Test Sprite)"] = "ChroniaHelper/StopclockFonts/differentSized/font",
        },
        editable = true,
    },
    positionAlign = {
        fieldType = "integer",
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
        fieldType = "integer",
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
        fieldType = "integer",
        options = {
            ["Compact"] = 0,
            ["Equal Distance"] = 1,
        },
        editable = false,
    },
    rendererColor = {
        fieldType = "color",
        useAlpha = true,
    },
    maximumUnit = {
        fieldType = "integer",
        options = { ["Year"] = 6, ["Month"] = 5, ["Day"] = 4, ["Hour"] = 3, ["Minute"] = 2, ["Second"] = 1, ["Millisecond"] = 0 },
        editable = false,
    },
    minimumUnit = {
        fieldType = "integer",
        options = { ["Year"] = 6, ["Month"] = 5, ["Day"] = 4, ["Hour"] = 3, ["Minute"] = 2, ["Second"] = 1, ["Millisecond"] = 0 },
        editable = false,
    },
    depth = require('mods').requireFromPlugin('helpers.field_options').depths,
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
local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local renderer = {
    name = "ChroniaHelper/MessageDisplayerNormal"
}

renderer.depth = -10000000

renderer.placements = {
    name = "renderer",
    data = {
        depth = -10000000,
        textures = "ChroniaHelper/DisplayFonts/font",
        characterReference = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/.<>()[]{}'\"?!\\:; =,",
        dialogID = "",
        letterOriginX = 0,
        letterOriginY = 0,
        lineOriginX = 0.5,
        lineOriginY = 0.5,
        overallOriginX = 0.5,
        overallOriginY = 0.5,
        parallaxX = 1,
        parallaxY = 1,
        screenX = 160,
        screenY = 90,
        renderMode = 0,
        lineDistance = 0,
        letterDistance = 0,
        renderDistance = -1,
        fadeInSpeed = 4,
        fadeOutSpeed = 2,
        letterDisplayInterval = 0.1,
        fontColor = "ffffff",
        triggerFlag = "",
        scale = "1",
        typewriterEffect = false,
    }, 
    --nodeLimits = {0,2}
}

renderer.fieldInformation = {
    scale = {
        fieldType = "list",
        minimumElements = 1,
        elementOptions = {
            fieldType = "integer",
        },
    },
    textures = {
        fieldType = "list",
        minimumElements = 1,
    },
    screenX = {
        fieldType = "integer",
    },
    screenY = {
        fieldType = "integer",
    },
    letterDistance = {
        fieldType = "integer",
    },
    lineDistance = {
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
    
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Message", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return renderer
local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local parallaxText = {}

parallaxText.name = "ChroniaHelper/ParallaxText"

parallaxText.nodeLimits = {1,1}

parallaxText.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = true,
    },
    borderColor = {
        fieldType = "color",
        useAlpha = true,
    },
    visibleDistanceX = {
        minimumValue = 0.0
    },
    visibleDistanceY = {
        minimumValue = 0.0
    }
}

parallaxText.placements = {
    name = "parallax_text",
    data = {
        x = 0,
        y = 0,
        width = 8,
        height = 8,
        parallaxX = 1.75,
        parallaxY = 1.75,
        textScalarX = 1.25,
        textScalarY = 1.25,
        zeroFadeX = 16,
        zeroFadeY = 16,
        zeroParallaxPositionX = 960,
        zeroParallaxPositionY = 540,
        color = "ffffff",
        borderColor = "000000",
        dialog = "",
        flag = "",
        border = false
    }
}

parallaxText.sprite = function(room, entity, viewport)
    return {
        require("structs.drawable_rectangle").fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height,
            {1,1,1,0.3}, {1,1,1,1}),
    }
end

parallaxText.nodeSprite = function(room, entity, node, nodeIndex, viewport)
    return {
        require("structs.drawable_rectangle").fromRectangle("bordered", node.x - 4, node.y - 4, 8, 8,
            {1,1,1,0.3}, {1,1,1,1}),
        drawableLine.fromPoints({node.x, node.y, entity.x + entity.width / 2, entity.y + entity.height / 2}, 1)
    }
end

return parallaxText
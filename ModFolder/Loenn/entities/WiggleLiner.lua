local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")
local draw = require("utils.drawing")
local drawLine = require("structs.drawable_line")

local PointAndLine = {}

PointAndLine.name = "ChroniaHelper/WiggleLiner"

PointAndLine.fieldInformation = {
    Depth = {
        fieldType = "integer",
    },
    Color = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
            useAlpha = true,
        },
    },
    ColorFadeResolution = {
        fieldType = "integer",
        minimumValue = 0,
    },
}

PointAndLine.placements = {
    name = "line",
    data = {
        Depth = 9000,
        Color = "ffffffff",
        WiggleFrequencyX = 0.0,
        WiggleAmplifyX = 0.0,
        WigglePhaseX = 0.0,
        WiggleFrequencyY = 2.0,
        WiggleAmplifyY = 4.0,
        WigglePhaseY = 0.0,
        LineThickness = 2,
        Path = "CorreWithCare/WiggleLine/dot",
        ColorFadeResolution = 10,
        AllowNodeAlpha = false,
    },
}

PointAndLine.fieldOrder = {
    "x", "y", "Depth", "Color", "WiggleFrequency", "WiggleAmplify", "WigglePhase",
}

PointAndLine.nodeLimits = {0, -1}
PointAndLine.nodeLineRenderType = "line"
PointAndLine.justification = {0.5, 0.5}

local function fetchColor(entity)
    local colors = entity.Color
    if #colors > 0 then
        return colors[1]
    end
    return "ffffffff"
end

local function getSafeThickness(entity)
    local thick = entity.LineThickness or 2
    if thick <= 0 then
        thick = 1
    end
    return thick
end

local function createCurve(startPos, stopPos, color, thickness)
    local control = {
        (startPos[1] + stopPos[1]) / 2,
        (startPos[2] + stopPos[2]) / 2
    }
    local points = draw.getSimpleCurve(startPos, stopPos, control, 2)
    return drawLine.fromPoints(points, color, thickness)
end

function PointAndLine.sprite(room, entity)
    local texture = entity.Path or "CorreWithCare/WiggleLine/dot"
    local color = fetchColor(entity)
    local thickness = getSafeThickness(entity)

    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setColor(color)

    local all_sprites = {sprite}

    if entity.nodes then
        for nodeIndex, value in ipairs(entity.nodes) do
            local nodeSprite = drawableSprite.fromTexture(texture, entity)
            nodeSprite:setColor(color)
            nodeSprite:setPosition(entity.nodes[nodeIndex].x, entity.nodes[nodeIndex].y)
            table.insert(all_sprites, nodeSprite)

            local startPos
            if nodeIndex == 1 then
                startPos = {entity.x, entity.y}
            else
                startPos = {entity.nodes[nodeIndex - 1].x, entity.nodes[nodeIndex - 1].y}
            end

            local stopPos = {value.x, value.y}
            local curve = createCurve(startPos, stopPos, color, thickness)
            table.insert(all_sprites, curve)
        end
    end

    return all_sprites
end

function PointAndLine.nodeSprite(room, entity, node, nodeIndex, viewport)
    local texture = entity.Path or "CorreWithCare/WiggleLine/dot"
    local color = fetchColor(entity)
    local thickness = getSafeThickness(entity)

    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setColor(color)
    sprite:setPosition(entity.nodes[nodeIndex].x, entity.nodes[nodeIndex].y)

    local all_sprites = {sprite}

    local startPos
    if nodeIndex == 1 then
        startPos = {entity.x, entity.y}
    else
        startPos = {entity.nodes[nodeIndex - 1].x, entity.nodes[nodeIndex - 1].y}
    end

    local stopPos = {entity.nodes[nodeIndex].x, entity.nodes[nodeIndex].y}
    local curve = createCurve(startPos, stopPos, color, thickness)
    table.insert(all_sprites, curve)

    return all_sprites
end

function PointAndLine.selection(room, entity)
    local texture = entity.Path or "CorreWithCare/WiggleLine/dot"
    local sprite = drawableSprite.fromTexture(texture, entity)
    local spriteWidth = sprite.meta.width
    local spriteHeight = sprite.meta.height

    local main = utils.rectangle(
        entity.x - 0.5 * spriteWidth,
        entity.y - 0.5 * spriteWidth,
        spriteWidth,
        spriteHeight
    )

    local nodes = {}

    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            nodes[i] = utils.rectangle(
                node.x - 0.5 * spriteWidth,
                node.y - 0.5 * spriteWidth,
                spriteWidth,
                spriteHeight
            )
        end
    end

    return main, nodes
end

return PointAndLine
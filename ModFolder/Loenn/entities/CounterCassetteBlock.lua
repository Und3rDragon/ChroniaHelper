local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")
local mods = require("mods")

local dashToggleBlock = {
    name="ChroniaHelper/CounterCassetteBlock",
    minimumSize = {16,16},
    fieldInformation={
        counterValue = {
            fieldType = "integer",
            minimumValue = 0,
        },
        color = {
            fieldType = "color",
            useAlpha = true,
        },
        disabledColor = {
            fieldType = "color",
        },
    },
    placements={
        name = "block",
        data = {
            width = 16,
            height = 16,
            counterValue = 0,
            color = "ffffffff",
            disabledColor = "667da5",
            directory = "objects/ChroniaHelper/counterCassetteBlock/",
            counter = "counterCassetteBlockCounter",
        },
    },
}

local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.counterValue == target.counterValue and entity.counter == target.counter
    end
end

local function getTileSprite(entity, x, y, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 0

        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 8

        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 16

        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 24

        else
            quadX, quadY = 8, 8
        end
    else
        if closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 8, 0

        elseif closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 8, 16

        elseif closedLeft and not closedRight and closedUp and closedDown then
            quadX, quadY = 16, 8

        elseif not closedLeft and closedRight and closedUp and closedDown then
            quadX, quadY = 0, 8

        elseif closedLeft and not closedRight and not closedUp and closedDown then
            quadX, quadY = 16, 0

        elseif not closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 0, 0

        elseif not closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 0, 16

        elseif closedLeft and not closedRight and closedUp and not closedDown then
            quadX, quadY = 16, 16
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(entity.directory .."Active", entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        sprite:setColor(require("mods").requireFromPlugin("helpers.vivUtilsMig").getColor(entity.color, true))

        sprite.depth = -10

        return sprite
    end
end

function dashToggleBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    for x = 1, math.ceil((entity.width or 32) / 8) do
        for y = 1, math.ceil((entity.height or 32) / 8) do
            local sprite = getTileSprite(entity, x, y, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

return dashToggleBlock

local omniSetups = require("mods").requireFromPlugin("consts.omniSetups")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("helpers.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")
local drawableText = require("structs.drawable_text")

local OmniZipMover = {}

OmniZipMover.name = "ChroniaHelper/OmniZipMover2"
OmniZipMover.depth = 4999
OmniZipMover.minimumSize = {16, 16}
OmniZipMover.nodeLimits = {1, -1}
OmniZipMover.nodeVisibility = "never"
OmniZipMover.fieldInformation = {
    theme = {
        options = {
            ["Normal"] = "objects/zipmover/",
            ["Moon"] = "objects/zipmover/moon/",
        },
        editable = true,
    },
    backgroundColor = {
        fieldType = "color",
    },
    customSkin = require("mods").requireFromPlugin("helpers.vivUtilsMig").getDirectoryPathFromFile(true),
    moveTimes = {
        fieldType = "list",
    },
    returnTimes = {
        fieldType = "list",
    },
    eases = {
        fieldType = "list",
        elementOptions = {
            options = ChroniaHelper.easers,
            editable = false,
        },
        allowEmpty = false,
    },
    returnEases = {
        fieldType = "list",
        elementOptions = {
            options = ChroniaHelper.easers,
            editable = false,
        },
        allowEmpty = false,
    },
    delays = {
        fieldType = "list",
    },
    returnDelays = {
        fieldType = "list",
    },
    ropeColor = {
        fieldType = "color",
    },
    ropeLightColor = {
        fieldType = "color",
    },
    mode = {
        options = {
            ["normal"] = 1,
            ["persistent"] = 2,
            ["cycle"] = 3,
        },
        editable = false,
    },
}

OmniZipMover.fieldOrder = omniSetups.fieldOrder

OmniZipMover.placements = {
    name = "OmniZipMover2",
    placementType = "rectangle",
    data = {
        width = 16,
        height = 16,
        sprite = "objects/zipmover/",
        moveTimes = "0.5",
        returnTimes = "1",
        eases = "sinein",
        returnEases = "sinein",
        delays = "0.2",
        returnDelays = "0.2",
        ropeColor = "663931",
        ropeLightColor = "9b6157",
        backgroundColor = "000000",
        mode = 1,
        startDelay = 0.1,
        shake = true,
        waitForPlayer = false,
        rememberPosition = false,
    }
}

OmniZipMover.ignoredFields = function(entity)
    return ignoredAttrs(entity)
end

function ignoredAttrs(entity)
    local attrs = {"_name","_id","x","y"}

    return attrs
end

local zipMoverRoleColor = {102 / 255, 57 / 255, 49 / 255}

local function getSearchPredicate()
    return function(target)
        return target._name == "ChroniaHelper/SolidExtension"
    end
end

local function getTileSprite(entity, x, y, block, inner, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false
    local frame = block

    if completelyClosed then
        frame = inner
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 8, 0
        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 0, 0
        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 8, 8
        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 0, 8
        else
            quadX, quadY = 8, 8
            frame = block
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
        local sprite = drawableSprite.fromTexture(frame, entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)

        return sprite
    end
end

local function getOmniZipMoverThemeData(entity)
    local path = entity.sprite

    return {
        block = entity.sprite .. "block",
        light = entity.sprite .. "light01",
        cog = entity.sprite .. "cog",
        inner = entity.sprite .. "innerCorners"
    }
end

function OmniZipMover.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, require('mods').requireFromPlugin('libraries.vivUtilsMig').getColorTable(entity.backgroundColor, true, {0,0,0,1}))
    local sprites = {rectangle:getDrawableSprite()}

    local themeData = getOmniZipMoverThemeData(entity)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeSprites = ChroniaHelper.getZipMoverNodeSprites(x, y, width, height, nodes, themeData.cog, {1, 1, 1}, require('mods').requireFromPlugin('libraries.vivUtilsMig').getColorTable(entity.ropeColor, true, {1,1,1,1}))
    for _, sprite in ipairs(nodeSprites) do
        table.insert(sprites, sprite)
    end

    local relevantBlocks = utils.filter(getSearchPredicate(), room.entities)
    connectedEntities.appendIfMissing(relevantBlocks, entity)
    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    for i = 1, tileWidth do
        for j = 1, tileHeight do
            local sprite = getTileSprite(entity, i, j, themeData.block, themeData.inner, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    for nodeIndex, node in ipairs(nodes) do
        for i = 1, tileWidth do
            for j = 1, tileHeight do
                local sprite = getTileSprite(entity, i, j, themeData.block, themeData.inner, rectangles)
                
                if sprite then
                    sprite:addPosition(node.x - entity.x, node.y - entity.y)
                    sprite:setColor({1,1,1,0.3})
                    table.insert(sprites, sprite)
                end
            end
        end

        --local text = drawableText.fromText(getStringAtIndex(entity.nodeSpeeds, nodeIndex), node.x, node.y)
        --local texta = drawableText.fromText(getStringAtIndex(entity.delays, nodeIndex), node.x, node.y, entity.width * 2, entity.height * 2)
        if entity.displayParameters then
            table.insert(sprites, text)
            table.insert(sprites, texta)
        end
    end

    local lightsSprite = drawableSprite.fromTexture(themeData.light, entity)
    lightsSprite:addPosition(math.floor(width / 2), 0)
    lightsSprite:setJustification(0.5, 0.0)
    table.insert(sprites, lightsSprite)

    return sprites
end

function getStringAtIndex(str, index)
    local strings = {}
    for substring in str:gmatch("([^,]+)") do
        strings[#strings + 1] = substring
    end
    
    if #strings == 0 then
        return ""
    elseif index <= #strings then
        return strings[index]
    else
        return strings[#strings]
    end
end

function OmniZipMover.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local mainRectangle = utils.rectangle(x, y, width, height)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeRectangles = {}
    for _, node in ipairs(nodes) do
        local centerNodeX, centerNodeY = node.x + halfWidth, node.y + halfHeight

        table.insert(nodeRectangles, utils.rectangle(centerNodeX - 5, centerNodeY - 5, 10, 10))
    end

    return mainRectangle, nodeRectangles
end

--return OmniZipMover

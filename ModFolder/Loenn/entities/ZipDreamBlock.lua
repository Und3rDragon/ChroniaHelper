local omniSetups = require("mods").requireFromPlugin("consts.omniSetups")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("libraries.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")

local OmniZipMover = {}


OmniZipMover.name = "ChroniaHelper/ZipDream"
OmniZipMover.depth = 4999
OmniZipMover.minimumSize = {16, 16}
OmniZipMover.nodeLimits = {1, -1}
OmniZipMover.nodeVisibility = "never"
OmniZipMover.fieldInformation = omniSetups.fieldInfo

OmniZipMover.fieldOrder = omniSetups.fieldOrder

OmniZipMover.placements = {
    name = "zipdreamblock",
        placementType = "rectangle",
        data = {
            width = 16,
            height = 16,
            delays = "0.2,0.2",
            permanent = false,
            waiting = false,
            ticking = false,
            synced = false,
            customSkin = "",
            backgroundColor = "000000",
            ropeColor = "",   --default 663931
            ropeLightColor = "",  --default 9b6157
            easing = "SineIn",
            nodeSpeeds = "1,1",
            ticks = 5,
            tickDelay = 1,
            startDelay = 0.1,
            syncTag = "",
            hideRope = false,
            hideCog = false,
            -- dashable params
            --dashable = false,
            onDash = "Normal",
            onlyDashActivate = false,
            dashableOnce = true,
            dashableRefill = false,
            -- return params
            tweakReturnParams = false,
            returnSpeeds = "1,1",
            returnDelays = "0.2,0.2",
            returnedIrrespondingTime = 0.5,
            returnEasing = "SineIn",
            -- time units
            timeUnits = false,
            sideFlag = false,
            nodeFlags = "",
            touchSensitive = "none",
            displayParameters = false,
            indicator = "",
            indicatorExpansion = 0,
        }
}

OmniZipMover.ignoredFields = function(entity)
    return ignoredAttrs(entity)
end

function ignoredAttrs(entity)
    local attrs = {"_name","_id","x","y"}
    local soundAttrs = {
        "startSound",
        "impactSound","tickSound",
        "returnSound","finishSound","nodeSound",
    }

    local shakeAttrs = {
        "startShaking", "nodeShaking",
        "returnShaking", "tickingShaking",
        "permanentArrivalShaking",
    }

    local returnAttrs = {
        "returnDelays", "returnSpeeds", "returnEasing",
        "returnedIrrespondingTime",
    }

    local dashableAttrs = {
        "dashableOnce", "dashableRefill","onlyDashActivate",
    }

    local waterAttrs = {
        "topSurface","bottomSurface",
    }

    local glassAttrs = {
        "starColors","starPath",
    }

    if not entity.customSound then
        for _, item in ipairs(soundAttrs) do
            table.insert(attrs, item)
        end
    end

    if not entity.tweakShakes then
        for _, item in ipairs(shakeAttrs) do
            table.insert(attrs, item)
        end
    end

    if not entity.tweakReturnParams then
        for _, item in ipairs(returnAttrs) do
            table.insert(attrs, item)
        end
    end

    --[[
    if not entity.dashable then
        for _, item in ipairs(dashableAttrs) do
            table.insert(attrs, item)
        end
    end
    ]]
    

    if entity.theme == "glass" then
        for _, item in ipairs(waterAttrs) do
            table.insert(attrs, item)
        end
    end

    if entity.theme == "water" then
        for _, item in ipairs(glassAttrs) do
            table.insert(attrs, item)
        end
    end

    return attrs
end

local zipMoverRoleColor = {102 / 255, 57 / 255, 49 / 255}

local function getSearchPredicate()
    return function(target)
        return target._name == "CommunalHelper/SolidExtension"
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
    local theme = string.lower(entity.theme or "water")
    local folder = "ChroniaHelper/omniZipMover" or "zipmover"
    local themePath = (theme == "water") and "" or (theme .. "/")

    local customSkin = entity.customSkin or ""
    if customSkin ~= "" then
        return {
            light = customSkin .. "/light01",
            cog = customSkin .. "/cog",
        }
    end

    return {
        light = "objects/" .. folder .. "/" .. themePath .. "light01",
        cog = "objects/" .. folder .. "/" .. themePath .. "cog",
    }
end

function OmniZipMover.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local rectangle = drawableRectangle.fromRectangle("bordered",x,y,width,height,{28,69,212,0.2},{255,255,255,1})
    local sprites = {rectangle}

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeSprites = ChroniaHelper.getZipMoverNodeSprites(x, y, width, height, nodes, "objects/zipmover/cog", {1, 1, 1}, require('mods').requireFromPlugin('libraries.vivUtilsMig').getColorTable(entity.ropeColor, true, {1,1,1,1}))
    for _, sprite in ipairs(nodeSprites) do
        table.insert(sprites, sprite)
    end

    for nodeIndex, node in ipairs(nodes) do
        local nodeRect = drawableRectangle.fromRectangle("bordered",node.x,node.y,width,height,{28,69,212,0.1},{255,255,255,0.3})
        table.insert(sprites, nodeRect)

        local text = drawableText.fromText(ChroniaHelper.getStringAtIndex(entity.nodeSpeeds, nodeIndex), node.x, node.y)
        local texta = drawableText.fromText(ChroniaHelper.getStringAtIndex(entity.delays, nodeIndex), node.x, node.y, entity.width * 2, entity.height * 2)
        if entity.displayParameters then
            table.insert(sprites, text)
            table.insert(sprites, texta)
        end
    end

    return sprites
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

return OmniZipMover

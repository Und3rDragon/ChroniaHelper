local omniSetups = require("mods").requireFromPlugin("consts.omniSetups")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("helpers.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")
local drawableText = require("structs.drawable_text")

local OmniZipMover = {}

local themes = {
    "Normal",
    "Moon",
    "Cliffside"
}

OmniZipMover.associatedMods = { "CommunalHelper", "ChroniaHelper" }

OmniZipMover.name = "ChroniaHelper/OmniZipMover"
OmniZipMover.depth = 4999
OmniZipMover.minimumSize = {16, 16}
OmniZipMover.nodeLimits = {1, -1}
OmniZipMover.nodeVisibility = "never"
OmniZipMover.fieldInformation = {
    theme = {
        options = themes,
        editable = false
    },
    backgroundColor = {
        fieldType = "color",
        allowEmpty = true,
    },
    ropeColor = {
        fieldType = "color",
        allowEmpty = true,
    },
    ropeLightColor = {
        fieldType = "color",
        allowEmpty = true,
    },
    easing = {
        fieldType = "list",
        elementOptions = {
            options = ChroniaHelper.easers,
            editable = false,
        },
        allowEmpty = false,
    },
    returnEasing = {
        fieldType = "list",
        elementOptions = {
            options = ChroniaHelper.easers,
            editable = false,
        },
        allowEmpty = false,
    },
    ticks = {
        fieldType = "integer",
    },
    startSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/start"},
        editable = true,
    },
    impactSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/impact"},
        editable = true,
    },
    tickSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/tick"},
        editable = true,
    },
    returnSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/return"},
        editable = true,
    },
    finishSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/finish"},
        editable = true,
    },
    customSkin = require("mods").requireFromPlugin("helpers.vivUtilsMig").getDirectoryPathFromFile(true),
    delays = {
        fieldType = "list",
    },
    nodeSpeeds = {
        fieldType = "list",
    },
    returnSpeeds = {
        fieldType = "list",
    },
    returnDelays = {
        fieldType = "list",
    },
    touchSensitive = {
        options = {
            "none", "bottom", "sideways", "always"
        },
        editable = false,
    },
    onDash = {
        options = {
            "Rebound", "Bounce", "Normal",
        },
        editable = false,
    },
    indicator = {
        fieldType = "color",
        allowEmpty = true,
    },
}

OmniZipMover.fieldOrder = omniSetups.fieldOrder

OmniZipMover.placements = {}
for i, theme in ipairs(themes) do
    OmniZipMover.placements[i] = {
        name = string.lower(theme),
        placementType = "rectangle",
        data = {
            width = 16,
            height = 16,
            theme = theme,
            delays = "0.2,0.2",
            permanent = false,
            waiting = false,
            ticking = false,
            synced = false,
            customSkin = "",
            backgroundColor = "",
            ropeColor = "",
            --default 663931
            ropeLightColor = "",
            --default 9b6157
            easing = "SineIn",
            nodeSpeeds = "1,1",
            ticks = 5,
            tickDelay = 1,
            startDelay = 0.1,
            customSound = false,
            startSound = "event:/CommunalHelperEvents/game/zipMover/normal/start",
            impactSound = "event:/CommunalHelperEvents/game/zipMover/normal/impact",
            tickSound = "event:/CommunalHelperEvents/game/zipMover/normal/tick",
            returnSound = "event:/CommunalHelperEvents/game/zipMover/normal/return",
            finishSound = "event:/CommunalHelperEvents/game/zipMover/normal/finish",
            nodeSound = true;
            startShaking = true,
            nodeShaking = true,
            returnShaking = true,
            tickingShaking = true,
            permanentArrivalShaking = true,
            tweakShakes = false,
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
            -- time
            timeUnits = false,
            sideFlag = false,
            nodeFlags = "",
            touchSensitive = "none",
            displayParameters = false,
            indicator = "",
            indicatorExpansion = 0,
        }
    }
end

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
    local theme = string.lower(entity.theme or "normal")
    local cliffside = theme == "cliffside"
    local folder = cliffside and "ChroniaHelper/omniZipMover" or "zipmover"
    local themePath = (theme == "normal") and "" or (theme .. "/")

    local customSkin = entity.customSkin or ""
    if customSkin ~= "" then
        return {
            block = customSkin .. "/block",
            light = customSkin .. "/light01",
            cog = customSkin .. "/cog",
            inner = customSkin .. "/innerCorners"
        }
    end

    return {
        block = "objects/" .. folder .. "/" .. themePath .. "block",
        light = "objects/" .. folder .. "/" .. themePath .. "light01",
        cog = "objects/" .. folder .. "/" .. themePath .. "cog",
        inner = "objects/" .. ((cliffside and "" or "ChroniaHelper/") .. folder) .. "/" .. themePath .. "innerCorners"
    }
end

function OmniZipMover.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, require('mods').requireFromPlugin('helpers.vivUtilsMig').getColorTable(entity.backgroundColor, true, {0,0,0,1}))
    local sprites = {rectangle:getDrawableSprite()}

    local themeData = getOmniZipMoverThemeData(entity)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeSprites = ChroniaHelper.getZipMoverNodeSprites(x, y, width, height, nodes, themeData.cog, {1, 1, 1}, require('mods').requireFromPlugin('helpers.vivUtilsMig').getColorTable(entity.ropeColor, true, {1,1,1,1}))
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

        local text = drawableText.fromText(getStringAtIndex(entity.nodeSpeeds, nodeIndex), node.x, node.y)
        local texta = drawableText.fromText(getStringAtIndex(entity.delays, nodeIndex), node.x, node.y, entity.width * 2, entity.height * 2)
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

return OmniZipMover

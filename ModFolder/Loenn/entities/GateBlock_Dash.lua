local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")
local drawing = require("utils.drawing")
local entities = require("entities")

-- somewhat based on flag switch gates from maddiehelpinghand as is the case for most game block stuff :p
-- https://github.com/maddie480/MaddieHelpingHand/blob/master/Loenn/entities/flagSwitchGate.lua
-- (rendering is mostly mine though and based on exclamation blocks if anything)

local dashGateBlock = {}

dashGateBlock.associatedMods = { "SorbetHelper", "ChroniaHelper" }

dashGateBlock.name = "ChroniaHelper/DashGateBlock"
dashGateBlock.depth = 0
dashGateBlock.nodeLimits = {1, 999}
dashGateBlock.nodeLineRenderType = false
dashGateBlock.warnBelowSize = {16, 16}

local axes = {
    "Both",
    "Horizontal",
    "Vertical",
}

dashGateBlock.placements = {}
for k,v in ipairs(axes) do
    table.insert(dashGateBlock.placements, {
        name = v,
        alternativeName = "kevinswitchgate",
        data = {
            width = 16,
            height = 16,
            blockSprite = "SorbetHelper/gateblock/dash/block",
            iconSprite = "switchgate/icon",
            inactiveColor = "5fcde4",
            activeColor = "ffffff",
            finishColor = "f141df",
            smoke = true,
            moveSound = "event:/sorbethelper/sfx/gateblock_open",
            finishedSound = "event:/sorbethelper/sfx/gateblock_finish",
            shakeTime = 0.5,
            moveTime = "1.8",
            --moveEased = true,
            persistent = false,
            allowReturn = false,
            axes = v,
            allowWavedash = false,
            dashCornerCorrection = false,
            linkTag = "", -- this is flag on activate in editor

            ease = "CubeOut",
            nodeDelays = "0",
            returnDelays = "0",
        }
    })
end

dashGateBlock.fieldOrder = {
    "x", "y", 
    "width", "height", 
    "inactiveColor", "activeColor", 
    "finishColor", "moveSound", 
    "blockSprite", "finishedSound", 
    "iconSprite", "shakeTime", 
    "axes", "moveTime", 
    "linkTag", "moveEased", "ease", 
    "nodeDelays", "returnDelays",
    "allowReturn", "allowWavedash", 
    "dashCornerCorrection", 
    "smoke", "persistent"}

dashGateBlock.fieldInformation = {
    inactiveColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    },
    blockSprite = {
        options = {
            "SorbetHelper/gateblock/dash/block"
        },
        editable = true
    },
    iconSprite = {
        options = {
            "switchgate/icon",
            -- "SorbetHelper/gateblock/dash/icon"        
        },
        editable = true
    },
    axes = {
        options = axes,
        editable = false
    },
    ease = {
        fieldType = "list",
        elementOptions = {
            options = require("mods").requireFromPlugin("helpers.chroniaHelper_old").easers,
            editable = false,
        },
        allowEmpty = false,
    },
    moveTime = {
        fieldType = "list",
        allowEmpty = false,
    },
    nodeDelays = {
        fieldType = "list",
    },
    returnDelays = {
        fieldType = "list"
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local function addNinePatchSprite(spriteTable, x, y, width, height, texture, color)
    color = color or {1.0, 1.0, 1.0, 1.0}

    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, height)
    ninePatch.color = color
    local sprites = ninePatch:getDrawableSprite()

    for _,v in ipairs(sprites) do
        table.insert(spriteTable, v)
    end
end

local function createSprites(entity, x, y, width, height, color)
    local sprites = {}

    local blockPath = entity.blockSprite or "SorbetHelper/gateblock/dash/block"
    local iconPath = entity.iconSprite or "switchgate/icon"

    local axes = entity.axes or "Both"
    if axes == "Horizontal" then
        blockPath ..= "_h"
    elseif axes == "Vertical" then
        blockPath ..= "_v"
    end

    local blockTexture = "objects/" .. blockPath
    local iconTexture = "objects/" .. iconPath .. "00"
    
    -- main block
    addNinePatchSprite(sprites, x, y, width, height, blockTexture)
    -- lights
    addNinePatchSprite(sprites, x, y, width, height, blockTexture .. "_lights", color)

    local iconSprite = drawableSprite.fromTexture(iconTexture, {x = x, y = y})

    iconSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, iconSprite)

    return sprites
end

function dashGateBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
    color = utils.getColor(entity.inactiveColor or "ffffff")

    -- base sprite
    local sprites = createSprites(entity, x, y, width, height, color)
    
    for nodeIndex, node in ipairs(entity.nodes) do
        local nodeSprites = createSprites(entity, node.x, node.y, width, height, color)
        for _, item in ipairs(nodeSprites) do
        item:setColor({1,1,1,0.3})
            table.insert(sprites, item)
        end

        --line sprite
        local lineColor = utils.getColor(entity.activeColor or "ffffff")
        lineColor[4] = 0.5

        local startX = entity.x
        local startY = entity.y
        if nodeIndex > 1 then
            startX = entity.nodes[nodeIndex - 1].x
            startY = entity.nodes[nodeIndex - 1].y
        end
        table.insert(sprites, drawableLine.fromPoints({startX + math.floor(width / 2), startY + math.floor(height / 2), node.x + math.floor(width / 2), node.y + math.floor(height / 2)}, lineColor))
    end

    return sprites
end

--[[
function dashGateBlock.nodeSprite(room, entity, node, nodeIndex, viewport)
    local x, y = entity.x or 0, entity.y or 0
    if nodeIndex >= 2 then
        x = entity.nodes[nodeIndex - 1].x or 0
        y = entity.nodes[nodeIndex - 1].y or 0
    end
    local nx, ny = node.x or 0, node.y or 0
    local width, height = entity.width or 24, entity.height or 24
    local color = utils.getColor(entity.finishColor or "ffffff")
    
    local sprites = createSprites(entity, nx, ny, width, height, color)

    local lineColor = utils.getColor(entity.activeColor or "ffffff")
    lineColor[4] = 0.75
    table.insert(sprites, drawableLine.fromPoints({x + math.floor(width / 2), y + math.floor(height / 2), nx + math.floor(width / 2), ny + math.floor(height / 2)}, lineColor))

    return sprites
end
]]

function dashGateBlock.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local nodeRecs = {}

    for nodeIndex, node in ipairs(nodes) do
        local nx, ny = nodes[nodeIndex].x or x, nodes[nodeIndex].y or y
        local nodeRec = utils.rectangle(nx, ny, width, height)
        table.insert(nodeRecs, nodeRec)
    end

    return utils.rectangle(x, y, width, height), nodeRecs
end

return dashGateBlock

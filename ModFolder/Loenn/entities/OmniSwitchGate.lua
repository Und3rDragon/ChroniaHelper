local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local switchGate = {}

switchGate.name = "ChroniaHelper/OmniSwitchGate"
switchGate.depth = 0
switchGate.nodeLimits = {1, -1}
switchGate.nodeLineRenderType = "line"
switchGate.minimumSize = {16, 16}
switchGate.placements = {}

local textures = {
    "block", "mirror", "temple", "stars"
}
local bundledIcons = {
    "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple"
}
local easeTypes = {
    "Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut"
}

for i, texture in ipairs(textures) do
    switchGate.placements[i] = {
        name = texture,
        data = {
            width = 16,
            height = 16,
            flag = "flag_touch_switch",
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            shakeTime = 0.5,
            moveTime = "0.5",
            returnMoveTime = "2",
            pauseTime = "0.2",
            returnPauseTime = "0.2",
            easers = "CubeOut",
            returnEasers = "CubeOut",
            sprite = texture,
            icon = "objects/switchgate/icon",
            beforeShatterTime = -1,
            canReturn = false,
            persistent = true,
            smoke = true,
            persistent = false,
            moveSound = "event:/game/general/touchswitch_gate_open",
            finishedSound = "event:/game/general/touchswitch_gate_finish",
            shatterShakeSound = "event:/game/general/fallblock_shake",
            shatterSound = "event:/game/general/wall_break_stone",
            shatterDebris = "debris/9",
        }
    }
end

switchGate.fieldInformation = {
    inactiveColor = {
        fieldType = "color",
        useAlpha = true,
    },
    activeColor = {
        fieldType = "color",
        useAlpha = true,
    },
    finishColor = {
        fieldType = "color",
        useAlpha = true,
    },
    sprite = {
        options = textures
    },
    icon = {
        fieldType = "path",
    },
    easing = {
        options = easeTypes,
        editable = false
    },
    moveTime = {
        fieldType = "list",
        minimumElements = 1,
    },
    returnMoveTime = {
        fieldType = "list",
        minimumElements = 1,
    },
    pauseTime = {
        fieldType = "list",
        minimumElements = 1,
    },
    returnPauseTime = {
        fieldType = "list",
        minimumElements = 1,
    },
    easers = {
        fieldType = "list",
        minimumElements = 1,
        elementOptions = require("mods").requireFromPlugin("helpers.field_options").easeMode,
    },
    returnEasers = {
        fieldType = "list",
        minimumElements = 1,
        elementOptions = require("mods").requireFromPlugin("helpers.field_options").easeMode,
    },
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local frameTexture = "objects/switchgate/%s"

function switchGate.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockSprite = entity.sprite or "block"
    local frame = string.format(frameTexture, blockSprite)
    
    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    local middleSprite = require("mods").requireFromPlugin("helpers.vivUtilsMig").getImageWithNumbers(entity.icon, 00, entity)
    local sprites = ninePatch:getDrawableSprite()

    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, middleSprite)

    return sprites
end

function switchGate.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local nodes = entity.nodes or {}
    local nodeRectangles = {}

    for i, node in ipairs(nodes) do
        local rectangle = utils.rectangle(node.x or x, node.y or y, width, height)
        table.insert(nodeRectangles, rectangle)
    end

    return utils.rectangle(x, y, width, height), nodeRectangles
end

return switchGate

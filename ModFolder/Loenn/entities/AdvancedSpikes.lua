local defaultFields = require("mods").requireFromPlugin("consts.default_fields")
local core = require("mods").requireFromPlugin("utils.core")
local string = require("mods").requireFromPlugin("utils.string")
local array = require("mods").requireFromPlugin("utils.array")
local drawableSprite = require("structs.drawable_sprite")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local fieldTable = {
    depth = {
        data = -50,
        info = {
            options = depthOptions,
            editable = true,
            fieldType = "integer",
        },
    },
    spikeType =
    {
        data = "Default",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "Default",
                "Outline",
                "Cliffside",
                "Reflection",
                "Custom"
            },
            editable = false
        }
    },
    sprite =
    {
        data = "",
        info = require("mods").requireFromPlugin("helpers.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    spriteColor =
    {
        data = "ffffff",
        info =
        {
            fieldType = "color",
            allowEmpty = false
        }
    },
    spriteAlpha =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    spriteRotation =
    {
        data = 0,
        info =
        {
            fieldType = "number",
            allowEmpty = false
        }
    },
    spriteScaleX =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    spriteScaleY =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    spriteDisplacement =
    {
        data = - 4,
        info =
        {
            fieldType = "integer",
            allowEmpty = false
        }
    },
    spriteFileIndex =
    {
        data = 0,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    spriteSpacing =
    {
        data = 8,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    animMoveMultiplier =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false
        }
    },
    touchDelay =
    {
        data = 0.05,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    touchSound =
    {
        data = "event:/game/03_resort/fluff_tendril_touch",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    flagNeedToTrigger =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    triggerDelay =
    {
        data = 0.4,
        info =
        {
            fieldType = "number",
            allowEmpty = false
        }
    },
    triggerAnimSpeed =
    {
        data = 8,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    triggerSound =
    {
        data = "event:/game/03_resort/fluff_tendril_emerge",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    flagNeedToRetract =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    retractDelay =
    {
        data = - 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false
        }
    },
    retractAnimSpeed =
    {
        data = 8,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    retractSound =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    trigger =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    waitPlayerLeave =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    attached =
    {
        data = true,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    grouped =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    rainbow =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    randomTexture =
    {
        data = true,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    canRefillDashOnTouch =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    }
}

function spikesFieldOrder(direction)
    local size = ""
    if direction == "horizontal" then
        size = "width"
    elseif direction == "vertical" then
        size = "height"
    end
    return {
        "x",
        "y",
        size,
        "spikeType",
        "sprite",
        "spriteColor",
        "spriteAlpha",
        "spriteRotation",
        "spriteScaleX",
        "spriteScaleY",
        "spriteDisplacement", --spriteOrigin
        "spriteFileIndex",
        "spriteSpacing", --singleSize
        "animMoveMultiplier",--LerpMoveTime
        "touchDelay",
        "touchSound",
        "flagNeedToTrigger",--TriggerFlag
        "triggerDelay",
        "triggerAnimSpeed",--TriggerLerpMove
        "triggerSound",
        "flagNeedToRetract",--RetractFlag
        "retractDelay",
        "retractAnimSpeed",--RetractLerpMove
        "retractSound",
        "trigger",
        "waitPlayerLeave",
        "attached",
        "grouped",
        "rainbow",
        "randomTexture"
    }
end

local spikePath = {
    Default = "danger/spikes/default",
    Outline = "danger/spikes/outline",
    Cliffside = "danger/spikes/cliffside",
    Reflection = "danger/spikes/reflection"
}

local spikeTexture = function(entity, direction)
    local texture = ""
    if entity.spikeType ~= "Custom" then
        texture = spikePath[entity.spikeType]
    else
        if entity.sprite == "" then
            texture = spikePath["Default"]
        else
            texture = entity.sprite
        end
    end
    return texture .. "_" .. direction .. string.supplyFront(entity.spriteFileIndex, "0", 2)
end

local spikeSize = function(entity, direction)
    if direction == "up" or direction == "down" then
        return entity.width
    elseif direction == "left" or direction == "right" then
        return entity.height
    end
    return 0
end

local spikeSprite = function(entity, direction)
    local spikeTexture = spikeTexture(entity, direction)
    local spikeSize = spikeSize(entity, direction)
    local sprites = { }
    for index = 0, spikeSize / entity.spriteSpacing - 1 do
        local sprite = drawableSprite.fromTexture(spikeTexture, entity)
        local position = {
            x = 0,
            y = 0
        }
        if direction == "up" then
            position.x = entity.x + 5 +(entity.spriteSpacing * index) -1
            position.y = entity.trigger and(entity.y - entity.spriteDisplacement - 4) or(entity.y - entity.spriteDisplacement - entity.animMoveMultiplier * 4 - 4)
        elseif direction == "down" then
            position.x = entity.x + 5 +(entity.spriteSpacing * index) -1
            position.y = entity.trigger and(entity.y - entity.spriteDisplacement - 4) or(entity.y - entity.spriteDisplacement - entity.animMoveMultiplier * 4 + 4)
        elseif direction == "left" then
            position.x = entity.trigger and(entity.x - entity.spriteDisplacement - 4) or(entity.x - entity.spriteDisplacement - entity.animMoveMultiplier * 4 - 4)
            position.y = entity.y + 5 +(entity.spriteSpacing * index) -1
        elseif direction == "right" then
            position.x = entity.trigger and(entity.x - entity.spriteDisplacement - 4) or(entity.x - entity.spriteDisplacement - entity.animMoveMultiplier * 4 + 4)
            position.y = entity.y + 5 +(entity.spriteSpacing * index) -1
        end
        sprite:setPosition(position)
        sprite:setColor(entity.spriteColor .. string.format("%x",(255 * entity.spriteAlpha)))
        sprite:setScale(entity.spriteScaleX, entity.spriteScaleY)
        sprite.rotation = entity.spriteRotation
        table.insert(sprites, sprite)
    end
    return sprites
end

local spikesNames = {
    "ChroniaHelper/UpAdvancedSpikes",
    "ChroniaHelper/RightAdvancedSpikes",
    "ChroniaHelper/DownAdvancedSpikes",
    "ChroniaHelper/LeftAdvancedSpikes"
}

function ignoredAttr(entity)
    local attrs = {}

    local triggerAttr = {
        "touchDelay",
        "touchSound",
        "flagNeedToTrigger",--TriggerFlag
        "triggerDelay",
        "triggerAnimSpeed",--TriggerLerpMove
        "triggerSound",
        "flagNeedToRetract",--RetractFlag
        "retractDelay",
        "retractAnimSpeed",--RetractLerpMove
        "retractSound",
        "waitPlayerLeave",
        "animMoveMultiplier",
        "spriteDisplacement",
    }

    if not entity.trigger then
        for _, item in ipairs(triggerAttr) do
            table.insert(attrs, item)
        end
    end

    return attrs
end

local UpAdvancedSpikes = {
    name = "ChroniaHelper/UpAdvancedSpikes",
    placements =
    {
        name = "UpAdvancedSpikes",
        data =
        {
            width = defaultFields.width.data,
        }
    },
    fieldInformation =
    {
        width = defaultFields.width.info,
    },
    fieldOrder = spikesFieldOrder("horizontal"),
    ignoredFields = function(entity)
        return ignoredAttr(entity)
    end,
    sprite = function(room, entity, viewport)
        return spikeSprite(entity, "up")
    end,
    rotate = function(room, entity, direction)
        entity._name = spikesNames[2]
        entity.height = entity.width
        entity.width = nil
        return true
    end
}

local DownAdvancedSpikes = {
    name = "ChroniaHelper/DownAdvancedSpikes",
    placements =
    {
        name = "DownAdvancedSpikes",
        data =
        {
            width = defaultFields.width.data,
        }
    },
    fieldInformation =
    {
        width = defaultFields.width.info,
    },
    fieldOrder = spikesFieldOrder("horizontal"),
    ignoredFields = function(entity)
        return ignoredAttr(entity)
    end,
    sprite = function(room, entity, viewport)
        return spikeSprite(entity, "down")
    end,
    rotate = function(room, entity, direction)
        entity._name = spikesNames[4]
        entity.height = entity.width
        entity.width = nil
        return true
    end
}

local LeftAdvancedSpikes = {
    name = "ChroniaHelper/LeftAdvancedSpikes",
    placements =
    {
        name = "LeftAdvancedSpikes",
        data =
        {
            height = defaultFields.height.data,
        }
    },
    fieldInformation =
    {
        height = defaultFields.height.info,
    },
    fieldOrder = spikesFieldOrder("vertical"),
    ignoredFields = function(entity)
        return ignoredAttr(entity)
    end,
    canResize = { false, true },
    sprite = function(room, entity, viewport)
        return spikeSprite(entity, "left")
    end,
    rotate = function(room, entity, direction)
        entity._name = spikesNames[1]
        entity.width = entity.height
        entity.height = nil
        return true
    end
}

local RightAdvancedSpikes = {
    name = "ChroniaHelper/RightAdvancedSpikes",
    placements =
    {
        name = "RightAdvancedSpikes",
        data =
        {
            height = defaultFields.height.data,
        }
    },
    fieldInformation =
    {
        height = defaultFields.height.info,
    },
    fieldOrder = spikesFieldOrder("vertical"),
    ignoredFields = function(entity)
        return ignoredAttr(entity)
    end,
    canResize = { false, true },
    sprite = function(room, entity, viewport)
        return spikeSprite(entity, "right")
    end,
    rotate = function(room, entity, direction)
        entity._name = spikesNames[3]
        entity.width = entity.height
        entity.height = nil
        return true
    end
}

local spikes = {
    UpAdvancedSpikes,
    DownAdvancedSpikes,
    LeftAdvancedSpikes,
    RightAdvancedSpikes
}

core.fieldCopy(fieldTable, spikes)

return spikes
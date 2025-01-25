local defaultFields = require("mods").requireFromPlugin("consts.default_fields")
local core = require("mods").requireFromPlugin("utils.core")
local string = require("mods").requireFromPlugin("utils.string")
local array = require("mods").requireFromPlugin("utils.array")
local drawableSprite = require("structs.drawable_sprite")
local depthOptions = require("mods").requireFromPlugin("consts.depthOptions")

local fieldTable = {
    depth = {
        data = -50,
        info = {
            options = depthOptions,
            editable = true,
            fieldType = "integer",
        }
    },
    spikeType =
    {
        data = "Dust",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "Dust",
                "Tentacles",
                "Custom"
            },
            editable = false
        }
    },
    spritingMode =
    {
        data = "MoveDisabledSprite",--onlyTentacle
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "MoveDisabledSprite",--onlyTentacle
                "ChangeSprite" --disabledAndTexture
            },
            editable = false
        }
    },
    disabledSprite =
    {
        data = "",
        info = require("mods").requireFromPlugin("libraries.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    disabledColor =
    {
        data = "ffffff",
        info =
        {
            fieldType = "color",
            allowEmpty = false
        }
    },
    disabledAlpha =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    disabledScaleX =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    disabledScaleY =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    disabledRotation =
    {
        data = 0,
        info =
        {
            fieldType = "number",
            allowEmpty = false
        }
    },
    disabledDisplacement =
    {
        data = - 4,
        info =
        {
            fieldType = "integer",
            allowEmpty = false
        }
    },
    disabledFrame =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    enabledSprite =
    {
        data = "",
        info = require("mods").requireFromPlugin("libraries.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    enabledColor =
    {
        data = "ffffff",
        info =
        {
            fieldType = "color",
            allowEmpty = false
        }
    },
    enabledAlpha =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    enabledScaleX =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    enabledScaleY =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    enabledRotationSpeed =
    {
        data = 0,
        info =
        {
            fieldType = "number",
            allowEmpty = false
        }
    },
    enabledFrame =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    enabledDisplacements =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    spriteSpacing =
    {
        data = 4,
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
        data = true,
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
        "spritingMode",--TentacleMode
        "disabledSprite",--TentacleSprite
        "disabledColor",--TentacleColor
        "disabledAlpha",--TentacleAlpha
        "disabledScaleX",--TentacleScaleX
        "disabledScaleY",--TentacleScaleY
        "disabledRotation",--TentacleRotation
        "disabledDisplacement",--TentacleOrigin
        "disabledFrame",
        "enabledSprite",
        "enabledColor",
        "enabledAlpha",
        "enabledScaleX",
        "enabledScaleY",
        "enabledRotationSpeed",
        "enabledFrame",
        "enabledDisplacements",
        "spriteSpacing",
        "animMoveMultiplier",
        "touchDelay",
        "touchSound",
        "flagNeedToTrigger",
        "triggerDelay",
        "triggerAnimSpeed",
        "triggerSound",
        "flagNeedToRetract",
        "retractDelay",
        "retractAnimSpeed",
        "retractSound",
        "trigger",
        "waitPlayerLeave",
        "attached",
        "grouped",
        "rainbow"
    }
end

local spikeTexture = function(entity, direction)
    if entity.spikeType == "Dust" then
        if direction == "up" or direction == "down" then
            return { "danger/triggertentacle/wiggle_v06", "danger/triggertentacle/wiggle_v03" }
        elseif direction == "left" or direction == "right" then
            return { "danger/triggertentacle/wiggle_h06", "danger/triggertentacle/wiggle_h03" }
        end
    end
    if entity.spikeType == "Tentacles" then
        return { "danger/tentacles00" }
    end
    return { entity.disabledSprite .. "_" .. direction .. "00" }
end

local spikeSize = function(entity, direction)
    if direction == "up" or direction == "down" then
        return entity.width
    elseif direction == "left" or direction == "right" then
        return entity.height
    end
    return 0
end

local spriteSpacing = function(entity)
    if entity.spikeType == "Dust" then
        return 4
    elseif entity.spikeType == "Tentacles" then
        return 16
    end
    return entity.spriteSpacing
end

local spriteOffset = function(entity, direction)
    if entity.spikeType == "Dust" then
        return(direction == "up" or direction == "down") and { 2, 4 } or { 4, 2 }
    elseif entity.spikeType == "Tentacles" then
        return { - 3 }
    end
    return { 1 }
end

local trigger = function(entity)
    if entity.spikeType == "Dust" then
        return true
    end
    return entity.trigger
end

local disabledDisplacement = function(entity, direction)
    if entity.spikeType == "Dust" then
        return(direction == "down" or direction == "right") and -5 or -2
    elseif entity.spikeType == "Tentacles" then
        return(direction == "down" or direction == "right") and 0 or -8
    end
    return entity.disabledDisplacement
end

local disabledRotation = function(entity, direction)
    if entity.spikeType ~= "Tentacles" then
        return entity.disabledRotation
    end
    if direction == "up" then
        return 0
    elseif direction == "down" then
        return 3.2
    elseif direction == "left" then
        return 4.8
    elseif direction == "right" then
        return 1.6
    end
end

local disabledColor = function(entity)
    if entity.spikeType == "Dust" then
        return "FF0000"
    end
    return entity.disabledColor
end

local spikeSprite = function(entity, direction)
    local spikeTexture = spikeTexture(entity, direction)
    local spikeSize = spikeSize(entity, direction)
    local spriteSpacing = spriteSpacing(entity)
    local spriteOffset = spriteOffset(entity, direction)
    local trigger = trigger(entity)
    local disabledDisplacement = disabledDisplacement(entity, direction)
    local disabledRotation = disabledRotation(entity, direction)
    local disabledColor = disabledColor(entity)
    local sprites = { }
    for index = 0, spikeSize / spriteSpacing - 1 do
        local sprite = drawableSprite.fromTexture(spikeTexture[index % #spikeTexture + 1], entity)
        local position = {
            x = 0,
            y = 0
        }
        if direction == "up" then
            position.x = entity.x + 5 +(spriteSpacing * index) - spriteOffset[index % #spriteOffset + 1]
            position.y = trigger and(entity.y - disabledDisplacement - 4) or(entity.y - disabledDisplacement - entity.animMoveMultiplier * 4 - 4)
        elseif direction == "down" then
            position.x = entity.x + 5 +(spriteSpacing * index) - spriteOffset[index % #spriteOffset + 1]
            position.y = trigger and(entity.y - disabledDisplacement - 4) or(entity.y - disabledDisplacement - entity.animMoveMultiplier * 4 + 4)
        elseif direction == "left" then
            position.x = trigger and(entity.x - disabledDisplacement - 4) or(entity.x - disabledDisplacement - entity.animMoveMultiplier * 4 - 4)
            position.y = entity.y + 5 +(spriteSpacing * index) - spriteOffset[index % #spriteOffset + 1]
        elseif direction == "right" then
            position.x = trigger and(entity.x - disabledDisplacement - 4) or(entity.x - disabledDisplacement - entity.animMoveMultiplier * 4 + 4)
            position.y = entity.y + 5 +(spriteSpacing * index) - spriteOffset[index % #spriteOffset + 1]
        end
        sprite:setPosition(position)
        sprite:setColor(disabledColor .. string.format("%x",(255 * entity.disabledAlpha)))
        sprite:setScale(entity.disabledScaleX, entity.disabledScaleY)
        sprite.rotation = disabledRotation
        table.insert(sprites, sprite)
    end
    return sprites
end

local spikesNames = {
    "ChroniaHelper/UpAnimatedSpikes",
    "ChroniaHelper/RightAnimatedSpikes",
    "ChroniaHelper/DownAnimatedSpikes",
    "ChroniaHelper/LeftAnimatedSpikes"
}


function ignoredAttr(entity)
    local attrs = {"_name", "_id"}

    local enabledAttr = {
        "enabledSprite",
        "enabledColor",
        "enabledAlpha",
        "enabledScaleX",
        "enabledScaleY",
        "enabledRotationSpeed",
        "enabledFrame",
        "enabledDisplacements",
    }

    local triggerAttr = {
        "animMoveMultiplier",
        "touchDelay",
        "touchSound",
        "flagNeedToTrigger",
        "triggerDelay",
        "triggerAnimSpeed",
        "triggerSound",
        "flagNeedToRetract",
        "retractDelay",
        "retractAnimSpeed",
        "retractSound",
        "waitPlayerLeave",
        "disabledDisplacement",
    }

    if entity.spritingMode == "MoveDisabledSprite" then
        for _, item in ipairs(enabledAttr) do
            table.insert(attrs, item)
        end
    end

    if not entity.trigger then
        for _, item in ipairs(triggerAttr) do
            table.insert(attrs, item)
        end
        for _, item in ipairs(enabledAttr) do
            table.insert(attrs, item)
        end
    end

    return attrs
end

local upSpikes = {
    name = "ChroniaHelper/UpAnimatedSpikes",
    placements =
    {
        name = "UpAnimatedSpikes",
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

local downSpikes = {
    name = "ChroniaHelper/DownAnimatedSpikes",
    placements =
    {
        name = "DownAnimatedSpikes",
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

local leftSpikes = {
    name = "ChroniaHelper/LeftAnimatedSpikes",
    placements =
    {
        name = "LeftAnimatedSpikes",
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

local rightSpikes = {
    name = "ChroniaHelper/RightAnimatedSpikes",
    placements =
    {
        name = "RightAnimatedSpikes",
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
    upSpikes,
    downSpikes,
    leftSpikes,
    rightSpikes
}

core.fieldCopy(fieldTable, spikes)

return spikes
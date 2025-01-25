local drawableSprite = require("structs.drawable_sprite")
local depthOptions = require("mods").requireFromPlugin("consts.depthOptions")

local spikeTypeOptions =
{
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

local fieldOrder = function(direction)
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
        "triggerDelay",
        "retractDelay",
        "trigger",
        "waitPlayerLeave",
        "attached",
        "grouped",
        "rainbow",
        "randomTexture"
    }
end

local spikesNames = {
    "ChroniaHelper/UpSpikes",
    "ChroniaHelper/RightSpikes",
    "ChroniaHelper/DownSpikes",
    "ChroniaHelper/LeftSpikes"
}

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
    return texture .. "_" .. direction .. "00"
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
    for index = 0, spikeSize / 8 - 1 do
        local sprite = drawableSprite.fromTexture(spikeTexture, entity)
        local position = {
            x = 0,
            y = 0
        }
        if direction == "up" then
            position.x = entity.x + 5 +(8 * index) -1
            position.y = entity.trigger and(entity.y) or(entity.y - 4)
        elseif direction == "down" then
            position.x = entity.x + 5 +(8 * index) -1
            position.y = entity.trigger and(entity.y) or(entity.y + 4)
        elseif direction == "left" then
            position.x = entity.trigger and(entity.x) or(entity.x - 4)
            position.y = entity.y + 5 +(8 * index) -1
        elseif direction == "right" then
            position.x = entity.trigger and(entity.x) or(entity.x + 4)
            position.y = entity.y + 5 +(8 * index) -1
        end
        sprite:setPosition(position)
        table.insert(sprites, sprite)
    end
    return sprites
end

local upSpikes = {
    name = "ChroniaHelper/UpSpikes",
    placements =
    {
        name = "UpSpikes",
        data =
        {
            width = 8,
            depth = -50,
            spikeType = "Default",
            sprite = "",
            triggerDelay = 0.4,
            retractDelay = - 1,
            trigger = false,
            waitPlayerLeave = false,
            attached = true,
            grouped = false,
            rainbow = false,
            randomTexture = true
        }
    },
    fieldInformation =
    {
        depth = {options = depthOptions, editable = true, fieldType = "integer",},
        spikeType = spikeTypeOptions,
        sprite = require("mods").requireFromPlugin("libraries.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    fieldOrder = fieldOrder("horizontal"),
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
    name = "ChroniaHelper/DownSpikes",
    placements =
    {
        name = "DownSpikes",
        data =
        {
            width = 8,
            depth = -50,
            spikeType = "Default",
            sprite = "",
            triggerDelay = 0.4,
            retractDelay = - 1,
            trigger = false,
            waitPlayerLeave = true,
            attached = true,
            grouped = false,
            rainbow = false,
            randomTexture = true
        }
    },
    fieldInformation =
    {
        depth = {options = depthOptions, editable = true, fieldType = "integer",},
        spikeType = spikeTypeOptions,
        sprite = require("mods").requireFromPlugin("libraries.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    fieldOrder = fieldOrder("horizontal"),
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
    name = "ChroniaHelper/LeftSpikes",
    placements =
    {
        name = "LeftSpikes",
        data =
        {
            height = 8,
            depth = -50,
            spikeType = "Default",
            sprite = "",
            triggerDelay = 0.4,
            retractDelay = - 1,
            trigger = false,
            waitPlayerLeave = true,
            attached = true,
            grouped = false,
            rainbow = false,
            randomTexture = true
        }
    },
    fieldInformation =
    {
        depth = {options = depthOptions, editable = true, fieldType = "integer",},
        spikeType = spikeTypeOptions,
        sprite = require("mods").requireFromPlugin("libraries.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    fieldOrder = fieldOrder("vertical"),
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
    name = "ChroniaHelper/RightSpikes",
    placements =
    {
        name = "RightSpikes",
        data =
        {
            height = 8,
            depth = -50,
            spikeType = "Default",
            sprite = "",
            triggerDelay = 0.4,
            retractDelay = - 1,
            trigger = false,
            waitPlayerLeave = true,
            attached = true,
            grouped = false,
            rainbow = false,
            randomTexture = true
        }
    },
    fieldInformation =
    {
        depth = {options = depthOptions, editable = true, fieldType = "integer",},
        spikeType = spikeTypeOptions,
        sprite = require("mods").requireFromPlugin("libraries.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    fieldOrder = fieldOrder("vertical"),
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

return {
    upSpikes,
    downSpikes,
    leftSpikes,
    rightSpikes
}
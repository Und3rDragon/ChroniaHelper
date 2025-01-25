local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local touchSwitch = {}

touchSwitch.associatedMods = { "MaxHelpingHand", "ChroniaHelper" }

touchSwitch.name = "ChroniaHelper/FlagTouchSwitch"
touchSwitch.depth = 2000
touchSwitch.placements = {
    {
        name = "touch_switch",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            flag = "flag_touch_switch",
            icon = "vanilla",
            borderTexture = "",
            persistent = false,
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            smoke = true,
            inverted = false,
            allowDisable = false,
            playerCanActivate = true,
            hitSound = "event:/game/general/touchswitch_any",
            completeSoundFromSwitch = "event:/game/general/touchswitch_last_cutoff",
            completeSoundFromScene = "event:/game/general/touchswitch_last_oneshot",
            hideIfFlag = "",
            switch = "touchSwitchWall",
            idleAnimDelay = 0.1,
            spinAnimDelay = 0.1,
        }
    }
}

touchSwitch.fieldOrder = {"x", "y", "inactiveColor", "activeColor", "finishColor", "hitSound", "completeSoundFromSwitch", "completeSoundFromScene"}

touchSwitch.fieldInformation = {
    inactiveColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    },
    icon = {
        options = { "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple" }
    },
    switch = {
        options = {"touchSwitch", "touchSwitchWall"}, editable = false
    },
    borderTexture = {
        options = {"", "particles/ChroniaHelper/none"}, editable = true,
    }
}

local containerTexture = "objects/touchswitch/container"

local iconOptions = { "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple" }

function isVanilla(table, value)
    for _, v in pairs(table) do
        if v == value then
            return true
        end
    end
    return false
end

function touchSwitch.sprite(room, entity)
    local borderTexture = entity.borderTexture ~= "" and entity.borderTexture or containerTexture
    local containerSprite = drawableSprite.fromTexture(borderTexture, entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
    
    local iconResource -- = "objects/touchswitch/icon00"

    if entity.icon == "vanilla" then
        iconResource = "objects/touchswitch/icon00"
    elseif isVanilla(iconOptions, entity.icon) then
        iconResource = "objects/ChroniaHelper/flagTouchSwitch/" .. entity.icon .."/icon00"
    else
        iconResource = entity.icon .. "00"
    end

    local iconSprite = drawableSprite.fromTexture(iconResource, entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)

    if entity.switch == "touchSwitch" then
        return {containerSprite, iconSprite}
    else
        return {
            require('structs.drawable_rectangle').fromRectangle('bordered',entity.x,entity.y, entity.width, entity.height, {0.0, 0.0, 0.0, 0.3}, {1.0,1.0,1.0,0.5}),
            drawableSprite.fromTexture(iconResource, entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)}
    end
    
end




return touchSwitch

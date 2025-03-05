local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local touchSwitch = {}

touchSwitch.name = "ChroniaHelper/RealFlagSwitchAlt"
touchSwitch.depth = 2000
touchSwitch.placements = {
    {
        name = "flagswitch",
        data = {
            x = 0,
            y = 0,
            flag = "flag",
            icon = "vanilla",
            borderTexture = "",
            persistent = false,
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            smoke = true,
            --inverted = false,
            allowDisable = false,
            playerCanActivate = true,
            hitSound = "event:/game/general/touchswitch_any",
            --completeSoundFromSwitch = "event:/game/general/touchswitch_last_cutoff",
            completeSoundFromScene = "event:/game/general/touchswitch_last_oneshot",
            --hideIfFlag = "",
            switch = "touchSwitch",
            idleAnimDelay = 0.1,
            spinAnimDelay = 0.1,
            activatedAnimRate = 4.0,
            finishedAnimRate = 0.1,
            passwordID = "",
            password = "",
        }
    }
}

touchSwitch.fieldOrder = {"x", "y",
"flag", "icon",
"borderTexture","inactiveColor", 
"activeColor", "finishColor", 
"hitSound", "completeSoundFromSwitch", "completeSoundFromScene",
"switch","idleAnimDelay",
"spinAnimDelay",
"activatedAnimRate", "finishedAnimRate",
"passwordID", "password",
}

touchSwitch.fieldInformation = {
    inactiveColor = {
        fieldType = "color", allowEmpty = false,
    },
    activeColor = {
        fieldType = "color", allowEmpty = false,
    },
    finishColor = {
        fieldType = "color", allowEmpty = false,
    },
    icon = {
        options = { "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple" }
    },
    switch = {
        options = {"touchSwitch"}, editable = false
    },
    borderTexture = {
        options = {"", "particles/ChroniaHelper/none"}, editable = true,
    },
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
    local containerSprite = drawableSprite.fromTexture(borderTexture, entity)
    
    local iconResource -- = "objects/touchswitch/icon00"

    if entity.icon == "vanilla" then
        iconResource = "objects/touchswitch/icon00"
    elseif isVanilla(iconOptions, entity.icon) then
        iconResource = "objects/ChroniaHelper/flagTouchSwitch/" .. entity.icon .."/icon00"
    else
        iconResource = entity.icon .. "00"
    end

    local iconSprite = drawableSprite.fromTexture(iconResource, entity)

    return {containerSprite, iconSprite}
    
end




return touchSwitch

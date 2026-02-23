local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local touchSwitch = {}

touchSwitch.name = "ChroniaHelper/RealFlagSwitch"
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
            allowDisable = false,
            playerCanActivate = true,
            hitSound = "event:/game/general/touchswitch_any",
            completeSoundFromScene = "event:/game/general/touchswitch_last_oneshot",
            switch = "touchSwitch",
            idleAnimDelay = 0.1,
            spinAnimDelay = 0.1,
            activatedAnimRate = 4.0,
            finishedAnimRate = 0.1,
            passwordID = "",
            password = "",
            resetMode = 0,
        }
    },
    {
        name = "flagswitchwall",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
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
            switch = "touchSwitchWall",
            idleAnimDelay = 0.1,
            spinAnimDelay = 0.1,
            activatedAnimRate = 4.0,
            finishedAnimRate = 0.1,
            passwordID = "",
            password = "",
            resetMode = 0,
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
        options = {"touchSwitch", "touchSwitchWall"}, editable = false
    },
    borderTexture = {
        options = {"", "particles/ChroniaHelper/none"}, editable = true,
    },
    resetMode = {
        options = {
            ["Default"] = 0,
            ["Reset On Room Switch"] = 1,
            ["Reset On Death"] = 2,
        },
        editable = false,
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
    local containerSprite = drawableSprite.fromTexture(borderTexture, entity)--:setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
    
    local iconResource -- = "objects/touchswitch/icon00"

    if entity.icon == "vanilla" then
        iconResource = "objects/touchswitch/icon00"
    elseif isVanilla(iconOptions, entity.icon) then
        iconResource = "objects/ChroniaHelper/flagTouchSwitch/" .. entity.icon .."/icon00"
    else
        iconResource = entity.icon .. "00"
    end

    local iconSprite = drawableSprite.fromTexture(iconResource, entity)--:setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)

    if entity.switch == "touchSwitch" then
        return {containerSprite, iconSprite}
    else
        containerSprite:setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
        iconSprite:setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
        return {
            require('structs.drawable_rectangle').fromRectangle('bordered',entity.x,entity.y, entity.width, entity.height, {0.0, 0.0, 0.0, 0.3}, {1.0,1.0,1.0,0.5}),
            drawableSprite.fromTexture(iconResource, entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)}
    end
    
end




return touchSwitch

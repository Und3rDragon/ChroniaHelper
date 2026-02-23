local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local vivUtils = require("mods").requireFromPlugin("helpers.vivUtilsMig")

local touchSwitch = {}

touchSwitch.name = "ChroniaHelper/RealFlagSwitch2"
touchSwitch.depth = 2000
touchSwitch.placements = {
    {
        name = "flagswitch",
        data = {
            x = 0,
            y = 0,
            switch = "touchSwitch",
            flag = "flag",
            icon = "objects/ChroniaHelper/flagTouchSwitchNew",
            iconIdleAnimation = 0.1,
            iconSpinAnimation = 0.02,
            iconFinishingAnimation = 0.1,
            iconFinishedAnimation = 0.1,
            borderTexture = "objects/ChroniaHelper/flagTouchSwitchNew/container",
            borderAnimation = 0.1,
            persistent = false,
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            smoke = true,
            allowDisable = false,
            playerCanActivate = true,
            hitSound = "event:/game/general/touchswitch_any",
            completeSoundFromScene = "event:/game/general/touchswitch_last_oneshot",
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
            switch = "touchSwitchWall",
            flag = "flag",
            icon = "objects/ChroniaHelper/flagTouchSwitchNew",
            borderTexture = "objects/ChroniaHelper/flagTouchSwitchNew/container",
            persistent = false,
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            smoke = true,
            allowDisable = false,
            playerCanActivate = true,
            hitSound = "event:/game/general/touchswitch_any",
            completeSoundFromScene = "event:/game/general/touchswitch_last_oneshot",
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
    local containerSprite = vivUtils.getImageWithNumbers(entity.borderTexture, 0, entity)
    
    local iconSprite = vivUtils.getImageWithNumbers(entity.icon .. "/idle", 0, entity)

    if entity.switch == "touchSwitch" then
        return {containerSprite, iconSprite}
    else
        containerSprite:setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
        iconSprite:setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
        return {
            require('structs.drawable_rectangle').fromRectangle('bordered',entity.x,entity.y, entity.width, entity.height, {0.0, 0.0, 0.0, 0.3}, {1.0,1.0,1.0,0.5}),
            iconSprite:setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)}
    end
    
end




return touchSwitch

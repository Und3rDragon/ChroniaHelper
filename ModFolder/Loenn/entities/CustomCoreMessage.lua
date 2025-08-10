local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local ccm = {name = "ChroniaHelper/CustomCoreMessage"}

ccm.texture = "@Internal@/core_message"
ccm.depth = -10000000

ccm.associatedMods = function(entity)
    if entity.detectSessionExpression then
        return {"ChroniaHelper", "FrostHelper"}
    end

    return {"ChroniaHelper"}
end
ccm.placements = {
    name = "CustomCoreMessage",
    data = {
        line = "0",
        dialog = "app_ending",
        OutlineColor="000000",
        align = 5,
        Scale=1.25,
        RenderDistance=128.0,
        AlwaysRender=false,
        LockPosition=false,
        DefaultFadedValue=0.0,
        AlphaMultiplier = 1,
        PauseType="Hidden",
        TextColor1="ffffff",
        EaseType="CubeInOut",
        wholeDialog = false,
        parallax = 1.2,
        screenPosX = 160,
        screenPosY = 90,
        detectSessionExpression = false,
        vanillaRenderDistanceBehaviour = true,
    }, 
    nodeLimits = {0,2}
}

ccm.fieldInformation = {
    align = {
        options = {
            ["center"] = 5,
            ["top left"] = 1,
            ["top center"] = 2,
            ["top right"] = 3,
            ["center left"] = 4,
            ["center right"] = 6,
            ["bottom left"] = 7,
            ["bottom center"] = 8,
            ["bottom right"] = 9,
        },
        editable = false,
    },
    dialog = {
        options = {
            "ChroniaHelperTimer",
            "ChroniaHelperTimerStatic",
            "ChroniaHelperFrames",
            "ChroniaHelperFramesStatic",
            ["keyboardSync_(tagID)"] = "keyboardSync_passwordKeyboard",
        },
        editable = true,
    },
    OutlineColor = {fieldType = "color", allowXNAColors=true, allowEmpty = true},
    TextColor1 = {fieldType = "color", allowXNAColors=true, useAlpha = true},
    Scale = {fieldType = "number", minimumValue = 0.125},
    EaseType = {
        options = {
            "Linear",
            "SineIn",
            "SineOut",
            "SineInOut",
            "QuadIn",
            "QuadOut",
            "QuadInOut",
            "CubeIn",
            "CubeOut",
            "CubeInOut",
            "QuintIn",
            "QuintOut",
            "QuintInOut",
            "ExpoIn",
            "ExpoOut",
            "ExpoInOut",
            "BackIn",
            "BackOut",
            "BackInOut",
            "BigBackIn",
            "BigBackOut",
            "BigBackInOut",
            "ElasticIn",
            "ElasticOut",
            "ElasticInOut",
            "BounceIn",
            "BounceOut",
            "BounceInOut"
        },
        editable = false,
    },
    PauseType = {fieldType = "string", options = {"Hidden","Shown","Fade"}, editable = false},
    AlphaMultiplier = {
        maximumValue = 1, minimumValue = 0,
    },
}

ccm.selection = function (room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 32, 32)
end

ccm.sprite = function(room,entity)
    return vivUtilsMig.getImageWithNumbers("ChroniaHelper/LoennIcons/customCoreMessage/align", entity.align, entity)
end

return ccm
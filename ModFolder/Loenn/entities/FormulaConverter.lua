local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/FormulaConverter"
controller.placements = {
    name = "controller",
    data = {
        chroniaFlagLogicExpression = "See tooltip",
        chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",

        parameters = "",
        mode = 7,

        targetName = "formulaCounter",
        timerSlider = "timer",
        isCounter = true,
        formulaTimeFields = "1,3,5",
        formulaExpressions = "timer|2*timer-1|4*timer-7|8*timer-27",
        resetTimerWhenActivated = true,
    },
}

controller.fieldOrder = {
    "_x", "_y", "x", "y", "_id", "_name",
    "chroniaMathExpession", "frostSessionExpression",
}

controller.fieldInformation = {
    chroniaFlagLogicExpression = {
        editable = false,
    },
    chroniaMathExpession = {
        editable = false,
    },
    mode = require("mods").requireFromPlugin("consts.field_options").generalSetup,
    formulaTimeFields = {
        fieldType = "list",
    },
    formulaExpressions = {
        fieldType = "list",
        elementSeparator = "|",
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Function", entity)

    table.insert(sprite, iconSprite)
    
    --local text = require("structs.drawable_text").fromText(entity.flags, entity.x + 12, entity.y - 12, 48, 24)
    
    --table.insert(sprite, text)
    
    return sprite
end

return controller
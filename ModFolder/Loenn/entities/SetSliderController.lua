local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/SetSliderController"
controller.placements = {
    name = "controller",
    data = {
        chroniaFlagLogicExpression = "See tooltip",
        chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",
        sliders = "slider",
        value = "0",
        parameters = "",
        mode = 0,
        value2 = "",
        randomizeValue = false,
        valueType = 0,
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
    flags = {
        fieldType = "list",
    },
    parameters = {
        fieldType = "list",
    },
    valueType = {
        fieldType = "integer",
        options = {
            ["Set"] = 0,
            ["Add"] = 1,
            ["Minus"] = 2,
            ["Multiply"] = 3,
            ["Divide"] = 4,
        },
        editable = false,
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Counter", entity)

    table.insert(sprite, iconSprite)
    
    local _text = entity.sliders .. "\n=\n"
    
    if entity.value2 ~= nil then
        if entity.value2 ~= "" then
            _text = _text .. "(" .. entity.value .. ", " .. entity.value2 .. ")"
        else
            _text = _text .. entity.value
        end
    else
        _text = _text .. entity.value
    end
    
    local text = require("structs.drawable_text").fromText(_text, entity.x + 12, entity.y - 12, 48, 24)
    
    table.insert(sprite, text)
    return sprite
end

return controller
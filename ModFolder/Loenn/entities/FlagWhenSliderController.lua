local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/FlagWhenSliderController"
controller.placements = {
    name = "Controller",
    data = {
        flag = "flag",
        slider = "slider",
        values = "0,0.5-1.2",
        inverted = false,
        globalEntity = false,
    },
}

controller.fieldInformation = {
    values = {
        fieldType = "list",
    }
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/SliderToFlag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
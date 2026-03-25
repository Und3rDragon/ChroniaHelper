local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/TimedRandomSliderController"
controller.placements = {
    name = "Controller",
    data = {
        slider = "slider",
        value1 = 0,
        value2 = 1,
        interval = 1,
        startDelay = -1,
        mode = 0,
        globalEntity = false,
    },
}

controller.fieldInformation = {
    mode = {
        fieldType = "integer",
        options = {
            ["On Added"] = 0,
        },
        editable = false,
    }
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/RandomSlider", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/FlagWhenCounterController"
controller.placements = {
    name = "Controller",
    data = {
        flag = "flag",
        counter = "counter",
        values = "1,2-3,4-6",
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
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/CounterToFlag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/TimedRandomCounterController"
controller.placements = {
    name = "Controller",
    data = {
        counter = "counter",
        value1 = 0,
        value2 = 1,
        interval = 1,
        startDelay = -1,
        mode = 0,
        global = false,
    },
}

controller.fieldInformation = {
    mode = {
        fieldType = "integer",
        options = {
            ["On Added"] = 0,
        },
        editable = false,
    },
    value1 = {
        fieldType = "integer",
    },
    value2 = {
        fieldType = "integer",
    }
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/RandomCounter", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
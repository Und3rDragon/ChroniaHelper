local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/RoomTagSessionController"
controller.placements = {
    name = "Controller",
    data = {
        createFlag  = true,
        setCounter = "",
        setSlider = "",
    },
}

controller.fieldInformation = {
    setCounter = {
        options = {
            ["default value"] = 0,
        },
    },
    setSlider = {
        options = {
            ["default value"] = 0,
        },
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/RoomTag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
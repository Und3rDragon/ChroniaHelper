local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/EntityTextbox"
controller.placements = {
    name = "box",
    data = {
        maxWidth = 1688,
        maxHeight = 272,
        justifyX = 0.5,
        justifyY = 0.5,
        dialog = "dialogID",
        operationFlag = "triggerDialog",
    },
}

controller.fieldInformation = {
    justifyX = {
        minimumValue = 0,
        maximumValue = 1,
    },
    justifyY = {
        minimumValue = 0,
        maximumValue = 1,
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/EntityTextBox", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
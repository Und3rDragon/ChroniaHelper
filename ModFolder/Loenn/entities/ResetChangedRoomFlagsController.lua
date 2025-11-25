local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/ResetChangedRoomFlagsController"
controller.placements = {
    name = "Controller",
    data = {
        inspectMode = 0,
        resetMethod = 0,
    },
}

controller.fieldInformation = {
    inspectMode = {
        options = {
            ["Any Changes"] = 0,
            ["Activated Only"] = 1,
            ["Deactivated Only"] = 2,
        },
        editable = false,
    },
    resetMethod = {
        options = {
            ["Deactivate"] = 0,
            ["Activate"] = 1,
            ["Invert State"] = 2,
        },
        editable = false,
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Flag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
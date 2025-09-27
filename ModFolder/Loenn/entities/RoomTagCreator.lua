local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depthOptions")

local controller = {}

controller.name = "ChroniaHelper/RoomTagCreator"
controller.placements = {
    name = "Creator",
    data = {
        setRoomtag = "&",
        forceLoad = false,
    },
}

controller.fieldInformation = {
    
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/RoomTag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
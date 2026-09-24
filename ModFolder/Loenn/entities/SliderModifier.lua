local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local modifier = {}

modifier.name = "ChroniaHelper/SliderModifier"
modifier.placements = {
    name = "modifier",
    data = {
        titleDialog = "",
        targetName = "targetSlider",
        offsetY = -24.0,
        step = "0.01",
        depth = -100,
    },
}

modifier.fieldOrder = {
    "_x", "_y", "x", "y", "_id", "_name",
    "targetName", "offsetY", "step",
}

modifier.fieldInformation = {
    depth = require("mods").requireFromPlugin("helpers.field_options").depths
}

modifier.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/CounterModifier", entity)

    table.insert(sprite, iconSprite)

    local _text = entity.targetName .. "\n±" .. tostring(entity.step)

    local text = require("structs.drawable_text").fromText(_text, entity.x + 12, entity.y - 12, 48, 24)

    table.insert(sprite, text)

    return sprite
end

modifier.selection = function(room, entity)
	return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return modifier

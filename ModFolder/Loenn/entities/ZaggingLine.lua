local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local zl = {}

zl.depth = function(room,entity) return entity.depth or 9500 end
zl.nodeLimits = {1, -1}
zl.nodeVisibility = "always"
zl.name = "ChroniaHelper/ZaggingLine"

zl.placements = {
	name = "ZaggingLine",
	data = {
		depth = 9500,
		bgLineColor = "ffffffff",
		nodeSprite = "ChroniaHelper/ZaggingLine/node",
		nodeColors = "ffffffff",
		lineColors = "ffffffff",
		intervals = "1",
		durations = "3",
		ease = "sinein",
		showBgLine = false,
		showNodes = true,
	}
}

zl.fieldInformation = {
	bgLineColor = {
		fieldType = "color",
		useAlpha = true,
	},
	nodeColors = {
		fieldType = "list",
		elementOptions = {
			fieldType = "color",
			useAlpha = true,
		},
	},
	lineColors = {
		fieldType = "list",
		elementOptions = {
			fieldType = "color",
			useAlpha = true,
		},
	},
}

zl.sprite = function(room, entity)
	local sprites = {}

	local baseSprite = vivUtilsMig.getImageWithNumbers(entity.nodeSprite, 0, entity)
	table.insert(sprites, baseSprite)
	
	return sprites
end

zl.nodeSprite = function(room, entity, node, nodeIndex, viewport)
	local sprites = {}

	local center = drawableRectangle.fromRectangle("bordered", node.x - 4, node.y - 4, 8, 8, {1, 1, 1, 0}, {1.0, 1.0, 1.0, 0.5})
	table.insert(sprites, center)

	return sprites
end

--return zl
local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local frostUtils = require("mods").requireFromPlugin("libraries.frostUtils")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")

local sr = {}

sr.name = "ChroniaHelper/SpeedRedirect"
sr.nodeLimits = {1,1}
sr.nodeVisibility = "always"

sr.placements = {
	name = "SpeedRedirect",
	data = {
		width = 16,
		height = 16,
		speedMultiplier = 1,
		onlyOnce = false,
	}
}

sr.sprites = function(room, entity)
	local sprites = {}

	local rectangle = drawableRectangle.fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,{255,255,255,0.4},{255,255,255,1})

	table.insert(sprites, rectangle)

	return sprites
end

return sr
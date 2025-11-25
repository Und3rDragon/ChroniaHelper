local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local lantern = {}

lantern.name = "ChroniaHelper/CustomResortLantern"
lantern.depth = function(room,entity) return entity.depth or 2000 end

lantern.placements = {
	name = "lantern",
	data = {
		x = 0,
		y = 0,
		depth = 2000,
		spriteDirectory = "objects/resortLantern/",
		frames = "0,0,1,2,1",
		animationInterval = 0.3,
		wiggleDuration = 2.5,
		wiggleFrequency = 1.2,
		wiggleMaxAmplitute = 30,
		lightColor = "ffffff",
		lightAlpha = 0.95,
		lightStartFade = 32,
		lightEndFade = 64,
		bloomAlpha = 0.8,
		bloomRadius = 8.0,
		flashFreqMultiplier = 1,
		flashAlpha = 0.05,
	}
}

lantern.fieldInformation = {
	depth = {
		options = depthOptions,
		editable = true,
		fieldType = "integer",
	},
	frames = {
		fieldType = "list",
		elementOptions = {
			fieldType = "integer",
			allowEmpty = false,
		},
		allowEmpty = false,
	},
	lightColor = {
		fieldType = "color",
	},
	lightStartFade ={
		fieldType = "integer",
	},
	lightEndFade = {
		fieldType = "integer",
	}
}

lantern.sprite = function(room, entity)
	local sprites = {}

	local lanternSprite = drawableSprite.fromTexture(entity.spriteDirectory .. "lantern00", entity)

	sprites = lanternSprite

	return sprites
end

return lantern
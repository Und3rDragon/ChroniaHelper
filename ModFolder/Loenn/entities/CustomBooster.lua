local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local frostUtils = require("mods").requireFromPlugin("libraries.frostUtils")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local booster = {}

booster.name="ChroniaHelper/CustomBooster"

booster.depth = function(room,entity) return entity.depth or -8500 end
booster.placements={
	name="normal",
	data={
		depth = -8500,
		directory = "objects/ChroniaHelper/customBoosterPresets/grey",
		outlineDirectory = "objects/ChroniaHelper/customBoosterPresets/grey/outline",
		colorOverlay = "ffffff",
		ch9_hub_booster = false,
		appearAnimInterval = 0.8,
		loopAnimInterval = 0.1,
		insideAnimInterval = 0.1,
		spinAnimInterval = 0.06,
		popAnimInterval = 0.08,
		respawnTime = 1.0,
		dashes = 1,
		stamina = 110,
		--moveSpeedMultiplier = 1,
		outSpeedMultiplier = 1,
		hitboxRadius = 10,
		hitboxX = 0,
		hitboxY = 2,
		red = false,
		setOrRefillDashes = false,
		setOrRefillStamina = false,
		--setOutSpeed = false,
		XMLOverride = false,
	}
}

booster.fieldInformation = {
	dashes = {
		fieldType = "integer",
	},
	stamina = {
		fieldType = "integer",
	},
	depth = require("mods").requireFromPlugin("helpers.field_options").depths,
	directory = {
		options = {
			"objects/ChroniaHelper/customBoosterPresets/green",
			"objects/ChroniaHelper/customBoosterPresets/red",
			"objects/ChroniaHelper/customBoosterPresets/pink",
			"objects/ChroniaHelper/customBoosterPresets/grey",
			"Preset_red",
			"Preset_green",
			"Preset_pink",
			"Default_booster",
		},
		editable = true,
	},
	colorOverlay = {
		fieldType = "color",
	}
}

booster.sprite = function(room,entity)

	-- parsing directory
	local dir = "objects/ChroniaHelper/customBoosterPresets/grey"
	if entity.directory == "Preset_red" then
		dir = "objects/ChroniaHelper/customBoosterPresets/red"
	elseif entity.directory == "Preset_green" then
		dir = "objects/ChroniaHelper/customBoosterPresets/green"
	elseif entity.directory == "Default_booster" then
		dir = "objects/ChroniaHelper/customBoosterPresets/grey"
	end

    local sprite = vivUtilsMig.getImageWithNumbers(entity.directory .. "/loop", 0, entity)

	if entity.XMLOverride then
		sprite = vivUtilsMig.getImageWithNumbers(dir .. "/loop", 0, entity)
	end
	sprite:setColor(frostUtils.getColor(entity.colorOverlay))
	return sprite
end

booster.selection = function(room, entity)
	return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return booster
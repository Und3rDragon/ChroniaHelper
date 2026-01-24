local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local frostUtils = require("mods").requireFromPlugin("helpers.frostUtils")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local booster = {}

booster.name="ChroniaHelper/CustomBoosterXML"

booster.depth = function(room,entity) return entity.depth or -8500 end
booster.placements={
	name = "normal",
	data = {
		depth = -8500,
		directory = "Default_booster",
		outlineDirectory = "objects/ChroniaHelper/customBoosterPresets/grey/outline",
		colorOverlay = "ffffff",
		ch9_hub_booster = false,
		redBoostMovingSpeed = 240,
		--appearAnimInterval = 0.8,
		--loopAnimInterval = 0.1,
		--insideAnimInterval = 0.1,
		--spinAnimInterval = 0.06,
		--popAnimInterval = 0.08,
		respawnTime = 1.0,
		dashes = 1,
		stamina = 110,
		holdTime = 0.25,
		outSpeedMultiplier = 1,
		hitboxRadius = 10,
		hitboxX = 0,
		hitboxY = 2,
		red = false,
		setOrRefillDashes = false,
		setOrRefillStamina = false,
		XMLOverride = true,
		burstParticleColorOverride = false,
		burstParticleColor = "ffffff",
		appearParticleColorOverride = false,
		appearParticleColor = "ffffff",
		disableFastBubble = false,
		allowDashOutWhenBoosting = true,
		onlyOnce = false,
        playerFollow = false,
        keepPlayerSpeed = false,
	}
}

booster.ignoredFields = {
	"_x", "_y", "x", "y", "_id", "_name",
	"XMLOverride",
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
			"Preset_red",
			"Preset_green",
			"Preset_pink",
            "Preset_yellow",
			"Default_booster",
		},
		editable = true,
	},
	colorOverlay = {
		fieldType = "color",
	},
	burstParticleColor = {
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
	elseif entity.directory == "Preset_pink" then
		dir = "objects/ChroniaHelper/customBoosterPresets/pink"
	elseif entity.directory == "Preset_yellow" then
		dir = "objects/ChroniaHelper/customBoosterPresets/yellow"
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
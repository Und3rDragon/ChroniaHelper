local fakeTilesHelper = require("helpers.fake_tiles")

return {
	name = "ChroniaHelper/CustomFallingBlock",
	placements = {
		name = "default",
		data = {
			tiletype = "3",
			accel = 500.0,
			maxSpeed = 160.0,
			shakeSfx = "event:/game/general/fallblock_shake",
			impactSfx = "event:/game/general/fallblock_impact",
			direction = 0,
			climbFall = true,
			bufferClimbFall = false,
			legacy = true,
			behind = false,
			crushBlockTrigger = false,
			flagTrigger = "",
			flagOnFall = "",
			flagOnGround = "",
			flagOnly = false,
			breakDashBlocks = true,
			shakeForAllFalls = false,
			fallDelay = 0.0,
			finalBoss = false,
			blendIn = false,
			bgTexture = false,
			depth = -12999,
			width = 8,
			height = 8,
			-- RoomWrapMaxRev = 0,
		}
	},
	fieldOrder = {
		
	},
	fieldInformation = function(entity)
		local orig = {}
		if entity.bgTexture then
			orig = fakeTilesHelper.getFieldInformation("tiletype", "tilesBg")(entity)
		else
			orig = fakeTilesHelper.getFieldInformation("tiletype", "tilesFg")(entity)
		end

		orig["direction"] = {
			fieldType = "integer",
			options = {
				["Down"] = 0,
				["Left"] = 1,
				["Up"] = 2,
				["Right"] = 3,
			},
			editable = false,
		}

		orig["depth"] = {
			fieldType = "integer",
			options = require("mods").requireFromPlugin("consts.depths"),
			editable = true,
		}

		return orig
	end,
	sprite = function(room, entity)
		local sprites = {}
		if entity.bgTexture then
			sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg")(room, entity)
		else
			sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg")(room, entity)
		end

		return sprites
	end,
	depth = function(room, entity) return entity.depth or -12999 end,
	associatedMods = { "ChroniaHelper", "VivHelper" },
}

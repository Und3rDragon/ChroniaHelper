local fakeTilesHelper = require("helpers.fake_tiles")

return {
	name = "ChroniaHelper/CrumbleBlockOnJump",
	placements = {
		name = "default",
		data = {
			tiletype = "3",
			delay = 0.2,
			breakSound = "event:/new_content/game/10_farewell/quake_rockbreak",
			triggerOnLean = true,
			permanent = false,
			blendIn = false,
			destroyStaticMovers = false,
			createDebris = true,
			width = 8,
			height = 8,
			depth = -12999,

			bgTexture = false,
			triggerFromBottom = false,
			autoGroupping = true,
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

		orig["depth"] = {
			fieldType = "integer",
			options = require("mods").requireFromPlugin("consts.depths"),
			editable = true,
		}

		orig["surfaceSoundIndex"] = require("mods").requireFromPlugin("consts.sounds").soundIDs

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
	depth = function(room,entity) return entity.depth or -9000 end,
	associatedMods = {"ChroniaHelper", "ProgHelper"},
}
local fakeTilesHelper = require("helpers.fake_tiles")

return {
	name = "ChroniaHelper/CustomPushBlock",
	placements = {
		name = "default",
		data = {
			tiletype = "3",
			stickyTop = false,
			stickyBottom = false,
			stickyLeft = false,
			stickyRight = false,
			debrisTileset = "5",
			bgTexture = false,
			depth = -12999,
			width = 32,
			height = 32,
			pushSpeed = 320,
		},
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

		return orig
	end,
	sprite = function(room, entity)
		if entity.bgTexture then
			return fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg")(room, entity)
		end

		return fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg")(room, entity)
	end,
	depth = function(room, entity) return entity.depth or -12999 end,
	associatedMods = { "ChroniaHelper", "CanyonHelper" },
}

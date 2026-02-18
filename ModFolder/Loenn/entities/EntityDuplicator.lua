

local e = {}

e.name = "ChroniaHelper/EntityDuplicator"

e.placements = {
	name = "duplicator",
	data = {
		width = 16,
		height = 16,
		generateFlag = "generate",
		duplicatorTag = "duplicator",
	},
}

e.sprite = function(room, entity, viewport)
    return {
        require("structs.drawable_rectangle").fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height,
            {1,1,1,0.3}, {1,1,1,1}),
    }
end

return e
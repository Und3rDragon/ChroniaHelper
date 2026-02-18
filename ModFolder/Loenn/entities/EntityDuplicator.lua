

local e = {}

e.name = "ChroniaHelper/EntityDuplicator"

e.placements = {
	name = "Entity Duplicator",
	data = {
		width = 16,
		height = 16,
	},
}

e.sprite = function(room, entity, viewport)
    return {
        require("structs.drawable_rectangle").fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height,
            {1,1,1,0.3}, {1,1,1,1}),
    }
end

return e
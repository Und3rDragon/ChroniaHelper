

local e = {}

e.name = "ChroniaHelper/EntityDuplicator"

e.associatedMods = function(entity)
    if entity.usingExpression == 2 then
        return {"FrostHelper", "ChroniaHelper"}
    end
    
    return {"ChroniaHelper"}
end

e.placements = {
	name = "duplicator",
	data = {
		width = 16,
		height = 16,
		chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",
		usingExpression = 0,
		generateCondition = "generate",
		duplicatorTag = "duplicator",
	},
}

e.fieldInformation = {
    chroniaMathExpession = {
        editable = false,
    },
	usingExpression = {
        fieldType = "integer",
        options = {
            ["Flags"] = 0, ["ChroniaMathExpression"] = 1, ["FrostSessionExpression"] = 2
        },
        editable = false,
    },
}

e.sprite = function(room, entity, viewport)
    return {
        require("structs.drawable_rectangle").fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height,
            {1,1,1,0.3}, {1,1,1,1}),
    }
end

--return e
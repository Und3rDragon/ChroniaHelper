local fakeTilesHelper = require('helpers.fake_tiles')

local floatyBgTile = {}

floatyBgTile.name = "ChroniaHelper/SeperatedTile"
floatyBgTile.depth = function(room,entity) return entity.Depth or 10000 end
floatyBgTile.placements = {
    name = "SeperatedTile",
    data = {
        tiletype = "3",
        Depth = 10000,
        width = 8,
        height = 8,
        fgTexture = true,
    }
}
--[[
floatyBgTile.fieldInformation = {
    Depth = {
        fieldType = "integer",
    },
}
]]

local defColor = {1.0, 1.0, 1.0, 0.7}

floatyBgTile.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg", defColor)

floatyBgTile.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype", true, "tilesBg")(entity)
    if entity.fgTexture then
        orig = fakeTilesHelper.getFieldInformation("tiletype", false, "tilesFg")(entity)
    end

    orig["Depth"] = {fieldType = "integer"}
    return orig
end

return floatyBgTile
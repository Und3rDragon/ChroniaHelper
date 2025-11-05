local fakeTilesHelper = require('helpers.fake_tiles')

local StaticBgTile = {}

StaticBgTile.name = "ChroniaHelper/StaticBgTile"
StaticBgTile.depth = function(room,entity) return entity.Depth or 10000 end
StaticBgTile.placements = {
    name = "tile",
    data = {
        tiletype = "3",
        Depth = 10000,
        width = 8,
        height = 8,
    }
}
--[[
StaticBgTile.fieldInformation = {
    Depth = {
        fieldType = "integer",
    },
}
]]
StaticBgTile.depth = function(room,entity) return entity.Depth or 10000 end

StaticBgTile.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg", {1.0, 1.0, 1.0, 0.7})

StaticBgTile.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype", true, "tilesBg")(entity)
    orig["Depth"] = {fieldType = "integer"}
    return orig
end

return StaticBgTile
local fakeTilesHelper = require('helpers.fake_tiles')

local floatyBgTile = {}

floatyBgTile.name = "ChroniaHelper/FloatyBgTile"
floatyBgTile.depth = function(room,entity) return entity.Depth or 10000 end
floatyBgTile.placements = {
    name = "Floaty BG Tile",
    data = {
        tiletype = "3",
        Depth = 10000,
        width = 8,
        height = 8,
        disableSpawnOffset = true,
        floatAmplitude = 4,
    }
}
--[[
floatyBgTile.fieldInformation = {
    Depth = {
        fieldType = "integer",
    },
}
]]
floatyBgTile.depth = function(room,entity) return entity.Depth or 10000 end

floatyBgTile.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg", {1.0, 1.0, 1.0, 0.7})

floatyBgTile.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype", true, "tilesBg")(entity)
    orig["Depth"] = {fieldType = "integer"}
    return orig
end

return floatyBgTile
local baseEntity = require("mods").requireFromPlugin("helpers.base_entity")
local fakeTilesHelper = require("helpers.fake_tiles")

local entity = {
    name = "ChroniaHelper/CustomNegaBlock",
    depth = function(room, entity) return entity.depth or -9000 end,
    placements =
    {
        name = "CustomNegaBlock",
        data =
        {
            depth = -9000,
            tileType = fakeTilesHelper.getPlacementMaterial(),
            lightOcclude = 1,
            blendIn = false
        }
    },
    fieldInformation =
    {
        tileType =
        {
            options = function()
                return fakeTilesHelper.getTilesOptions()
            end,
            editable = false
        },
        lightOcclude =
        {
            minimumValue = 0,
            maximumValue = 1
        },
        depth = require("mods").requireFromPlugin("helpers.field_options").depths,
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "tileType",
        "lightOcclude",
        "blendIn"
    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tileType","blendIn")
}

baseEntity.invoke(entity)

return entity
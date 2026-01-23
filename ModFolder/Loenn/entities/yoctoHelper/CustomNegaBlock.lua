local baseEntity = require("mods").requireFromPlugin("helpers.base_entity")
local fakeTilesHelper = require("helpers.fake_tiles")

local entity = {
    name = "ChroniaHelper/CustomNegaBlock",
    placements =
    {
        name = "CustomNegaBlock",
        data =
        {
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
        }
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
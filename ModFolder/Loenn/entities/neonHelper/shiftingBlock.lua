-- The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
-- The original author is ricky06, code modified by UnderDragon

local fakeTilesHelper = require("helpers.fake_tiles")

local shiftingBlock = {}

shiftingBlock.name = "ChroniaHelper/ShiftingBlock"
shiftingBlock.depth = 0

shiftingBlock.placements = {
    name = "Shifting Block",
    data = {
        tiletype = "3",
        width = 8,
        height = 8,
        shakeTime = 0.4,
        easing = "QuadInOut",
        noConnector = false,
    }
}

shiftingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
shiftingBlock.fieldInformation = {
    tiletype =
        {
            options = function()
                return fakeTilesHelper.getTilesOptions()
            end,
            editable = false
        },
    easing = {
        options = require("mods").requireFromPlugin("libraries.chroniaHelper").easers,
        editable = false,
    }
}

return shiftingBlock
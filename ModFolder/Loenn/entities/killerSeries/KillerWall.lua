local fakeTilesHelper = require("helpers.fake_tiles")
local cons = require("mods").requireFromPlugin("utils.constants")

return {
    name = "ChroniaHelper/KillerWall",
    placements =
    {
        name = "KillerWall",
        data =
        {
            width = 8,
            height = 8,
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            topKillTimer = - 1,
            bottomKillTimer = - 1,
            leftKillTimer = - 1,
            rightKillTimer = - 1,
            depth = -13000,
            dashRebound = false,
            dashReboundRefill = false,
            --springBlockOverride = false,
            safe = true,
        }
    },
    fieldInformation =
    {
        tiletype =
        {
            options = function()
                return fakeTilesHelper.getTilesOptions()
            end,
            editable = false
        },
        topKillTimer =
        {
            minimumValue = - 1
        },
        bottomKillTimer =
        {
            minimumValue = - 1
        },
        leftKillTimer =
        {
            minimumValue = - 1
        },
        rightKillTimer =
        {
            minimumValue = - 1
        },
        depth = {
            fieldType = "integer",
            options = require("mods").requireFromPlugin("consts.depths"),
            editable = true,
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "depth",
        "tiletype",
        "topKillTimer",
        "bottomKillTimer",
        "leftKillTimer",
        "rightKillTimer",
    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype","behind"),
    depth = function(room,entity) return entity.depth or -13000 end
}
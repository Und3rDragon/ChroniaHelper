local fakeTilesHelper = require("helpers.fake_tiles")
local cons = require("mods").requireFromPlugin("utils.constants")

return {
    name = "ChroniaHelper/KillerBlock",
    placements =
    {
        name = "KillerBlock",
        data =
        {
            width = 8,
            height = 8,
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            topKillTimer = - 1,
            bottomKillTimer = - 1,
            leftKillTimer = - 1,
            rightKillTimer = - 1,
            depth = -9000,
            finalBoss = false,
            climbFall = true,
            standFall = true,
            canTrigger = true,
            fallDelay = 0.4,
            --movement = "0, 130, 500, 160",
            dashRebound = false,
            dashReboundRefill = false,
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
            options = require("mods").requireFromPlugin("consts.depthOptions"),
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
        "behind",
        "fallDelay",
        -- bools
        "finalBoss",
        "climbFall",
        "standFall",
        "canTrigger",
        "dashRebound",
        "dashReboundRefill",

    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype","behind"),
    depth = function(room,entity) return entity.depth or -9000 end
}
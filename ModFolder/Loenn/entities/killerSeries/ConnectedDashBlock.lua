local fakeTilesHelper = require("helpers.fake_tiles")
local cons = require("mods").requireFromPlugin("utils.constants")

return {
    name = "ChroniaHelper/ConnectedDashBlock",
    placements =
    {
        name = "ConnectedDashBlock",
        data =
        {
            width = 8,
            height = 8,
            tiletype = '3',
            blendin = false,
            permanent = true,
            canDash = true,
            depth = -12999,
            safe = true,
            bgTexture = false,
        }
    },
    fieldInformation = function(entity)
        local orig = {}
        if entity.bgTexture then
            orig = fakeTilesHelper.getFieldInformation("tiletype", "tilesBg")(entity)
        else
            orig = fakeTilesHelper.getFieldInformation("tiletype", "tilesFg")(entity)
        end

        orig["depth"] = {
            fieldType = "integer",
            options = require("mods").requireFromPlugin("consts.depths"),
            editable = true,
        }

        return orig
    end,
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "depth",
        "tiletype",
        "blendin",
        "permanent",
        "canDash",
    },
    sprite = function(room, entity)
        local sprites = {}
        if entity.bgTexture then
           sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg")(room, entity)
        else
           sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg")(room, entity)
        end

        return sprites
    end,
    depth = function(room, entity) return entity.depth or -12999 end
}

local fakeTilesHelper = require("helpers.fake_tiles")
local cons = require("mods").requireFromPlugin("utils.constants")

local block = {}

block.name = "ChroniaHelper/AnotherTileEntity"

block.placements =
{
    name = "tile",
    data =
    {
        width = 8,
        height = 8,
        tiletype = '3',
        depth = -9000,
        surfaceSoundIndex = 8,
        bgTexture = false,
        safe = true,
    }
}

block.fieldInformation = function(entity)
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
end

block.fieldOrder =
{
    
}

block.sprite = function(room, entity)
    local sprites = {}
    if entity.bgTexture then
       sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg")(room, entity)
    else
       sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg")(room, entity)
    end

    return sprites
end

block.depth = function(room,entity) return entity.depth or -9000 end

block.selection = function(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    
    local mainRectangle = require("utils").rectangle(entity.x, entity.y, entity.width, entity.height)

    return mainRectangle
end

return block
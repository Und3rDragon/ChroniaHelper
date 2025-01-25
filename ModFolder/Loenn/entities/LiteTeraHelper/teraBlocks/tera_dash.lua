local fakeTilesHelper = require("helpers.fake_tiles")
local drawableSprite = require("structs.drawable_sprite")
local teraEnum = require("mods").requireFromPlugin("libraries.enum")

local teraDashBlock = {}

teraDashBlock.name = "ChroniaHelper/teraDashBlock"
teraDashBlock.depth = 0
teraDashBlock.placements = {
    name = "TeraLite - Dash Block",
    data = {
        tiletype = "3",
        tera = "Normal",
        blendin = true,
        canDash = true,
        permanent = true,
        width = 8,
        height = 8
    }
}

local getFakeTilesSprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
function teraDashBlock.sprite(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 0, entity.height or 0
    local sprites = getFakeTilesSprite(room, entity)
    local tera = entity.tera or "Normal"
    local texture = "ChroniaHelper/objects/tera/Block/" .. tera
    table.insert(sprites, drawableSprite.fromTexture(texture, { x = x + width/2, y = y + height/2 }))
    return sprites
end

local getFakeTilesFieldInformation = fakeTilesHelper.getFieldInformation("tiletype")
function teraDashBlock.fieldInformation()
    local fields = getFakeTilesFieldInformation()

    fields.tera = {
        options = teraEnum.teraType,
        editable = false
    }

    return fields
end

return teraDashBlock
local fakeTilesHelper = require("helpers.fake_tiles")
local drawableSprite = require("structs.drawable_sprite")
local teraEnum = require("mods").requireFromPlugin("libraries.enum")

local teraFallingBlock = {}

teraFallingBlock.name = "ChroniaHelper/teraFallingBlock"
teraFallingBlock.placements = {
    name = "TeraLite - Falling Block",
    data = {
        tiletype = "3",
        tera = "Normal",
        climbFall = true,
        behind = false,
        width = 8,
        height = 8
    }
}

local getFakeTilesSprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
function teraFallingBlock.sprite(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 0, entity.height or 0
    local sprites = getFakeTilesSprite(room, entity)
    local tera = entity.tera or "Normal"
    local texture = "ChroniaHelper/objects/tera/Block/" .. tera
    table.insert(sprites, drawableSprite.fromTexture(texture, { x = x + width/2, y = y + height/2 }))
    return sprites
end

local getFakeTilesFieldInformation = fakeTilesHelper.getFieldInformation("tiletype")
function teraFallingBlock.fieldInformation()
    local fields = getFakeTilesFieldInformation()

    fields.tera = {
        options = teraEnum.teraType,
        editable = false
    }

    return fields
end

function teraFallingBlock.depth(room, entity)
    return entity.behind and 5000 or 0
end

return teraFallingBlock
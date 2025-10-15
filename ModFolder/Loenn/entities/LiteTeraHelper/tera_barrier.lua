local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local teraEnum = require("mods").requireFromPlugin("helpers.enum")
local teraBarrier = {}

teraBarrier.name = "ChroniaHelper/teraBarrier"

teraBarrier.depth = 0
teraBarrier.placements = {
    name = "TeraLite - Barrier",
    data = {
        tera = "Normal",
        width = 8,
        height = 8
    }
}
teraBarrier.fieldInformation = {
    tera = {
        options = teraEnum.teraType,
        editable = false
    }
}

function teraBarrier.sprite(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 0, entity.height or 0
    local sprites = {}
    local rectangleSprite = drawableRectangle.fromRectangle("bordered", x, y, width, height, {0.25, 0.25, 0.25, 0.8}, {1.0, 1.0, 1.0})
    rectangleSprite.depth = 0
    table.insert(sprites, rectangleSprite)
    local tera = entity.tera or "Normal"
    local texture = "ChroniaHelper/objects/tera/Block/" .. tera
    table.insert(sprites, drawableSprite.fromTexture(texture, { x = x + width/2, y = y + height/2 }))
    return sprites
end

return teraBarrier
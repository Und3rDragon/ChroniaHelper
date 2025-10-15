local drawableSprite = require("structs.drawable_sprite")
local utils = require('utils')
local teraEnum = require("mods").requireFromPlugin("helpers.enum")
local teraCrystal = {}

teraCrystal.name = "ChroniaHelper/teraCrystal"
teraCrystal.depth = 100
teraCrystal.placements = {
	name = "TeraLite - Crystal",
    data = {
        tera = "Normal",
    }
}
teraCrystal.fieldInformation = {
    tera = {
        options = teraEnum.teraType,
        editable = false
    }
}
local offsetY = -10
local texture = "ChroniaHelper/objects/tera/Crystal/idle00"

function teraCrystal.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite.y += offsetY
    local sprites = {}
    table.insert(sprites, sprite)
    local tera = entity.tera or "Normal"
    local teraTexture = "ChroniaHelper/objects/tera/Block/" .. tera
    table.insert(sprites, drawableSprite.fromTexture(teraTexture, { x = x, y = y - 8}))
    return sprites
end

function teraCrystal.selection(room, entity)
    return utils.rectangle(entity.x - 11, entity.y - 21, 21, 22)
end

return teraCrystal
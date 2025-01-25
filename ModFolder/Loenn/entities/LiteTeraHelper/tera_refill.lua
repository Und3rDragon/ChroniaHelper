local drawableSprite = require("structs.drawable_sprite")
local teraEnum = require("mods").requireFromPlugin("libraries.enum")
local teraRefill = {}

teraRefill.name = "ChroniaHelper/teraRefill"
teraRefill.depth = 0
teraRefill.placements = {
	name = "TeraLite - Refill",
    data = {
        oneUse = false,
        shield = false,
        tera = "Normal",
    }
}
teraRefill.fieldInformation = {
    tera = {
        fieldType = "anything",
        options = teraEnum.teraType,
        editable = false
    }
}

function teraRefill.sprite(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local tera = entity.tera or "Normal"
    local texture = "ChroniaHelper/objects/tera/Block/" .. tera
    local sprites = { drawableSprite.fromTexture(texture, { x = x, y = y }) }
    local shield = entity.shield or false
    if shield then
        table.insert(sprites, drawableSprite.fromTexture("ChroniaHelper/objects/tera/Block/Shield", { x = x, y = y }))
    end
    return sprites
end

return teraRefill
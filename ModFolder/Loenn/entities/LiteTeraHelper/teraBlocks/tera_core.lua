local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local teraEnum = require("mods").requireFromPlugin("libraries.enum")

local teraCoreBlock = {}

teraCoreBlock.name = "ChroniaHelper/teraCoreBlock"
teraCoreBlock.depth = 8990
teraCoreBlock.minimumSize = {16, 16}
teraCoreBlock.placements = {
    {
        name = "TeraLite - Core Block (Fire)",
        alternativeName = "fire_bounce",
        data = {
            width = 16,
            height = 16,
            tera = "Normal",
            notCoreMode = false
        }
    },
    {
        name = "TeraLite - Core Block (Ice)",
        data = {
            width = 16,
            height = 16,
            tera = "Normal",
            notCoreMode = true
        }
    },
}
teraCoreBlock.fieldInformation = {
    tera = {
        options = teraEnum.teraType,
        editable = false
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local fireBlockTexture = "objects/BumpBlockNew/fire00"
local fireCrystalTexture = "objects/BumpBlockNew/fire_center00"

local iceBlockTexture = "objects/BumpBlockNew/ice00"
local iceCrystalTexture = "objects/BumpBlockNew/ice_center00"

local function getBlockTexture(entity)
    if entity.notCoreMode then
        return iceBlockTexture, iceCrystalTexture

    else
        return fireBlockTexture, fireCrystalTexture
    end
end

function teraCoreBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockTexture, crystalTexture = getBlockTexture(entity)

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, crystalSprite)
    local tera = entity.tera or "Normal"
    local texture = "ChroniaHelper/objects/tera/Block/" .. tera
    table.insert(sprites, drawableSprite.fromTexture(texture, { x = x + width/2, y = y + height/2 }))
    return sprites
end

return teraCoreBlock
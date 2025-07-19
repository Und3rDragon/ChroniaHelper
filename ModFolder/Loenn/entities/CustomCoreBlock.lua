local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local CustomCoreBlock = {}

CustomCoreBlock.name = "ChroniaHelper/CustomCoreBlock"
CustomCoreBlock.depth = 8990
CustomCoreBlock.minimumSize = {16, 16}
CustomCoreBlock.placements = {
    {
        name = "fire",
        data = {
            width = 16,
            height = 16,
            type = 0,
            iceBlockTexture = "objects/BumpBlockNew/ice00",
            fireBlockTexture = "objects/BumpBlockNew/fire00",
            notCoreMode = false
        }
    },
    {
        name = "ice",
        data = {
            width = 16,
            height = 16,
            type = 1,
            iceBlockTexture = "objects/BumpBlockNew/ice00",
            fireBlockTexture = "objects/BumpBlockNew/fire00",
            notCoreMode = true
        }
    },
}

CustomCoreBlock.fieldInformation = {
    type = {
        options = {
            ["fire"] = 0,
            ["ice"] = 1,
        },
        editable = false,
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
    if entity.type == 1 then
        return iceBlockTexture, iceCrystalTexture
    else
        return fireBlockTexture, fireCrystalTexture
    end
end

function CustomCoreBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockTexture, crystalTexture = getBlockTexture(entity)

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, crystalSprite)

    return sprites
end

return CustomCoreBlock
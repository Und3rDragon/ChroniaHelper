local baseEntity = require("mods").requireFromPlugin("helpers.base_entity")
local fakeTilesHelper = require("helpers.fake_tiles")

local entity = {
    name = "ChroniaHelper/AlternateTileBlock",
    placements =
    {
        name = "TileBlock",
        data =
        {
            depth = -9000,
            sprite = "objects/switchgate/block",
            lightOcclude = 1,
            unitSize = 8,
        }
    },
    fieldInformation =
    {
        depth = require("mods").requireFromPlugin("helpers.field_options").depths,
        tileType =
        {
            options = function()
                return fakeTilesHelper.getTilesOptions()
            end,
            editable = false
        },
        lightOcclude =
        {
            minimumValue = 0,
            maximumValue = 1
        },
        unitSize = {
            fieldType = "integer",
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "tileType",
        "lightOcclude",
        "blendIn"
    }
}

entity.depth = function(room,entity) return entity.Depth or -9000 end

local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

function entity.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or entity.unitSize * 3, entity.height or entity.unitSize * 3
    local blockSprite = entity.sprite or "objects/switchgate/block"

    -- sprite judgement, prevent lonn from crashing
    local sprite = drawableSprite.fromTexture(blockSprite, entity)
    local spriteWidth = sprite.meta.width
    local spriteHeight = sprite.meta.height
    local displayUnitSize = entity.unitSize
    if spriteWidth < entity.unitSize * 3 or spriteHeight < entity.unitSize * 3 then
        blockSprite = "objects/switchgate/block"
        displayUnitSize = 8
        -- a fallback protocol
    end

    local ninePatchOptions = {
        tileSize = displayUnitSize,
        useRealSize = true,
    }

    local ninePatch = drawableNinePatch.fromTexture(blockSprite, ninePatchOptions, x, y, width, height)
    local sprites = ninePatch:getDrawableSprite()

    return sprites
end

baseEntity.invoke(entity)

return entity
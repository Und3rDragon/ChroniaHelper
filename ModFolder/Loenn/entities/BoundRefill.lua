local drawableSprite = require("structs.drawable_sprite")
local dir = require("mods").requireFromPlugin("chroniaHelper.utils.directory")

local refill = {}

refill.name = "ChroniaHelper/BoundRefill"
refill.depth = -100
refill.associatedMods = { "FemtoHelper", "ChroniaHelper" }
refill.placements = {
    {
        name = "bound",
        data = {
            oneUse = false,
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0,
            direction = 0,
            pulseFlag = "",
            directory = "objects/FemtoHelper/boundRefill/",
            speedXOnHorizontal = 240,
            --speedYOnHorizontal = 140,
            --speedXOnVertical = 0,
            speedYOnVertical = 300,
        }
    },
}

refill.fieldInformation = function(entity)
    local orig = {}

    orig["direction"] = {
        fieldType = "integer",
        options = {
            ["Up"] = 0,
            ["Left"] = 1,
            ["Right"] = 2,
            ["Down"] = 3,
            ["Same as Dash"] = 4,
        },
        editable = false,
    }

    return orig
end

function refill.sprite(room, entity)
    local sprPath = entity.directory

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;

    if vx ~= 0 or vy ~= 0 then
        --local main = drawableSprite.fromTexture(sprPath.."outline", entity)
        local main = dir.fetchSprite(sprPath .. "outline", 0, entity)
        table.insert(sprites, main)

        --local offset_spr = drawableSprite.fromTexture(sprPath.."idle00", entity)
        local offset_spr = dir.fetchSprite(sprPath.."idle", 0, entity)
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        table.insert(sprites, offset_spr)
    else
        --local main = drawableSprite.fromTexture(sprPath.."idle00", entity)
        local main = dir.fetchSprite(sprPath .. "idle", 0, entity)
        table.insert(sprites, main)
    end

    return sprites
end

return refill

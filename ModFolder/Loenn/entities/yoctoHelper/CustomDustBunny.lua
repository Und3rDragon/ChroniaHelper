local drawableSprite = require("structs.drawable_sprite")

local entity = {
    name = "ChroniaHelper/CustomDustBunny",
    placements =
    {
        name = "CustomDustBunny",
        data =
        {
            tintColor = "000000",
            eyesColor = "ff0000",
            borderColor = "ff0000",
            centerTexture = "ChroniaHelper/CustomDustBunny/center",
            baseTexture = "ChroniaHelper/CustomDustBunny/base",
            overlayTexture = "ChroniaHelper/CustomDustBunny/overlay",
            hasEyes = true,
            attached = false
        }
    },
    fieldInformation =
    {
        tintColor =
        {
            fieldType = "color"
        },
        eyesColor =
        {
            fieldType = "color"
        },
        borderColor =
        {
            fieldType = "color"
        }
    },
    fieldOrder =
    {
        "x",
        "y",
        "tintColor",
        "eyesColor",
        "borderColor",
        "hasEyes",
        "attached"
    },
    sprite = function(room, entity, viewport)
        local texture = drawableSprite.fromTexture("danger/dustcreature/base00", entity)
        texture:setColor(entity.tintColor)
        local internalTexture = drawableSprite.fromInternalTexture("dust_creature_outlines/base00", entity)
        internalTexture:setColor(entity.borderColor)
        internalTexture.depth = -49
        return {
            internalTexture,
            texture
        }
    end,
    depth = - 50
}

return entity
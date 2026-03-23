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
            attached = false,
            --moving bunny
            duration = 1,
            movement = 4, --int
            easer = "CubeInOut",
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
        },
        easer = {
            options = require("mods").requireFromPlugin("helpers.chroniaHelper_old").easers,
            editable = false,
        },
        movement = {
            options = {
                ["Persist"] = 0,
                ["Oneshot"] = 1,
                ["Looping"] = 2,
                ["YoyoOneshot"] = 3,
                ["YoyoLooping"] = 4,
            },
            editable = false,
        },
        duration = {
            minimumValue = 0,
        },
    },
    --[[
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
    ]]
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
    depth = - 50,
    nodeLimits = {0, 1},
    ignoredFields = function(entity)
        local attrs = {"_x", "_y", "_id", "_name"}
        if entity.nodes == nil then
            table.insert(attrs, "easer")
            table.insert(attrs, "movement")
            table.insert(attrs, "duration")
            return attrs
        elseif #entity.nodes == 0 then
            table.insert(attrs, "easer")
            table.insert(attrs, "movement")
            table.insert(attrs, "duration")
            return attrs
        else
            return attrs
        end
    end,
}

return entity
local drawableSpriteStruct = require("structs.drawable_sprite")

local cloud = {}

cloud.name = "ChroniaHelper/CustomCloud"
cloud.depth = 0
cloud.placements = {
    {
        name = "normal",
        data = {
            colliderWidth = 32,
            colliderOffset = -16,
            particleColor = "",
            soundIndex = 4,
            lightOcclude = 0.2,
            scaleX = 1,
            scaleY = 1,
            scaleXOnPress = 1.3,
            scaleYOnPress = 0.7,
            cloudSound = "default",
            cloudAppearSound = "default",
            respawnTime = 2.5,
            --liftSpeedMultiplier = 1,
            spriteXMLTag = "cloud",
            depth = 0,
            neutralLiftSpeed = -200,
            bumperness = 1,
            forceSafe = false,
            -- vanilla parameters
            fragile = false,
            --small = false
        }
    },
    {
        name = "fragile",
        data = {
            colliderWidth = 32,
            colliderOffset = -16,
            particleColor = "",
            soundIndex = 4,
            lightOcclude = 0.2,
            scaleX = 1,
            scaleY = 1,
            scaleXOnPress = 1.3,
            scaleYOnPress = 0.7,
            cloudSound = "default",
            cloudAppearSound = "default",
            respawnTime = 2.5,
            --liftSpeedMultiplier = 1,
            spriteXMLTag = "cloud",
            depth = 0,
            neutralLiftSpeed = -200,
            bumperness = 1,
            forceSafe = false,
            -- vanilla parameters
            fragile = true,
            --small = false
        }
    }
}

cloud.fieldInformation = {
    colliderWidth = {
        fieldType = "integer",
        minimumValue = 1,
    },
    particleColor = {
        fieldType = "color",
        useAlpha = true,
        allowEmpty = true,
    },
    soundIndex = {
        fieldType = "integer",
        options = require("consts.celeste_enums").tileset_sound_ids,
        editable = false,
    },
    lightOcclude = {
        minimumValue = 0,
        maximumValue = 1,
    },
    cloudSound = {
        options = { "default" },
    },
    cloudAppearSound = {
        options = { "default" },
    },
    respawnTime = {
        minimumValue = 0
    },
    spriteXMLTag = {
        options = { "cloud" },
    },
    depth = require("mods").requireFromPlugin("helpers.field_options").depths,
}

local normalScale = 1.0
local smallScale = 29 / 35

local function getTexture(entity)
    local fragile = entity.fragile

    if fragile then
        return "objects/clouds/fragile00"

    else
        return "objects/clouds/cloud00"
    end
end

function cloud.sprite(room, entity)
    local texture = getTexture(entity)
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    local small = entity.small
    local scale = small and smallScale or normalScale

    sprite:setScale(scale, 1.0)

    return sprite
end

return cloud
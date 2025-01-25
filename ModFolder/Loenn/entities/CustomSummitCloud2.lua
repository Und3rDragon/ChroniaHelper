local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("libraries.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")
local vivUtils = require("mods").requireFromPlugin("libraries.vivUtilsMig")

local cloud = {}

cloud.name = "ChroniaHelper/CustomSummitCloud2"

cloud.depth = function(room,entity) return entity.depth or -10550 end
cloud.placements = {
	name = "cloud",
	data = {
        path = "ChroniaHelper/CustomSummitClouds/cloud",
        color = "ffffff",
        floatingFreq = 0.1,
        floatyAmplitude = 8,
        randomFloatingFreq = 0.05,
        depth = -10550,
        parallax = 1.0,
        randomParallax = 0.1,
        randomFlipX = true,
        randomFlipY = false,
        screenPosX = 160,
        screenPosY = 90,
	},
}

cloud.fieldInformation = {
    depth = fo.depths,
    color = {
        fieldType = "color",
    }
}

function cloud.sprite(room, entity)
    utils.setSimpleCoordinateSeed(entity.x, entity.y)

    local n = string.format("%02d", math.random(0,2))
    local m = math.random(0,2)
    local sprite = drawableSprite.fromTexture(entity.path .. n, entity)

    local picW = sprite.meta.width
    local picH = sprite.meta.height

    return sprite
end

--Render code for Version 2
cloud.selection = function(room, entity)
    --same as sprite
    utils.setSimpleCoordinateSeed(entity.x, entity.y)

    local n = string.format("%02d", math.random(0,2))
    local m = math.random(0,2)
    local sprite = drawableSprite.fromTexture(entity.path .. n, entity)

    local picW = sprite.meta.width
    local picH = sprite.meta.height
    --same as sprite

    return require('utils').rectangle(entity.x - picW / 2, entity.y - picH / 2, picW, picH)
end

return cloud
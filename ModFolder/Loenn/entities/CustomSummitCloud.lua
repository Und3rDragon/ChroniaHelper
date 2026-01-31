local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("helpers.chroniaHelper_old")
local fo = require("mods").requireFromPlugin("helpers.field_options")
local vivUtils = require("mods").requireFromPlugin("helpers.vivUtilsMig")

local cloud = {}

cloud.name = "ChroniaHelper/CustomSummitCloud"

cloud.depth = function(room,entity) return entity.depth or -10550 end
cloud.placements = {
	name = "cloud",
	data = {
        width = 8,
        height = 8,
        path = "ChroniaHelper/CustomSummitClouds/cloud",
        color = "ffffff",
        floatiness = 1.0,
        depth = -10550,
        parallax = 1.0,
        randomizeParallax = true,
        randomFlipX = true,
        randomFlipY = false,
        alignment = "PositionTopLeft",
        screenPosX = 160,
        screenPosY = 90,
	},
}

cloud.fieldInformation = {
    depth = fo.depths,
    color = {
        fieldType = "color",
    },
    alignment = {
        options = {
            "PositionTopLeft", "PositionTopCenter", "PositionTopRight",
            "PositionCenterLeft", "PositionCenter", "PositionCenterRight",
            "PositionBottomLeft", "PositionBottomCenter", "PositionBottomRight",
            "AlignTopLeft", "AlignTopCenter", "AlignTopRight",
            "AlignCenterLeft", "AlignCenter", "AlignCenterRight",
            "AlignBottomLeft", "AlignBottomCenter", "AlignBottomRight"
        },
        editable = false,
    }
}

local picW, picH, dx, dy

function cloud.sprite(room, entity)
    utils.setSimpleCoordinateSeed(entity.x, entity.y)

    local n = string.format("%02d", math.random(0,2))
    local m = math.random(0,2)
    local sprite = drawableSprite.fromTexture(entity.path .. n, entity)

    dx = 0
    dy = 0
    picW = sprite.meta.width
    picH = sprite.meta.height

    if entity.alignment == "PositionTopCenter" then
        dx = entity.width / 2
    elseif entity.alignment == "PositionTopRight" then
        dx = entity.width
    elseif entity.alignment == "PositionCenterLeft" then
        dy = entity.height / 2
    elseif entity.alignment == "PositionCenter" then
        dx = entity.width / 2
        dy = entity.height / 2
    elseif entity.alignment == "PositionCenterRight" then
        dx = entity.width
        dy = entity.height / 2
    elseif entity.alignment == "PositionBottomLeft" then
        dy = entity.height
    elseif entity.alignment == "PositionBottomCenter" then
        dx = entity.width / 2
        dy = entity.height
    elseif entity.alignment == "PositionBottomRight" then
        dx = entity.width
        dy = entity.height
    elseif entity.alignment == "AlignTopLeft" then
        dx = picW / 2
        dy = picH / 2
    elseif entity.alignment == "AlignTopCenter" then
        dy = picH / 2
    elseif entity.alignment == "AlignTopRight" then
        dx = -picW / 2
        dy = picH / 2
    elseif entity.alignment == "AlignCenterLeft" then
        dx = picW / 2
    elseif entity.alignment == "AlignCenterRight" then
        dx = -picW / 2
    elseif entity.alignment == "AlignBottomLeft" then
        dx = picW / 2
        dy = -picH / 2
    elseif entity.alignment == "AlignBottomCenter" then
        dy = -picH / 2
    elseif entity.alignment == "AlignBottomRight" then
        dx = -picW / 2
        dy = -picH / 2
    else
        dx = 0
        dy = 0
    end

    return sprite:addPosition(dx,dy)
end

--[[
--Render code for Version 2
cloud.selection = function(room, entity)
    return require('utils').rectangle(entity.x - picW / 2 + dx, entity.y - picH / 2 + dy, picW, picH)
end
]]

return cloud
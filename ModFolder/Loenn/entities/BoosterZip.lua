local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("libraries.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')

local boosterzip = {}

boosterzip.name = "ChroniaHelper/BoosterZipper"
boosterzip.depth = 4999
boosterzip.nodeLimits = {1, -1}

boosterzip.placements = {
	name = "zipbooster",
	data = {
		boosterRespawnTime = 1,
		zipperMoveTime = 0.5,
        ropeColor = "663931",
        ropeLightColor = "9b6157",
	}
}

boosterzip.fieldInformation = {
    ropeColor = {
        fieldType = "color",
        allowEmpty = false,
    },
    ropeLightColor = {
        fieldType = "color",
        allowEmpty = false,
    },
}

function boosterzip.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local boosterDir = "objects/booster/booster00"
    
    local sprites = {
        --rectangle
        drawableSprite.fromTexture(boosterDir, entity)
    }

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeSprites = ChroniaHelper.getZipMoverNodeSprites(x, y, 0, 0, nodes, "objects/zipmover/cog", {1, 1, 1}, require('mods').requireFromPlugin('libraries.vivUtilsMig').getColorTable(entity.ropeColor, true, {1,1,1,1}))
    for _, sprite in ipairs(nodeSprites) do
        table.insert(sprites, sprite)
    end

    for nodeIndex, node in ipairs(nodes) do
        local sprite = drawableSprite.fromTexture(boosterDir, entity)
        sprite:setColor({1,1,1,0.3})
        table.insert(sprites, sprite)

        --[[
        local text = drawableText.fromText(ChroniaHelper.getStringAtIndex(entity.nodeSpeeds, nodeIndex), node.x, node.y)
        local texta = drawableText.fromText(ChroniaHelper.getStringAtIndex(entity.delays, nodeIndex), node.x, node.y, entity.width * 2, entity.height * 2)
        if entity.displayParameters then
            table.insert(sprites, text)
            table.insert(sprites, texta)
        end
        ]]
    end

    return sprites
end

function boosterzip.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0

    local mainRectangle = utils.rectangle(x - 8, y - 8, 16, 16)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeRectangles = {}
    for _, node in ipairs(nodes) do
        table.insert(nodeRectangles, utils.rectangle(node.x - 8, node.y - 8, 16, 16))
    end

    return mainRectangle, nodeRectangles
end

return boosterzip
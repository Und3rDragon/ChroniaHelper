-- The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
-- The original author is ricky06, code modified by UnderDragon

local drawableText = require("structs.drawable_text")
local drawableRectangle = require("structs.drawable_rectangle")
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local omniSetups = require("mods").requireFromPlugin("consts.omniSetups")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("helpers.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")

local shiftingSwitch = {}

shiftingSwitch.name = "ChroniaHelper/ShiftingSwitch"
shiftingSwitch.depth = 0
--shiftingSwitch.texture = "objects/NeonCity/shiftingSwitch/idle00"
--shiftingSwitch.nodeTexture = nil
shiftingSwitch.nodeLimits = {0, -1}
shiftingSwitch.nodeLineRenderType = "fan"

shiftingSwitch.placements = {
    name = "Shifting Switch",
    data = {
        sprite = "shiftingSwitch",
        connectorSprite = "objects/ChroniaHelper/shiftingSwitch/connector",
        speed = 3.0,
        distance = 32.0,
        onNormal = 1,
        onRebound = 0,
        left = true,
        right = true,
        top = true,
        down = true,
        oneConnector = false,
        silent = false,
    }
}

shiftingSwitch.justification = {0, 0}

shiftingSwitch.fieldInformation = {
    onNormal = {
        options = {
            ["Rebound"] = 0,
            ["Normal Collision"] = 1,
            ["Normal Override"] = 2,
            ["Bounce"] = 3,
            ["Ignore"] = 4,
        },
        editable = false,
    },
    onRebound = {
        options = {
            ["Rebound"] = 0,
            ["Normal Collision"] = 1,
            ["Normal Override"] = 2,
            ["Bounce"] = 3,
            ["Ignore"] = 4,
        },
        editable = false,
    },
}

shiftingSwitch.sprite = function(room, entity)
    local sprites = {}

    local base = vivUtilsMig.getImageWithNumbers("objects/ChroniaHelper/shiftingSwitch/idle", 0, entity)
    base = base:addPosition(base.meta.width / 2, base.meta.height / 2)
    local text = drawableText.fromText(entity._id, entity.x, entity.y)
    table.insert(sprites, base)
    table.insert(sprites, text)

    return sprites
end

shiftingSwitch.nodeSprite = function(room, entity, node, nodeIndex, viewport)
	local sprites = {}

	local text = drawableText.fromText(entity._id, node.x, node.y)
    local rectangle = drawableRectangle.fromRectangle("bordered", node.x - 8, node.y - 8, 16, 16, {1, 1, 1, 0}, {1.0, 1.0, 1.0, 0.5})

    table.insert(sprites, text)
    table.insert(sprites, rectangle)

	return sprites
end

shiftingSwitch.selection = function(room, entity)
    local sprite = vivUtilsMig.getImageWithNumbers("objects/ChroniaHelper/shiftingSwitch/idle", 0, entity)
    local baseRec = utils.rectangle(entity.x, entity.y, sprite.meta.width, sprite.meta.height)

    if entity.nodes ~= nil then
        local nodeRec = {}
        for _, node in ipairs(entity.nodes) do
            local rec = utils.rectangle(node.x - 8, node.y - 8, 16, 16)
            table.insert(nodeRec, rec)
        end

        return baseRec, nodeRec
    end

    return baseRec
end

return shiftingSwitch
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/GroupedWindTunnelController"

controller.placements = {
    name = "GroupedWindTunnelController",
    data = {
        groupIDs = "tunnel",
        groupDepth = 9000,
        strength = 1,
        particleStrength = -1,
        condition = "",
        conditionType = 0,
        angle = 0,
        particleDensity = 0.3,
        colors = "ffffff",
        windUpSpeed = 1,
        windDownSpeed = 0.6,
        conditionMode = 0,
        affectPlayer = true,
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y", "_id", "_name"
}

controller.fieldInformation = 
{
    conditionType = {
        fieldType = "integer",
        options = {
            ["Flags"] = 0, ["ChroniaMathExpression"] = 1, ["FrostSessionExpression"] = 2
        },
        editable = false,
    },
    conditionMode = {
        fieldType = "integer",
        options = {
            ["Always"] = 0, ["Normal"] = 1, ["Inverted"] = 2,
        },
        editable = false,
    },
    groupIDs = {
        fieldType = "list",
        minimumElements = 1,
    },
    groupDepth = require("mods").requireFromPlugin("helpers.field_options").depths,
    particleDensity = {
        minimumValue = 0,
    },
    colors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
            useAlpha = true,
        },
    },
    windUpSpeed = {
        minimumValue = 0.0001, 
    },
    windDownSpeed = {
        minimumValue = 0.0001, 
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/GlobalWindTunnel", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
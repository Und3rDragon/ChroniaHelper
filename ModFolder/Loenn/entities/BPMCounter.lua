local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/BPMCounter"

controller.placements = {
    {
        name = "controller",
        data = {
            bpm = 144,
            beatsPerLoop = 8,
            counter = "bpmCounter",
            flag = "",
            mode = 0,
            offsetSeconds = 0,
        }
    }
}

controller.fieldInformation = 
{
    bpm = {
        fieldType = "integer",
        minimumValue = 1,
    },
    beatsPerLoop = {
        fieldType = "integer",
        minimumValue = 1,
    },
    mode = {
        fieldType = "integer",
        options = {
            ["Use Raw Level Active Time"] = 0,
            ["Use Relative Raw Level Time (Resets on Flag Inactive)"] = 1,
        },
        editable = false,
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/BPMCounter", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/CodeButtonTargetController"

controller.placements = {
    name = "controller",
    data = {
        x = 0,
        y = 0,
        sessionKeyID = "buttonKey",
        buttonCodeTarget = "000000",
        targetFlag = "flag",
        hitEnterToConfirm = true,
        deactivateFlagWhenNotSatisfied = false,
    },
}

controller.ignoredFields = {
    
}

controller.fieldInformation = 
{
    
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/ButtonTarget", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/ComprehensiveController"

controller.placements = {
    {
        name = "controller",
        data = {
            Introduction = "",
            PlayerCollidingBGTiles = "ChroniaHelper_PlayerCollidingBGTiles",
            PlayerTouchingTriggers = "ChroniaHelper_PlayerTouchingTriggers",
            PlayerCollidingEntitiesWithSameDepth = "ChroniaHelper_PlayerCollidingEntitiesWithSameDepth",
            PlayerCollidingEntitiesAbove = "ChroniaHelper_PlayerCollidingEntitiesAbove",
            PlayerCollidingEntitiesBelow = "ChroniaHelper_PlayerCollidingEntitiesBelow",
        }
    }
}

controller.ignoredFields = {
    "_x", "_y", "x", "y"
}

controller.fieldInformation = 
{
    Introduction = {
        options = {}, editable = false,
    }
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/BGTilesCollideController", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

--return controller
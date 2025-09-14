local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/BGTileCollideController"

controller.placements = {
    {
        name = "controller",
        data = {
            conditionFlags = "",
            flag = "ChroniaHelper_PlayerCollidingBGTiles",
            indicatesColliding = true,
            killPlayer = false,
            killWhenNotColliding = true,
        }
    }
}

controller.fieldInformation = 
{
    flag = {
        allowsEmpty = true,
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/BGTilesCollideController", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
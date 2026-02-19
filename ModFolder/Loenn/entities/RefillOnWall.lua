local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local comfyWall = {}

comfyWall.name = "ChroniaHelper/RefillOnWall"
comfyWall.depth = function(room, entity) return entity.depth or -10500 end
--comfyWall.canResize = {true, false}
comfyWall.minimumSize = {8, 8}
comfyWall.placements = {
    name = "wall",
    --placementType = "rectangle",
    data = {
        depth = -10500,
        width = 8,
        height = 8,
        sides = 2,
        directory = "objects/ChroniaHelper/comfyWall/",
        createStaticMover = false,
        requireGrab = true,
    }
}

comfyWall.fieldInformation = {
    depth = require("mods").requireFromPlugin("helpers.field_options").depths,
    sides = {
        fieldType = "integer",
        options = {
            ["Left"] = 0,
            ["Right"] = 1,
            ["Both"] = 2,
        },
        editable = false,
    },
}

local leftTexture = "objects/ChroniaHelper/comfyWall/top00"
local midTexture = "objects/ChroniaHelper/comfyWall/mid00"
local rightTexture = "objects/ChroniaHelper/comfyWall/bottom00"

function comfyWall.sprite(room, entity)
    local sprites = {}

    local width = entity.width or 8
    local height = entity.height or 8
    local tileWidth = math.floor(width / 8)
    local tileHeight = math.floor(height / 8)

    if entity.sides == 0 or entity.sides == 2 then
        for i = 2, tileHeight - 1 do
            local middleSprite = drawableSprite.fromTexture(midTexture, entity)

            middleSprite:addPosition(0, (i - 1) * 8)
            middleSprite:setJustification(0.0, 0.0)

            table.insert(sprites, middleSprite)
        end

        local leftSprite = drawableSprite.fromTexture(leftTexture, entity)
        local rightSprite = drawableSprite.fromTexture(rightTexture, entity)

        leftSprite:addPosition(0, 0)
        leftSprite:setJustification(0.0, 0.0)

        rightSprite:addPosition(0, (tileHeight - 1) * 8)
        rightSprite:setJustification(0.0, 0.0)

        table.insert(sprites, leftSprite)
        table.insert(sprites, rightSprite)
    end

    if entity.sides == 1 or entity.sides == 2 then
        for i = 2, tileHeight - 1 do
            local middleSprite = drawableSprite.fromTexture(midTexture, entity)

            middleSprite:addPosition(width, (i - 1) * 8)
            middleSprite:setJustification(0.0, 0.0)
            middleSprite:setScale(-1,1)

            table.insert(sprites, middleSprite)
        end

        local leftSprite = drawableSprite.fromTexture(leftTexture, entity)
        local rightSprite = drawableSprite.fromTexture(rightTexture, entity)

        leftSprite:addPosition(width, 0)
        leftSprite:setJustification(0.0, 0.0)
        leftSprite:setScale(-1,1)

        rightSprite:addPosition(width, (tileHeight - 1) * 8)
        rightSprite:setJustification(0.0, 0.0)
        rightSprite:setScale(-1,1)

        table.insert(sprites, leftSprite)
        table.insert(sprites, rightSprite)
    end

    return sprites
end

--[[
function comfyWall.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x, y + 5, entity.width or 8, 3)
end
]]

return comfyWall
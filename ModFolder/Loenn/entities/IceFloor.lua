local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local icyFloor = {}

icyFloor.name = "ChroniaHelper/IceFloor"
icyFloor.depth = function(room, entity) return entity.depth or -9999 end
icyFloor.canResize = {true, false}
icyFloor.minimumSize = {8, 0}
icyFloor.placements = {
    name = "IceFloor",
    placementType = "rectangle",
    data = {
        depth = -9999,
        width = 8,
        spriteXML = "iceFloor",
        createStaticMover = false,
        onGroundSpriteOffset = false,
    }
}

icyFloor.fieldInformation = {
    depth = require("mods").requireFromPlugin("helpers.field_options").depths,
}

local leftTexture = "objects/wallBooster/iceTop00"
local midTexture = "objects/wallBooster/iceMid00"
local rightTexture = "objects/wallBooster/iceBottom00"

function icyFloor.sprite(room, entity)
    local sprites = {}

    local width = entity.width or 8
    local tileWidth = math.floor(width / 8)

    for i = 2, tileWidth - 1 do
        local middleSprite = drawableSprite.fromTexture(midTexture, entity)

        middleSprite:addPosition((i - 1) * 8, 8)
        middleSprite:setJustification(0.0, 0.0)
        middleSprite.rotation = -math.pi / 2

        table.insert(sprites, middleSprite)
    end

    local leftSprite = drawableSprite.fromTexture(leftTexture, entity)
    local rightSprite = drawableSprite.fromTexture(rightTexture, entity)

    leftSprite:addPosition(0, 8)
    leftSprite:setJustification(0.0, 0.0)
    leftSprite.rotation = -math.pi / 2

    rightSprite:addPosition((tileWidth - 1) * 8, 8)
    rightSprite:setJustification(0.0, 0.0)
    rightSprite.rotation = -math.pi / 2

    table.insert(sprites, leftSprite)
    table.insert(sprites, rightSprite)

    return sprites
end

function icyFloor.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x, y + 5, entity.width or 8, 3)
end

return icyFloor
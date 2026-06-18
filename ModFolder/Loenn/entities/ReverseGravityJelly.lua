local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local reskinJelly = {}

reskinJelly.name = "ChroniaHelper/ReversedGravityJelly"

reskinJelly.ignoredFields = function(entity)
    local attrs = {"_id", "_name", "XMLOverride"}
    
    local gfxAttrs = {
        "idleAnimInterval",
        "heldAnimInterval",
        "fallAnimInterval",
        "fallLoopAnimInterval",
        "deathAnimInterval",
    }

    if entity.XMLOverride then
        for _, item in ipairs(gfxAttrs) do
            table.insert(attrs, item)
        end
    end

    return attrs
end

reskinJelly.placements = {
    name = "glider",
    data = {
        SpriteXML = "ChroniaHelper_ReversedGravityJelly",
        bubble = false, 
        Depth = -5,
        downThrowMultiplier = 1.8,
        diagThrowYMultiplier = 1.6,
        diagThrowXMultiplier = 1.8,
        gravity = -30,
        canBoostUp = true,
        downholdSpeed = -24,
        windUpholdSpeed = -176,
        upholdSpeed = -120,
        upwindSpeed = -80,
        neutralSpeed = -40,
    }
}

reskinJelly.fieldInformation = {
    Depth = require("mods").requireFromPlugin("helpers.field_options").depths,
}

reskinJelly.depth = function(room,entity) return entity.Depth or -5 end

reskinJelly.sprite = function(room,entity)
    local sprite = vivUtilsMig.getImageWithNumbers("objects/ChroniaHelper/customJelly" .. "/idle", 0, entity)

    sprite:addPosition(0, -4)

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()

        table.insert(lineSprites, sprite)

        return lineSprites
    else
        return sprite 
    end
end

reskinJelly.selection = function(room, entity)
    return require('utils').rectangle(entity.x - 16, entity.y - 14, 32, 16)
end

return reskinJelly
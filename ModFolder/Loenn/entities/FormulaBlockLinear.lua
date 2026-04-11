local fakeTilesHelper = require("helpers.fake_tiles")
local cons = require("mods").requireFromPlugin("utils.constants")

local block = {}

block.name = "ChroniaHelper/FormulaBlockLinear"
block.nodeLimits = {0,1}
block.placements =
{
    name = "FormulaBlockLinear",
    data =
    {
        width = 8,
        height = 8,
        tiletype = '3',
        duration = 1,
        startDelay = -1,
        maxMoveDuration = -1,
        depth = -9000,
        flag = "flag",
        reverseFlag = "moveReversed",
        surfaceSoundIndex = 8,
        returnOnFlagDisable = true,
        returnDuration = 0.5,
        bgTexture = false,
        tutorial = "See tooltip",
    }
}

block.fieldInformation = function(entity)
    local orig = {}
    if entity.bgTexture then
        orig = fakeTilesHelper.getFieldInformation("tiletype", "tilesBg")(entity)
    else
        orig = fakeTilesHelper.getFieldInformation("tiletype", "tilesFg")(entity)
    end

    orig["depth"] = {
        fieldType = "integer",
        options = require("mods").requireFromPlugin("consts.depths"),
        editable = true,
    }
    orig["tutorial"] = {editable = false}

    return orig
end

block.fieldOrder =
{
    
}

block.sprite = function(room, entity)
    local sprites = {}
    if entity.bgTexture then
       sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg")(room, entity)
    else
       sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg")(room, entity)
    end

    local nodeSprites = {}
    if entity.nodes ~= nil then
        if #entity.nodes > 0 then
            if entity.bgTexture then
               nodeSprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg")(room, entity)
            else
               nodeSprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg")(room, entity)
            end
            for _, s in ipairs(nodeSprites) do
                s:addPosition(entity.nodes[1].x - entity.x, entity.nodes[1].y - entity.y)
                s:setColor({1,1,1,0.3})
            end
            local line = require("structs.drawable_line").fromPoints({entity.nodes[1].x + entity.width/2, entity.nodes[1].y + entity.height/2, entity.x + entity.width/2, entity.y + entity.height/2})
            table.insert(sprites, line)
        end
    end
    for _, s in ipairs(nodeSprites) do
        table.insert(sprites, s)
    end

    return sprites
end

--[[
block.nodeSprite = function(room, entity, node)
    local sprites = {}
    
    if entity.bgTexture then
       sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesBg")(room, entity, entity.node)
    else
       sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg")(room, entity, entity.node)
    end

    if entity.nodes ~= nil then
        local line = require("structs.drawable_line").fromPoints({node.x + entity.width/2, node.y + entity.height/2, entity.x + entity.width/2, entity.y + entity.height/2})
        table.insert(sprites, line)
    end        

    return sprites
end
]]

block.depth = function(room,entity) return entity.depth or -9000 end

block.selection = function(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    
    local mainRectangle = require("utils").rectangle(entity.x, entity.y, entity.width, entity.height)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeRectangles = {}
    for _, node in ipairs(nodes) do
        table.insert(nodeRectangles, utils.rectangle(node.x, node.y, entity.width, entity.height))
    end

    return mainRectangle, nodeRectangles
end

return block
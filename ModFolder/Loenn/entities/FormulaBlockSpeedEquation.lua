local fakeTilesHelper = require("helpers.fake_tiles")
local cons = require("mods").requireFromPlugin("utils.constants")

local block = {}

block.name = "ChroniaHelper/FormulaBlockSpeedEquation"
block.placements =
{
    name = "FormulaBlockSpeedEquation",
    data =
    {
        width = 8,
        height = 8,
        functionVX = "",
        functionVY = "",
        tiletype = '3',
        startDelay = -1,
        maxMoveDuration = -1,
        depth = -9000,
        flag = "flag",
        surfaceSoundIndex = 8,
        expressionType = 1,
        bgTexture = false,
        tutorial = "See tooltip",
    }
}
block.associatedMods = function(entity)
    if entity["expressionType"] == nil then
        return {"ChroniaHelper"}
    end
    
    if entity.expressionType == 2 then
        return {"ChroniaHelper", "FrostHelper"}
    else
        return {"ChroniaHelper"}
    end
end

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
    orig["expressionType"] = {
        fieldType = "integer",
        options = {
            ["Chronia Helper Expression"] = 1,
            ["Frost Helper Session Expression"] = 2,
        },
        editable = false,
    }

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

    return sprites
end

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
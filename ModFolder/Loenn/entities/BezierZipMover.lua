local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("libraries.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")
local drawableText = require("structs.drawable_text")

local zip = {}

--zip.associatedMods = { "CommunalHelper", "ChroniaHelper" }

zip.name = "ChroniaHelper/BezierZipmover"
zip.depth = function(room,entity) return entity.depth or 4999 end
zip.minimumSize = {16, 16}
zip.nodeLimits = {1, -1}
zip.nodeVisibility = "never"
zip.fieldInformation = {
    sound = {
        options = {
            ["Default"] = "event:/game/01_forsaken_city/zip_mover",
            ["Moon"] = "event:/new_content/game/10_farewell/zip_mover",
        },
        editable = true,
    },
    moveEase = {
        options = require("mods").requireFromPlugin("libraries.chroniaHelper").easers,
        editable = false,
    },
    returnEase = {
        options = require("mods").requireFromPlugin("libraries.chroniaHelper").easers,
        editable = false,
    },
    baseColor = {
        fieldType = "color",
        allowXNAColors = true,
        useAlpha = true,
    },
    ropeColor = {
        fieldType = "color",
        allowXNAColors = true,
        useAlpha = true,
    },
    ropeLightColor = {
        fieldType = "color",
        allowXNAColors = true,
        useAlpha = true,
    },
    renderGap = {
        fieldType = "integer",
    },
    renderStyle = {
        options = {
            ["Vanilla Style"] = 0,
            ["Single Line"] = 1,
        },
        editable = false,
    },
}

zip.fieldOrder = {
    
}

zip.placements = {
    name = "zipmover",
    placementType = "rectangle",
    data = {
        depth = -9999,
        width = 16,
        height = 16,
        directory = "objects/zipmover/",
        sfx = "event:/game/01_forsaken_city/zip_mover",
        moveTime = 0.5,
        returnTime = 2,
        moveEase = "sinein",
        returnEase = "sinein",
        baseColor = "000000",
        ropeColor = "663931",
        ropeLightColor = "9b6157",
        renderGap = 0,
        renderStyle = 0,
        drawBorder = true,
    }
}

zip.ignoredFields = function(entity)
    return ignoredAttrs(entity)
end

function ignoredAttrs(entity)
    local attrs = {"_name","_id","x","y"}
    
    return attrs
end

local ropeColor = {255,255,255,1}
local assistColor = {255,255,255,0.3}

local function addNodeSprites(sprites, entity, cogTexture, centerX, centerY, centerNodeX, centerNodeY)
    local nodeCogSprite = drawableSprite.fromTexture(cogTexture, entity)

    nodeCogSprite:setPosition(centerNodeX, centerNodeY)
    nodeCogSprite:setJustification(0.5, 0.5)

    local points = {centerX, centerY, centerNodeX, centerNodeY}
    local leftLine = drawableLine.fromPoints(points, assistColor, 1)
    local rightLine = drawableLine.fromPoints(points, assistColor, 1)

    leftLine:setOffset(0, 4.5)
    rightLine:setOffset(0, -4.5)

    leftLine.depth = 5000
    rightLine.depth = 5000

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, nodeCogSprite)
end

local function addBlockSprites(sprites, entity, blockTexture, lightsTexture, x, y, width, height)
    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, centerColor)

    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    local lightsSprite = drawableSprite.fromTexture(lightsTexture, entity)

    lightsSprite:addPosition(math.floor(width / 2), 0)
    lightsSprite:setJustification(0.5, 0.0)

    table.insert(sprites, rectangle:getDrawableSprite())

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, lightsSprite)
end

function zip.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    
    for index, node in ipairs(nodes) do
        local nodeX, nodeY = node.x, node.y

        local centerX, centerY = x + halfWidth, y + halfHeight
        if index > 1 then
            centerX = nodes[index - 1].x + halfWidth
            centerY = nodes[index - 1].y + halfHeight
        end
        local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

        addNodeSprites(sprites, entity, entity.directory .. "cog", centerX, centerY, centerNodeX, centerNodeY)
    end
    
    local curvePoints = {}
    table.insert(curvePoints, {entity.x + halfWidth, entity.y + halfHeight})
    for _, node in ipairs(nodes) do
        local nodeX, nodeY = node.x, node.y
        local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight
        
        table.insert(curvePoints, {centerNodeX, centerNodeY})
    end
    
    CreateBezierCurve(sprites, curvePoints, 100, {0, 0}, 2)
    
    addBlockSprites(sprites, entity, entity.directory .. "block", entity.directory .. "light01", x, y, width, height)

    return sprites
end

function zip.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)
    
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeRectangles = {}
    for _, node in ipairs(nodes) do
        local nodeX, nodeY = node.x, node.y
        local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

        local cogSprite = drawableSprite.fromTexture(entity.directory .. "cog", entity)
        local cogWidth, cogHeight = cogSprite.meta.width, cogSprite.meta.height
        
        local nodeRectangle = utils.rectangle(centerNodeX - math.floor(cogWidth / 2), centerNodeY - math.floor(cogHeight / 2), cogWidth, cogHeight)
        table.insert(nodeRectangles, nodeRectangle)
    end

    local mainRectangle = utils.rectangle(x, y, width, height)
    
    return mainRectangle, nodeRectangles
end

-- 贝塞尔曲线
-- 辅助：统一获取点的 x, y
local function getXY(p)
    if type(p[1]) == "number" then
        return p[1], p[2]
    else
        return p.x, p.y
    end
end

-- 创建贝塞尔曲线采样点的函数
local function evalBezier(points, t)
    local n = #points
    if n == 0 then return {x = 0, y = 0} end
    if n == 1 then
        local x, y = getXY(points[1])
        return {x = x, y = y}
    end

    -- {x, y}
    local temp = {}
    for i = 1, n do
        local x, y = getXY(points[i])
        temp[i] = {x = x, y = y}
    end

    -- de Casteljau计算方法
    for r = 1, n - 1 do
        for i = 1, n - r do
            temp[i].x = (1 - t) * temp[i].x + t * temp[i + 1].x
            temp[i].y = (1 - t) * temp[i].y + t * temp[i + 1].y
        end
    end

    return {x = temp[1].x, y = temp[1].y}
end

-- 主函数：创建并绘制贝塞尔曲线
function CreateBezierCurve(sprite, points, resolution, offset, thickness)
    if not points or #points == 0 then
        error("点集不能为空")
    end
    if not resolution or resolution < 1 then
        resolution = 10  -- 默认值
    end

    -- 解析 offset
    local ox, oy = getXY(offset or {0, 0})

    -- 采样点列表
    local samples = {}
    for i = 0, resolution do
        local t = i / resolution
        local pt = evalBezier(points, t)
        -- 应用偏移
        table.insert(samples, {x = pt.x + ox, y = pt.y + oy})
    end

    -- 绘制线段
    for i = 1, #samples - 1 do
        local p1 = samples[i]
        local p2 = samples[i + 1]
        local p = {p1.x, p1.y, p2.x, p2.y}
        local line = drawableLine.fromPoints(p, ropeColor, thickness)
        
        for _, s in ipairs(line:getDrawableSprite()) do
            table.insert(sprite, s)
        end
    end
end

return zip

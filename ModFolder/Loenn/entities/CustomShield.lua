local drawableLine = require("structs.drawable_line")

local shield = {}

shield.name = "ChroniaHelper/CustomShield"

shield.depth = 0

shield.placements = {
    name = "shield",
    data = {
        x = 0,
        y = 0,
        radius = "10",
        bloomAlpha = "0.5",
        bloomRadius = "20",
        lightFlag = "lightOn",
        lightColor = "ffffff",
        lightAlpha = "1",
        lightStartFade = "16",
        lightEndFade="48",
        color = "ffffff",
        square = false,
    },
}

shield.fieldInformation = {
    lightColor = {
        fieldType = "color",
        useAlpha = true,
    },
    color = {
        fieldType = "color",
        useAlpha = true,
    },
}

shield.sprite = function(room, entity)
    local r = tonumber(entity.radius)
    if r == nil then
        r = 8
    end

    if not entity.square then
        return drawCircle({entity.x, entity.y, entity.x + r, entity.y}, 1)
    else
        return require("structs.drawable_rectangle").fromRectangle("bordered", entity.x - r, entity.y - r, 2 * r, 2 * r, {1,1,1,0.1}, {1,1,1,1})
    end
end

shield.selection = function(room, entity)
    local r = tonumber(entity.radius)
    if r == nil then
        r = 8
    end

    return require("utils").rectangle(entity.x - r, entity.y - r, r * 2, r * 2)
end

-- 定义画圆函数
function getDrawCircle(entity, res)
    local sprites = {}

    local r = tonumber(entity.radius)
    if r == nil then
        r = 8
    end

    -- 计算每个圆周片段的角度大小
    local angleIncrement = (2 * math.pi) / res
    
    -- 初始化前一个点为圆的起点
    local prevPoint = {x = entity.x + r, y = entity.y}
    
    -- 遍历所有圆周点
    for i = 1, res do
        -- 当前角度
        local theta = i * angleIncrement
        
        -- 计算当前点的位置
        local currentPoint = {
            x = entity.x + r * math.cos(theta),
            y = entity.y + r * math.sin(theta)
        }
        
        -- 绘制从上一点到当前点的线段
        table.insert(sprites, drawableLine.fromPoints(prevPoint, currentPoint))
        
        -- 更新前一个点为当前点
        prevPoint = currentPoint
    end
    
    -- 最后还需连接最后一个点和第一个点，以闭合图形
    table.insert(sprites, drawableLine.fromPoints(prevPoint, {x = entity.x + r, y = entity.y}))

    return sprites
end

return shield
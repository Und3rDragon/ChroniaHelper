local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")

local cellSize = 8

-- 板体厚度与实体侧的碰撞盒厚度一致
local thickness = 5

-- 各朝向的专属字段
local sidewaysFields = {"allowClimbing", "allowWallJumping", "letSeekersThrough", "cornerCorrect"}
local downFields = {"squishPlayer", "pushPlayer"}

-- 顺时针旋转目标：上、右、下、左
local rotateTarget = {up = "right", right = "down", down = "left", left = "up"}

-- 贴图所在图集的路径前缀
local texturePrefix = "Graphics/Atlases/Gameplay/objects/jumpthru/"

-- 贴图字段：可浏览图集选择文件，存入时剥去图集前缀与结尾编号，得到可手动填写的材质名
local textureField = {
    fieldType = "path",
    allowEmpty = true,
    allowFiles = true,
    allowFolders = false,
    filenameProcessor = function(filename)
        if filename == nil or filename == "" then
            return ""
        end

        local str = filename:gsub("^%s*(.-)%s*$", "%1")

        local expected = texturePrefix
        local length = #expected

        -- 选中项不在本图集内时，仅去掉扩展名
        if #str < length or str:sub(1, length) ~= expected then
            return str:match("(.+)%.[^%.]+$") or str
        end

        local name = str:sub(length + 1)
        name = name:match("(.+)%.[^%.]+$") or name

        -- 剥去结尾的连续编号
        local base = name:match("^(.-)%d*$")
        return base ~= "" and base or name
    end
}

local fieldInfo = {
    texture = textureField,
    surfaceIndex = {fieldType = "integer"},
    animationDelay = {fieldType = "number"},
    attached = {fieldType = "boolean"},
    pushPlayer = {fieldType = "boolean"},
    squishPlayer = {fieldType = "boolean"},
    allowClimbing = {fieldType = "boolean"},
    allowWallJumping = {fieldType = "boolean"},
    letSeekersThrough = {fieldType = "boolean"},
    cornerCorrect = {fieldType = "boolean"}
}

-- name       实体注册名
-- placement  放置项名称，亦用于旋转与 lang
-- horizontal 是否沿宽度排布
-- animate    是否支持逐格动画
-- extra      该朝向专属字段
local boardOrder = {"up", "right", "down", "left"}

local boardDefinition = {
    up = {
        name = "ChroniaHelper/SampleJumpThroughUp",
        horizontal = true,
        animate = true,
        extra = {}
    },
    right = {
        name = "ChroniaHelper/SampleJumpThroughRight",
        horizontal = false,
        animate = true,
        extra = sidewaysFields
    },
    down = {
        name = "ChroniaHelper/SampleJumpThroughDown",
        horizontal = true,
        animate = true,
        extra = downFields
    },
    left = {
        name = "ChroniaHelper/SampleJumpThroughLeft",
        horizontal = false,
        animate = true,
        extra = sidewaysFields
    }
}

-- 实体名 -> 朝向
local placementByName = {}

for placement, definition in pairs(boardDefinition) do
    placementByName[definition.name] = placement
end

-- 上下朝向持有宽度，左右朝向持有高度
local sizeField = function(horizontal)
    return horizontal and "width" or "height"
end

-- 图集为 24x16，按 8x8 切成 2 行 3 列，六块依次为：
--   A B C
--   D E F
-- 各朝向的板体序列：首格与末格取端块，中间格取 B（可按位置稳定地换成 E）
local atlasBlocks = {
    A = {0, 0},
    B = {1, 0},
    C = {2, 0},
    D = {0, 1},
    E = {1, 1},
    F = {2, 1}
}

-- 各朝向的端块与中间块
local boardBlocks = {
    up = {first = "D", middle = "B", last = "F"},
    down = {first = "D", middle = "B", last = "F"},
    left = {first = "F", middle = "B", last = "D"},
    right = {first = "D", middle = "B", last = "F"}
}

-- 依位置取稳定随机值：同一格每次绘制结果一致，避免贴图逐帧跳动
-- 取三分之一概率，分布较为均匀
local stableHash = function(x, y)
    return (x * x * 31 + x * 17 + y * 7) % 3 == 0 and 1 or 0
end

-- 解析实际可用的贴图名：优先使用原名字；取不到时按编号序列取第一帧
-- 例如 wood 取 wood；woodA 取 woodA00 / woodA0 / woodA000 等首个存在的
local resolveTexturePath = function(name)
    local path = "objects/jumpthru/" .. name

    if atlases.getResource(path, "Gameplay") ~= nil then
        return path
    end

    for digits = 1, 6 do
        local frame = path .. string.format("%0" .. digits .. "d", 0)

        if atlases.getResource(frame, "Gameplay") ~= nil then
            return frame
        end
    end

    return path
end

-- 按朝向取出板体占据的格子数量
local boardCells = function(entity, horizontal)
    local size = horizontal and (entity.width or cellSize) or (entity.height or cellSize)

    return math.max(1, math.floor(size / cellSize))
end

-- 取该格应使用的图集块名
local boardBlock = function(placement, index, cells, x, y)
    local blocks = boardBlocks[placement]

    if blocks == nil then
        return nil
    end

    if index == 0 then
        return blocks.first
    end

    if index == cells - 1 then
        return blocks.last
    end

    -- 中间格：按位置稳定随机决定是否使用 E 变体
    if stableHash(x + index, y) == 1 then
        return "E"
    end

    return blocks.middle
end

-- 依朝向取一格贴图的旋转
local boardRotation = function(placement)
    if placement == "left" then
        return -math.pi / 2
    elseif placement == "right" then
        return math.pi / 2
    end

    return 0
end

local boardSprite = function(room, entity)
    local placement = placementByName[entity._name]
    local definition = placement and boardDefinition[placement]

    if definition == nil then
        return {}
    end

    local horizontal = definition.horizontal
    local cells = boardCells(entity, horizontal)

    -- 未指定贴图时交由实体按所在区域的默认材质绘制
    local texture = entity.texture

    if texture == nil or texture == "" then
        return {}
    end

    local sprites = {}
    local x = entity.x or 0
    local y = entity.y or 0
    local rotation = boardRotation(placement)

    -- 贴图名可能带编号序列：不带编号直接用，带编号取第一帧
    local texturePath = resolveTexturePath(texture)

    for index = 0, cells - 1 do
        local block = boardBlock(placement, index, cells, x, y)

        if block ~= nil then
            local column, row = atlasBlocks[block][1], atlasBlocks[block][2]

            local sprite = drawableSprite.fromTexture(texturePath, entity)
            sprite:setJustification(0, 0)
            sprite:useRelativeQuad(column * cellSize, row * cellSize, cellSize, cellSize)
            sprite:setOffset(cellSize / 2, cellSize / 2)

            if horizontal then
                sprite:setPosition(x + index * cellSize + cellSize / 2, y + cellSize / 2)
            else
                sprite:setPosition(x + cellSize / 2, y + index * cellSize + cellSize / 2)
            end

            sprite.rotation = rotation

            -- 倒置板上下翻转
            if placement == "down" then
                sprite.scaleY = -1
            end

            table.insert(sprites, sprite)
        end
    end

    return sprites
end

-- 选取范围：板体贴靠格子的一侧
local boardSelection = function(entity, horizontal)
    local x = entity.x or 0
    local y = entity.y or 0

    if horizontal then
        return utils.rectangle(x, y, entity.width or cellSize, cellSize)
    end

    return utils.rectangle(x, y, cellSize, entity.height or cellSize)
end

-- 绘制面板
local makeBoard = function(placement)
    local definition = boardDefinition[placement]
    local horizontal = definition.horizontal
    local size = sizeField(horizontal)

    -- 面板数据：列出该朝向适用的全部字段并给出默认值
    local data = {
        [size] = cellSize,
        texture = "wood",
        surfaceIndex = 8,
        attached = false
    }

    if definition.animate then
        data.animationDelay = 0
    end

    for _, field in ipairs(definition.extra) do
        -- 攀附相关的开关默认开放，其余默认关闭
        data[field] = field == "allowClimbing" or field == "allowWallJumping"
    end

    -- 字段顺序：尺寸、通用字段、专属字段
    local order = {"x", "y", size, "texture", "surfaceIndex"}

    if definition.animate then
        table.insert(order, "animationDelay")
    end

    for _, field in ipairs(definition.extra) do
        table.insert(order, field)
    end

    table.insert(order, "attached")

    -- 字段信息：只列出该朝向适用的字段
    local fieldInformation = {}

    for _, field in ipairs(order) do
        if fieldInfo[field] ~= nil then
            fieldInformation[field] = fieldInfo[field]
        end
    end

    -- 不显示的字段：另一轴的尺寸，以及其它朝向的专属字段
    local ignored = {"_id", "_name", sizeField(not horizontal)}

    if not definition.animate then
        table.insert(ignored, "animationDelay")
    end

    if placement ~= "down" then
        table.insert(ignored, "squishPlayer")
        table.insert(ignored, "pushPlayer")
    end

    if placement == "up" or placement == "down" then
        for _, field in ipairs(sidewaysFields) do
            table.insert(ignored, field)
        end
    end

    local board = {
        name = definition.name,
        placements = {
            name = placement,
            data = data
        },
        fieldInformation = fieldInformation,
        fieldOrder = order,
        ignoredFields = ignored,
        sprite = function(room, entity)
            return boardSprite(room, entity)
        end,
        selection = function(room, entity)
            return boardSelection(entity, horizontal)
        end
    }

    -- 旋转：切到顺时针的下一个朝向，换轴时尺寸字段互换
    board.rotate = function(room, entity, rotationDirection)
        local current = placementByName[entity._name]

        if current == nil then
            return false
        end

        local target = rotateTarget[current]

        if target == nil or target == current then
            return false
        end

        entity._name = boardDefinition[target].name

        if horizontal ~= boardDefinition[target].horizontal then
            entity.width, entity.height = entity.height, entity.width
        end

        return true
    end

    if horizontal then
        board.canResize = {true, false}
    else
        board.canResize = {false, true}
    end

    return board
end

local boards = {}

for _, placement in ipairs(boardOrder) do
    table.insert(boards, makeBoard(placement))
end

return boards

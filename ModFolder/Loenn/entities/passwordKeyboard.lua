local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")

local entity = {}

entity.name = "ChroniaHelper/PasswordKeyboard"

--entity.justification = { 0.5, 1.0 }
entity.placements = {
    name = "normal",
    data = {
        --width = 16,
        --height = 16,

        mode = 0,
        flagToEnable = "",
        password = "",
        --rightDialog = "rightDialog",
        --wrongDialog = "wrongDialog",
        caseSensitive = false,
        useTimes = -1,
        accessZone = "-16,0,32,8",
        accessZoneIndicator = false,
        --globalFlag = false,
    }
}

entity.fieldInformation = {
    mode = {
        fieldType = "integer",
        options = {
            ["Exclusive"] = 0,
            ["Normal"] = 1,
            ["OutputFlag"] = 2
        },
        editable = false
    },
    useTimes = {
        fieldType = "integer"
    },
    accessZone = {
        fieldType = "list",
        elementOptions = {
            fieldType = "integer",
        },
    }
}

entity.sprite = function(room, entity)
    local defaultTexture = "PasswordKeyboard/keyboard"
    local sprites = {}

    sprite = drawableSprite.fromTexture(defaultTexture, entity)
    table.insert(sprites, sprite)

    local str = entity.accessZone -- 待分割的字符串
    local parameters = {} -- 用于存储分割后的参数
    local start = 1 -- 子串的起始位置

    -- 循环查找逗号，分割字符串
    while true do
        local commaPos = string.find(str, ",", start) -- 查找下一个逗号的位置
        if commaPos == nil then -- 如果没有找到逗号，说明是最后一个参数
            table.insert(parameters, string.sub(str, start)) -- 添加最后一个参数
            break -- 退出循环
        else
            table.insert(parameters, string.sub(str, start, commaPos - 1)) -- 添加找到的参数
            start = commaPos + 1 -- 更新起始位置到下一个参数的开始
        end
    end

    -- 设定一个矩形标志区域
    local rectangle = drawableRectangle.fromRectangle("bordered",entity.x + tonumber(parameters[1]),entity.y + tonumber(parameters[2]),tonumber(parameters[3]),tonumber(parameters[4]),{0,0,0,0.01},{255,255,255,1})

    if entity.accessZoneIndicator then
        table.insert(sprites, rectangle)
    end
    
    return sprites
end

return entity

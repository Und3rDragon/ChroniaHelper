local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")

local entity = {}

entity.name = "ChroniaHelper/PasswordKeyboard"
entity.depth = function(room,entity) return entity.depth or 9000 end
--entity.justification = { 0.5, 1.0 }
entity.placements = {
    name = "normal",
    data = {
        --width = 16,
        --height = 16,
        texture = "ChroniaHelper/PasswordKeyboard/keyboard",
        tag = "passwordKeyboard",
        mode = 1,
        flagToEnable = "",
        password = "",
        characterLimit = 12,
        --rightDialog = "rightDialog",
        --wrongDialog = "wrongDialog",
        caseSensitive = true,
        useTimes = -1,
        accessZone = "-16,0,32,8",
        accessZoneIndicator = false,
        talkIconPosition = "0,-8",
        depth = 9000,
        globalFlag = false,
        toggleFlag = false,
        passwordEncrypted = false,
        showEncryptedPasswordInConsole = false,
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
        minimumElements = 4,
        maximumElements = 4,
        elementOptions = {
            fieldType = "integer",
        },
    },
    tag = {
        allowEmpty = false,
    },
    texture = {
        allowEmpty = false,
    },
    talkIconPosition = {
        fieldType = "list",
        minimumElements = 2,
        maximumElements = 2
    },
    depth = require("mods").requireFromPlugin("helpers.field_options").depths,
    characterLimit = {
        minimumValue = 1,
        fieldType = "integer",
    }
}

entity.sprite = function(room, entity)
    --local defaultTexture = "ChroniaHelper/PasswordKeyboard/keyboard"
    local defaultTexture = entity.texture
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

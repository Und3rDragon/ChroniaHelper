local tilesStruct = require("structs.tiles")
local utils = require("utils")

local script = {
    name = "replaceAdjacentTiles",
    displayName = "Replace Adjacent Tiles",
    parameters = {
        layer = "Fg",
        receiver = "R",
        applyer = "A", 
        replacer = "X",
    },
    fieldInformation = {
        layer = {
            fieldType = "loennScripts.dropdown",
            options = {
                "Fg", "Bg"
            }
        }
    },
    fieldOrder = { "receiver", "applyer", "replacer" },
    tooltip = "Replaces receiver tiles that are adjacent to applyer tiles with replacer tiles",
    tooltips = {
        layer = "The layer to replace tiles in",
        receiver = "The tile type that will be replaced (must be adjacent to applyer)",
        applyer = "The tile type that triggers the replacement",
        replacer = "The tile type that will replace the receiver",
    },
}

-- UTF-8字符处理函数
local function utf8Chars(str)
    local chars = {}
    local i = 1
    while i <= #str do
        local byte = str:byte(i)
        local charLen = 1
        
        -- 检测UTF-8字符长度
        if byte >= 0xF0 then
            charLen = 4
        elseif byte >= 0xE0 then
            charLen = 3
        elseif byte >= 0xC0 then
            charLen = 2
        end
        
        table.insert(chars, str:sub(i, i + charLen - 1))
        i = i + charLen
    end
    return chars
end

-- 在字符串中查找UTF-8字符位置
local function findUtf8CharPositions(str, targetChar)
    local positions = {}
    local chars = utf8Chars(str)
    
    for i, char in ipairs(chars) do
        if char == targetChar then
            table.insert(positions, i)
        end
    end
    
    return positions, chars
end

-- 替换字符串中指定位置的UTF-8字符
local function replaceUtf8CharAt(str, position, newChar)
    local chars = utf8Chars(str)
    if position >= 1 and position <= #chars then
        chars[position] = newChar
        return table.concat(chars)
    end
    return str
end

local function encodeString(str)
    if _G.encodeString then
        return _G.encodeString(str)
    else
        return str
    end
end

function script.run(room, args)
    local receiver = args.receiver or "R"
    local applyer = args.applyer or "A"
    local replacer = args.replacer or "X"
    local propertyName = "tiles" .. args.layer
    
    local currentTiles = room[propertyName]
    if not currentTiles then
        return
    end
    
    local matrix = currentTiles.matrix
    if not matrix then
        return
    end
    
    local tileString = tilesStruct.matrixToTileString(matrix)
    
    local lines = {}
    for line in tileString:gmatch("[^\r\n]+") do
        table.insert(lines, line)
    end
    
    local rows = #lines
    if rows == 0 then return end
    
    -- 预处理每行的UTF-8字符
    local lineChars = {}
    for row = 1, rows do
        lineChars[row] = utf8Chars(lines[row])
    end
    
    local cols = #lineChars[1]
    
    local resultLines = utils.deepcopy(lines)
    local replacedCount = 0
    
    for row = 1, rows do
        -- 查找当前行中所有的receiver位置
        local receiverPositions = findUtf8CharPositions(lines[row], receiver)
        
        for _, col in ipairs(receiverPositions) do
            local hasAdjacent = false
            
            -- 检查四个方向的邻居
            local directions = {
                {row-1, col},  -- 上
                {row+1, col},  -- 下
                {row, col-1},  -- 左  
                {row, col+1}   -- 右
            }
            
            for _, dir in ipairs(directions) do
                local r, c = dir[1], dir[2]
                if r >= 1 and r <= rows and c >= 1 and c <= #lineChars[r] then
                    local neighborChar = lineChars[r][c]
                    if neighborChar == applyer then
                        hasAdjacent = true
                        break
                    end
                end
            end
            
            -- 如果有相邻的应用者，进行替换
            if hasAdjacent then
                resultLines[row] = replaceUtf8CharAt(resultLines[row], col, replacer)
                replacedCount = replacedCount + 1
            end
        end
    end
    
    -- 更新房间数据
    if replacedCount > 0 then
        local resultString = table.concat(resultLines, "\n")
        local encodedResult = encodeString(resultString)
        room[propertyName] = tilesStruct.decode({innerText = encodedResult})
    end
end

return script

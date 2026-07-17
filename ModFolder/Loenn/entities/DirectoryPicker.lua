local picker = {}

picker.name = "ChroniaHelper/DirectoryPicker"

picker.placements = {
    name = "Directory Picker",
    data = {
        ignoreBaseDirectory = "Graphics/Atlases/Gameplay/",
        pickFileHere = "",
        ignoreNumbers = false,
        excludeSuffix = true,
    },
}

picker.fieldInformation = function(entity)
    local orig = {}
        
    orig["ignoreBaseDirectory"] = {
        options = {
            ["Root Folder"] = "",
            ["Gameplay"] = "Graphics/Atlases/Gameplay/",
            ["Gui"] = "Graphics/Atlases/Gui/",
            ["Chapter Icons"] = "Graphics/Atlases/Gui/areas/",
            ["Decals"] = "Graphics/Atlases/Gameplay/decals/",
            ["Objects"] = "Graphics/Atlases/Gameplay/objects/",
            ["Danger"] = "Graphics/Atlases/Gameplay/danger/",
            ["Jump Throughs"] = "Graphics/Atlases/Gameplay/objects/jumpthru/",
            ["Maddie's Switch Gates"] = "Graphics/Atlases/Gameplay/objects/switchgate/",
            ["Crumble Blocks"] = "Graphics/Atlases/Gameplay/objects/crumbleBlock/",
        },
        editable = true,
    }

    orig["pickFileHere"] = picker.versatilePath(entity.ignoreBaseDirectory, entity.ignoreNumbers, entity.excludeSuffix, true)
    
    return orig
end

function picker.sprite(room, entity)
    local sprite = {}
    
    local iconSprite = require("structs.drawable_sprite").fromTexture("ChroniaHelper/LoennIcons/Folder", entity)
    
    table.insert(sprite, iconSprite)
    
    return sprite
end

-- ============================================================
-- 统一路径处理函数
-- ============================================================
-- 参数说明：
--   trimPath      : 需要移除的路径前缀，如 "Graphics/Atlases/Gameplay/"
--   ignoreNumbers : 是否忽略结尾的数字（如 xxx00.png → xxx.png）
--   ignoreSuffix  : 是否忽略扩展名（如 xxx00.png → xxx00）
--   empty         : 是否允许空字符串
-- ============================================================
function picker.versatilePath(trimPath, ignoreNumbers, ignoreSuffix, empty)
    return {
        fieldType = "path",
        allowEmpty = not not empty,
        allowFiles = true,
        allowFolders = true,
        filenameProcessor = function(filename, rawFilename, prefix)
            -- 1. 基础清理
            local str = (filename or "")
            str = str:gsub("^%s+", ""):gsub("%s+$", "")
            
            if str == "" then
                return empty and "" or nil
            end

            -- 2. 移除路径前缀
            local path = str
            if trimPath and trimPath ~= "" then
                -- 特殊处理：如果 trimPath 是 "/" 或 "\"，只移除开头的斜杠
                if trimPath == "/" or trimPath == "\\" then
                    if str:sub(1, 1) == trimPath then
                        path = str:sub(2)
                    end
                else
                    -- 正常处理：确保 trimPath 以 / 结尾
                    local trimmed = trimPath
                    if trimmed:sub(-1) ~= "/" then
                        trimmed = trimmed .. "/"
                    end
                    
                    -- 方法1: 直接匹配完整前缀
                    local prefixLen = #trimmed
                    if #str >= prefixLen and str:sub(1, prefixLen) == trimmed then
                        path = str:sub(prefixLen + 1)
                    else
                        -- 方法2: 从后往前匹配最后一部分路径
                        local lastPart = trimPath:match("([^/]+)/?$")
                        if lastPart then
                            local searchStr = lastPart .. "/"
                            local pos = str:find(searchStr, 1, true)
                            if pos then
                                if pos == 1 or str:sub(pos - 1, pos - 1) == "/" then
                                    path = str:sub(pos + #searchStr)
                                end
                            end
                        end
                        
                        -- 方法3: 如果还是没匹配到，尝试从 trimPath 中提取所有路径段
                        if path == str and trimPath:match("^.+/") then
                            local segments = {}
                            for segment in (trimPath:gmatch("([^/]+)/")) do
                                table.insert(segments, segment)
                            end
                            
                            for i = #segments, 1, -1 do
                                local searchStr = segments[i] .. "/"
                                local pos = str:find(searchStr, 1, true)
                                if pos then
                                    if pos == 1 or str:sub(pos - 1, pos - 1) == "/" then
                                        path = str:sub(pos + #searchStr)
                                        break
                                    end
                                end
                            end
                        end
                    end
                end
            end
            
            -- 如果 path 为空且不允许空，返回原始路径
            if path == "" and not empty then
                return str
            end
            
            if path == "" then
                return ""
            end

            -- 3. 判断是文件还是文件夹
            if path:sub(-1) == "/" then
                return path:sub(1, -2)
            end

            -- 4. 判断是否有扩展名
            local hasExtension = path:match("%.[^/%.]+$") ~= nil
            
            -- 5. 提取文件名和扩展名
            local name = path
            local ext = ""
            
            if hasExtension then
                ext = path:match("%.([^%.]+)$") or ""
                name = path:match("(.+)%.[^%.]+$") or path
            end
            
            -- 6. 移除尾部数字
            if ignoreNumbers then
                local lastSlash = name:match("^(.*)/")
                if lastSlash then
                    local fileName = name:sub(#lastSlash + 2)
                    local baseName = fileName:gsub("%d+$", "")
                    name = lastSlash .. "/" .. baseName
                else
                    name = name:gsub("%d+$", "")
                end
            end

            -- 7. 根据 ignoreSuffix 决定返回格式
            if ignoreSuffix then
                return name
            else
                if ext ~= "" then
                    return name .. "." .. ext
                else
                    return name
                end
            end
        end
    }
end

return picker
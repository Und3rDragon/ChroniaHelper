local picker = {}

picker.name = "ChroniaHelper/DirectoryPickerAuto"

picker.placements = {
    name = "Directory Picker (Auto Detect)",
    data = {
        pickFileHere = ""
    },
}

picker.fieldInformation = function(entity)
    local orig = {}
        
    orig["pickFileHere"] = picker.versatilePath(true)
    
    return orig
end

function picker.sprite(room, entity)
    local sprite = {}
    
    local iconSprite = require("structs.drawable_sprite").fromTexture("ChroniaHelper/LoennIcons/Folder", entity)
    
    table.insert(sprite, iconSprite)
    
    return sprite
end

-- ============================================================
-- 路径处理函数 - 根据文件路径自动判断处理方式
-- ============================================================
-- 参数说明：
--   empty : 是否允许空字符串
-- ============================================================
function picker.versatilePath(empty)
    -- 配置规则：按顺序匹配，第一个匹配的生效
    local rules = {
        -- {
        --     pattern = "^Graphics/Atlases/Gameplay/",   -- 路径匹配模式
        --     trim = "Graphics/Atlases/Gameplay/",       -- 要移除的前缀
        --     ignoreNumbers = true,                      -- 是否移除尾部数字
        --     ignoreSuffix = false,                      -- 是否移除扩展名
        -- },
        -- {
        --     pattern = "^Graphics/Atlases/Overworld/",
        --     trim = "Graphics/Atlases/Overworld/",
        --     ignoreNumbers = false,
        --     ignoreSuffix = true,
        -- },
        {
            pattern = "^Graphics/Atlases/Gameplay/decals/",
            trim = "Graphics/Atlases/Gameplay/decals/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/objects/jumpthru/",
            trim = "Graphics/Atlases/Gameplay/objects/jumpthru/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/objects/switchgate/",
            trim = "Graphics/Atlases/Gameplay/objects/switchgate/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/objects/crumbleBlock/",
            trim = "Graphics/Atlases/Gameplay/objects/crumbleBlock/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/objects/",
            trim = "Graphics/Atlases/Gameplay/objects/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/danger/",
            trim = "Graphics/Atlases/Gameplay/danger/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/bgs/",
            trim = "Graphics/Atlases/Gameplay/bgs/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/tilesets/",
            trim = "Graphics/Atlases/Gameplay/tilesets/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gameplay/",
            trim = "Graphics/Atlases/Gameplay/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gui/areas/",
            trim = "Graphics/Atlases/Gui/areas/",
            ignoreNumbers = true,
            ignoreSuffix = true,
        },
        {
            pattern = "^Graphics/Atlases/Gui/",
            trim = "Graphics/Atlases/Gui/",
            ignoreNumbers = false,
            ignoreSuffix = true,
        },
        -- 默认规则
        { 
            pattern = "^",
            trim = "",
            ignoreNumbers = false,
            ignoreSuffix = false
        },
    }

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

            -- 2. 根据路径匹配规则
            local matchedRule = nil
            for _, rule in ipairs(rules) do
                if str:find(rule.pattern) then
                    matchedRule = rule
                    break
                end
            end

            -- 如果匹配到规则，使用规则的参数；否则使用默认值
            local trimPath = matchedRule and matchedRule.trim or ""
            local ignoreNumbers = matchedRule and matchedRule.ignoreNumbers or false
            local ignoreSuffix = matchedRule and matchedRule.ignoreSuffix or true

            -- 3. 移除路径前缀
            local path = str
            if trimPath and trimPath ~= "" then
                local trimmed = trimPath
                if trimmed:sub(-1) ~= "/" then
                    trimmed = trimmed .. "/"
                end
                
                local prefixLen = #trimmed
                if #str >= prefixLen and str:sub(1, prefixLen) == trimmed then
                    path = str:sub(prefixLen + 1)
                else
                    -- 如果直接匹配失败，尝试匹配最后一部分
                    local lastPart = trimPath:match("([^/]+)/?$")
                    if lastPart then
                        local searchStr = lastPart .. "/"
                        local pos = str:find(searchStr, 1, true)
                        if pos and (pos == 1 or str:sub(pos - 1, pos - 1) == "/") then
                            path = str:sub(pos + #searchStr)
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

            -- 4. 判断是文件还是文件夹
            if path:sub(-1) == "/" then
                return path:sub(1, -2)
            end

            -- 5. 提取扩展名
            local hasExtension = path:match("%.[^/%.]+$") ~= nil
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
                    name = lastSlash .. "/" .. fileName:gsub("%d+$", "")
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
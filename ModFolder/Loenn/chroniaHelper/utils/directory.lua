local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")

--- local directory = require("mods").requireFromPlugin("chroniaHelper.utils.directory")
local directory = {}

--- Returns a sprite by appending zero-padded index to the base path.
--- Tries: base0, base00, base000, ..., up to 5 digits.
--- If idx is negative or not a number, treats as 0 (or you can error)
--- Example Usage: 
--- fetchSpritefromPath("object/booster/idle", 0, entity)
function directory.fetchSprite(basePath, idx, data, _atlas)
    -- Normalize idx: ensure it's a non-negative integer
    if type(idx) ~= "number" or idx ~= math.floor(idx) then
        idx = 0  -- or error("Index must be an integer")
    elseif idx < 0 then
        idx = 0  -- or handle negative as special case if needed
    end

    local atlas = _atlas or data.atlas or "Gameplay"
    local val = nil

    -- Try from 1-digit to 5-digit zero-padding (as per your spec)
    for digits = 1, 5 do
        local padded = string.format("%0" .. digits .. "d", idx)
        val = require('atlases').getResource(basePath .. padded, atlas)
        if val then
            break
        end
    end

    if val then
        return drawableSprite.fromMeta(val, data)
    else
        -- Fallback to missing image
        local missingPath = require('mods').internalModContent .. "/missing_image"
        local missingRes = require('atlases').getResource(missingPath, atlas)
        return drawableSprite.fromMeta(missingRes, data)
    end
end

--- Processes a full atlas file path into a cleaned base path.
-- @param allowEmpty boolean: whether to allow empty result
-- @param keepSuffixNumbers boolean: if true, keeps trailing digits (e.g. "idle03"); if false, strips them (e.g. "idle")
-- @param atlasName string: name of the atlas (default: "Gameplay")
-- @return table: field definition for Everest entity editor
-- Example:
-- fetchPath(true, true)
function directory.fetchPath(allowEmpty, keepSuffixNumbers, atlasName)
    local atlas = atlasName or "Gameplay"
    keepSuffixNumbers = keepSuffixNumbers == true  -- normalize to boolean (false if nil/false)

    return {
        fieldType = "path",
        allowEmpty = allowEmpty == true,
        allowFiles = true,
        allowFolders = false,
        filenameProcessor = function(filename, rawFilename, prefix)
            if not filename or filename == "" then
                return allowEmpty and "" or nil
            end

            local str = directory.trim(filename)

            -- Build expected prefix: "Graphics/Atlases/<atlas>/"
            local expectedPrefix = "Graphics/Atlases/" .. atlas .. "/"
            local prefixLen = #expectedPrefix

            -- Safety check: ensure path starts with expected prefix
            if #str < prefixLen or str:sub(1, prefixLen) ~= expectedPrefix then
                -- Fallback: process as raw filename (remove extension only)
                local fallback = str:match("(.+)%.[^%.]+$") or str
                if keepSuffixNumbers then
                    return fallback
                else
                    local stripped = fallback:match("^(.-)%d*$")
                    return stripped ~= "" and stripped or fallback
                end
            end

            -- Extract relative part after prefix
            local relativePath = str:sub(prefixLen + 1)

            -- Remove file extension (e.g., .png)
            local nameWithoutExt = relativePath:match("(.+)%.[^%.]+$") or relativePath

            -- Optionally strip trailing digits
            if keepSuffixNumbers then
                return nameWithoutExt
            else
                local base = nameWithoutExt:match("^(.-)%d*$")
                return base ~= "" and base or nameWithoutExt
            end
        end
    }
end

-- Helper: trims leading/trailing whitespace
function directory.trim(s)
    if not s then return "" end
    return (s:gsub("^%s*(.-)%s*$", "%1"))
end

-- Example:
-- fetchDirectory(true)
function directory.fetchDirectory(allowEmpty, atlasName)
    local atlas = atlasName or "Gameplay"
    return {
        fieldType = "path",
        allowEmpty = allowEmpty == true,
        allowFiles = true,
        allowFolders = false,
        filenameProcessor = function(filename, rawFilename, prefix)
            if not filename or filename == "" then
                return allowEmpty and "" or nil
            end

            local str = directory.trim(filename)

            local expectedPrefix = "Graphics/Atlases/" .. atlas .. "/"
            local prefixLen = #expectedPrefix

            if #str < prefixLen or str:sub(1, prefixLen) ~= expectedPrefix then
                -- Fallback: try to extract dir from raw path
                local lastSlash = #str
                for i = #str, 1, -1 do
                    if str:sub(i, i) == "/" then
                        lastSlash = i - 1
                        break
                    end
                end
                local dir = str:sub(1, lastSlash)
                return dir ~= "" and dir .. "/" or ""
            end

            -- Extract relative path (e.g., "object/idle00.png")
            local relative = str:sub(prefixLen + 1)

            -- Find last slash in relative path
            local lastSlash = nil
            for i = #relative, 1, -1 do
                if relative:sub(i, i) == "/" then
                    lastSlash = i
                    break
                end
            end

            if lastSlash then
                -- Return directory with trailing slash: "object/idle/"
                return relative:sub(1, lastSlash)
            else
                -- File is in root of atlas (e.g., "idle00.png")
                return "/"  -- or "/" if you really want, but "" is standard
            end
        end
    }
end

return directory
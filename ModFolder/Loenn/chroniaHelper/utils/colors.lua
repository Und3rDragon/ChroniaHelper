local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")

local colors = {}

--- Converts a hex color string to a {r, g, b, a} table with float values in [0, 1].
-- Supports:
--   3-digit: "rgb" or "#rgb"          ¡ú rrggbb, a=1
--   6-digit: "rrggbb" or "#rrggbb"    ¡ú rrggbb, a=1
--   8-digit: "rrggbbaa" or "#rrggbbaa" ¡ú if leadingAlpha=false (default)
--            "aabbggrr" or "#aabbggrr" ¡ú if leadingAlpha=true
-- @param hex string: hex color string
-- @param leadingAlpha boolean: if true, 8-digit format is AABBGGRR; else RRGGBBAA (default: false)
-- @return table: {r=..., g=..., b=..., a=...} with values in [0.0, 1.0]
function colors.hexToColor(hex, leadingAlpha)
    -- Normalize parameters
    if type(hex) ~= "string" then
        return {r = 0.0, g = 0.0, b = 0.0, a = 1.0}
    end
    leadingAlpha = leadingAlpha == true  -- default: false

    -- Remove optional leading '#'
    local clean = hex:match("^#?(%x+)$")
    if not clean then
        return {r = 0.0, g = 0.0, b = 0.0, a = 1.0}
    end

    local len = #clean
    local r, g, b, a = 0, 0, 0, 255  -- default alpha = 255 (opaque)

    if len == 3 then
        -- "rgb" -> "rrggbb"
        r = tonumber(clean:sub(1, 1):rep(2), 16)
        g = tonumber(clean:sub(2, 2):rep(2), 16)
        b = tonumber(clean:sub(3, 3):rep(2), 16)
        -- a remains 255

    elseif len == 6 then
        -- "rrggbb"
        r = tonumber(clean:sub(1, 2), 16)
        g = tonumber(clean:sub(3, 4), 16)
        b = tonumber(clean:sub(5, 6), 16)
        -- a remains 255

    elseif len == 8 then
        if leadingAlpha then
            -- Format: AABBGGRR
            a = tonumber(clean:sub(1, 2), 16)
            b = tonumber(clean:sub(3, 4), 16)
            g = tonumber(clean:sub(5, 6), 16)
            r = tonumber(clean:sub(7, 8), 16)
        else
            -- Format: RRGGBBAA (default)
            r = tonumber(clean:sub(1, 2), 16)
            g = tonumber(clean:sub(3, 4), 16)
            b = tonumber(clean:sub(5, 6), 16)
            a = tonumber(clean:sub(7, 8), 16)
        end

    else
        -- Invalid length (not 3, 6, or 8)
        return {r = 0.0, g = 0.0, b = 0.0, a = 1.0}
    end

    -- Clamp to valid byte range (defensive, though tonumber should be safe)
    r = math.min(255, math.max(0, r or 0))
    g = math.min(255, math.max(0, g or 0))
    b = math.min(255, math.max(0, b or 0))
    a = math.min(255, math.max(0, a or 255))

    -- Convert to [0.0, 1.0] floats
    return {
        r = r / 255.0,
        g = g / 255.0,
        b = b / 255.0,
        a = a / 255.0
    }
end

--- Converts a {r, g, b[, a]} color table to a hex string.
-- Input values should be in [0.0, 1.0] range (clamped automatically).
-- Outputs 6-digit hex if alpha is 1.0 (or missing), 8-digit hex otherwise.
-- @param color table: {r=..., g=..., b=..., [a=...]} with values in [0, 1]
-- @param leadSymbol boolean: if true, prepend '#'; otherwise return plain hex
-- @return string: hex color string (6 or 8 digits, with or without '#')
function colors.colorToHex(color, leadSymbol)
    -- Validate input
    if type(color) ~= "table" then
        color = {r = 0.0, g = 0.0, b = 0.0, a = 1.0}
    end

    -- Extract and clamp components to [0, 1]
    local r = math.min(1.0, math.max(0.0, color.r or 0.0))
    local g = math.min(1.0, math.max(0.0, color.g or 0.0))
    local b = math.min(1.0, math.max(0.0, color.b or 0.0))
    local a = math.min(1.0, math.max(0.0, color.a or 1.0))

    -- Convert to 0-255 integers
    local ri = math.floor(r * 255 + 0.5)
    local gi = math.floor(g * 255 + 0.5)
    local bi = math.floor(b * 255 + 0.5)
    local ai = math.floor(a * 255 + 0.5)

    local hexStr
    if ai == 255 then
        -- Opaque: output 6-digit RRGGBB
        hexStr = string.format("%02x%02x%02x", ri, gi, bi)
    else
        -- Transparent: output 8-digit RRGGBBAA
        hexStr = string.format("%02x%02x%02x%02x", ri, gi, bi, ai)
    end

    -- Add leading '#' if requested
    if leadSymbol then
        return "#" .. hexStr
    else
        return hexStr
    end
end

return colors
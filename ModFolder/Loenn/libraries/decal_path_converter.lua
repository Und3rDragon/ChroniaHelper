-- Converts pasted image file paths into decal entries.

local decalBuilder = {}

-- Path segment every decal texture sits behind. Everything before it is mod
-- specific, so only what follows is written into the entity.
local ATLAS_MARKER = "Graphics/Atlases/Gameplay/decals/"

-- Defaults written into every generated decal.
local DECAL_TEMPLATE = {
    _editorLayer = 0,
    color = "ffffff",
    rotation = 0,
    scaleX = 1,
    scaleY = 1,
}

-- Layer each decal is registered under, keyed by the editor layer that owns it.
local DEFAULT_LAYER = "decalsBg"

local DECAL_KEYS = {
    "_editorLayer",
    "_fromLayer",
    "color",
    "rotation",
    "scaleX",
    "scaleY",
    "texture",
    "x",
    "y",
}

local function trim(value)
    return (value:gsub("^%s+", ""):gsub("%s+$", ""))
end


--[[
    Strips the surrounding quotes Windows adds when copying a file path, then
    normalises the separators so the result can be matched against the atlas
    marker regardless of which slash style it used.
]]
local function normalizePath(raw)
    local path = trim(raw or "")

    if #path >= 2 and path:sub(1, 1) == '"' and path:sub(-1) == '"' then
        path = path:sub(2, #path - 1)
    end

    return (path:gsub("\\", "/"))
end


--[[
    Turns an absolute image path into the texture name Lönn uses for atlases.

    Everything from the decals folder onwards is kept, the extension is dropped
    and a trailing run of digits is removed, so every animation frame resolves to
    the same base name.

    For example, "E:/.../Gameplay/decals/a/b00.png" becomes "a/b".
]]
local function pathToTexture(path)
    local markerStart = path:find(ATLAS_MARKER, 1, true)

    if not markerStart then
        return nil
    end

    -- The decal texture name is relative to the decals folder, so the entity
    -- reads it back from there without repeating the folder itself.
    local texture = "decals/" .. path:sub(markerStart + #ATLAS_MARKER)

    if texture:lower():sub(-4) == ".png" then
        texture = texture:sub(1, -5)
    end

    -- Drop the trailing digits, which mark an animation frame
    texture = texture:gsub("%d+$", "")

    if texture == "decals/" then
        return nil
    end

    return texture
end


--[[
    Splits a multi line clipboard payload into individual paths.
    Empty lines are ignored.
]]
local function splitLines(text)
    local lines = {}

    for line in (text .. "\n"):gmatch("(.-)\n") do
        if trim(line) ~= "" then
            table.insert(lines, line)
        end
    end

    return lines
end


local function escapeString(value)
    return (value:gsub("\\", "\\\\"):gsub('"', '\\"'))
end


local function formatNumber(value)
    if value == math.floor(value) then
        return string.format("%d", value)
    end

    return tostring(value)
end


--[[
    Serializes a single decal into the table syntax Lönn accepts on paste.
]]
local function serializeDecal(decal)
    local parts = {}

    for _, key in ipairs(DECAL_KEYS) do
        local value = decal[key]

        if type(value) == "string" then
            table.insert(parts, string.format('%s = "%s"', key, escapeString(value)))

        else
            table.insert(parts, string.format("%s = %s", key, formatNumber(value)))
        end
    end

    return "        " .. table.concat(parts, ",\n        ")
end


local function serializeDecals(decals)
    local blocks = {}

    for _, decal in ipairs(decals) do
        table.insert(blocks, "    {\n" .. serializeDecal(decal) .. "\n    }")
    end

    return "{\n" .. table.concat(blocks, ",\n") .. "\n}"
end


--[[
    Picks the layer a generated decal should live on.

    Only the foreground decals layer produces a foreground decal; every other
    editor layer falls back to the background decals layer.
]]
local function resolveLayer(editorLayer)
    if editorLayer == "decalsFg" then
        return "decalsFg"
    end

    return DEFAULT_LAYER
end


--[[
    Builds decal entries from clipboard text.

    Paths that resolve to the same texture are collapsed into a single entry, so
    animation frames such as "timerA0" and "timerA1" count as one decal.

    The supplied editor layer decides whether the result lands on the foreground
    or background decals layer.

    Returns the serialized table text and the number of entries, or nil when the
    text does not look like one or more atlas image paths.
]]
function decalBuilder.buildFromClipboardText(text, editorLayer)
    if type(text) ~= "string" or trim(text) == "" then
        return nil, 0
    end

    local layer = resolveLayer(editorLayer)
    local decals = {}
    local seen = {}

    for _, line in ipairs(splitLines(text)) do
        local texture = pathToTexture(normalizePath(line))

        if not texture then
            return nil, 0
        end

        if not seen[texture] then
            seen[texture] = true

            local decal = {}

            for key, value in pairs(DECAL_TEMPLATE) do
                decal[key] = value
            end

            decal._fromLayer = layer
            decal.texture = texture
            decal.x = 0
            decal.y = 0

            table.insert(decals, decal)
        end
    end

    if #decals == 0 then
        return nil, 0
    end

    return serializeDecals(decals), #decals
end


return decalBuilder

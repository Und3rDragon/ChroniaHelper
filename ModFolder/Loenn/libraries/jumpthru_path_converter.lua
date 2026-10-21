-- Converts pasted jump through texture paths into vanilla jump through entries.

local jumpThruBuilder = {}

-- Folder the vanilla jump through textures live in. Everything before it is
-- mod specific, so only what follows is written into the entity.
local JUMPTHRU_FOLDER = "Graphics/Atlases/Gameplay/objects/jumpthru/"

-- Default field values, mirroring the vanilla entity.
local JUMPTHRU_TEMPLATE = {
    _editorLayer = 0,
    _fromLayer = "entities",
    _name = "jumpThru",
    surfaceIndex = -1,
    width = 16,
}

-- Field order used when writing the table, so the output stays readable.
local JUMPTHRU_KEYS = {
    "_editorLayer",
    "_fromLayer",
    "_id",
    "_name",
    "surfaceIndex",
    "texture",
    "width",
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
    Turns an absolute texture path into the texture name the vanilla entity reads.

    Everything from the jump through folder onwards is kept, the extension is
    dropped and a trailing run of digits is removed, so nested folders survive
    while every animation frame resolves to the same base name.

    For example, "objects/jumpthru/wood.png" becomes "wood", and
    "objects/jumpthru/sub/xxx00.png" becomes "sub/xxx".
]]
local function pathToTexture(path)
    local folderStart = path:find(JUMPTHRU_FOLDER, 1, true)

    if not folderStart then
        return nil
    end

    local texture = path:sub(folderStart + #JUMPTHRU_FOLDER)

    if texture:lower():sub(-4) == ".png" then
        texture = texture:sub(1, -5)
    end

    -- Drop the trailing digits, which mark an animation frame
    texture = texture:gsub("%d+$", "")

    if texture == "" then
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


local function formatValue(value)
    if type(value) == "string" then
        return string.format('"%s"', escapeString(value))
    end

    if type(value) == "boolean" then
        return tostring(value)
    end

    if value == math.floor(value) then
        return string.format("%d", value)
    end

    return tostring(value)
end


--[[
    Serializes a single jump through into the table syntax Lönn accepts on paste.
]]
local function serializeJumpThru(jumpThru, id)
    local parts = {}

    for _, key in ipairs(JUMPTHRU_KEYS) do
        local value

        if key == "_id" then
            value = id
        elseif key == "x" or key == "y" then
            value = 0
        else
            value = jumpThru[key]
        end

        table.insert(parts, string.format("%s = %s", key, formatValue(value)))
    end

    return "        " .. table.concat(parts, ",\n        ")
end


--[[
    Builds jump through entries from clipboard text.

    Paths that resolve to the same texture are collapsed into a single entity.

    Returns the serialized table text and the number of entries, or nil when the
    text does not look like one or more jump through texture paths.
]]
function jumpThruBuilder.buildFromClipboardText(text)
    if type(text) ~= "string" or trim(text) == "" then
        return nil, 0
    end

    local entries = {}
    local seen = {}

    for _, line in ipairs(splitLines(text)) do
        local texture = pathToTexture(normalizePath(line))

        if not texture then
            return nil, 0
        end

        if not seen[texture] then
            seen[texture] = true

            local jumpThru = {}

            for key, value in pairs(JUMPTHRU_TEMPLATE) do
                jumpThru[key] = value
            end

            jumpThru.texture = texture

            table.insert(entries, jumpThru)
        end
    end

    if #entries == 0 then
        return nil, 0
    end

    local blocks = {}

    for index, jumpThru in ipairs(entries) do
        table.insert(blocks, "    {\n" .. serializeJumpThru(jumpThru, index - 1) .. "\n    }")
    end

    return "{\n" .. table.concat(blocks, ",\n") .. "\n}", #entries
end


return jumpThruBuilder

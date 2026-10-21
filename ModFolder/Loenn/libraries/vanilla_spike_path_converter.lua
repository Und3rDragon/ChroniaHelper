-- Converts pasted spike texture paths into vanilla spike entries.

local vanillaSpikeBuilder = {}

-- Folder the vanilla spike textures live in. Everything before it is mod
-- specific, so only what follows is written into the entity.
local SPIKES_FOLDER = "Graphics/Atlases/Gameplay/danger/spikes/"

-- Length written into every generated spike.
local SPIKE_LENGTH = 16

-- Per direction: the vanilla entry name, the field that stores the length, and
-- the offset that keeps the four sides of a tile ring aligned when the paths of
-- a whole ring are pasted at once.
local SPIKE_DIRECTIONS = {
    up = { name = "spikesUp", lengthKey = "width", x = 0, y = 0 },
    right = { name = "spikesRight", lengthKey = "height", x = 16, y = 0 },
    down = { name = "spikesDown", lengthKey = "width", x = 0, y = 16 },
    left = { name = "spikesLeft", lengthKey = "height", x = 0, y = 0 },
}

-- Field order used when writing the table, so the output stays readable.
local SPIKE_KEYS = {
    "_editorLayer",
    "_fromLayer",
    "_id",
    "_name",
    "attachToSolid",
    "type",
    "width",
    "height",
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
    Splits a spike texture path into its material name and direction.

    Everything from the spikes folder onwards is kept, the extension and the
    trailing animation frame digits are dropped, and the final underscore
    separated word is read as the direction. Nested folders survive, so
    "danger/spikes/sub/xxx_up00.png" becomes "sub/xxx" with direction "up".
]]
local function pathToSpike(path)
    local folderStart = path:find(SPIKES_FOLDER, 1, true)

    if not folderStart then
        return nil
    end

    local texture = path:sub(folderStart + #SPIKES_FOLDER)

    if texture:lower():sub(-4) == ".png" then
        texture = texture:sub(1, -5)
    end

    -- Drop the trailing digits, which mark an animation frame
    texture = texture:gsub("%d+$", "")

    local material, direction = texture:match("^(.*)_([%a]+)$")

    if not material or material == "" or not SPIKE_DIRECTIONS[direction] then
        return nil
    end

    return material, direction
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
    Serializes a single spike into the table syntax Lönn accepts on paste.
]]
local function serializeSpike(spike, id)
    local parts = {}

    for _, key in ipairs(SPIKE_KEYS) do
        local value

        if key == "_id" then
            value = id
        else
            value = spike[key]
        end

        if value ~= nil then
            table.insert(parts, string.format("%s = %s", key, formatValue(value)))
        end
    end

    return "        " .. table.concat(parts, ",\n        ")
end


--[[
    Builds vanilla spike entries from clipboard text.

    Paths that resolve to the same material and direction are collapsed into a
    single entity.

    Returns the serialized table text and the number of entries, or nil when the
    text does not look like one or more spike texture paths.
]]
function vanillaSpikeBuilder.buildFromClipboardText(text)
    if type(text) ~= "string" or trim(text) == "" then
        return nil, 0
    end

    local entries = {}
    local seen = {}

    for _, line in ipairs(splitLines(text)) do
        local material, direction = pathToSpike(normalizePath(line))

        if not material then
            return nil, 0
        end

        local key = material .. "|" .. direction

        if not seen[key] then
            seen[key] = true

            local info = SPIKE_DIRECTIONS[direction]

            local spike = {
                _editorLayer = 0,
                _fromLayer = "entities",
                _name = info.name,
                attachToSolid = true,
                type = material,
                x = info.x,
                y = info.y,
            }

            spike[info.lengthKey] = SPIKE_LENGTH

            table.insert(entries, spike)
        end
    end

    if #entries == 0 then
        return nil, 0
    end

    local blocks = {}

    for index, spike in ipairs(entries) do
        table.insert(blocks, "    {\n" .. serializeSpike(spike, index - 1) .. "\n    }")
    end

    return "{\n" .. table.concat(blocks, ",\n") .. "\n}", #entries
end


return vanillaSpikeBuilder

-- Converts pasted spike texture paths into Chronia spine entries.

local chroniaSpikeBuilder = {}

-- Path segment every gameplay texture sits behind. Everything before it is mod
-- specific, so only what follows is written into the entity.
local ATLAS_MARKER = "Graphics/Atlases/Gameplay/"

-- Length written into every generated spike.
local SPIKE_LENGTH = 16

-- The four spikes a single texture turns into, in the order they are written.
-- The offsets lay them out as the four sides of a tile ring, so the generated
-- set drops into a room the same way a hand placed ring does.
local SPIKE_DIRECTIONS = {
    { direction = "up", name = "ChroniaHelper/UpSpikes", lengthKey = "width", x = 0, y = 0 },
    { direction = "right", name = "ChroniaHelper/RightSpikes", lengthKey = "height", x = 16, y = 0 },
    { direction = "down", name = "ChroniaHelper/DownSpikes", lengthKey = "width", x = 0, y = 16 },
    { direction = "left", name = "ChroniaHelper/LeftSpikes", lengthKey = "height", x = 0, y = 0 },
}

-- Field order used when writing the table, so the output stays readable.
local SPIKE_KEYS = {
    "_editorLayer",
    "_fromLayer",
    "_id",
    "_name",
    "attached",
    "childMode",
    "depth",
    "grouped",
    "rainbow",
    "randomTexture",
    "retractDelay",
    "spikeType",
    "sprite",
    "trigger",
    "triggerDelay",
    "waitPlayerLeave",
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


local function isDirection(direction)
    for _, info in ipairs(SPIKE_DIRECTIONS) do
        if info.direction == direction then
            return true
        end
    end

    return false
end


--[[
    Turns an absolute texture path into the sprite name the spikes read.

    Everything from the gameplay folder onwards is kept, the extension and the
    trailing animation frame digits are dropped, and the final underscore
    separated word is treated as the direction, so the same material works for
    all four sides.

    For example, "danger/spikes/sub/xxx_up00.png" becomes "danger/spikes/sub/xxx".
]]
local function pathToSprite(path)
    local markerStart = path:find(ATLAS_MARKER, 1, true)

    if not markerStart then
        return nil
    end

    local texture = path:sub(markerStart + #ATLAS_MARKER)

    if texture:lower():sub(-4) == ".png" then
        texture = texture:sub(1, -5)
    end

    -- Drop the trailing digits, which mark an animation frame
    texture = texture:gsub("%d+$", "")

    local sprite, direction = texture:match("^(.*)_([%a]+)$")

    if not sprite or sprite == "" or not isDirection(direction) then
        return nil
    end

    return sprite
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
    Builds a four sided spike ring from a single clipboard texture path.

    Paths that resolve to the same sprite are collapsed into one ring.

    Returns the serialized table text and the number of entries, or nil when the
    text does not look like one or more spike texture paths.
]]
function chroniaSpikeBuilder.buildFromClipboardText(text)
    if type(text) ~= "string" or trim(text) == "" then
        return nil, 0
    end

    local entries = {}
    local seen = {}

    for _, line in ipairs(splitLines(text)) do
        local sprite = pathToSprite(normalizePath(line))

        if not sprite then
            return nil, 0
        end

        if not seen[sprite] then
            seen[sprite] = true

            for _, info in ipairs(SPIKE_DIRECTIONS) do
                local spike = {
                    _editorLayer = 0,
                    _fromLayer = "entities",
                    _name = info.name,
                    attached = true,
                    childMode = "",
                    depth = -50,
                    grouped = false,
                    rainbow = false,
                    randomTexture = true,
                    retractDelay = -1,
                    spikeType = "Custom",
                    sprite = sprite,
                    trigger = false,
                    triggerDelay = 0.4,
                    waitPlayerLeave = true,
                    x = info.x,
                    y = info.y,
                }

                spike[info.lengthKey] = SPIKE_LENGTH

                table.insert(entries, spike)
            end
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


return chroniaSpikeBuilder

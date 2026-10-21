-- Converts pasted booster texture paths into Chronia custom booster entries.

local boosterBuilder = {}

-- Path segment every booster texture sits behind.
local ATLAS_MARKER = "Graphics/Atlases/Gameplay/"

-- Default field values, mirroring the entity definition so a generated booster
-- behaves exactly like a freshly placed one.
local BOOSTER_TEMPLATE = {
    XMLOverride = false,
    _editorLayer = 0,
    _fromLayer = "entities",
    _name = "ChroniaHelper/CustomBooster",
    allowDashOutWhenBoosting = true,
    appearAnimInterval = 0.8,
    appearParticleColor = "ffffff",
    appearParticleColorOverride = false,
    burstParticleColor = "ffffff",
    burstParticleColorOverride = false,
    ch9_hub_booster = false,
    colorOverlay = "ffffff",
    dashes = "1",
    depth = -8500,
    disableFastBubble = false,
    forceCoyoteTime = "-1",
    greenBoostMovingSpeed = "240",
    hitboxRadius = 10,
    hitboxX = 0,
    hitboxY = 2,
    holdTime = "0.25",
    insideAnimInterval = 0.1,
    keepPlayerSpeed = false,
    loopAnimInterval = 0.1,
    onlyOnce = false,
    outSpeedMultiplier = "1",
    outlineDirectory = "objects/ChroniaHelper/customBoosterPresets/grey/outline",
    playerFollow = false,
    popAnimInterval = 0.08,
    red = false,
    redBoostMovingSpeed = "240",
    respawnTime = "1.0",
    setOrRefillDashes = false,
    setOrRefillStamina = false,
    spinAnimInterval = 0.06,
    stamina = "110",
}

-- Field order used when writing the table, so the output stays readable.
local BOOSTER_KEYS = {
    "XMLOverride",
    "_editorLayer",
    "_fromLayer",
    "_id",
    "_name",
    "allowDashOutWhenBoosting",
    "appearAnimInterval",
    "appearParticleColor",
    "appearParticleColorOverride",
    "burstParticleColor",
    "burstParticleColorOverride",
    "ch9_hub_booster",
    "colorOverlay",
    "dashes",
    "depth",
    "directory",
    "disableFastBubble",
    "forceCoyoteTime",
    "greenBoostMovingSpeed",
    "hitboxRadius",
    "hitboxX",
    "hitboxY",
    "holdTime",
    "insideAnimInterval",
    "keepPlayerSpeed",
    "loopAnimInterval",
    "onlyOnce",
    "outSpeedMultiplier",
    "outlineDirectory",
    "playerFollow",
    "popAnimInterval",
    "red",
    "redBoostMovingSpeed",
    "respawnTime",
    "setOrRefillDashes",
    "setOrRefillStamina",
    "spinAnimInterval",
    "stamina",
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
    Turns an absolute texture path into the directory a booster is loaded from.

    Everything past the gameplay atlas root is kept, and the last segment is only
    dropped when the path points at an image, so pasting a folder path keeps it
    as the directory itself.

    For example, "objects/ChroniaHelper/customBoosterPresets/grey/idle00.png"
    becomes "objects/ChroniaHelper/customBoosterPresets/grey", while
    "objects/ChroniaHelper/customBoosterPresets/grey" stays unchanged.
]]
local function pathToDirectory(path)
    local markerStart = path:find(ATLAS_MARKER, 1, true)

    if not markerStart then
        return nil
    end

    local relative = path:sub(markerStart + #ATLAS_MARKER)

    if relative == "" then
        return nil
    end

    -- Drop the file name only when the path points at an image
    if relative:lower():sub(-4) == ".png" then
        local directory = relative:match("^(.*)/[^/]+$")

        if not directory or directory == "" then
            return nil
        end

        return directory
    end

    return relative
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
    Serializes a single booster into the table syntax Lönn accepts on paste.
]]
local function serializeBooster(booster, id)
    local parts = {}

    for _, key in ipairs(BOOSTER_KEYS) do
        local value

        if key == "_id" then
            value = id
        elseif key == "x" or key == "y" then
            value = 0
        else
            value = booster[key]
        end

        table.insert(parts, string.format("%s = %s", key, formatValue(value)))
    end

    return "        " .. table.concat(parts, ",\n        ")
end


--[[
    Builds booster entries from clipboard text.

    Paths that resolve to the same directory are collapsed into a single entity,
    so every frame of one animation set produces one booster.

    Returns the serialized table text and the number of entries, or nil when the
    text does not look like one or more booster texture paths.
]]
function boosterBuilder.buildFromClipboardText(text)
    if type(text) ~= "string" or trim(text) == "" then
        return nil, 0
    end

    local entries = {}
    local seen = {}

    for _, line in ipairs(splitLines(text)) do
        local directory = pathToDirectory(normalizePath(line))

        if not directory then
            return nil, 0
        end

        if not seen[directory] then
            seen[directory] = true

            local booster = {}

            for key, value in pairs(BOOSTER_TEMPLATE) do
                booster[key] = value
            end

            booster.directory = directory

            table.insert(entries, booster)
        end
    end

    if #entries == 0 then
        return nil, 0
    end

    local blocks = {}

    for index, booster in ipairs(entries) do
        table.insert(blocks, "    {\n" .. serializeBooster(booster, index - 1) .. "\n    }")
    end

    return "{\n" .. table.concat(blocks, ",\n") .. "\n}", #entries
end


return boosterBuilder

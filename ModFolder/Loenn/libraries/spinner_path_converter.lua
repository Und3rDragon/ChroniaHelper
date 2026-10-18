-- Converts pasted spinner texture paths into Chronia seamless spinner entries.

local spinnerBuilder = {}

-- Path segment every spinner texture sits behind. Everything before it is mod
-- specific, so only what follows is written into the entity.
local ATLAS_MARKER = "Graphics/Atlases/Gameplay/"

-- Default field values, mirroring the entity definition.
local SPINNER_TEMPLATE = {
    _editorLayer = 0,
    _fromLayer = "entities",
    _name = "ChroniaHelper/SeamlessSpinner",
    attachToSolid = false,
    backDirectory = "blue",
    bgAnimDelay = 0.1,
    bgAnimRandomize = 0,
    bgColorOverlay = "",
    bgFlipX = "none",
    bgFlipY = "none",
    bloomAlpha = 0,
    bloomRadius = 0,
    childMode = "",
    coldCoreModeBGSpritePath = "danger/crystal/bg_blue",
    coldCoreModeSpritePath = "danger/crystal/fg_blue",
    coldCoreModeTriggerSpritePath = "objects/ChroniaHelper/timedSpinner/blue/fg_blue_base",
    colorOverlay = "ffffff",
    customHitbox = "r,16,16,-8,-8;c,8,-8,-8",
    depth = -8500,
    dynamic = false,
    fgAnimDelay = 0.1,
    fgAnimRandomize = 0,
    fgFlipX = "none",
    fgFlipY = "none",
    hitboxType = "default",
    hotCoreModeBGSpritePath = "danger/crystal/bg_red",
    hotCoreModeSpritePath = "danger/crystal/fg_red",
    hotCoreModeTriggerSpritePath = "objects/ChroniaHelper/timedSpinner/red/fg_red_base",
    lightAlpha = 0,
    lightColor = "ffffff",
    lightEndFade = 48,
    lightStartFade = 24,
    noBorder = false,
    rainbow = false,
    trigger = false,
    triggerAnimDelay = 0.05,
    triggerDelay = 0.5,
    useCoreModeStyle = false,
}

-- Field order used when writing the table, so the output stays readable.
local SPINNER_KEYS = {
    "_editorLayer",
    "_fromLayer",
    "_id",
    "_name",
    "attachToSolid",
    "backDirectory",
    "bgAnimDelay",
    "bgAnimRandomize",
    "bgColorOverlay",
    "bgFlipX",
    "bgFlipY",
    "bloomAlpha",
    "bloomRadius",
    "childMode",
    "coldCoreModeBGSpritePath",
    "coldCoreModeSpritePath",
    "coldCoreModeTriggerSpritePath",
    "colorOverlay",
    "customHitbox",
    "depth",
    "dynamic",
    "fgAnimDelay",
    "fgAnimRandomize",
    "fgFlipX",
    "fgFlipY",
    "foreDirectory",
    "hitboxType",
    "hotCoreModeBGSpritePath",
    "hotCoreModeSpritePath",
    "hotCoreModeTriggerSpritePath",
    "lightAlpha",
    "lightColor",
    "lightEndFade",
    "lightStartFade",
    "noBorder",
    "rainbow",
    "trigger",
    "triggerAnimDelay",
    "triggerDelay",
    "useCoreModeStyle",
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
    Turns an absolute texture path into the sprite paths a spinner reads.

    Everything past the gameplay atlas root is kept, the extension is dropped and
    a trailing run of digits is removed.

    The foreground sprite names its counterpart by swapping the "fg_" prefix for
    "bg_". When the file name carries no such prefix, the background sprite is
    taken from the same folder.

    For example, "danger/a/fg_b00.png" gives a foreground of "danger/a/fg_b" and
    a background of "danger/a/bg_b", while "danger/a/spin.png" gives
    "danger/a/spin" for both.
]]
local function pathToSprite(path)
    local markerStart = path:find(ATLAS_MARKER, 1, true)

    if not markerStart then
        return nil
    end

    local relative = path:sub(markerStart + #ATLAS_MARKER)

    if relative:lower():sub(-4) == ".png" then
        relative = relative:sub(1, -5)
    end

    -- Drop the trailing digits, which mark an animation frame
    relative = relative:gsub("%d+$", "")

    if relative == "" then
        return nil
    end

    local folder, fileName = relative:match("^(.*)/([^/]+)$")

    if not folder or not fileName then
        return nil
    end

    local fore = relative
    local back

    local swapped = fileName:gsub("^fg_", "bg_")

    if swapped ~= fileName then
        back = folder .. "/" .. swapped
    else
        back = folder
    end

    return fore, back
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
    Serializes a single spinner into the table syntax Lönn accepts on paste.
]]
local function serializeSpinner(spinner, id)
    local parts = {}

    for _, key in ipairs(SPINNER_KEYS) do
        local value

        if key == "_id" then
            value = id
        elseif key == "x" or key == "y" then
            value = 0
        else
            value = spinner[key]
        end

        table.insert(parts, string.format("%s = %s", key, formatValue(value)))
    end

    return "        " .. table.concat(parts, ",\n        ")
end


--[[
    Builds spinner entries from clipboard text.

    Paths that resolve to the same directory are collapsed into a single entity.

    Returns the serialized table text and the number of entries, or nil when the
    text does not look like one or more spinner texture paths.
]]
function spinnerBuilder.buildFromClipboardText(text)
    if type(text) ~= "string" or trim(text) == "" then
        return nil, 0
    end

    local entries = {}
    local seen = {}

    for _, line in ipairs(splitLines(text)) do
        local fore, back = pathToSprite(normalizePath(line))

        if not fore then
            return nil, 0
        end

        if not seen[fore] then
            seen[fore] = true

            local spinner = {}

            for key, value in pairs(SPINNER_TEMPLATE) do
                spinner[key] = value
            end

            spinner.foreDirectory = fore
            spinner.backDirectory = back

            table.insert(entries, spinner)
        end
    end

    if #entries == 0 then
        return nil, 0
    end

    local blocks = {}

    for index, spinner in ipairs(entries) do
        table.insert(blocks, "    {\n" .. serializeSpinner(spinner, index - 1) .. "\n    }")
    end

    return "{\n" .. table.concat(blocks, ",\n") .. "\n}", #entries
end


return spinnerBuilder

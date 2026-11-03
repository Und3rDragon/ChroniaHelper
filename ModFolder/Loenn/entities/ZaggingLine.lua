local utils = require("utils")
local drawableLine = require("structs.drawable_line")

local zaggingLine = {}

zaggingLine.name = "ChroniaHelper/ZaggingLine"
zaggingLine.nodeLimits = { 1, 1 }
zaggingLine.nodeLineRenderType = "line"
zaggingLine.nodeVisibility = "always"

zaggingLine.depth = function(room, entity) return entity.depth or 9500 end

zaggingLine.placements = {
    name = "zaggingLine",
    type = "line",
    data = {
        color = "4da6ffff",
        lineColors = "",
        thickness = 2.0,
        dashLength = 8.0,
        gapLength = 6.0,
        speed = 40.0,
        fadeLength = 3.0,
        resolution = 10,
        showBgLine = false,
        bgLineColor = "ffffffff",
        showNodes = true,
        nodeColors = "ffffffff",
        depth = 9500,
        nodeTexture = "ChroniaHelper/ZaggingLine/node00",
    }
}

zaggingLine.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = true,
    },
    lineColors = {
        fieldType = "list_scroll",
        elementOptions = {
            fieldType = "color",
            useAlpha = true,
        },
    },
    bgLineColor = {
        fieldType = "color",
        useAlpha = true,
    },
    nodeColors = {
        fieldType = "list_scroll",
        elementOptions = {
            fieldType = "color",
            useAlpha = true,
        },
    },
    thickness = {
        fieldType = "number",
        minimumValue = 0,
    },
    dashLength = {
        fieldType = "number",
        minimumValue = 1,
    },
    gapLength = {
        fieldType = "number",
        minimumValue = 0,
    },
    speed = {
        fieldType = "number",
    },
    fadeLength = {
        fieldType = "number",
        minimumValue = 0,
    },
    resolution = {
        fieldType = "integer",
        minimumValue = 1,
    },
    depth = require('mods').requireFromPlugin('helpers.field_options').depths,
}

local function hexToColor(hex)
    if hex == nil or string.len(hex) < 6 then
        return { 1.0, 1.0, 1.0, 1.0 }
    end

    local red = tonumber(string.sub(hex, 1, 2), 16) / 255.0
    local green = tonumber(string.sub(hex, 3, 4), 16) / 255.0
    local blue = tonumber(string.sub(hex, 5, 6), 16) / 255.0
    local alpha = 1.0

    if string.len(hex) >= 8 then
        alpha = tonumber(string.sub(hex, 7, 8), 16) / 255.0
    end

    return { red, green, blue, alpha }
end

local function splitColors(value)
    local colors = {}

    if value == nil or value == "" then
        return colors
    end

    for entry in string.gmatch(value, "[^,]+") do
        entry = entry:gsub("^%s+", ""):gsub("%s+$", "")

        if entry ~= "" then
            table.insert(colors, entry)
        end
    end

    return colors
end

zaggingLine.sprite = function(room, entity)
    local sprites = {}
    local nodes = entity.nodes or {}
    local node = nodes[1]

    if node == nil then
        return sprites
    end

    local x1, y1, x2, y2 = entity.x, entity.y, node.x, node.y
    local thickness = entity.thickness or 2

    if entity.showBgLine then
        table.insert(sprites, drawableLine.fromPoints({ x1, y1, x2, y2 },
            hexToColor(entity.bgLineColor), thickness))
    end

    local deltaX, deltaY = x2 - x1, y2 - y1
    local length = math.sqrt(deltaX * deltaX + deltaY * deltaY)

    local colors = splitColors(entity.lineColors)
    if #colors == 0 then
        colors = { entity.color }
    end

    local dashLength = entity.dashLength or 8
    local gapLength = entity.gapLength or 6
    local period = dashLength + gapLength

    if length > 0.01 and period > 0 then
        local unitX, unitY = deltaX / length, deltaY / length
        local distance, index = 0, 0

        while distance < length do
            local dashEnd = math.min(distance + dashLength, length)

            table.insert(sprites, drawableLine.fromPoints({
                x1 + unitX * distance, y1 + unitY * distance,
                x1 + unitX * dashEnd, y1 + unitY * dashEnd,
            }, hexToColor(colors[(index % #colors) + 1]), thickness))

            distance = distance + period
            index = index + 1
        end
    end

    return sprites
end

zaggingLine.selection = function(room, entity)
    local mainRectangle = utils.rectangle(entity.x - 4, entity.y - 4, 8, 8)
    local nodeRectangles = {}
    local nodes = entity.nodes or {}

    for _, node in ipairs(nodes) do
        table.insert(nodeRectangles, utils.rectangle(node.x - 4, node.y - 4, 8, 8))
    end

    return mainRectangle, nodeRectangles
end

return zaggingLine

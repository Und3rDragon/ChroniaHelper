local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

local controller = {}

controller.name = "ChroniaHelper/CameraGrid"

controller.placements = {
    name = "grid",
    data = {
        zoom = 1,
        roomBorder = "ffffffff",
        grid = "ffffff16",
        axisNumbers = true,
        depth = 0,
    }
}

controller.depth = function(room, entity) return entity.depth or 0 end

controller.ignoredFields = {
    "_x", "_y", "x", "y"
}

controller.fieldInformation = {
    zoom = {
        minimumValue = 0,
    },
    roomBorder = {
        fieldType = "color",
        useAlpha = true,
    },
    grid = {
        fieldType = "color",
        useAlpha = true,
    },
    depth = {
        fieldType = "integer",
        options = require("mods").requireFromPlugin("consts.depths"),
        editable = true,
    }
}

function controller.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

local FRAME_WIDTH = 320
local FRAME_HEIGHT = 180
local CELL_WIDTH = 48
local CELL_HEIGHT = 32
local BORDER_THICKNESS = 4
local GRID_THICKNESS = 2
local INDICATOR_SIZE = 16
local INDICATOR_THICKNESS = 1
local NUMBER_OFFSET = 2
local NUMBER_FONT_SIZE = 1
local NUMBER_COLOR = { 1, 1, 1, 1 }

local function hexToColor(hex)
    if type(hex) ~= "string" or string.len(hex) < 6 then
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

local function addRectangle(sprites, x, y, width, height, color)
    if width <= 0 or height <= 0 then
        return
    end

    table.insert(sprites, drawableRectangle.fromRectangle("fill", x, y, width, height, color))
end

local function addLine(sprites, x1, y1, x2, y2, thickness, color)
    table.insert(sprites, drawableLine.fromPoints({ x1, y1, x2, y2 }, color, thickness))
end

-- Room frame, centered on the entity
local function addRoomBorder(sprites, entity, width, height, color)
    local x = entity.x - width / 2
    local y = entity.y - height / 2

    addRectangle(sprites, x, y, width, BORDER_THICKNESS, color)
    addRectangle(sprites, x, y + height - BORDER_THICKNESS, width, BORDER_THICKNESS, color)
    addRectangle(sprites, x, y + BORDER_THICKNESS, BORDER_THICKNESS, height - BORDER_THICKNESS * 2, color)
    addRectangle(sprites, x + width - BORDER_THICKNESS, y + BORDER_THICKNESS, BORDER_THICKNESS, height - BORDER_THICKNESS * 2, color)
end

-- First and last line index that stays inside the room, origin included
local function stepRange(origin, step, length)
    return math.ceil(-origin / step), math.floor((length - origin) / step)
end

local function addGridLines(sprites, entity, room, color)
    local zoom = entity.zoom or 1

    if zoom <= 0 then
        return
    end

    local stepX = CELL_WIDTH * zoom
    local stepY = CELL_HEIGHT * zoom

    local firstX, lastX = stepRange(entity.x, stepX, room.width)

    for index = firstX, lastX do
        local x = entity.x + index * stepX

        addLine(sprites, x, 0, x, room.height, GRID_THICKNESS, color)
    end

    local firstY, lastY = stepRange(entity.y, stepY, room.height)

    for index = firstY, lastY do
        local y = entity.y + index * stepY

        addLine(sprites, 0, y, room.width, y, GRID_THICKNESS, color)
    end
end

local function addAxisNumbers(sprites, entity, room)
    local zoom = entity.zoom or 1

    if zoom <= 0 then
        return
    end

    local stepX = CELL_WIDTH * zoom
    local stepY = CELL_HEIGHT * zoom

    local firstX, lastX = stepRange(entity.x, stepX, room.width)

    for index = firstX, lastX do
        local x = entity.x + index * stepX + NUMBER_OFFSET
        local y = entity.y + NUMBER_OFFSET

        if x >= 0 and x <= room.width and y >= 0 and y <= room.height then
            table.insert(sprites, drawableText.fromText(tostring(index), x, y, nil, nil, nil, NUMBER_FONT_SIZE, NUMBER_COLOR))
        end
    end

    local firstY, lastY = stepRange(entity.y, stepY, room.height)

    for index = firstY, lastY do
        local x = entity.x + NUMBER_OFFSET
        local y = entity.y + index * stepY + NUMBER_OFFSET

        if x >= 0 and x <= room.width and y >= 0 and y <= room.height then
            table.insert(sprites, drawableText.fromText(tostring(index), x, y, nil, nil, nil, NUMBER_FONT_SIZE, NUMBER_COLOR))
        end
    end
end

local function addIndicator(sprites, entity)
    local left = entity.x - INDICATOR_SIZE / 2
    local top = entity.y - INDICATOR_SIZE / 2
    local right = left + INDICATOR_SIZE
    local bottom = top + INDICATOR_SIZE

    addRectangle(sprites, left, top, INDICATOR_SIZE, INDICATOR_SIZE, { 1, 1, 1, 0.1 })

    addLine(sprites, left, top, right, top, INDICATOR_THICKNESS, { 1, 1, 1, 1 })
    addLine(sprites, left, bottom, right, bottom, INDICATOR_THICKNESS, { 1, 1, 1, 1 })
    addLine(sprites, left, top, left, bottom, INDICATOR_THICKNESS, { 1, 1, 1, 1 })
    addLine(sprites, right, top, right, bottom, INDICATOR_THICKNESS, { 1, 1, 1, 1 })
end

function controller.sprite(room, entity)
    local sprites = {}
    local zoom = entity.zoom or 1

    addGridLines(sprites, entity, room, hexToColor(entity.grid))
    addRoomBorder(sprites, entity, FRAME_WIDTH * zoom, FRAME_HEIGHT * zoom, hexToColor(entity.roomBorder))

    if entity.axisNumbers ~= false then
        addAxisNumbers(sprites, entity, room)
    end

    addIndicator(sprites, entity)

    return sprites
end

return controller

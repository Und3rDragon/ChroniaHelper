local drawableSpriteStruct = require("structs.drawable_sprite")
local drawableRectangleStruct = require("structs.drawable_rectangle")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableLineStruct = require("structs.drawable_line")
local utils = require("utils")
local xnaColors = require("consts.xna_colors")

--[[
local frostUtils = require("mods").requireFromPlugin("helpers.frostUtils")
]]

local celesteDepths = --require("consts.object_depths")
{
    ["BG Terrain (10000)"] = 10000,
    ["BG Mirrors (9500)"] = 9500,
    ["BG Decals (9000)"] = 9000,
    ["BG Particles (8000)"] = 8000,
    ["Solids Below (5000)"] = 5000,
    ["Below (2000)"] = 2000,
    ["NPCs (1000)"] = 1000,
    ["Theo Crystal (100)"] = 100,
    ["Player (0)"] = 0,
    ["Dust (-50)"] = -50,
    ["Pickups (-100)"] = -100,
    ["Seeker (-200)"] = -200,
    ["Particles (-8000)"] = -8000,
    ["Above (-8500)"] = -8500,
    ["Solids (-9000)"] = -9000,
    ["FG Terrain (-10000)"] = -10000,
    ["FG Decals (-10500)"] = -10500,
    ["Dream Blocks (-11000)"] = -11000,
    ["Crystal Spinners (-11500)"] = -11500,
    ["Player Dream Dashing (-12000)"] = -12000,
    ["Enemy (-12500)"] = -12500,
    ["Fake Walls (-13000)"] = -13000,
    ["FG Particles (-50000)"] = -50000,
    ["Top (-1000000)"] = -1000000,
    ["Formation Sequences (-2000000)"] = -2000000,
}


local frostUtils = {}

function frostUtils.createPlacementsPreserveOrder(handler, placementName, placementData, appendSize)
    handler.placements = {{
        name = placementName,
        data = {}
    }}
    local fieldOrder = { "x", "y" }
    local fieldInformation = {}
    local hasAnyFieldInformation = false

    if appendSize then
        table.insert(placementData, 1, { "height", 16 })
        table.insert(placementData, 1, { "width", 16 })
    end

    for _,v in ipairs(placementData) do
        local fieldName, defaultValue, fieldType, fieldData = v[1], v[2], v[3], v[4]

        table.insert(fieldOrder, fieldName)
        handler.placements[1].data[fieldName] = defaultValue
        if fieldType then
            local override = frostUtils.fieldTypeOverrides[fieldType]
            if override then
                -- use an override from frostUtils if available
                -- used by color to automatically support XNA color names
                local typ = type(override)
                if typ == "function" then
                    fieldInformation[fieldName] = override(fieldData)
                else
                    fieldInformation[fieldName] = override
                end
            else
                -- otherwise just use it normally
                local typ = type(fieldType)
                if typ == "table" then
                    if fieldType["fieldType"] then
                        -- we have a full field definition here, don't do anything about it
                        fieldInformation[fieldName] = fieldType
                    else
                        -- didn't define a type, treat it as a dropdown
                        fieldInformation[fieldName] = frostUtils.fieldTypeOverrides.dropdown(fieldType)
                    end
                else
                    -- if you just pass a string, treat it as the field type
                    fieldInformation[fieldName] = { fieldType = fieldType }
                end
            end

            hasAnyFieldInformation = true
        end
    end

    handler.fieldOrder = fieldOrder
    if hasAnyFieldInformation then
        handler.fieldInformation = fieldInformation
    end
end

function frostUtils.getFilledRectangleSprite(rectangle, fillColor)
    return drawableRectangleStruct.fromRectangle("fill", rectangle.x, rectangle.y, rectangle.width, rectangle.height, fillColor or frostUtils.colorWhite):getDrawableSprite()
end

frostUtils.colorWhite = utils.getColor(ffffff)

function frostUtils.addAll(addTo, toAddTable, insertLoc)
    if insertLoc then
        for _, value in ipairs(toAddTable) do
            table.insert(addTo, insertLoc, value)
        end
    else
        for _, value in ipairs(toAddTable) do
            table.insert(addTo, value)
        end
    end


    return addTo
end

function frostUtils.getColors(list)
    local colors = {}
    local split = string.split(list, ",")()
    for _, value in ipairs(split) do
        table.insert(colors, frostUtils.getColor(value))
    end

    return colors
end

function frostUtils.getColor(color)
    local colorType = type(color)

    if colorType == "string" then
        -- Check XNA colors, otherwise parse as hex color
        if xnaColors[color] then
            return xnaColors[color]

        else
            local success, r, g, b, a = frostUtils.parseHexColor(color)

            if success then
                return {r, g, b, a}
            end
            print("Invalid hex color " .. color)
            return success
        end

    elseif colorType == "table" and (#color == 3 or #color == 4) then
        return color
    end
end

function frostUtils.getPixelSprite(x, y, color)
    return drawableSpriteStruct.fromInternalTexture(drawableRectangleStruct.tintingPixelTexture, { x = x, y = y, jx = 0, jy = 0, color = color or frostUtils.colorWhite })
end

frostUtils.windingOrders = {
    "Clockwise", "CounterClockwise", "Auto"
}

frostUtils.fieldTypeOverrides = {
    color =  function (data) return {
        fieldType = "color",
        allowXNAColors = true,
    } end,
    list = function (data)
        local baseOverride = frostUtils.fieldTypeOverrides[data.listType]
        local base = baseOverride and baseOverride(data) or {
            fieldType = data.listType,
        }
        base["ext:list"] = {
            separator = data.separator or ",",
            minElements = data.minElements or 1, -- the minimum amount of elements in this list. Defaults to 1. Can be 0, in which case an empty string is allowed.
            maxElements = data.maxElements or -1, -- the maximum amount of elements in this list. Defaults to -1 (unlimited).
        }
        return base
    end,
    editableDropdown = function(data)
        return {
            options = data,
            editable = true
        }
    end,
    dropdown = function(data)
        return {
            options = data,
            editable = false
        }
    end,
    depth = function (data)
        return {
            options = celesteDepths,
            editable = true,
            fieldType = "integer",
        }
    end,
    path = function (data)
        print("path property here!!!")
        return {
            fieldType = "path",
            allowFolders = true,
            allowFiles = false,
            relativeToMod = true,
            allowMissingPath = false,
            allowEmpty = false,
        }
    end
}

function frostUtils.parseHexColor(color)
    if #color == 6 then
        local success, r, g, b = utils.parseHexColor(color)
        return success, r, g, b, 1
    elseif #color == 8 then
        color := match("^#?([0-9a-fA-F]+)$")

        if color then
            local number = tonumber(color, 16)
            local r, g, b, a = math.floor(number / 256^3) % 256, math.floor(number / 256^2) % 256, math.floor(number / 256) % 256, math.floor(number) % 256

            return true, r / 255, g / 255, b / 255, a / 255
        end
    end

    return false, 0, 0, 0
end

return frostUtils
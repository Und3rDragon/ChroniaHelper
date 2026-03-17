local drawableLine = require("structs.drawable_line")

local shield = {}

shield.name = "ChroniaHelper/CustomShield"

shield.depth = 0

shield.placements = {
    name = "shield",
    data = {
        x = 0,
        y = 0,
        radius = "10",
        bloomAlpha = "0.5",
        bloomRadius = "20",
        lightFlag = "lightOn",
        lightColor = "ffffff",
        lightAlpha = "1",
        lightStartFade = "16",
        lightEndFade="48",
        color = "ffffff",
        square = false,
    },
}

shield.fieldInformation = {
    lightColor = {
        fieldType = "color",
        useAlpha = true,
    },
    color = {
        fieldType = "color",
        useAlpha = true,
    },
}

shield.sprite = function(room, entity)
    local r = tonumber(entity.radius)
    if r == nil then
        r = 8
    end

    if not entity.square then
        -- drawCircle函数来自于Refill.lua
        return drawCircle({entity.x, entity.y, entity.x + r, entity.y}, 1)
    else
        return require("structs.drawable_rectangle").fromRectangle("bordered", entity.x - r, entity.y - r, 2 * r, 2 * r, {1,1,1,0.1}, {1,1,1,1})
    end
end

shield.selection = function(room, entity)
    local r = tonumber(entity.radius)
    if r == nil then
        r = 8
    end

    return require("utils").rectangle(entity.x - r, entity.y - r, r * 2, r * 2)
end

return shield
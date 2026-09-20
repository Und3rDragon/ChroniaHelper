local entity = {}

entity.name = "ChroniaHelper/ShattersongZone"
entity.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,

        depth = 1,

        Draw = true,
        Alt = false,

        NoDash = false,
        NoDashRefill = false,
        NoStaminaRefill = false,
    }
}

entity.fieldInformation = {
    depth = {
        fieldType = "integer",
        options = require("mods").requireFromPlugin("consts.depths"),
        editable = true
    },
}

entity.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "mode",
    "controlFlag",
    "zoneColor",
    "depth",
    "iconPrefix",
    "icons",
    "iconOffsets",
    "iconColors",
    "flagMode",
    "flag",
    "renderBorder",
    "renderInside",
    "renderContinuousLine"
}

entity.sprite = function(room, entity, viewport)
    local borderColor = "ffffff"
    local innerColor = "ffffff"
    borderColor = borderColor .. string.format("%x", (255 * 0.7))
    innerColor = innerColor .. string.format("%x", (255 * 0.2))
    return {
        require("structs.drawable_rectangle").fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height,
            innerColor, borderColor),
    }
end

entity.depth = function(room,entity) return entity.depth or 1 end

return entity

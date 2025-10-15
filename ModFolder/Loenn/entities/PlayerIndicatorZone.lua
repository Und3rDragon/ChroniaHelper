local entity = {}

entity.name = "ChroniaHelper/PlayerIndicatorZoneCustom"
entity.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,

        mode = 0,
        controlFlag = "",

        renderBorder = false,
        renderInside = false,
        renderContinuousLine = false,
        zoneColor = "ffffff",
        depth = -20000,

        iconPrefix = "ChroniaHelper/PlayerIndicator/",
        icons = "chevron,triangle",
        iconOffsets = "0,6;-11,6",
        iconColors = "ffffff,ffffff",

        flagMode = 0,
        flag = "flag",
        independentFlag = false,
    }
}

entity.fieldInformation = {
    mode = {
        fieldType = "integer",
        options = {
            ["Limited"] = 0,
            ["Toggle"] = 1,
            ["None"] = 2
        },
        editable = false
    },
    depth = {
        fieldType = "integer",
        options = require("mods").requireFromPlugin("consts.depths"),
        editable = true
    },
    zoneColor = {
        fieldType = "color",
    },
    icons = {
        options = {
            "chevron,triangle",
            "chevron",
            "triangle",
            "square",
            "circle",
            "none"
        },
        editable = true,
    },
    flagMode = {
        fieldType = "integer",
        options = {
            ["None"] = 0,
            ["Zone"] = 1,
            ["Enable"] = 2,
            ["Disable"] = 3
        },
        editable = false,
    },
    iconColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        },
    },
    icons = {
        fieldType = "list",
    },
    iconOffsets = {
        fieldType = "list",
        elementSeparator = ";",
    }
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
    local borderColor = entity.zoneColor
    local innerColor = entity.zoneColor
    borderColor = borderColor .. string.format("%x", (255 * 0.7))
    innerColor = innerColor .. string.format("%x", (255 * 0.2))
    return {
        require("structs.drawable_rectangle").fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height,
            innerColor, borderColor),
    }
end

entity.depth = function(room,entity) return entity.depth or -20000 end

return entity

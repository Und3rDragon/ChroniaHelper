local afc = {}

afc.name = "ChroniaHelper/EntityTextAdder"

afc.placements = {
    name = "EntityTextAdder",
    data = {
        targetTextID = "dialogID",
        relativePositionX = 0,
        relativePositionY = 0,
        alignX = 0.5,
        alignY = 0.5,
        scaleX = 1,
        scaleY = 1,
        textColor = "ffffffff",
        stroke = 0,
        strokeColor = "ffffffff",
        edgeDepth = 0,
        edgeColor = "ffffffff",
        outlined = false,
    }
}

afc.fieldInformation = {
    textColor = {
        fieldType = "color",
        useAlpha = true,
    },
    strokeColor = {
        fieldType = "color",
        useAlpha = true,
    },
    edgeColor = {
        fieldType = "color",
        useAlpha = true,
    },
}

afc.triggerText = function(room, entity)
    return "Active Font Adder"
end

return afc
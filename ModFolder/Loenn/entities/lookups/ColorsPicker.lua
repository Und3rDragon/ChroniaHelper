local picker = {}

picker.name = "ChroniaHelper/ColorsPicker"

picker.placements = {
    name = "Colors Picker",
    data = {
        colors8 = "ffffffff",
        colors6 = "ffffff",
        separator = ",",
    },
}

picker.fieldInformation = function(entity)
    local orig = {}
        
    orig["colors8"] = {
        fieldType = "list_scroll",
        elementOptions = {
            fieldType = "color",
            useAlpha = true,
        },
        elementSeparator = entity.separator,
    }

    orig["colors6"] = {
        fieldType = "list_scroll",
        elementOptions = {
            fieldType = "color",
        },
        elementSeparator = entity.separator,
    }
    
    return orig
end

function picker.sprite(room, entity)
    local sprite = {}
    
    local iconSprite = require("structs.drawable_sprite").fromTexture("ChroniaHelper/LoennIcons/Folder", entity)
    
    table.insert(sprite, iconSprite)
    
    return sprite
end

return picker
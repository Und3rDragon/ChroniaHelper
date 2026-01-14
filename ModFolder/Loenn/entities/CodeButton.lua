local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local codeButton = {}

codeButton.name = "ChroniaHelper/CodeButton"

codeButton.placements = {
    name = "codeButton",
    data = {
        x = 0,
        y = 0,
        buttonBaseTexture = "objects/ChroniaHelper/button/base00",
        buttonTexture = "objects/ChroniaHelper/button/button00",
        depth = 8020,
        sessionKeyID = "buttonKey",
        buttonCode = "0",
        buttonMode = 0,
        pressSound = "",
        releaseSound = "",
    },
}

codeButton.ignoredFields = {
    
}

codeButton.fieldInformation = 
{
    buttonMode = {
        fieldType = "integer",
        options = {
            ["Input"] = 0,
            ["Enter"] = 1,
            ["Backspace"] = 2,
            ["Clear"] = 3,
        },
        editable = false,
    },
}

function codeButton.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function codeButton.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("objects/ChroniaHelper/button/overall", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return codeButton
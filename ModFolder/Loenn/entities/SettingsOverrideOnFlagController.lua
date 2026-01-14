local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/SettingsOverrideOnFlagController"

controller.placements = {
    name = "controller",
    data = {
        flag = "flag",
        photoSensitiveMode = 0,
        fullScreen = 0,
        windowScale = "",
        language = -1,
        grabMode = 0,
    },
}

controller.ignoredFields = {
    
}

controller.fieldInformation = 
{
    photoSensitiveMode = {
        fieldType = "integer",
        options = {
            ["Do not change"] = 0,
            ["On"] = 1,
            ["Off"] = 2,
        },
        editable = false,
    },
    fullScreen = {
        fieldType = "integer",
        options = {
            ["Do not change"] = 0,
            ["On"] = 1,
            ["Off"] = 2,
        },
        editable = false,
    },
    language = {
        fieldType = "integer",
        options = {
            ["No Changes"] = -1,
            ["English"] = 0,
            ["Brazilian"] = 1,
            ["French"] = 2,
            ["German"] = 3,
            ["Italian"] = 4,
            ["Japanese"] = 5,
            ["Korean"] =6,
            ["Russian"] = 7,
            ["Simplified Chinese"] = 8,
            ["Spanish"] = 9,
        },
        editable = false,
    },
    grabMode = {
        fieldType = "integer",
        options = {
            ["No Changes"] = 0,
            ["Hold"] = 1,
            ["Inverted"] = 2,
            ["Toggle"] = 3,
        },
        editable = false,
    }
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/SettingsOverride", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

--return controller
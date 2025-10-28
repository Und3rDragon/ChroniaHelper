local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")

local entity = {}

entity.name = "ChroniaHelper/PasswordTrigger"
entity.depth = function(room,entity) return entity.depth or 9000 end
--entity.justification = { 0.5, 1.0 }
entity.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,
        --texture = "ChroniaHelper/PasswordKeyboard/keyboard",
        tag = "passwordKeyboard",
        mode = 1,
        flagToEnable = "",
        password = "",
        characterLimit = 12,
        --rightDialog = "rightDialog",
        --wrongDialog = "wrongDialog",
        caseSensitive = true,
        useTimes = -1,
        --accessZone = "-16,0,32,8",
        --accessZoneIndicator = false,
        talkIconPosition = "0,-8",
        depth = 9000,
        globalFlag = false,
        toggleFlag = false,
        passwordEncrypted = false,
        showEncryptedPasswordInConsole = false,
    }
}

entity.fieldInformation = {
    mode = {
        fieldType = "integer",
        options = {
            ["Exclusive"] = 0,
            ["Normal"] = 1,
            ["OutputFlag"] = 2,
            ["Systematic"] = 3,
        },
        editable = false
    },
    useTimes = {
        fieldType = "integer"
    },
    accessZone = {
        fieldType = "list",
        minimumElements = 4,
        maximumElements = 4,
        elementOptions = {
            fieldType = "integer",
        },
    },
    tag = {
        allowEmpty = false,
    },
    texture = {
        allowEmpty = false,
    },
    talkIconPosition = {
        fieldType = "list",
        minimumElements = 2,
        maximumElements = 2
    },
    depth = require("mods").requireFromPlugin("helpers.field_options").depths,
    characterLimit = {
        minimumValue = 1,
        fieldType = "integer",
    },
    password = {
        fieldType = "list",
        elementSeparator = ";",
    },
    flagToEnable = {
        fieldType = "list",
    },
}

return entity

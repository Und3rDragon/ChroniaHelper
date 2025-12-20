local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/ConditionDelayListener"

controller.placements = {
    name = "controller",
    data = {
        chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",
        listenerTag = "conditionListener",
        condition = "someFlag",
        operationMode = 0,
        flagOperationMode = 0,
        usingExpression = 0,
        delayOrInterval = 0,
        flag = "flag",
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y", "_id", "_name"
}

controller.fieldOrder = {
    "_x", "_y", "x", "y", "_id", "_name",
    "chroniaMathExpession", "frostSessionExpression",
}

controller.fieldInformation = 
{
    chroniaMathExpession = {
        editable = false,
    },
    operationMode = {
        fieldType = "integer",
        options = {
            ["Constantly When Condition Satisfied"] = 0, ["Once When Satisfaction Changed"] = 1, ["Once When Satisfied"] = 2, ["Once When Not Satisfied"] = 3
        },
        editable = false,
    },
    flagOperationMode = {
        fieldType = "integer",
        options = {
            ["On"] = 0, ["Off"] = 1, ["Switch"] = 2
        },
        editable = false,
    },
    usingExpression = {
        fieldType = "integer",
        options = {
            ["Flags"] = 0, ["ChroniaMathExpression"] = 1, ["FrostSessionExpression"] = 2
        },
        editable = false,
    },
    delayOrInterval = {
        minimumValue = 0,
    }
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/ConditionListener", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
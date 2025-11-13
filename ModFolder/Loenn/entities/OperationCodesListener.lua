local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/OperationCodesListener"

controller.placements = {
    name = "controller",
    data = {
        flag = "operationsDone",
        interceptLength = 5,
        targetSequence = "2;3;2;3;2,4",
        listener = 1,
        logOperationsInConsole = false,
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y", "_id", "_name"
}

controller.fieldInformation = 
{
    listener = {
        options = {
            ["None"] = 0,
            ["On Press"] = 1,
            ["On Hold"] = 2,
            ["Both"] = 3,
        },
        editable = false,
    },
    interceptLength = {
        fieldType = "integer",
        minimumValue = 1,
    },
    targetSequence = {
        fieldType = "list",
        elementSeparator = ";",
        elementOptions = {
            fieldType = "list",
            elementOptions ={
                options = {
                    ["ESC = 0"] = 0,
                    ["Pause = 1"] = 1,
                    ["Left = 2"] = 2,
                    ["Right = 3"] = 3,
                    ["Up = 4"] = 4,
                    ["Down = 5"] = 5,
                    ["MenuConfirm = 6"] = 6,
                    ["MenuJournal = 7"] = 7,
                    ["QuickRestart = 8"] = 8,
                    ["Jump = 9"] = 9,
                    ["Dash = 10"] = 10,
                    ["Grab = 11"] = 11,
                    ["Talk = 12"] = 12,
                    ["CrouchDash = 13"] = 13,
                },
                editable = false,
            },
            minimumElements = 1,
        },
    }
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Flag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
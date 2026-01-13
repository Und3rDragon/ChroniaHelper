local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/SetFlagSequenceController"
controller.placements = {
    name = "controller",
    data = {
        chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",
        flagSequence = "flag1,flag2;0.5;flag3,!flag4,*flag5,?flag6",
        parameters = "",
        mode = 0,
        globalCoroutine = false,
    },
}

controller.fieldOrder = {
    "_x", "_y", "x", "y", "_id", "_name",
    "chroniaMathExpession", "frostSessionExpression",
}

controller.fieldInformation = {
    chroniaMathExpession = {
        editable = false,
    },
    mode = {
        options = {
            ["On Level Load"] = 0, ["Always Set"] = 1, ["On Scene Start"] = 2, ["On Scene End"] = 3, ["On Interval"] = 4,
            ["On Player Die"] = 5, ["On Player Respawn"] = 6, ["On Entity Added"] = 7, ["On Entity Removed"] = 8,
            ["On Flags"] = 9, ["On Chronia Expression"] = 10, ["On Frost Session Expression"] = 11
        },
        editable = false,
    },
    flagSequence = {
        fieldType = "list",
        elementSeparator = ";",
        elementOptions = {
            fieldType = "list",
            minimumElements = 1,
        },
    },
    parameters = {
        fieldType = "list",
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Flag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
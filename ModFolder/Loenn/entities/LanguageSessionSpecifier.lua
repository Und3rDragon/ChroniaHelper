local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/LanguageSessionSpecifier"
controller.placements = {
    name = "controller",
    data = {
        english = "",
        brazilian = "",
        french = "",
        german = "",
        italian = "",
        japanese = "",
        korean = "",
        russian = "",
        simplifiedChinese = "",
        spanish = "",
        sessionType = 0,
        targetName = "languageSessionValue",
    },
}

controller.fieldOrder = {
    "-x","_y","x","y","_name","_id","name","id",
    "english","brazilian","french","german","italian","japanese",
    "korean","russian","simplifiedChinese","spanish",
    "sessionType","targetName"
}

controller.fieldInformation = {
    sessionType = {
        fieldType = "integer",
        options = {
            ["Flag"] = 0,
            ["Counter"] = 1,
            ["Slider"] = 2,
        },
        editable = false,
    }
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Flag", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
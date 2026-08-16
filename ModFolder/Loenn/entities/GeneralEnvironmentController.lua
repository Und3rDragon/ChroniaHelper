local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/GeneralEnviromentController"
controller.placements = {
    name = "controller",
    data = {
        chroniaFlagLogicExpression = "See tooltip",
        chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",

        parameters = "",
        mode = 7,

        fadeTime = -1;
        bloomBaseTo = "";
        --bloomColorTo = "";
        bloomStrengthTo = "";
        lightingBaseTo = "";
        lightingAddTo = "";
        lightingColorTo = "";
    },
}

controller.fieldOrder = {
    "_x", "_y", "x", "y", "_id", "_name",
    "chroniaMathExpession", "frostSessionExpression",
}

controller.fieldInformation = {
    chroniaFlagLogicExpression = {
        editable = false,
    },
    chroniaMathExpession = {
        editable = false,
    },
    mode = require("mods").requireFromPlugin("consts.field_options").generalSetup,
    flags = {
        fieldType = "list",
    },
    parameters = {
        fieldType = "list",
    },
    bloomColorTo = {
        fieldType = "color",
        allowEmpty = true,
    },
    lightingColorTo = {
        fieldType = "color",
        allowEmpty = true,
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Environment", entity)

    table.insert(sprite, iconSprite)
    
    --local text = require("structs.drawable_text").fromText(entity.flags, entity.x + 12, entity.y - 12, 48, 24)
    
    --table.insert(sprite, text)
    
    return sprite
end

return controller
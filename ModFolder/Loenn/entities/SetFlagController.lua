local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/SetFlagController"
controller.placements = {
    name = "controller",
    data = {
        chroniaFlagLogicExpression = "See tooltip",
        chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",
        flags = "flag",
        parameters = "",
        mode = 7,
        valueType = 0,
    },
}

controller.associatedMods = function(entity)
    if entity["mode"] == nil then
      return {"ChroniaHelper"}
    end
    
    if entity.mode == 11 or entity.mode == 15 or entity.mode == 19 then
      return {"ChroniaHelper", "FrostHelper"}
    else
      return {"ChroniaHelper"}
    end
end

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
        fieldType = "list_scroll",
    },
    parameters = {
        fieldType = "list_scroll",
    },
    valueType = {
        fieldType = "integer",
        options = {
            ["General Set"] = 0,
            ["General Toggle"] = 1,
        },
        editable = false,
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/Flag", entity)

    table.insert(sprite, iconSprite)
    
    local text = require("structs.drawable_text").fromText(entity.flags, entity.x + 12, entity.y - 12, 48, 24)
    
    table.insert(sprite, text)
    
    return sprite
end

return controller
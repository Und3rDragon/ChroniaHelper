local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/SliderWindController"
controller.placements = {
    name = "controller",
    data = {
        chroniaFlagLogicExpression = "See tooltip",
        chroniaMathExpession = "See tooltip",
        frostSessionExpression = "https://github.com/JaThePlayer/FrostHelper/wiki/Session-Expressions",
        parameters = "",
        mode = 1,

        speedX = "0",
        speedY = "0",

        otherActivators = -1,
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
    parameters = {
        fieldType = "list_scroll",
    },
    otherActivators = {
        fieldType = "integer",
        options = {
            ["Default"] = -1,
            ["Seeds"] = 0,
            ["Strawberry"] = 1,
            ["Locked Door"] = 2,
            ["Refill"] = 2,
            ["Jellyfish"] = 2,
            ["Theo"] = 2,
            ["Core Mode Hot"] = 3,
            ["Core Mode Cold"] = 4,
            ["Death"] = 5,
        },
        editable = false,
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/SliderWind", entity)

    table.insert(sprite, iconSprite)
    
    local text = require("structs.drawable_text").fromText(entity.speedX .. ", " .. entity.speedY, entity.x + 12, entity.y - 12, 48, 24)
    
    table.insert(sprite, text)
    
    return sprite
end

return controller
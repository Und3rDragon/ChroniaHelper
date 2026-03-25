local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depths")

local controller = {}

controller.name = "ChroniaHelper/RandomMusicController"
controller.placements = {
    name = "Controller",
    data = {
        musics = "music_city,60;music_credits,60",
        mode = 0,
        startDelay = 0,
        allowRepeat = true,
        global = false,
    },
}

controller.fieldInformation = {
    mode = {
        fieldType = "integer",
        options = {
            ["On Added"] = 0,
        },
        editable = false,
    },
    value1 = {
        fieldType = "integer",
    },
    value2 = {
        fieldType = "integer",
    },
    musics = {
        fieldType = "list",
        elementSeparator = ";",
        minimumElements = 2,
        elementOptions = {
            fieldType = "list",
            minimumElements = 2,
            maximumElements = 2,
        },
    },
}

controller.sprite = function(room, entity)
	local sprite = {}
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/RandomMusic", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local controller = {}

controller.name = "ChroniaHelper/FishPriceController"

controller.placements = {
    name = "controller",
    data = {
        FishingCreditsCounter = "ChroniaHelper_LakesideFishScore",
        --DefaultPrize = 0,
        --FishCoinPrize = 20,
        FishBassPrize = 100,
        FishTroutPrize = 100,
        FishSpringPrize = 150,
        FishStonePrize = 180,
        FishStoneEaterPrize = 200,
        FishBlahajPrize = 220,
        FishBombPrize = 250,
        FishLeafPrize = 280,
        FishAngelPrize = 350,
        FishDevilPrize = 400,
        FishCookedPrize = 600,
        FishMythicPrize = 1000,
        --DefaultRandomness = 0,
        --FishCoinRandomness = 0,
        FishBassRandomness = 10,
        FishTroutRandomness = 10,
        FishSpringRandomness = 20,
        FishStoneRandomness = 20,
        FishStoneEaterRandomness = 40,
        FishBlahajRandomness = 40,
        FishBombRandomness = 50,
        FishLeafRandomness = 80,
        FishAngelRandomness = 100,
        FishDevilRandomness = 100,
        FishCookedRandomness = 150,
        FishMythicRandomness = 200,
    },
}

controller.ignoredFields = {
    "_x", "_y", "x", "y", "_id", "_name"
}

controller.fieldInformation = 
{
    DefaultPrize = {
        fieldType = "integer",
    },
    FishCoinPrize = {
        fieldType = "integer",
    },
    FishBassPrize = {
        fieldType = "integer",
    },
    FishTroutPrize = {
        fieldType = "integer",
    },
    FishSpringPrize = {
        fieldType = "integer",
    },
    FishStonePrize = {
        fieldType = "integer",
    },
    FishStoneEaterPrize = {
        fieldType = "integer",
    },
    FishBlahajPrize = {
        fieldType = "integer",
    },
    FishBombPrize = {
        fieldType = "integer",
    },
    FishLeafPrize = {
        fieldType = "integer",
    },
    FishAngelPrize = {
        fieldType = "integer",
    },
    FishDevilPrize = {
        fieldType = "integer",
    },
    FishCookedPrize = {
        fieldType = "integer",
    },
    FishMythicPrize = {
        fieldType = "integer",
    },
    DefaultRandomness = {
        fieldType = "integer",
    },
    FishCoinRandomness = {
        fieldType = "integer",
    },
    FishBassRandomness = {
        fieldType = "integer",
    },
    FishTroutRandomness = {
        fieldType = "integer",
    },
    FishSpringRandomness = {
        fieldType = "integer",
    },
    FishStoneRandomness = {
        fieldType = "integer",
    },
    FishStoneEaterRandomness = {
        fieldType = "integer",
    },
    FishBlahajRandomness = {
        fieldType = "integer",
    },
    FishBombRandomness = {
        fieldType = "integer",
    },
    FishLeafRandomness = {
        fieldType = "integer",
    },
    FishAngelRandomness = {
        fieldType = "integer",
    },
    FishDevilRandomness = {
        fieldType = "integer",
    },
    FishCookedRandomness = {
        fieldType = "integer",
    },
    FishMythicRandomness = {
        fieldType = "integer",
    },
}

function controller.sprite(room, entity)
    local sprite = {}
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 16, 16, {0.0, 0.0, 0.0})
    local iconSprite = drawableSprite.fromTexture("ChroniaHelper/LoennIcons/FishPriceController", entity)

    table.insert(sprite, iconSprite)
    return sprite
end

return controller
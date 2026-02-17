local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('helpers.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local reskinJelly = {
    name = "ChroniaHelper/CustomGliderXML",
    fieldOrder = {
        "x","y",
        'Directory','Depth',
        'GlidePath','GlowPath',
        'GlideColor1','GlideColor2',
        'GlowColor1','GlowColor2',
        "droppingSpeedX", "droppingSpeedY",
        "accelerationX", "accelerationY",
        "idlePath","idleAnimInterval",
        "heldPath","heldAnimInterval",
        "fallPath","fallAnimInterval",
        "fallLoopPath","fallLoopAnimInterval",
        "deathPath","deathAnimInterval",
        "respawnPath","respawnAnimInterval",
        "hitboxParameters","holdableParameters",
        "tutorialTitle","tutorialText",
        'bubble','tutorial',
    },
    fieldInformation = {
        Directory = vivUtilsMig.getDirectoryPathFromFile(false),
        Depth = require("mods").requireFromPlugin("helpers.field_options").depths,
        GlidePath = vivUtilsMig.GetFilePathWithNoTrailingNumbers(true),
        GlowPath = vivUtilsMig.GetFilePathWithNoTrailingNumbers(true),
        GlideColor1 = {fieldType = 'color', allowXNAColors = true, useAlpha = true, allowEmpty = true },
        GlideColor2 = {fieldType = 'color', allowXNAColors = true, useAlpha = true, allowEmpty = true },
        GlowColor1 = {fieldType = 'color', allowXNAColors = true, useAlpha = true, allowEmpty = true },
        GlowColor2 = {fieldType = 'color', allowXNAColors = true, useAlpha = true, allowEmpty = true },
        accelerationX = {
            minimumValue = 0,
        },
        accelerationY = {
            minimumValue = 0,
        },
        tutorialText = {
            fieldType = "list",
        },
    }
}

reskinJelly.ignoredFields = function(entity)
    local attrs = {"_id", "_name", "XMLOverride"}
    
    local gfxAttrs = {
        "idleAnimInterval",
        "heldAnimInterval",
        "fallAnimInterval",
        "fallLoopAnimInterval",
        "deathAnimInterval",
    }

    if entity.XMLOverride then
        for _, item in ipairs(gfxAttrs) do
            table.insert(attrs, item)
        end
    end

    return attrs
end

reskinJelly.placements = {
    name = "glider",
    data = {
        bubble = false, tutorial = false,
        Depth = -5,
        droppingSpeedX = 0,
        droppingSpeedY = 30,
        accelerationX = 1,
        accelerationY = 1,
        GlidePath = "particles/rect", GlowPath = "",
        GlideColor1="4FFFF3", GlideColor2="FFF899",
        GlowColor1="B7F3FF", GlowColor2="F4FDFF",
        Directory = "glider",
        --idlePath = "idle",
        --idleAnimInterval = 0.1,
        --heldPath = "held",
        --heldAnimInterval = 0.1,
        --fallPath = "fall",
        --fallAnimInterval = 0.06,
        --fallLoopPath = "fallLoop",
        --fallLoopAnimInterval = 0.06,
        --deathPath = "death",
        --deathAnimInterval = 0.06,
        --respawnPath = "respawn",
        --respawnAnimInterval = 0.03,
        hitboxParameters = "8,10,-4,-10",
        holdableParameters = "20,22,-10,-16",
        tutorialTitle = "",
        tutorialText = "",
        droppingSpeedXMultiplier = false,
        XMLOverride = true,
        outline = true,
    }
}

reskinJelly.depth = function(room,entity) return entity.Depth or -5 end

reskinJelly.sprite = function(room,entity)
    local sprite = vivUtilsMig.getImageWithNumbers(entity.Directory .. "/idle", 0, entity)
    if entity.XMLOverride then
        sprite = vivUtilsMig.getImageWithNumbers("objects/ChroniaHelper/customJelly" .. "/idle", 0, entity)
    end

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()

        table.insert(lineSprites, sprite)

        return lineSprites
    else
        return sprite 
    end
end

reskinJelly.selection = function(room, entity)
    return require('utils').rectangle(entity.x - 16, entity.y - 14, 32, 16)
end

return reskinJelly
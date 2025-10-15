local vivUtils = require("mods").requireFromPlugin("helpers.vivUtilsMig")
local fo = require("mods").requireFromPlugin("helpers.field_options")
local drawableSprite = require("structs.drawable_sprite")
local frostUtils = require("mods").requireFromPlugin("helpers.frostUtils")
local seamlessSpinner = {}

seamlessSpinner.name = "ChroniaHelper/SeamlessSpinner"
seamlessSpinner.placements = {
    name = "SeamlessSpinner",
    data = {
        foreDirectory = "blue",
        backDirectory = "blue",
        attachToSolid = false,
        hitboxType = "default",
        --dust = false,
        customHitbox = "r,16,16,-8,-8;c,8,-8,-8",
        colorOverlay = "ffffff",
        bgColorOverlay = "",
        depth = -8500,
        --solidTileCutoff = true,
        noBorder = false,
        bloomAlpha = 0.0,
        bloomRadius = 0.0,
        dynamic = false,
        fgAnimDelay = 0.1,
        fgAnimRandomize = 0.0,
        bgAnimDelay = 0.1,
        bgAnimRandomize = 0.0,
        triggerDelay = 0.5,
        triggerAnimDelay = 0.05,
        fgFlipX = "none",
        fgFlipY = "none",
        bgFlipX = "none",
        bgFlipY = "none",
        trigger = false,
        rainbow = false,
        
        useCoreModeStyle = false,
        coldCoreModeBGSpritePath = "danger/crystal/bg_blue",
        hotCoreModeBGSpritePath = "danger/crystal/bg_red",
        
        coldCoreModeSpritePath = "danger/crystal/fg_blue",
        hotCoreModeSpritePath = "danger/crystal/fg_red",
        
        coldCoreModeTriggerSpritePath = "objects/ChroniaHelper/timedSpinner/blue/fg_blue_base",
        hotCoreModeTriggerSpritePath = "objects/ChroniaHelper/timedSpinner/red/fg_red_base",
    },
}
seamlessSpinner.depth = function(room,entity) return entity.depth or -8500 end
seamlessSpinner.fieldInformation = {
    foreDirectory = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    backDirectory = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    coldCoreModeBGSpritePath = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    hotCoreModeBGSpritePath = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    coldCoreModeSpritePath = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    hotCoreModeSpritePath = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    coldCoreModeTriggerSpritePath = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    hotCoreModeTriggerSpritePath = vivUtils.GetFilePathWithNoTrailingNumbers(false),
    hitboxType={
        options={
            "default",
            "loosened",
            "seamlessRound",
            "seamlessSquare",
            "custom",
        },
        editable = false,
    },
    depth = fo.depths,
    customHitbox = {
        fieldType = "list",
        elementSeparator = ";",
    },
    colorOverlay = {
        fieldType = "color",
    },
    bgColorOverlay = {
        fieldType = "color",
        allowEmpty = true,
    },
    fgFlipX = {
        options = {"none", "flipped", "random"},
        editable = false,
    },
    fgFlipY = {
        options = {"none", "flipped", "random"},
        editable = false,
    },
    bgFlipX = {
        options = {"none", "flipped", "random"},
        editable = false,
    },
    bgFlipY = {
        options = {"none", "flipped", "random"},
        editable = false,
    },
}

seamlessSpinner.fieldOrder = {
    -- texts
    "foreDirectory", "backDirectory",
    "depth", "useCoreModeStyle",
    "coldCoreModeBGSpritePath", "hotCoreModeBGSpritePath",
    "coldCoreModeSpritePath", "hotCoreModeSpritePath",
    "coldCoreModeTriggerSpritePath", "hotCoreModeTriggerSpritePath",
    "hitboxType", 
    "customHitbox", "colorOverlay",
    "bloomAlpha", "bloomRadius",
    "fgAnimDelay", "bgAnimDelay",
    "fgAnimRandomize", "bgAnimRandomize",
    "triggerDelay","triggerAnimDelay",
    "bgColorOverlay", "fgFlipX",
    "fgFlipY", "bgFlipX",
    "bgFlipY",
    -- bools
    "attachToSolid", "dust", "rainbow",
    "dynamic", "solidTileCutoff", 
    "noBorder","trigger",
}

seamlessSpinner.ignoredFields = function(entity)
    local attrs = { "x", "y", "_id", "_name"}
    local non_dyn = {"solidTileCutoff", "dust"}
    local dyn = {"fgAnimDelay", "bgAnimDelay","fgAnimRandomize", "bgAnimRandomize", "fgFlipX", "fgFlipY", "bgFlipX", "bgFlipY"}
    local core = {"coldCoreModeBGSpritePath", "hotCoreModeBGSpritePath",
    "coldCoreModeSpritePath", "hotCoreModeSpritePath",
    "coldCoreModeTriggerSpritePath", "hotCoreModeTriggerSpritePath",
    }

    if entity.dynamic then
        for _, item in ipairs (non_dyn) do
            table.insert(attrs, item)
        end
    else
        for _, item in ipairs (dyn) do
            table.insert(attrs, item)
        end
    end

    if not entity.useCoreModeStyle then
        for _, item in ipairs (core) do
            table.insert(attrs, item)
        end
    end

    return attrs

end

function chooseSprite(entity)
    if entity.foreDirectory == "blue" or entity.foreDirectory == "Blue" then
        return "objects/ChroniaHelper/timedSpinner/blue/fg_blue00"
    end
    if entity.foreDirectory == "red" or entity.foreDirectory == "Red" then
        return "objects/ChroniaHelper/timedSpinner/red/fg_red00"
    end
    if entity.foreDirectory == "Purple" or entity.foreDirectory == "purple" then
        return "objects/ChroniaHelper/timedSpinner/purple/fg_purple00"
    end
    if entity.foreDirectory == "white" or entity.foreDirectory == "White" then
        return "objects/ChroniaHelper/timedSpinner/white/fg_white00"
    end
    if entity.foreDirectory == "Rainbow" or entity.foreDirectory == "rainbow" then
        return "objects/ChroniaHelper/timedSpinner/white/fg_white00"
    end
    return entity.foreDirectory .. "00"
end

seamlessSpinner.sprite = function(room,entity)
    local sprite = drawableSprite.fromTexture(chooseSprite(entity), entity)
	sprite:setColor(frostUtils.getColor(entity.colorOverlay))
	return sprite
end

seamlessSpinner.selection = function(room, entity)
    return require('utils').rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return seamlessSpinner
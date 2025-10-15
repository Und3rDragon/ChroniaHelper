local defaultFields = require("mods").requireFromPlugin("consts.default_fields")
local core = require("mods").requireFromPlugin("utils.core")

local fieldTable = {
    sprite =
    {
        data = "",
        info = require("mods").requireFromPlugin("helpers.vivUtilsMig").getDirectoryPathFromFile(true),
    },
    spriteColor =
    {
        data = "ffffff",
        info = {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = false,
        }
    },
    respawnTime =
    {
        data = 2.5,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0.0167
        }
    },
    freeze =
    {
        data = 3,
        info = defaultFields.freeze.info
    },
    fewerMode =
    {
        data = "AnyOne",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
                "AnyOne",
                "Dashes",
                "Stamina",
                "All"
            },
            editable = false
        }
    },
    resetMode =
    {
        data = "All",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
                "Dashes",
                "Stamina",
                "All",
                "RandomAnyOne",
                "RandomDashes",
                "RandomStamina",
                "RandomAll"
            },
            editable = false
        }
    },
    fewerDashes =
    {
        data = - 1,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = - 1
        }
    },
    resetDashes =
    {
        data = - 1,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = - 1
        }
    },
    fewerStamina =
    {
        data = 20,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    resetStamina =
    {
        data = 110,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    touchSound =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    respawnSound =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    outlineColor =
    {
        data = "000000",
        info =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        }
    },
    outlineWidth =
    {
        data = 1,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    bloomAlpha =
    {
        data = 0.8,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    bloomRadius =
    {
        data = 16,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    particleShatterColor1 =
    {
        data = "",
        info =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        }
    },
    particleShatterColor2 =
    {
        data = "",
        info =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        }
    },
    particleRegenColor1 =
    {
        data = "",
        info =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        }
    },
    particleRegenColor2 =
    {
        data = "",
        info =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        }
    },
    particleGlowColor1 =
    {
        data = "",
        info =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        }
    },
    particleGlowColor2 =
    {
        data = "",
        info =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        }
    },
    waveFrequency =
    {
        data = 0.6,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    waveRadius =
    {
        data = 2,
        info =
        {
            fieldType = "number",
            allowEmpty = false
        }
    },
    waveMode =
    {
        data = "Vertical",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
                "Vertical",
                "Horizontal",
                "ForwardSlash",
                "BackSlash",
                "Clockwise",
                "CounterClockwise"
            },
            editable = false
        }
    },
    depth = {
        data = -100,
        info = {
            options = require("mods").requireFromPlugin("consts.depths"),
            editable = true,
        }
    },
    onlyOnce = defaultFields.onlyOnce,
    twoDashes =
    {
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    }
}

function ignoreAttr(entity)
    local attrs = {
        "_id",
        "_name",
    }

    local fewerattrs = {
        "fewerDashes",
        "fewerStamina",
    }

    local resetattrs = {
        "resetDashes",
        "resetStamina",
    }

    local customizeattrs = {
        "spriteColor",
        "touchSound",
        "respawnSound",
        "outlineColor",
        "outlineWidth",
        "bloomAlpha",
        "bloomRadius",
        "particleShatterColor1",
        "particleShatterColor2",
        "particleRegenColor1",
        "particleRegenColor2",
        "particleGlowColor1",
        "particleGlowColor2",
        "waveFrequency",
        "waveRadius",
        "waveMode",
    }

    local animparams = {
        "idleAnimInterval",
        "flashAnimInterval",
    }

    if entity.fewerMode == "None" then
        for _, item in ipairs(fewerattrs) do
            table.insert(attrs, item)
        end
    end

    if entity.resetMode == "None" then
        for _, item in ipairs(resetattrs) do
            table.insert(attrs, item)
        end
    end

    if  not entity.moreCustomOptions then
        for _, item in ipairs(customizeattrs) do
            table.insert(attrs, item)
        end
    end

    if  not entity.customAnimation then
        for _, item in ipairs(animparams) do
            table.insert(attrs, item)
        end
    end

    return attrs
end

local refill = {
    name = "ChroniaHelper/Refill",
    placements =
    {
        {
            name = "Refill",
            data =
            {
                twoDashes = false,
                moreCustomOptions = false,
                customAnimation = false,
                idleAnimInterval = 0.1,
                flashAnimInterval = 0.05,
                flagOnCollected = "Chronia_Refill_Flag_On_Collected"
            }
        },
        {
            name = "TwoDashesRefill",
            data =
            {
                twoDashes = true,
                moreCustomOptions = false,
                customAnimation = false,
                idleAnimInterval = 0.1,
                flashAnimInterval = 0.05,
                flagOnCollected = "Chronia_Refill_Flag_On_Collected"
            }
        }
    },
    fieldInformation = { },
    fieldOrder =
    {
        "x",
        "y",
        "sprite",
        "spriteColor",
        "respawnTime",
        "freeze",
        "fewerMode",
        "resetMode",
        "fewerDashes",
        "resetDashes",
        "fewerStamina",
        "resetStamina",
        "touchSound",
        "respawnSound",
        "outlineColor",
        "outlineWidth",
        "bloomAlpha",
        "bloomRadius",
        "particleShatterColor1",
        "particleShatterColor2",
        "particleRegenColor1",
        "particleRegenColor2",
        "particleGlowColor1",
        "particleGlowColor2",
        "waveFrequency",
        "waveRadius",
        "waveMode",
        "depth",
        "onlyOnce",
        "twoDashes"
    },
    ignoredFields = function(entity)
        return ignoreAttr(entity)
    end,
    texture = function(room, entity)
        if entity.sprite == "" then
            return not(entity.twoDashes) and "objects/refill/idle00" or "objects/refillTwo/idle00"
        else
            return string.sub(entity.sprite, -1) == "/" and entity.sprite .. "/idle00" or entity.sprite
        end
    end,
    depth = function(room,entity) return entity.depth end
}

local refillWall = {
    name = "ChroniaHelper/RefillWall",
    placements =
    {
        {
            name = "RefillWall",
            data =
            {
                width = defaultFields.width.data,
                height = defaultFields.height.data,
                borderColor = "",
                borderAlpha = 0.2,
                innerColor = "",
                innerAlpha = 0.1,
                twoDashes = false,
                moreCustomOptions = false,
                customAnimation = false,
                idleAnimInterval = 0.1,
                flashAnimInterval = 0.05,
            }
        },
        {
            name = "TwoDashesRefillWall",
            data =
            {
                width = defaultFields.width.data,
                height = defaultFields.height.data,
                borderColor = "",
                borderAlpha = 0.2,
                innerColor = "",
                innerAlpha = 0.1,
                twoDashes = true,
                moreCustomOptions = false,
                customAnimation = false,
                idleAnimInterval = 0.1,
                flashAnimInterval = 0.05,
            }
        }
    },
    fieldInformation =
    {
        width = defaultFields.width.info,
        height = defaultFields.height.info,
        borderColor =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        },
        borderAlpha =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        },
        innerColor =
        {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true,
            allowEmpty = true
        },
        innerAlpha =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "sprite",
        "spriteColor",
        "borderColor",
        "borderAlpha",
        "innerColor",
        "innerAlpha",
        "respawnTime",
        "freeze",
        "fewerMode",
        "resetMode",
        "fewerDashes",
        "resetDashes",
        "fewerStamina",
        "resetStamina",
        "touchSound",
        "respawnSound",
        "outlineColor",
        "outlineWidth",
        "bloomAlpha",
        "bloomRadius",
        "particleShatterColor1",
        "particleShatterColor2",
        "particleRegenColor1",
        "particleRegenColor2",
        "particleGlowColor1",
        "particleGlowColor2",
        "waveFrequency",
        "waveRadius",
        "waveMode",
        "depth",
        "onlyOnce",
        "twoDashes"
    },
    ignoredFields = function(entity)
        return ignoreAttr(entity)
    end,
    sprite = function(room, entity, viewport)
        local path = "objects/refill/idle00"
        local borderColor = "#d3ffd4"
        local innerColor = "#a5fff7"
        if entity.twoDashes then
            path = "objects/refillTwo/idle00"
            borderColor = "#ffd3f9"
            innerColor = "#ffa5aa"
        end
        if entity.sprite ~= "" then
            path = string.sub(entity.sprite, -1) == "/" and entity.sprite .. "/idle00" or entity.sprite
        end
        if entity.borderColor ~= "" then
            borderColor = entity.borderColor
        end
        if entity.innerColor ~= "" then
            innerColor = entity.innerColor
        end

        local renderBorderAlpha = entity.borderAlpha
        local renderInnerAlpha = entity.innerAlpha

        if renderBorderAlpha <= 0.1 then
            renderBorderAlpha = 0.2
        end
        if renderInnerAlpha <= 0.1 then
            renderInnerAlpha = 0.1
        end
        borderColor = borderColor .. string.format("%x",(255 * renderBorderAlpha))
        innerColor = innerColor .. string.format("%x",(255 * renderInnerAlpha))
        return {
            require("structs.drawable_rectangle").fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,innerColor,borderColor),
            require("structs.drawable_sprite").fromTexture(path,entity):setPosition(entity.x + entity.width / 2,entity.y + entity.height / 2)
        }
    end,
    depth = function(room,entity) return entity.depth end
}

local refills = {
    refill,
    refillWall
}

core.fieldCopy(fieldTable, refills)

return refills
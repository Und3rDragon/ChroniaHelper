local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    category = "visual",
    name = "ChroniaHelper/BloomTrigger",
    placements =
    {
        name = "BloomTrigger",
        data =
        {
            bloomBase = 0,
            bloomStrength = 1,
            bloomColor = "ffffff",
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            enterIfFlag = "",
            enterSound = "",
            leaveMode = "Any",
            leaveDelay = 0,
            leaveIfFlag = "",
            leaveSound = "",
            onlyOnce = false,
            leaveReset = false
        }
    },
    fieldInformation =
    {
        bloomBase =
        {
            minimumValue = 0
        },
        bloomStrength =
        {
            minimumValue = 0
        },
        bloomColor =
        {
            fieldType = "color"
        },
        enterMode = fieldOptions.enterMode,
        enterDelay =
        {
            minimumValue = 0
        },
        leaveMode = fieldOptions.leaveMode,
        leaveDelay =
        {
            minimumValue = 0
        }
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "bloomBase",
        "bloomStrength",
        "bloomColor",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "enterIfFlag",
        "enterSound",
        "leaveMode",
        "leaveDelay",
        "leaveIfFlag",
        "leaveSound",
        "onlyOnce",
        "leaveReset"
    },
    triggerText = function(room, entity)
        return "Bloom=" .. entity.bloomBase .. " " .. "Strth=" .. entity.bloomStrength .. " " .. "Color=" .. entity.bloomColor
    end
}
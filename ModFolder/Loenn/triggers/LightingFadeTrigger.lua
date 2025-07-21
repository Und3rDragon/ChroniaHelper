local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    category = "visual",
    name = "ChroniaHelper/LightingFadeTrigger",
    placements =
    {
        name = "LightingFadeTrigger",
        data =
        {
            lightingColorFrom = "000000",
            lightingColorTo = "000000",
            lightingAlphaFrom = 0,
            lightingAlphaTo = 0,
            positionMode = "NoEffect",
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
            timed = -1,
            onlyOnce = false,
            leaveReset = false
        }
    },
    fieldInformation =
    {
        lightingColorFrom =
        {
            fieldType = "color"
        },
        lightingColorTo =
        {
            fieldType = "color"
        },
        lightingAlphaFrom =
        {
            minimumValue = 0
        },
        lightingAlphaTo =
        {
            minimumValue = 0
        },
        positionMode = fieldOptions.positionMode,
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
        "lightingColorFrom",
        "lightingColorTo",
        "lightingAlphaFrom",
        "lightingAlphaTo",
        "positionMode",
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
    }
}
local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    category = "visual",
    name = "ChroniaHelper/LightingTrigger",
    placements =
    {
        name = "LightingTrigger",
        data =
        {
            lightingColor = "000000",
            lightingAlpha = 0,
            lightingAlphaAdd = 0,
            baseLightingAlpha = 0,
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
            changeLightingColor = true,
            changeLightingAlpha = true,
            changeLightingAlphaAdd = false,
            changeBaseLightingAlpha = false,
            onlyOnce = false,
            leaveReset = false,
        }
    },
    fieldInformation =
    {
        lightingColor =
        {
            fieldType = "color"
        },
        lightingAlpha =
        {
            minimumValue = 0
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
        "lightingColor",
        "lightingAlpha",
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
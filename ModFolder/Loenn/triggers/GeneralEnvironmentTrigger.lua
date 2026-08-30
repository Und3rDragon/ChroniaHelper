local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    category = "visual",
    name = "ChroniaHelper/GeneralEnviromentTrigger",
    placements =
    {
        name = "EnvironmentTrigger",
        data =
        {
            fadeTime = -1,
            bloomBaseTo = "",
            bloomColorTo = "",
            bloomStrengthTo = "",
            lightingTo = "",
            lightingColorTo = "",
            bloomBaseFrom = "",
            bloomColorFrom = "",
            bloomStrengthFrom = "",
            lightingFrom = "",
            lightingColorFrom = "",
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
            onlyOnce = false,
            leaveReset = false
        }
    },
    fieldInformation =
    {
        bloomColorFrom =
        {
            fieldType = "color",
            allowEmpty = true,
        },
        bloomColorTo =
        {
            fieldType = "color",
            allowEmpty = true,
        },
        lightingColorFrom =
        {
            fieldType = "color",
            allowEmpty = true,
        },
        lightingColorTo =
        {
            fieldType = "color",
            allowEmpty = true,
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
        "bloomBaseTo",
        "bloomColorTo",
        "bloomStrengthTo",
        "lightingTo",
        "lightingColorTo",
        "bloomBaseFrom",
        "bloomColorFrom",
        "bloomStrengthFrom",
        "lightingFrom",
        "lightingColorFrom",
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
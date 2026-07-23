local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    category = "visual",
    name = "ChroniaHelper/BloomFadeTrigger",
    placements =
    {
        name = "BloomFadeTrigger",
        data =
        {
            bloomBaseFrom = "",
            bloomBaseTo = "",
            bloomStrengthFrom = "",
            bloomStrengthTo = "",
            bloomColorFrom = "",
            bloomColorTo = "",
            positionMode = "NoEffect",
            timedFade = -1,
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
        "bloomBaseFrom",
        "bloomBaseTo",
        "bloomStrengthFrom",
        "bloomStrengthTo",
        "bloomColorFrom",
        "bloomColorTo",
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
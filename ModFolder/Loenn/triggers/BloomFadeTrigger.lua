local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    category = "visual",
    name = "ChroniaHelper/BloomFadeTrigger",
    placements =
    {
        name = "BloomFadeTrigger",
        data =
        {
            bloomBaseFrom = 0,
            bloomBaseTo = 0,
            bloomStrengthFrom = 1,
            bloomStrengthTo = 1,
            bloomColorFrom = "ffffff",
            bloomColorTo = "ffffff",
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
        bloomBaseFrom =
        {
            minimumValue = 0
        },
        bloomBaseTo =
        {
            minimumValue = 0
        },
        bloomStrengthFrom =
        {
            minimumValue = 0
        },
        bloomStrengthTo =
        {
            minimumValue = 0
        },
        bloomColorFrom =
        {
            fieldType = "color"
        },
        bloomColorTo =
        {
            fieldType = "color"
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
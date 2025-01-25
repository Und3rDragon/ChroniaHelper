local fieldOptions = require("mods").requireFromPlugin("consts.field_options")
local celesteData = require("mods").requireFromPlugin("consts.celeste_data")

return {
    category = "audio",
    name = "ChroniaHelper/AmbienceTrigger",
    placements =
    {
        name = "AmbienceTrigger",
        data =
        {
            ambienceTrack = "",
            ambienceProgress = 0,
            ambienceParameter = "",
            ambienceParameterValue = 1,
            ambienceVolume = 1,
            ambienceLayers = "",
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
        ambienceTrack =
        {
            options = celesteData.ambiences,
            editable = true
        },
        ambienceProgress =
        {
            fieldType = "integer",
            minimumValue = 0
        },
        ambienceParameterValue =
        {
            minimumValue = 0
        },
        ambienceVolume =
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
        "ambienceTrack",
        "ambienceProgress",
        "ambienceParameter",
        "ambienceParameterValue",
        "ambienceVolume",
        "ambienceLayers",
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
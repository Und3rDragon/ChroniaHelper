local fieldOptions = require("mods").requireFromPlugin("consts.field_options")
local celesteData = require("mods").requireFromPlugin("consts.celeste_data")

return {
    category = "audio",
    name = "ChroniaHelper/AmbienceFadeTrigger",
    placements =
    {
        name = "AmbienceFadeTrigger",
        data =
        {
            ambienceVolumeFrom = 1,
            ambienceVolumeTo = 1,
            ambienceParameterValueFrom = 1,
            ambienceParameterValueTo = 1,
            ambienceParameter = "",
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
        ambienceVolumeFrom =
        {
            minimumValue = 0
        },
        ambienceVolumeTo =
        {
            minimumValue = 0
        },
        ambienceParameterValueFrom =
        {
            minimumValue = 0
        },
        ambienceParameterValueTo =
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
        "ambienceVolumeFrom",
        "ambienceVolumeTo",
        "ambienceParameterValueFrom",
        "ambienceParameterValueTo",
        "ambienceParameter",
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
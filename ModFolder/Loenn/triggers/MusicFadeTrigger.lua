local fieldOptions = require("mods").requireFromPlugin("consts.field_options")
local celesteData = require("mods").requireFromPlugin("consts.celeste_data")

return {
    category = "audio",
    name = "ChroniaHelper/MusicFadeTrigger",
    placements =
    {
        name = "MusicFadeTrigger",
        data =
        {
            musicParameterValueFrom = 1,
            musicParameterValueTo = 0,
            musicParameter = "",
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
        musicParameterValueFrom =
        {
            minimumValue = 0
        },
        musicParameterValueTo =
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
        "musicParameterValueFrom",
        "musicParameterValueTo",
        "musicParameter",
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
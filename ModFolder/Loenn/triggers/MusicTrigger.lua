local fieldOptions = require("mods").requireFromPlugin("consts.field_options")
local celesteData = require("mods").requireFromPlugin("consts.celeste_data")

return {
    name = "ChroniaHelper/MusicTrigger",
    placements =
    {
        name = "MusicTrigger",
        data =
        {
            musicTrack = "",
            musicProgress = 0,
            musicParameter = "",
            musicParameterValue = 1,
            musicLayers = "",
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
            leaveReset = false,
            resetOnDeath = false,
        }
    },
    fieldInformation =
    {
        musicTrack =
        {
            options = celesteData.musics,
            editable = true
        },
        musicProgress =
        {
            fieldType = "integer",
            minimumValue = 0
        },
        musicParameterValue =
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
        "musicTrack",
        "musicProgress",
        "musicParameter",
        "musicParameterValue",
        "musicLayers",
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
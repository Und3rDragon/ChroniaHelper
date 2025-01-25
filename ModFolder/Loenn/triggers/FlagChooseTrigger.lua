local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagChooseTrigger",
    placements =
    {
        name = "FlagChooseTrigger",
        data =
        {
            flagDictionary = "",
            defaultFlag = "",
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            leaveMode = "Any",
            leaveDelay = 0,
            onlyOnce = false,
            leaveReset = false,
            deleteKeyFlag = false,
            multiExecute = false
        }
    },
    fieldInformation =
    {
        enterMode = fieldOptions.enterMode,
        enterDelay =
        {
            minimumValue = 0
        },
        leaveMode = fieldOptions.leaveMode,
        leaveDelay =
        {
            minimumValue = 0
        },
        flagDictionary = {
            fieldType = "list",
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "flagDictionary",
        "defaultFlag",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset",
        "deleteKeyFlag",
        "multiExecute"
    }
}
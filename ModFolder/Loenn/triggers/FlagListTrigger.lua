﻿local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagListTrigger",
    placements =
    {
        name = "FlagListTrigger",
        data =
        {
            flagList = "",
            index = 0,
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            leaveMode = "Any",
            leaveDelay = 0,
            onlyOnce = false,
            leaveReset = false,
            reverse = false
        }
    },
    fieldInformation =
    {
        index =
        {
            fieldType = "integer",
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
        },
        flagList = {
            fieldType = "list",
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "flagList",
        "index",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset",
        "reverse"
    }
}
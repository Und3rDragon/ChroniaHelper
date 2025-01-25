return {
    name = "ChroniaHelper/FlagStateTrigger",
    placements =
    {
        name = "FlagStateTrigger",
        data =
        {
            flag = "",
            stateMode = "None",
            stateDelay = 0,
            levelDeath = "-1",
            totalDeath = "-1",
            onlyOnce = false,
            persistent = true
        }
    },
    fieldInformation =
    {
        stateMode =
        {
            options =
            {
                "None",
                "Add",
                "Remove",
                "Clear"
            },
            editable = false
        },
        stateDelay =
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
        "flag",
        "stateMode",
        "stateDelay",
        "levelDeath",
        "totalDeath",
        "onlyOnce",
        "persistent"
    }
}
local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagToggleTrigger",
    placements =
    {
        name = "trigger",
        data =
        {
            flag = "flag",
            enterDelay = -1,
        }
    },
    fieldInformation =
    {
        
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset"
    }
}
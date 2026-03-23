local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/ClearPackedFlagsTrigger",
    placements =
    {
        name = "trigger",
        data =
        {
            labels = "pack",
            mode = 0,
        }
    },
    fieldInformation =
    {
        mode = {
            options = {
                ["Remove Flags and Tags"] = 0,
                ["Remove Tags"] = 1,
            },
            editable = false,
        },
        labels = {
            fieldType = "list",
            minimumElements = 1,
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "ifFlag",
        "trueFlag",
        "falseFlag",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset",
        "deleteIfFlag"
    }
}
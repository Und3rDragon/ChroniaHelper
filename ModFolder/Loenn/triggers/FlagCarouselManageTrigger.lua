local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        return "Flag Carousel Manage" .. " id= " .. entity.carouselId .. "(" .. entity.carouselMode .. ")"
    end,
    name = "ChroniaHelper/FlagCarouselManageTrigger",
    placements =
    {
        name = "FlagCarouselManageTrigger",
        data =
        {
            carouselId = "",
            carouselMode = "None",
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            onlyOnce = false
        }
    },
    fieldInformation =
    {
        carouselMode =
        {
            options =
            {
                "None",
                "Pause",
                "Resume",
                "Cancel",
                "Remove"
            },
            editable = false
        },
        enterMode = fieldOptions.enterMode,
        enterDelay =
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
        "carouselId",
        "carouselMode",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "onlyOnce"
    }
}
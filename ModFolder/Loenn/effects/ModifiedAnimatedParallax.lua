return {
    name = "ChroniaHelper/ModifiedAnimatedParallax",
    defaultData = { texture = "", x = 0, y = 0, only ="*", exclude="", flag = "", notflag = "", tag = "",
        scrollx=1.0, scrolly=1.0, loopX = false, loopY = false, fps = 12, frames = "",
        triggerFlag = "", playOnce = false, resetFlag = "", resetFrame = 0,
    },
    fieldOrder = {
        "only", "exclude", "flag", "notflag","tag","scrollx","scrolly",
    },
    canBackground = true,
    fieldInformation = {
        resetFrame = {
            fieldType = "integer",
        },
        texture = require("mods").requireFromPlugin("libraries.vivUtilsMig").GetFilePathWithTrailingNumbers(false)
    },
}
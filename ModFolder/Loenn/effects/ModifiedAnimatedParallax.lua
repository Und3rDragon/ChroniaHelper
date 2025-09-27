local bg = {}

bg.name = "ChroniaHelper/ModifiedAnimatedParallax"
bg.defaultData = { texture = "", x = 0, y = 0, only ="*", exclude="", flag = "", notflag = "", tag = "",
    scrollx=1.0, scrolly=1.0, loopX = false, loopY = false, fps = 12, frames = "",
    triggerFlag = "", playOnce = false, resetFlag = "", resetFrame = 0,
}
bg.fieldOrder = {
    "only", "exclude", "flag", "notflag","tag","scrollx","scrolly",
}
bg.canBackground = true
bg.fieldInformation = {
    resetFrame = {
        fieldType = "integer",
    },
    texture = require("mods").requireFromPlugin("libraries.vivUtilsMig").GetFilePathWithTrailingNumbers(false)
}
    
--return bg
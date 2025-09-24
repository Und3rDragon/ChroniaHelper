return {
    name = "ChroniaHelper/ModifiedAnimatedParallax",
    defaultData = { texture = "", only ="*", exclude="", flag = "", notflag = "", tag = "",
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
        }
    },
}
return {
    name = "ChroniaHelper/WaterSurface",
    defaultData = { only ="*", exclude="", flag = "", notflag = "",
        yFar=90, yNear=120,
        scrollXFar=1.0, scrollYFar=1.0, scrollXNear=1.2, scrollYNear=1.2, particleColors="ffffff", particleCount = 50, 
        alphaFar = 0.5, alphaNear = 1.0,
        waterSpeedFar=10, waterSpeedNear=20, particleScaleFar=6, particleScaleNear=2,
        extendedBorderX = 0, extendedBorderY = 0,
        hasFarLine = false, hasCloseLine = true,
    },
    fieldOrder = {
        "only", "exclude", "flag", "notflag","tag","scrollx","scrolly","Colors"
    },
    canBackground = true,
    fieldInformation = {
        particleColors = {
            fieldType = "list",
            elementOptions = {
                fieldType = "color",
            },
        },
    },
}